using System;
using System.Collections.Generic;
using MusicBeeWrapped;

namespace MusicBeePlugin.Services
{
    /// <summary>
    /// Captures and stores track metadata at the moment tracking begins
    /// </summary>
    public class CapturedTrackMetadata
    {
        public string FileUrl { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public string Genre { get; set; }
        public string Year { get; set; }
        public int Duration { get; set; }
        public string PlaylistName { get; set; }
        public string ListeningMode { get; set; }
        public DateTime CapturedAt { get; set; }
    }

    /// <summary>
    /// Service responsible for tracking and recording track plays
    /// </summary>
    public class TrackingService
    {
        private readonly Plugin _plugin;
        private readonly YearBasedDataService _dataService;
        
        private CapturedTrackMetadata _currentTrackMetadata;
        private DateTime _trackStartTime;
        private DateTime _pauseStartTime;
        private TimeSpan _accumulatedPlayTime;
        private bool _isTrackingActive;
        private DateTime _lastFiveSecondCheck;
        private bool _recordedByFiveSecondRule;
        
        /// <summary>
        /// Minimum play duration (in seconds) required to record a track
        /// </summary>
        public const int MinimumPlayDuration = 30;
        
        /// <summary>
        /// Duration (in seconds) after which a track is automatically recorded regardless of what happens next
        /// </summary>
        public const int FiveSecondRuleDuration = 5;
        
        public TrackingService(Plugin plugin, YearBasedDataService dataService)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        }
        
        /// <summary>
        /// Initializes the tracking service
        /// </summary>
        public void Initialize()
        {
            // Initialize tracking state
            ClearCurrentTrack();
            
            // The YearBasedDataService handles its own initialization
            _plugin.ApiInterface.MB_Trace("TrackingService initialized with XML-based data storage and enhanced pause/resume support");
        }
        
        /// <summary>
        /// Gets the current play history for a specific year
        /// </summary>
        public MusicBeeWrapped.PlayHistory GetPlayHistory(int year) => _dataService.GetYearData(year);
        
        /// <summary>
        /// Gets all available years with data
        /// </summary>
        public List<int> GetAvailableYears() => _dataService.GetAvailableYears();
        
        /// <summary>
        /// Handles track change notifications
        /// </summary>
        public void OnTrackChanged()
        {
            string nowPlayingUrl = _plugin.ApiInterface.NowPlaying_GetFileUrl();
            
            // Record the previous track if there was one
            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                RecordTrackEnd();
            }
            
            // Start tracking the new track
            if (!string.IsNullOrEmpty(nowPlayingUrl))
            {
                StartTrackingNewTrack(nowPlayingUrl);
            }
            else
            {
                ClearCurrentTrack();
            }
        }
        
        /// <summary>
        /// Handles play state change notifications with proper pause/resume support
        /// </summary>
        public void OnPlayStateChanged(Plugin.PlayState playState)
        {
            switch (playState)
            {
                case Plugin.PlayState.Playing:
                    HandlePlayState();
                    break;
                    
                case Plugin.PlayState.Paused:
                    HandlePauseState();
                    break;
                    
                case Plugin.PlayState.Stopped:
                    HandleStopState();
                    break;
            }
        }
        
        private void HandlePlayState()
        {
            string nowPlayingUrl = _plugin.ApiInterface.NowPlaying_GetFileUrl();
            
            if (string.IsNullOrEmpty(nowPlayingUrl))
                return;
                
            if (_currentTrackMetadata?.FileUrl == nowPlayingUrl && _isTrackingActive)
            {
                // Resuming the same track - add pause duration to accumulated time
                if (_pauseStartTime != default(DateTime))
                {
                    var pauseDuration = DateTime.Now - _pauseStartTime;
                    _accumulatedPlayTime += pauseDuration;
                    _pauseStartTime = default(DateTime);
                    
                    _plugin.ApiInterface.MB_Trace($"Resumed tracking: {_currentTrackMetadata.Artist} - {_currentTrackMetadata.Title} (paused for {pauseDuration.TotalSeconds:F1}s)");
                }
                
                // Check for 5-second rule
                CheckFiveSecondRule();
            }
            else
            {
                // New track started - record previous if exists
                if (_isTrackingActive && _currentTrackMetadata != null)
                {
                    RecordTrackEnd();
                }
                
                // Start tracking new track
                StartTrackingNewTrack(nowPlayingUrl);
            }
        }
        
