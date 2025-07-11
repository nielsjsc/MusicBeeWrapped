using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using MusicBeePlugin.Services;
using MusicBeeWrapped;

namespace MusicBeePlugin.Services
{
    /// <summary>
    /// Year-based data collection service with enhanced organization and migration support
    /// </summary>
    public class YearBasedDataService
    {
        private readonly string _baseDataPath;
        private readonly Dictionary<int, PlayHistory> _yearlyData;
        private readonly ConcurrentQueue<TrackPlay> _pendingWrites;
        private readonly Timer _saveTimer;
        private readonly object _saveLock = new object();
        private YearMetadataCollection _metadata;
        private bool _disposed = false;
        private int _currentYear;

        public YearBasedDataService(string baseDataPath)
        {
            _baseDataPath = baseDataPath;
            _yearlyData = new Dictionary<int, PlayHistory>();
            _pendingWrites = new ConcurrentQueue<TrackPlay>();
            _currentYear = DateTime.Now.Year;
            
            System.Diagnostics.Debug.WriteLine($"YearBasedDataService: Initializing with base path: {_baseDataPath}");
            System.Diagnostics.Debug.WriteLine($"YearBasedDataService: Current year: {_currentYear}");
            
            // Auto-save every 30 seconds if there are pending writes
            _saveTimer = new Timer(AutoSave, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            
            InitializeData();
        }

        /// <summary>
        /// Gets play history for a specific year
        /// </summary>
        public PlayHistory GetYearData(int year)
        {
            if (!_yearlyData.ContainsKey(year))
            {
                LoadYearData(year);
            }
            
            return _yearlyData.ContainsKey(year) ? _yearlyData[year] : new PlayHistory();
        }

        /// <summary>
        /// Gets the current year's play history
        /// </summary>
        public PlayHistory GetCurrentYearData()
        {
            return GetYearData(_currentYear);
        }
        

        /// <summary>
        /// Records a new track play (automatically assigned to correct year)
        /// </summary>
        public void RecordPlay(TrackPlay trackPlay)
        {
            if (trackPlay == null) return;

            try
            {
                int playYear = trackPlay.PlayedAt.Year;
                
                // Ensure we have data loaded for this year
                var yearData = GetYearData(playYear);
                
                // Add to in-memory collection immediately
                yearData.AddPlay(trackPlay);
                
                // Queue for async save
                _pendingWrites.Enqueue(trackPlay);
                
                // Update year metadata
                UpdateYearMetadata(playYear);
                
                // Force immediate save for debugging
                SaveYearData(playYear);
                
                System.Diagnostics.Debug.WriteLine($"Recorded play for {trackPlay.Artist} - {trackPlay.Title} in year {playYear} - Data saved immediately");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recording play: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the play duration of an existing track play record
        /// Used when a track was initially recorded by the 5-second rule but then played to completion
        /// </summary>
        public void UpdatePlayDuration(TrackPlay trackPlay)
        {
            if (trackPlay == null) return;

            try
            {
                int playYear = trackPlay.PlayedAt.Year;
                
                // Ensure we have data loaded for this year
                var yearData = GetYearData(playYear);
                
                // Find existing play record that matches file URL and start time
                var existingPlay = yearData.Plays.FirstOrDefault(p => 
                    p.FileUrl == trackPlay.FileUrl && 
                    Math.Abs((p.PlayedAt - trackPlay.PlayedAt).TotalSeconds) < 5); // Allow 5 second tolerance for timing differences
                
                if (existingPlay != null)
                {
                    // Update the play duration
                    int oldDuration = existingPlay.PlayDuration;
                    existingPlay.PlayDuration = trackPlay.PlayDuration;
                    
                    // Queue for async save
                    _pendingWrites.Enqueue(trackPlay);
                    
                    // Update year metadata
                    UpdateYearMetadata(playYear);
                    
                    // Force immediate save for debugging
                    SaveYearData(playYear);
                    
                    System.Diagnostics.Debug.WriteLine($"Updated play duration for {trackPlay.Artist} - {trackPlay.Title} from {oldDuration}s to {trackPlay.PlayDuration}s in year {playYear}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No matching play record found to update for {trackPlay.Artist} - {trackPlay.Title} at {trackPlay.PlayedAt}");
                    // If we can't find the existing record, just record it as a new play
                    RecordPlay(trackPlay);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating play duration: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all available years with data
        /// </summary>
        public List<int> GetAvailableYears()
        {
            var years = DataPathService.GetAvailableYears(_baseDataPath);
            
            // Also include any years we have in memory
            foreach (var year in _yearlyData.Keys)
            {
                if (!years.Contains(year))
                {
                    years.Add(year);
                }
            }
            
            // Preload metadata for all discovered years
            foreach (var year in years)
            {
                LoadYearMetadata(year);
            }
            
            return years.OrderByDescending(y => y).ToList();
        }

        /// <summary>
        /// Gets metadata for all years
        /// </summary>
        public YearMetadataCollection GetMetadata()
        {
            return _metadata;
        }

        /// <summary>
        /// Gets metadata for a specific year
        /// </summary>
        public YearMetadata GetYearMetadata(int year)
        {
            return _metadata.GetYearMetadata(year);
        }

        private void InitializeData()
        {
            System.Diagnostics.Debug.WriteLine($"YearBasedDataService: InitializeData called");
            
            LoadMetadata();
            
            // Check for legacy data migration
            if (DataPathService.HasLegacyData(_baseDataPath))
            {
                System.Diagnostics.Debug.WriteLine($"YearBasedDataService: Legacy data found, migrating...");
                MigrateLegacyData();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"YearBasedDataService: No legacy data found");
            }
            
            // Load current year data
            LoadYearData(_currentYear);
            
            System.Diagnostics.Debug.WriteLine($"YearBasedDataService: InitializeData completed");
        }

        private void LoadYearData(int year)
        {
            if (_yearlyData.ContainsKey(year)) return;

            var historyFilePath = DataPathService.GetYearHistoryFilePath(_baseDataPath, year);
            var backupFilePath = DataPathService.GetYearBackupFilePath(_baseDataPath, year);
            
            System.Diagnostics.Debug.WriteLine($"LoadYearData: Attempting to load year {year}");
            System.Diagnostics.Debug.WriteLine($"LoadYearData: Looking for main file at: {historyFilePath}");
            System.Diagnostics.Debug.WriteLine($"LoadYearData: Main file exists: {File.Exists(historyFilePath)}");
            
            PlayHistory yearHistory = null;

            // Try loading from main file
            if (TryLoadPlayHistory(historyFilePath, out yearHistory))
            {
                _yearlyData[year] = yearHistory;
                System.Diagnostics.Debug.WriteLine($"LoadYearData: Successfully loaded {yearHistory.Plays.Count} plays for year {year}");
                
                // Debug: Check if any plays are actually for the requested year
                var playsForYear = yearHistory.Plays.Where(p => p.PlayedAt.Year == year).Count();
                System.Diagnostics.Debug.WriteLine($"LoadYearData: {playsForYear} plays are actually for year {year}");
                
                // Also load the metadata for this year
                LoadYearMetadata(year);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"LoadYearData: Failed to load main file, trying backup at: {backupFilePath}");
            System.Diagnostics.Debug.WriteLine($"LoadYearData: Backup file exists: {File.Exists(backupFilePath)}");

            // Try loading from backup
            if (TryLoadPlayHistory(backupFilePath, out yearHistory))
            {
                _yearlyData[year] = yearHistory;
                System.Diagnostics.Debug.WriteLine($"LoadYearData: Successfully loaded {yearHistory.Plays.Count} plays for year {year} from backup");
                
                // Debug: Check if any plays are actually for the requested year
                var playsForYear = yearHistory.Plays.Where(p => p.PlayedAt.Year == year).Count();
                System.Diagnostics.Debug.WriteLine($"LoadYearData: {playsForYear} plays are actually for year {year}");
                
                // Also load the metadata for this year
                LoadYearMetadata(year);
                return;
            }

            // Create new history for this year
            _yearlyData[year] = new PlayHistory();
            System.Diagnostics.Debug.WriteLine($"LoadYearData: No files found, created new empty play history for year {year}");
        }

        private bool TryLoadPlayHistory(string filePath, out PlayHistory playHistory)
        {
            playHistory = null;

            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"TryLoadPlayHistory: File does not exist: {filePath}");
                return false;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"TryLoadPlayHistory: Attempting to load: {filePath}");
                
                // Try XML first (.xml files)
                if (filePath.EndsWith(".xml"))
                {
                    playHistory = XmlDataService.LoadFromXml<PlayHistory>(filePath);
                    bool success = playHistory != null;
                    System.Diagnostics.Debug.WriteLine($"TryLoadPlayHistory: XML load result: {success}, plays: {(playHistory?.Plays?.Count ?? 0)}");
                    return success;
                }
                
                // For legacy .dat files, try binary format
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    playHistory = (PlayHistory)formatter.Deserialize(fs);
                    bool success = playHistory != null;
                    System.Diagnostics.Debug.WriteLine($"TryLoadPlayHistory: Binary load result: {success}, plays: {(playHistory?.Plays?.Count ?? 0)}");
                    return success;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TryLoadPlayHistory: Error loading from {filePath}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TryLoadPlayHistory: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private void MigrateLegacyData()
        {
            var legacyPath = DataPathService.GetLegacyHistoryFilePath(_baseDataPath);
            
            try
            {
                if (TryLoadPlayHistory(legacyPath, out PlayHistory legacyHistory))
                {
                    System.Diagnostics.Debug.WriteLine($"Migrating {legacyHistory.Plays.Count} legacy plays to year-based storage");
                    
                    // Group plays by year
                    var playsByYear = legacyHistory.Plays.GroupBy(p => p.PlayedAt.Year);
                    
                    foreach (var yearGroup in playsByYear)
                    {
                        int year = yearGroup.Key;
                        var yearData = GetYearData(year);
                        
                        foreach (var play in yearGroup)
                        {
                            yearData.AddPlay(play);
                        }
                        
                        // Save this year's data immediately
                        SaveYearData(year);
                        UpdateYearMetadata(year);
                    }
                    
                    // Backup legacy file
                    var legacyBackup = legacyPath + ".legacy_backup";
                    File.Move(legacyPath, legacyBackup);
                    
                    System.Diagnostics.Debug.WriteLine("Legacy data migration completed successfully");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error migrating legacy data: {ex.Message}");
            }
        }

        private void LoadMetadata()
        {
            // For backwards compatibility, try to load global metadata first
            var globalMetadataPath = DataPathService.GetMetadataFilePath(_baseDataPath);
            
            if (TryLoadMetadata(globalMetadataPath, out YearMetadataCollection metadata))
            {
                _metadata = metadata;
                
                // Migrate global metadata to per-year files
                MigrateGlobalMetadataToPerYear();
            }
            else
            {
                _metadata = new YearMetadataCollection();
            }
        }
        
        private void MigrateGlobalMetadataToPerYear()
        {
            try
            {
                foreach (var yearMetadata in _metadata.Years)
                {
                    SaveYearMetadata(yearMetadata.Key, yearMetadata.Value);
                }
                
                // After successful migration, you could optionally delete the global file
                // File.Delete(DataPathService.GetMetadataFilePath(_baseDataPath));
                
                System.Diagnostics.Debug.WriteLine("Migrated global metadata to per-year files");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error migrating global metadata: {ex.Message}");
            }
        }
        
        private void LoadYearMetadata(int year)
        {
            var yearMetadataPath = DataPathService.GetYearMetadataFilePath(_baseDataPath, year);
            
            if (TryLoadYearMetadata(yearMetadataPath, out YearMetadata yearMetadata))
            {
                _metadata.UpdateYearMetadata(year, yearMetadata);
            }
        }
        
        private bool TryLoadYearMetadata(string filePath, out YearMetadata metadata)
        {
            metadata = null;

            if (!File.Exists(filePath)) return false;

            try
            {
                metadata = XmlDataService.LoadFromXml<YearMetadata>(filePath);
                return metadata != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading year metadata from {filePath}: {ex.Message}");
                return false;
            }
        }

        private bool TryLoadMetadata(string filePath, out YearMetadataCollection metadata)
        {
            metadata = null;

            if (!File.Exists(filePath)) return false;

            try
            {
                // Try XML first (.xml files)
                if (filePath.EndsWith(".xml"))
                {
                    metadata = XmlDataService.LoadFromXml<YearMetadataCollection>(filePath);
                    return metadata != null;
                }
                
                // For legacy .dat files, try binary format
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    metadata = (YearMetadataCollection)formatter.Deserialize(fs);
                    return metadata != null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading metadata: {ex.Message}");
                return false;
            }
        }

        private void UpdateYearMetadata(int year)
        {
            if (!_yearlyData.ContainsKey(year)) return;
            
            var yearData = _yearlyData[year];
            var plays = yearData.Plays;
            
            if (!plays.Any()) return;
            
            var metadata = new YearMetadata
            {
                Year = year,
                TotalPlays = plays.Count,
                TotalMinutes = plays.Sum(p => p.PlayDuration) / 60,
                FirstPlay = plays.Min(p => p.PlayedAt),
                LastPlay = plays.Max(p => p.PlayedAt),
                TopArtist = plays.GroupBy(p => p.Artist).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "",
                TopTrack = plays.GroupBy(p => $"{p.Artist} - {p.Title}").OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "",
                TopGenre = plays.Where(p => !string.IsNullOrEmpty(p.Genre)).GroupBy(p => p.Genre).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? ""
            };
            
            _metadata.UpdateYearMetadata(year, metadata);
            
            // Save the individual year metadata file
            SaveYearMetadata(year, metadata);
        }
        
        private void SaveYearMetadata(int year, YearMetadata metadata)
        {
            var yearMetadataPath = DataPathService.GetYearMetadataFilePath(_baseDataPath, year);
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to save metadata for year {year} to: {yearMetadataPath}");
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(yearMetadataPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"Created directory for year metadata: {directory}");
                }
                
                XmlDataService.SaveToXml(metadata, yearMetadataPath);
                System.Diagnostics.Debug.WriteLine($"Successfully saved metadata for year {year} to {yearMetadataPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving metadata for year {year}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public void SaveAllData()
        {
            if (_disposed) return;

            lock (_saveLock)
            {
                try
                {
                    // Save all loaded year data and their metadata
                    foreach (var year in _yearlyData.Keys)
                    {
                        SaveYearData(year);
                        
                        // Update and save metadata for this year
                        UpdateYearMetadata(year);
                    }
                    
                    // Clear pending writes
                    while (_pendingWrites.TryDequeue(out _)) { }
                    
                    System.Diagnostics.Debug.WriteLine("Saved all year-based data and metadata");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving all data: {ex.Message}");
                }
            }
        }

        private void SaveYearData(int year)
        {
            if (!_yearlyData.ContainsKey(year)) return;

            var historyFilePath = DataPathService.GetYearHistoryFilePath(_baseDataPath, year);
            var backupFilePath = DataPathService.GetYearBackupFilePath(_baseDataPath, year);
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to save year {year} data to: {historyFilePath}");
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(historyFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"Created directory: {directory}");
                }
                
                // Create backup of current file
                if (File.Exists(historyFilePath))
                {
                    File.Copy(historyFilePath, backupFilePath, true);
                    System.Diagnostics.Debug.WriteLine($"Created backup: {backupFilePath}");
                }

                // Save to XML file
                XmlDataService.SaveToXml(_yearlyData[year], historyFilePath);
                
                System.Diagnostics.Debug.WriteLine($"Successfully saved {_yearlyData[year].Plays.Count} plays for year {year} to {historyFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving data for year {year}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void SaveMetadata()
        {
            var metadataPath = DataPathService.GetMetadataFilePath(_baseDataPath);
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to save metadata to: {metadataPath}");
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(metadataPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"Created directory for metadata: {directory}");
                }
                
                XmlDataService.SaveToXml(_metadata, metadataPath);
                System.Diagnostics.Debug.WriteLine($"Successfully saved metadata to {metadataPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving metadata: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void AutoSave(object state)
        {
            if (_disposed || _pendingWrites.IsEmpty) return;
            
            // Save if there are pending writes
            Task.Run(() => SaveAllData());
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            _saveTimer?.Dispose();
            
            // Final save
            SaveAllData();
        }
    }
}
