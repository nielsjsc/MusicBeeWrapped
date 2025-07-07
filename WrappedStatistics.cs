using System;
using System.Collections.Generic;
using System.Linq;
using MusicBeeWrapped.Models;

namespace MusicBeeWrapped
{
    public class WrappedStatistics
    {
        // Basic stats
        public int TotalTracks { get; private set; }
        public int TotalUniqueArtists { get; private set; }
        public int TotalUniqueAlbums { get; private set; }
        public double TotalHours { get; private set; }
        public double AverageTracksPerDay { get; private set; }
        public double AverageHoursPerMonth { get; private set; }
        
        // Engagement metrics
        public int SkippedTracks { get; private set; }
        public int FullPlays { get; private set; }
        
        // Top lists
        public List<KeyValuePair<string, int>> TopArtists { get; private set; }
        public List<KeyValuePair<string, int>> TopTracks { get; private set; }
        public List<KeyValuePair<string, int>> TopAlbums { get; private set; }
        public List<KeyValuePair<string, double>> TopGenres { get; private set; }
        
        // Time patterns
        public Dictionary<DayOfWeek, int> ListeningByDay { get; private set; }
        public Dictionary<int, int> ListeningByHour { get; private set; }
        public Dictionary<string, int> ListeningByMonth { get; private set; }
        public Dictionary<string, int> ListeningBySeason { get; private set; }
        
        // Streaks and patterns
        public int LongestListeningStreak { get; private set; }
        public int CurrentListeningStreak { get; private set; }
        public string MostActiveMonth { get; private set; }
        public string MostActiveSeason { get; private set; }
        public int MostActiveHour { get; private set; }
        public DayOfWeek MostActiveDay { get; private set; }
        
        // Discovery metrics
        public int NewArtistsDiscovered { get; private set; }
        public int NewGenresExplored { get; private set; }
        public List<string> GenresExplored { get; private set; }
        
        // New enhanced metrics
        public Dictionary<string, int> ListeningModeStats { get; private set; }
        public Dictionary<string, int> PlaylistStats { get; private set; }
        public Dictionary<string, double> DailyListeningMinutes { get; private set; }
        public Dictionary<string, int> DailyPlayCounts { get; private set; }
        public Dictionary<string, double> MonthlyListeningHours { get; private set; }
        public double WeekendVsWeekdayRatio { get; private set; }
        public List<KeyValuePair<string, int>> TopListeningSources { get; private set; }
        
        // Advanced analytics
        public List<ObsessionPeriod> ObsessionPeriods { get; private set; }
        public AlbumListeningBehavior AlbumBehavior { get; private set; }

        public WrappedStatistics(PlayHistory playHistory, int year)
        {
            var playsInYear = playHistory.GetPlaysByYear(year).ToList();
            CalculateBasicStats(playsInYear, year);
            CalculateEngagementMetrics(playsInYear);
            CalculateTopLists(playsInYear);
            CalculateTimePatterns(playsInYear);
            CalculateStreaksAndPatterns(playsInYear, playHistory, year);
            CalculateDiscoveryMetrics(playsInYear, playHistory, year);
            CalculateEnhancedMetrics(playsInYear);
            
            // Advanced analytics
            CalculateObsessionPeriods(playsInYear);
            CalculateAlbumListeningBehavior(playsInYear);
        }

        private void CalculateBasicStats(List<TrackPlay> plays, int year)
        {
            TotalTracks = plays.Count;
            TotalUniqueArtists = plays.Select(p => p.Artist).Distinct().Count();
            TotalUniqueAlbums = plays.Select(p => p.Album).Distinct().Count();
            TotalHours = Math.Round(plays.Sum(p => p.PlayDuration) / 3600.0, 1);
            
            // Calculate averages based on time range
            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = new DateTime(year, 12, 31).Date > DateTime.Today.Date 
                ? DateTime.Today.Date 
                : new DateTime(year, 12, 31);
            
            int totalDays = (endDate - startDate).Days + 1;
            AverageTracksPerDay = Math.Round((double)TotalTracks / totalDays, 1);
            
            int totalMonths = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;
            AverageHoursPerMonth = Math.Round(TotalHours / totalMonths, 1);
        }

