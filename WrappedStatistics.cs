using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}