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
            ClearCurrentTrack();
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

            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                RecordTrackEnd();
            }

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
                if (_pauseStartTime != default(DateTime))
                {
                    var pauseDuration = DateTime.Now - _pauseStartTime;
                    _accumulatedPlayTime += pauseDuration;
                    _pauseStartTime = default(DateTime);
                }

                CheckFiveSecondRule();
            }
            else
            {
                if (_isTrackingActive && _currentTrackMetadata != null)
                {
                    RecordTrackEnd();
                }

                StartTrackingNewTrack(nowPlayingUrl);
            }
        }

        private void HandlePauseState()
        {
            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                _pauseStartTime = DateTime.Now;
            }
        }

        private void HandleStopState()
        {
            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                RecordTrackEnd();
                ClearCurrentTrack();
            }
        }

        private void StartTrackingNewTrack(string trackUrl)
        {
            _currentTrackMetadata = CaptureTrackMetadata(trackUrl);
            _trackStartTime = DateTime.Now;
            _pauseStartTime = default(DateTime);
            _accumulatedPlayTime = TimeSpan.Zero;
            _isTrackingActive = true;
            _lastFiveSecondCheck = DateTime.Now;
            _recordedByFiveSecondRule = false;
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

            TimeSpan actualPlayDuration = CalculateActualPlayDuration();

            if (actualPlayDuration.TotalSeconds >= MinimumPlayDuration)
            {
                var track = new MusicBeeWrapped.TrackPlay
                {
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
                    PlaylistName = _currentTrackMetadata.PlaylistName,
                    ListeningMode = _currentTrackMetadata.ListeningMode
                };

                if (_recordedByFiveSecondRule)
                {
                    _dataService.UpdatePlayDuration(track);
                }
                else
                {
                    _dataService.RecordPlay(track);
                }
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

            if (_pauseStartTime != default(DateTime))
            {
                TimeSpan currentPauseDuration = DateTime.Now - _pauseStartTime;
                return totalElapsed - _accumulatedPlayTime - currentPauseDuration;
            }

            return totalElapsed - _accumulatedPlayTime;
        }

        /// <summary>
        /// Finalizes tracking and saves any pending data
        /// </summary>
        public void Shutdown()
        {
            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                RecordTrackEnd();
                ClearCurrentTrack();
            }
        }

        private string GetCurrentPlaylist()
        {
            try
            {
                return "Library";
            }
            catch
            {
                return "Unknown";
            }
        }

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
                return "Sequential";
            }
        }

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

        private void CheckFiveSecondRule()
        {
            if (!_isTrackingActive || _currentTrackMetadata == null || _recordedByFiveSecondRule)
                return;

            TimeSpan actualPlayDuration = CalculateActualPlayDuration();

            if (actualPlayDuration.TotalSeconds >= FiveSecondRuleDuration &&
                (DateTime.Now - _lastFiveSecondCheck).TotalSeconds >= 1)
            {
                _lastFiveSecondCheck = DateTime.Now;

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
                _recordedByFiveSecondRule = true;
            }
        }

        public void PeriodicCheck()
        {
            if (_isTrackingActive && _currentTrackMetadata != null)
            {
                CheckFiveSecondRule();
            }
        }
    }
}