        private void CalculateEngagementMetrics(List<TrackPlay> plays)
        {
            SkippedTracks = plays.Count(p => p.IsSkipped);
            FullPlays = plays.Count(p => p.IsFullPlay);
        }

        private void CalculateTopLists(List<TrackPlay> plays)
        {
            // Top Artists by play count
            TopArtists = plays
                .GroupBy(p => p.Artist)
                .Select(g => new KeyValuePair<string, int>(
                    string.IsNullOrEmpty(g.Key) ? "Unknown Artist" : g.Key, 
                    g.Count()))
                .OrderByDescending(x => x.Value)
                .Take(50)
                .ToList();

            // Top Tracks by play count
            TopTracks = plays
                .GroupBy(p => $"{p.Artist} - {p.Title}")
                .Select(g => new KeyValuePair<string, int>(
                    string.IsNullOrEmpty(g.Key) ? "Unknown Track" : g.Key,
                    g.Count()))
                .OrderByDescending(x => x.Value)
                .Take(50)
                .ToList();
                
            // Top Albums by play count
            TopAlbums = plays
                .GroupBy(p => $"{p.Album} - {p.Artist}")
                .Select(g => new KeyValuePair<string, int>(
                    string.IsNullOrEmpty(g.Key) ? "Unknown Album" : g.Key,
                    g.Count()))
                .OrderByDescending(x => x.Value)
                .Take(50)
                .ToList();

            // Top Genres by listening time
            var genreDurations = plays
                .GroupBy(p => string.IsNullOrEmpty(p.Genre) ? "Unknown Genre" : p.Genre)
                .Select(g => new { 
                    Genre = g.Key, 
                    TotalDuration = g.Sum(p => p.PlayDuration) 
                })
                .OrderByDescending(x => x.TotalDuration)
                .ToList();

            double totalDuration = genreDurations.Sum(g => g.TotalDuration);
            
            TopGenres = genreDurations
                .Select(g => new KeyValuePair<string, double>(
                    g.Genre,
                    Math.Round((g.TotalDuration / totalDuration) * 100, 1)
                ))
                .Take(20)
                .ToList();
        }

        private void CalculateTimePatterns(List<TrackPlay> plays)
        {
            // Calculate plays by day of week
            ListeningByDay = new Dictionary<DayOfWeek, int>();
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                ListeningByDay[day] = plays.Count(p => p.DayOfWeek == day);
            }
            MostActiveDay = ListeningByDay.OrderByDescending(x => x.Value).First().Key;

            // Calculate plays by hour of day
            ListeningByHour = new Dictionary<int, int>();
            for (int hour = 0; hour < 24; hour++)
            {
                ListeningByHour[hour] = plays.Count(p => p.HourOfDay == hour);
            }
            MostActiveHour = ListeningByHour.OrderByDescending(x => x.Value).First().Key;

            // Calculate plays by month
            ListeningByMonth = plays
                .GroupBy(p => p.MonthYear)
                .ToDictionary(g => g.Key, g => g.Count());
            MostActiveMonth = ListeningByMonth.Any() ? 
                ListeningByMonth.OrderByDescending(x => x.Value).First().Key : "";

            // Calculate plays by season
            ListeningBySeason = plays
                .GroupBy(p => p.Season)
                .ToDictionary(g => g.Key, g => g.Count());
            MostActiveSeason = ListeningBySeason.Any() ? 
                ListeningBySeason.OrderByDescending(x => x.Value).First().Key : "";
        }

        private void CalculateStreaksAndPatterns(List<TrackPlay> plays, PlayHistory playHistory, int year)
        {
            var listeningDays = playHistory.GetListeningDays(year);
            
            // Calculate longest streak
            LongestListeningStreak = CalculateLongestStreak(listeningDays);
            
            // Calculate current streak (if year is current year)
            if (year == DateTime.Now.Year)
            {
                CurrentListeningStreak = CalculateCurrentStreak(listeningDays);
            }
        }

        private void CalculateDiscoveryMetrics(List<TrackPlay> plays, PlayHistory playHistory, int year)
        {
            var previousYearPlays = playHistory.Plays.Where(p => p.PlayedAt.Year < year).ToList();
            var previousArtists = previousYearPlays.Select(p => p.Artist).Distinct().ToHashSet();
            var previousGenres = previousYearPlays.Select(p => p.Genre).Distinct().ToHashSet();
            
            var currentYearArtists = plays.Select(p => p.Artist).Distinct().ToList();
            var currentYearGenres = plays.Select(p => p.Genre).Distinct().ToList();
            
            NewArtistsDiscovered = currentYearArtists.Count(a => !previousArtists.Contains(a));
            NewGenresExplored = currentYearGenres.Count(g => !previousGenres.Contains(g));
            GenresExplored = currentYearGenres.Where(g => !string.IsNullOrEmpty(g)).ToList();
        }

