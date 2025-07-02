using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MusicBeePlugin.Services
{
    /// <summary>
    /// Service for managing year-based data paths and organization
    /// </summary>
    public static class DataPathService
    {
        /// <summary>
        /// Creates and returns the plugin data directory path
        /// </summary>
        public static string InitializeDataDirectory()
        {
            // Use user's AppData\Roaming\MusicBee for plugin data
            string userAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dataPath = Path.Combine(userAppData, "MusicBee");
            
            // Create MusicBee directory if it doesn't exist
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            
            // Create plugin-specific subdirectory
            string wrappedFolder = Path.Combine(dataPath, "Plugins", "MusicBeeWrapped");
            if (!Directory.Exists(wrappedFolder))
            {
                Directory.CreateDirectory(wrappedFolder);
            }
            
            return wrappedFolder;
        }
        
        /// <summary>
        /// Gets the data directory for a specific year
        /// </summary>
        public static string GetYearDataPath(string baseDataPath, int year)
        {
            var yearPath = Path.Combine(baseDataPath, year.ToString());
            if (!Directory.Exists(yearPath))
            {
                Directory.CreateDirectory(yearPath);
            }
            return yearPath;
        }
        
        /// <summary>
        /// Gets the play history file path for a specific year
        /// </summary>
        public static string GetYearHistoryFilePath(string baseDataPath, int year)
        {
            return Path.Combine(GetYearDataPath(baseDataPath, year), "play_history.xml");
        }
        
        /// <summary>
        /// Gets the backup file path for a specific year
        /// </summary>
        public static string GetYearBackupFilePath(string baseDataPath, int year)
        {
            return Path.Combine(GetYearDataPath(baseDataPath, year), "play_history_backup.xml");
        }
        
        /// <summary>
        /// Gets all years that have data available
        /// </summary>
        public static List<int> GetAvailableYears(string baseDataPath)
        {
            var years = new List<int>();
            
            if (!Directory.Exists(baseDataPath))
                return years;
                
            foreach (var directory in Directory.GetDirectories(baseDataPath))
            {
                var dirName = Path.GetFileName(directory);
                if (int.TryParse(dirName, out int year) && year >= 2000 && year <= DateTime.Now.Year + 1)
                {
                    years.Add(year);
                }
            }
            
            return years.OrderByDescending(y => y).ToList();
        }
        
        /// <summary>
        /// Gets the legacy data file path (for migration)
        /// </summary>
        public static string GetLegacyHistoryFilePath(string baseDataPath)
        {
            return Path.Combine(baseDataPath, "play_history.dat");
        }
        
        /// <summary>
        /// Checks if legacy data exists (needs migration)
        /// </summary>
        public static bool HasLegacyData(string baseDataPath)
        {
            return File.Exists(GetLegacyHistoryFilePath(baseDataPath));
        }
        
        /// <summary>
        /// Gets metadata file path for storing year summaries (legacy - for backwards compatibility)
        /// </summary>
        public static string GetMetadataFilePath(string baseDataPath)
        {
            return Path.Combine(baseDataPath, "year_metadata.xml");
        }
        
        /// <summary>
        /// Gets metadata file path for a specific year
        /// </summary>
        public static string GetYearMetadataFilePath(string baseDataPath, int year)
        {
            return Path.Combine(GetYearDataPath(baseDataPath, year), "year_metadata.xml");
        }
        
        /// <summary>
        /// Gets the full path for the play history data file (legacy method for backwards compatibility)
        /// </summary>
        public static string GetHistoryFilePath(string dataDirectory)
        {
            return Path.Combine(dataDirectory, "play_history.dat");
        }
    }
}