        private void HandlePauseState()
        {
            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                _pauseStartTime = DateTime.Now;
                
                _plugin.ApiInterface.MB_Trace($"Paused tracking: {_currentTrackMetadata.Artist} - {_currentTrackMetadata.Title}");
            }
        }
        
        private void HandleStopState()
        {
            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                // Stop means end of track - record it and clear tracking
                RecordTrackEnd();
                ClearCurrentTrack();
                
                _plugin.ApiInterface.MB_Trace("Stopped playback - recorded current track");
            }
        }
        
        private void StartTrackingNewTrack(string trackUrl)
        {
            // Capture all metadata at the moment tracking begins
            _currentTrackMetadata = CaptureTrackMetadata(trackUrl);
            _trackStartTime = DateTime.Now;
            _pauseStartTime = default(DateTime);
            _accumulatedPlayTime = TimeSpan.Zero;
            _isTrackingActive = true;
            _lastFiveSecondCheck = DateTime.Now;
            _recordedByFiveSecondRule = false;
            
            _plugin.ApiInterface.MB_Trace($"Started tracking new track: {_currentTrackMetadata.Artist} - {_currentTrackMetadata.Title}");
        }
        
        private void ClearCurrentTrack()
        {
            _currentTrackMetadata = null;
            _trackStartTime = default(DateTime);
            _pauseStartTime = default(DateTime);
            _accumulatedPlayTime = TimeSpan.Zero;
            _isTrackingActive = false;
            _lastFiveSecondCheck = default(DateTime);
            _recordedByFiveSecondRule = false;
        }
        
        /// <summary>
        /// Records the end of the current track if it meets minimum duration requirements
        /// </summary>
        private void RecordTrackEnd()
        {
            if (!_isTrackingActive || _currentTrackMetadata == null) return;
            
            // Calculate actual play duration (excluding pauses)
            TimeSpan actualPlayDuration = CalculateActualPlayDuration();
            
            // Record or update track play duration
            if (actualPlayDuration.TotalSeconds >= MinimumPlayDuration)
            {
                var track = new MusicBeeWrapped.TrackPlay
                {
                    // Use the captured metadata instead of querying again
                    FileUrl = _currentTrackMetadata.FileUrl,
                    Title = _currentTrackMetadata.Title ?? "Unknown Title",
                    Artist = _currentTrackMetadata.Artist ?? "Unknown Artist", 
                    Album = _currentTrackMetadata.Album ?? "Unknown Album",
                    AlbumArtist = _currentTrackMetadata.AlbumArtist ?? "Unknown Album Artist",
                    Genre = _currentTrackMetadata.Genre ?? "Unknown Genre",
                    Year = _currentTrackMetadata.Year ?? DateTime.Now.Year.ToString(),
                    Duration = _currentTrackMetadata.Duration,
                    PlayedAt = _trackStartTime,
                    PlayDuration = (int)actualPlayDuration.TotalSeconds,
                    
                    // Enhanced context fields
                    PlaylistName = _currentTrackMetadata.PlaylistName,
                    ListeningMode = _currentTrackMetadata.ListeningMode
                };
                
                if (_recordedByFiveSecondRule)
                {
                    // Update existing 5-second record with full play duration
                    _dataService.UpdatePlayDuration(track);
                    _plugin.ApiInterface.MB_Trace($"Updated 5-second record: {track.Artist} - {track.Title} (updated to {track.PlayDuration}s actual play time)");
                }
                else
                {
                    // Record new play using the XML-based data service
                    _dataService.RecordPlay(track);
                    _plugin.ApiInterface.MB_Trace($"Recorded: {track.Artist} - {track.Title} ({track.PlayDuration}s actual play time)");
                }
            }
            else if (_recordedByFiveSecondRule)
            {
                _plugin.ApiInterface.MB_Trace($"Track already recorded by 5-second rule and play time insufficient for update: {_currentTrackMetadata.Artist} - {_currentTrackMetadata.Title} ({actualPlayDuration.TotalSeconds:F1}s < {MinimumPlayDuration}s required)");
            }
            else
            {
                _plugin.ApiInterface.MB_Trace($"Track not recorded - insufficient play time ({actualPlayDuration.TotalSeconds:F1}s < {MinimumPlayDuration}s required)");
            }
        }
        
        /// <summary>
        /// Calculates the actual play duration excluding pause time
        /// </summary>
        private TimeSpan CalculateActualPlayDuration()
        {
            if (_trackStartTime == default(DateTime))
                return TimeSpan.Zero;
                
            TimeSpan totalElapsed = DateTime.Now - _trackStartTime;
            
            // If currently paused, add the current pause duration to accumulated pause time
            if (_pauseStartTime != default(DateTime))
            {
                TimeSpan currentPauseDuration = DateTime.Now - _pauseStartTime;
                return totalElapsed - _accumulatedPlayTime - currentPauseDuration;
            }
            
            // Otherwise, just subtract accumulated pause time
            return totalElapsed - _accumulatedPlayTime;
        }

        /// <summary>
        /// Finalizes tracking and saves any pending data
        /// </summary>
        public void Shutdown()
        {
            // Record any current track before closing
            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                RecordTrackEnd();
                ClearCurrentTrack();
            }
            
            // YearBasedDataService handles its own data persistence
            _plugin.ApiInterface.MB_Trace("TrackingService shutdown - data saved via YearBasedDataService");
        }

        /// <summary>
        /// Gets the current playlist name if available
        /// </summary>
        private string GetCurrentPlaylist()
        {
            try
            {
                // Try to get current playlist info - this may be limited by MusicBee API
                // For now, we'll use a simpler approach
                return "Library"; // Default - can be enhanced later with more sophisticated detection
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Determines the current listening mode (shuffle, sequential, etc.)
        /// </summary>
        private string GetListeningMode()
        {
            try
            {
                bool isShuffleMode = _plugin.ApiInterface.Player_GetShuffle();
                var repeatMode = _plugin.ApiInterface.Player_GetRepeat();
                
                if (isShuffleMode)
                {
                    return "Shuffle";
                }
                
                switch (repeatMode)
                {
                    case Plugin.RepeatMode.All:
                        return "Repeat All";
                    case Plugin.RepeatMode.One:
                        return "Repeat One";
                    case Plugin.RepeatMode.None:
                    default:
                        return "Sequential";
                }
            }
            catch
            {
                return "Sequential"; // Default fallback
            }
        }

        /// <summary>
        /// Captures all relevant track metadata at the moment tracking begins
        /// </summary>
        private CapturedTrackMetadata CaptureTrackMetadata(string trackUrl)
        {
            return new CapturedTrackMetadata
            {
                FileUrl = trackUrl,
                Title = _plugin.ApiInterface.NowPlaying_GetFileTag(Plugin.MetaDataType.TrackTitle) ?? "Unknown Title",
                Artist = _plugin.ApiInterface.NowPlaying_GetFileTag(Plugin.MetaDataType.Artist) ?? "Unknown Artist",
                Album = _plugin.ApiInterface.NowPlaying_GetFileTag(Plugin.MetaDataType.Album) ?? "Unknown Album",
                AlbumArtist = _plugin.ApiInterface.NowPlaying_GetFileTag(Plugin.MetaDataType.AlbumArtist) ?? "Unknown Album Artist",
                Genre = _plugin.ApiInterface.NowPlaying_GetFileTag(Plugin.MetaDataType.Genre) ?? "Unknown Genre",
                Year = _plugin.ApiInterface.NowPlaying_GetFileTag(Plugin.MetaDataType.Year) ?? DateTime.Now.Year.ToString(),
                Duration = _plugin.ApiInterface.NowPlaying_GetDuration(),
                PlaylistName = GetCurrentPlaylist(),
                ListeningMode = GetListeningMode(),
                CapturedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Checks if the track has been playing for 5 seconds and records it if so
        /// </summary>
        private void CheckFiveSecondRule()
        {
            if (!_isTrackingActive || _currentTrackMetadata == null || _recordedByFiveSecondRule)
                return;
                
            TimeSpan actualPlayDuration = CalculateActualPlayDuration();
            
            // If we've been playing for 5 seconds and haven't checked recently
            if (actualPlayDuration.TotalSeconds >= FiveSecondRuleDuration && 
                (DateTime.Now - _lastFiveSecondCheck).TotalSeconds >= 1)
            {
                _lastFiveSecondCheck = DateTime.Now;
                
                // Record the play immediately (this is separate from the normal 30-second rule)
                var track = new MusicBeeWrapped.TrackPlay
                {
                    FileUrl = _currentTrackMetadata.FileUrl,
                    Title = _currentTrackMetadata.Title,
                    Artist = _currentTrackMetadata.Artist,
                    Album = _currentTrackMetadata.Album,
                    AlbumArtist = _currentTrackMetadata.AlbumArtist,
                    Genre = _currentTrackMetadata.Genre,
                    Year = _currentTrackMetadata.Year,
                    Duration = _currentTrackMetadata.Duration,
                    PlayedAt = _trackStartTime,
                    PlayDuration = (int)actualPlayDuration.TotalSeconds,
                    PlaylistName = _currentTrackMetadata.PlaylistName,
                    ListeningMode = _currentTrackMetadata.ListeningMode
                };
                
                _dataService.RecordPlay(track);
                _plugin.ApiInterface.MB_Trace($"5-second rule: Recorded {track.Artist} - {track.Title} ({track.PlayDuration}s)");
                
                // Mark as recorded by 5-second rule, but keep tracking for potential 30-second record
                _recordedByFiveSecondRule = true;
            }
        }

        /// <summary>
        /// Should be called periodically (e.g., every second) to check for 5-second rule enforcement
        /// </summary>
        public void PeriodicCheck()
        {
            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                CheckFiveSecondRule();
            }
        }
    }
}