        private int CalculateLongestStreak(List<DateTime> listeningDays)
        {
            if (!listeningDays.Any()) return 0;
            
            int maxStreak = 1;
            int currentStreak = 1;
            
            for (int i = 1; i < listeningDays.Count; i++)
            {
                if (listeningDays[i] == listeningDays[i - 1].AddDays(1))
                {
                    currentStreak++;
                    maxStreak = Math.Max(maxStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1;
                }
            }
            
            return maxStreak;
        }

        private int CalculateCurrentStreak(List<DateTime> listeningDays)
        {
            if (!listeningDays.Any()) return 0;
            
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            
            // Check if user listened today or yesterday
            bool listenedToday = listeningDays.Contains(today);
            bool listenedYesterday = listeningDays.Contains(yesterday);
            
            if (!listenedToday && !listenedYesterday) return 0;
            
            // Start from the most recent listening day
            var startDate = listenedToday ? today : yesterday;
            int streak = 0;
            
            for (var date = startDate; listeningDays.Contains(date); date = date.AddDays(-1))
            {
                streak++;
            }
            
            return streak;
        }

        private void CalculateEnhancedMetrics(List<TrackPlay> plays)
        {
            // Calculate listening mode preferences
            ListeningModeStats = plays
                .GroupBy(p => p.ListeningMode ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate playlist/source statistics
            PlaylistStats = plays
                .GroupBy(p => p.PlaylistName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            TopListeningSources = PlaylistStats
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();

            // Calculate daily listening minutes for legacy support
            DailyListeningMinutes = plays
                .GroupBy(p => p.DateString)
                .ToDictionary(g => g.Key, g => Math.Round(g.Sum(p => p.PlayDuration) / 60.0, 1));

            // Calculate daily play counts for improved chart
            DailyPlayCounts = plays
                .GroupBy(p => p.DateString)
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate monthly listening hours for overview
            MonthlyListeningHours = plays
                .GroupBy(p => p.MonthYear)
                .ToDictionary(g => g.Key, g => Math.Round(g.Sum(p => p.PlayDuration) / 3600.0, 1));

            // Calculate weekend vs weekday ratio
            var weekendMinutes = plays.Where(p => p.IsWeekend).Sum(p => p.PlayDuration) / 60.0;
            var weekdayMinutes = plays.Where(p => !p.IsWeekend).Sum(p => p.PlayDuration) / 60.0;
            
            WeekendVsWeekdayRatio = weekdayMinutes > 0 ? Math.Round(weekendMinutes / weekdayMinutes, 2) : 0;
        }

        /// <summary>
        /// Detects obsession periods using simplified statistical analysis
        /// </summary>
        private void CalculateObsessionPeriods(List<TrackPlay> plays)
        {
            ObsessionPeriods = new List<ObsessionPeriod>();

            // Detect artist obsessions
            var artistObsessions = DetectArtistObsessions(plays);
            ObsessionPeriods.AddRange(artistObsessions);

            // Detect album obsessions
            var albumObsessions = DetectAlbumObsessions(plays);
            ObsessionPeriods.AddRange(albumObsessions);

            // Detect track obsessions
            var trackObsessions = DetectTrackObsessions(plays);
            ObsessionPeriods.AddRange(trackObsessions);

            // Sort by intensity score (highest first)
            ObsessionPeriods = ObsessionPeriods.OrderByDescending(o => o.IntensityScore).ToList();
        }

        /// <summary>
        /// Detects artist obsessions based on days with significantly higher play counts than normal
        /// </summary>
        private List<ObsessionPeriod> DetectArtistObsessions(List<TrackPlay> plays)
        {
            var obsessions = new List<ObsessionPeriod>();
            
            // Group: "How many times did I play each artist each day?"
            var dailyStats = plays.GroupBy(p => new { Date = p.PlayedAt.Date, p.Artist })
                                 .ToDictionary(g => g.Key, g => g.Count());
            
            // For each artist, what's their typical daily play count?
            var artistAverages = dailyStats.GroupBy(kvp => kvp.Key.Artist)
                                          .ToDictionary(g => g.Key, g => g.Average(x => x.Value));
            
            foreach (var artist in artistAverages.Keys)
            {
                var avgDaily = artistAverages[artist];
                var threshold = Math.Max(3, avgDaily * 2.5); // At least 3 plays, or 2.5x normal
                
                // Find days exceeding threshold
                var obsessionDays = dailyStats.Where(kvp => kvp.Key.Artist == artist && kvp.Value >= threshold)
                                             .OrderBy(kvp => kvp.Key.Date)
                                             .ToList();
                
                if (obsessionDays.Count >= 3) // At least 3 obsession days
                {
                    var totalPlays = obsessionDays.Sum(kvp => kvp.Value);
                    var startDate = obsessionDays.First().Key.Date;
                    var endDate = obsessionDays.Last().Key.Date;
                    var intensity = (double)totalPlays / obsessionDays.Count;
                    
                    var obsession = new ObsessionPeriod
                    {
                        Artist = artist,
                        StartDate = startDate,
                        EndDate = endDate,
                        PlayCount = totalPlays,
                        IntensityScore = intensity,
                        ObsessionType = "Artist"
                    };
                    obsession.Description = obsession.GenerateDescription();
                    obsessions.Add(obsession);
                }
            }
            
            return obsessions;
        }

        /// <summary>
        /// Detects album obsessions based on same album played across multiple days
        /// </summary>
        private List<ObsessionPeriod> DetectAlbumObsessions(List<TrackPlay> plays)
        {
            var obsessions = new List<ObsessionPeriod>();
            
            // Group by album (Artist + Album combo) per day
            var dailyAlbumStats = plays.Where(p => !string.IsNullOrEmpty(p.Album))
                                      .GroupBy(p => new { Date = p.PlayedAt.Date, p.Artist, p.Album })
                                      .ToDictionary(g => g.Key, g => g.Count());
            
            // Look for albums played 2+ times per day across 2+ days
            var albumGroups = dailyAlbumStats.GroupBy(kvp => new { kvp.Key.Artist, kvp.Key.Album });
            
            foreach (var albumGroup in albumGroups)
            {
                var heavyDays = albumGroup.Where(kvp => kvp.Value >= 2) // 2+ plays in a day
                                         .OrderBy(kvp => kvp.Key.Date)
                                         .ToList();
                
                if (heavyDays.Count >= 2) // At least 2 heavy days
                {
                    var totalPlays = heavyDays.Sum(kvp => kvp.Value);
                    var startDate = heavyDays.First().Key.Date;
                    var endDate = heavyDays.Last().Key.Date;
                    var intensity = (double)totalPlays / heavyDays.Count;
                    
                    var obsession = new ObsessionPeriod
                    {
                        Artist = albumGroup.Key.Artist,
                        Album = albumGroup.Key.Album,
                        StartDate = startDate,
                        EndDate = endDate,
                        PlayCount = totalPlays,
                        IntensityScore = intensity,
                        ObsessionType = "Album"
                    };
                    obsession.Description = obsession.GenerateDescription();
                    obsessions.Add(obsession);
                }
            }
            
            return obsessions;
        }

        /// <summary>
        /// Detects track obsessions based on single tracks played repeatedly in a day
        /// </summary>
        private List<ObsessionPeriod> DetectTrackObsessions(List<TrackPlay> plays)
        {
            var obsessions = new List<ObsessionPeriod>();
            
            // Group by individual track per day
            var dailyTrackStats = plays.GroupBy(p => new { Date = p.PlayedAt.Date, p.Artist, p.Title })
                                      .ToDictionary(g => g.Key, g => g.Count());
            
            // Look for tracks played 5+ times in a single day
            var trackObsessions = dailyTrackStats.Where(kvp => kvp.Value >= 5)
                                                 .GroupBy(kvp => new { kvp.Key.Artist, kvp.Key.Title });
            
            foreach (var trackGroup in trackObsessions)
            {
                var obsessionDays = trackGroup.OrderBy(kvp => kvp.Key.Date).ToList();
                var totalPlays = obsessionDays.Sum(kvp => kvp.Value);
                var startDate = obsessionDays.First().Key.Date;
                var endDate = obsessionDays.Last().Key.Date;
                var intensity = (double)totalPlays / obsessionDays.Count;
                
                var obsession = new ObsessionPeriod
                {
                    Artist = trackGroup.Key.Artist,
                    Track = trackGroup.Key.Title,
                    StartDate = startDate,
                    EndDate = endDate,
                    PlayCount = totalPlays,
                    IntensityScore = intensity,
                    ObsessionType = "Track"
                };
                obsession.Description = obsession.GenerateDescription();
                obsessions.Add(obsession);
            }
            
            return obsessions;
        }

        /// <summary>
        /// Analyzes how the user listens to albums - sequential vs. shuffled, full vs. partial
        /// </summary>
        private void CalculateAlbumListeningBehavior(List<TrackPlay> plays)
        {
            var albumSessions = DetectAlbumSessions(plays);
            
            AlbumBehavior = new AlbumListeningBehavior
            {
                TotalAlbumSessions = albumSessions.Count,
                NotableAlbumSessions = albumSessions.OrderByDescending(s => s.TracksPlayed).Take(10).ToList()
            };

            if (albumSessions.Count > 0)
            {
                // Calculate full album percentage
                var fullAlbumSessions = albumSessions.Where(s => s.IsComplete).Count();
                AlbumBehavior.FullAlbumPercentage = Math.Round((double)fullAlbumSessions / albumSessions.Count * 100, 1);

                // Calculate sequential listening percentage  
                var sequentialSessions = albumSessions.Where(s => s.IsSequential).Count();
                AlbumBehavior.SequentialListeningPercentage = Math.Round((double)sequentialSessions / albumSessions.Count * 100, 1);

                // Calculate average tracks per session
                AlbumBehavior.AverageTracksPerAlbumSession = Math.Round(albumSessions.Average(s => s.TracksPlayed), 1);
            }

            AlbumBehavior.DetermineListenerPersonality();
        }

        /// <summary>
        /// Detects album listening sessions - periods where tracks from the same album were played close together
        /// </summary>
        private List<AlbumSession> DetectAlbumSessions(List<TrackPlay> plays)
        {
            var sessions = new List<AlbumSession>();
            var albumPlays = plays.Where(p => !string.IsNullOrEmpty(p.Album))
                                 .OrderBy(p => p.PlayedAt)
                                 .ToList();

            var currentSession = new AlbumSession();
            var sessionThresholdMinutes = 30; // Max gap between tracks in same session

            foreach (var play in albumPlays)
            {
                var albumKey = $"{play.Artist} - {play.Album}";
                
                // Check if this continues the current session
                if (currentSession.Album == albumKey && 
                    (play.PlayedAt - currentSession.EndTime).TotalMinutes <= sessionThresholdMinutes)
                {
                    // Continue current session
                    currentSession.EndTime = play.PlayedAt;
                    currentSession.TracksPlayed++;
                }
                else
                {
                    // Start new session (save previous if it had multiple tracks)
                    if (currentSession.TracksPlayed > 1)
                    {
                        sessions.Add(currentSession);
                    }
                    
                    currentSession = new AlbumSession
                    {
                        Album = play.Album,
                        Artist = play.Artist,
                        StartTime = play.PlayedAt,
                        EndTime = play.PlayedAt,
                        TracksPlayed = 1
                    };
                }
            }

            // Don't forget the last session
            if (currentSession.TracksPlayed > 1)
            {
                sessions.Add(currentSession);
            }

            // Analyze each session for completeness and sequentiality
            foreach (var session in sessions)
            {
                // Get all unique tracks from this album in our data
                var albumTracks = plays.Where(p => p.Artist == session.Artist && p.Album == session.Album)
                                      .Select(p => p.Title)
                                      .Distinct()
                                      .Count();
                
                session.TotalTracksInAlbum = albumTracks;
                session.IsComplete = session.TracksPlayed >= albumTracks * 0.8; // 80% completion counts as "full"
                
                // Simple heuristic for sequential listening
                // (In reality, we'd need track numbers, but this is a reasonable approximation)
                session.IsSequential = session.TracksPlayed >= 3; // Assume 3+ tracks is likely sequential
            }

            return sessions;
        }
    }
}