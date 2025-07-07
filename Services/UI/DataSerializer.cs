using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Handles serialization of wrapped statistics data to JSON format for web UI consumption
    /// Provides type-safe serialization with proper escaping and formatting
    /// </summary>
    public class DataSerializer
    {
        /// <summary>
        /// Serializes a collection of key-value pairs to JSON array format
        /// </summary>
        /// <param name="items">Collection of items to serialize</param>
        /// <returns>JSON array string</returns>
        public string SerializeTopList(IEnumerable<KeyValuePair<string, int>> items)
        {
            if (items == null) return "[]";
            
            var jsonItems = items.Select(item => 
                $"{{\"name\":\"{EscapeJsonString(item.Key)}\",\"count\":{item.Value}}}");
            return $"[{string.Join(",", jsonItems)}]";
        }

        /// <summary>
        /// Serializes top tracks with artist/title separation for enhanced display
        /// </summary>
        /// <param name="tracks">Collection of track data (format: "Artist - Title")</param>
        /// <returns>JSON array with separated artist and title fields</returns>
        public string SerializeTopTracks(IEnumerable<KeyValuePair<string, int>> tracks)
        {
            if (tracks == null) return "[]";
            
            var jsonItems = tracks.Select(track => 
            {
                // Try to split track into title and artist if format is "Artist - Title"
                var parts = track.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    return $"{{\"title\":\"{EscapeJsonString(parts[1])}\",\"artist\":\"{EscapeJsonString(parts[0])}\",\"count\":{track.Value}}}";
                }
                else
                {
                    return $"{{\"title\":\"{EscapeJsonString(track.Key)}\",\"artist\":\"Unknown\",\"count\":{track.Value}}}";
                }
            });
            return $"[{string.Join(",", jsonItems)}]";
        }

        /// <summary>
        /// Serializes album data with associated track information
        /// </summary>
        /// <param name="albums">Collection of album data</param>
        /// <param name="playHistory">Play history for track lookup</param>
        /// <param name="year">Year to filter tracks</param>
        /// <returns>JSON array with album and track information</returns>
        public string SerializeTopAlbumsWithTracks(IEnumerable<KeyValuePair<string, int>> albums, PlayHistory playHistory, int year)
        {
            if (albums == null) return "[]";
            
            var jsonItems = albums.Select(album =>
            {
                // Parse album format: "Album - Artist"
                var parts = album.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                var albumName = parts.Length >= 1 ? parts[0] : album.Key;
                var artistName = parts.Length >= 2 ? parts[1] : "Unknown";

                // Find top tracks from this album
                var albumTracks = playHistory.GetPlaysByYear(year)
                    .Where(p => p.Album == albumName && p.Artist == artistName)
                    .GroupBy(p => p.Title)
                    .Select(g => new { Title = g.Key, Count = g.Count() })
                    .OrderByDescending(t => t.Count)
                    .Take(3)
                    .Select(t => $"{{\"title\":\"{EscapeJsonString(t.Title)}\",\"count\":{t.Count}}}")
                    .ToList();

                var tracksJson = $"[{string.Join(",", albumTracks)}]";

                return $"{{\"album\":\"{EscapeJsonString(albumName)}\",\"artist\":\"{EscapeJsonString(artistName)}\",\"count\":{album.Value},\"topTracks\":{tracksJson}}}";
            });

            return $"[{string.Join(",", jsonItems)}]";
        }

        /// <summary>
        /// Serializes daily listening minutes data for chart visualization
        /// </summary>
        /// <param name="dailyMinutes">Dictionary of date -> minutes</param>
        /// <returns>JSON object with date keys and minute values</returns>
        public string SerializeDailyMinutes(Dictionary<string, double> dailyMinutes)
        {
            if (dailyMinutes == null) return "{}";
            
            var jsonItems = dailyMinutes.OrderBy(x => x.Key).Select(item => 
                $"\"{item.Key}\":{item.Value.ToString("F1", CultureInfo.InvariantCulture)}");
            return $"{{{string.Join(",", jsonItems)}}}";
        }

        /// <summary>
        /// Serializes daily play counts for chart visualization
        /// </summary>
        /// <param name="dailyPlays">Dictionary of date -> play count</param>
        /// <returns>JSON object with date keys and play count values</returns>
        public string SerializeDailyPlays(Dictionary<string, int> dailyPlays)
        {
            if (dailyPlays == null) return "{}";
            
            var jsonItems = dailyPlays.OrderBy(x => x.Key).Select(item => 
                $"\"{item.Key}\":{item.Value}");
            return $"{{{string.Join(",", jsonItems)}}}";
        }

        /// <summary>
        /// Serializes monthly listening hours for overview visualization
        /// </summary>
        /// <param name="monthlyHours">Dictionary of month -> hours</param>
        /// <returns>JSON object with month keys and hour values</returns>
        public string SerializeMonthlyHours(Dictionary<string, double> monthlyHours)
        {
            if (monthlyHours == null) return "{}";
            
            var jsonItems = monthlyHours.OrderBy(x => x.Key).Select(item => 
                $"\"{item.Key}\":{item.Value.ToString("F1", CultureInfo.InvariantCulture)}");
            return $"{{{string.Join(",", jsonItems)}}}";
        }

        /// <summary>
        /// Serializes individual track play data for detailed display
        /// </summary>
        /// <param name="play">Track play instance</param>
        /// <returns>JSON object with track details or null</returns>
        public string SerializePlayData(TrackPlay play)
        {
            if (play == null) return "null";
            
            return $"{{" +
                   $"\"title\":\"{EscapeJsonString(play.Title)}\"," +
                   $"\"artist\":\"{EscapeJsonString(play.Artist)}\"," +
                   $"\"album\":\"{EscapeJsonString(play.Album)}\"," +
                   $"\"playedAt\":\"{play.PlayedAt:yyyy-MM-dd}\"," +
                   $"\"month\":\"{play.PlayedAt:MMMM}\"," +
                   $"\"day\":{play.PlayedAt.Day}," +
                   $"\"year\":{play.PlayedAt.Year}" +
                   $"}}";
        }

        /// <summary>
        /// Escapes special characters in strings for safe JSON embedding
        /// </summary>
        /// <param name="input">Raw string input</param>
        /// <returns>JSON-safe escaped string</returns>
        public string EscapeJsonString(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            
            return input.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t")
                       .Replace("\b", "\\b")
                       .Replace("\f", "\\f");
        }

        /// <summary>
        /// Creates the complete data object for embedding in HTML template
        /// </summary>
        /// <param name="stats">Wrapped statistics</param>
        /// <param name="playHistory">Play history data</param>
        /// <param name="year">Target year</param>
        /// <returns>Complete JavaScript object literal string</returns>
        public string CreateWrappedDataObject(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = playHistory.GetPlaysByYear(year).OrderBy(p => p.PlayedAt).ToList();
            var firstPlay = yearPlays.FirstOrDefault();
            var lastPlay = yearPlays.LastOrDefault();

            var dataObject = new StringBuilder();
            dataObject.AppendLine("        window.WRAPPED_DATA = {");
            dataObject.AppendLine($"            year: {year},");
            dataObject.AppendLine($"            totalTracks: {stats.TotalTracks},");
            dataObject.AppendLine($"            totalHours: {stats.TotalHours.ToString("F1", CultureInfo.InvariantCulture)},");
            dataObject.AppendLine($"            totalArtists: {stats.TotalUniqueArtists},");
            dataObject.AppendLine($"            totalAlbums: {stats.TotalUniqueAlbums},");
            dataObject.AppendLine($"            topArtists: {SerializeTopList(stats.TopArtists.Take(5))},");
            dataObject.AppendLine($"            topTracks: {SerializeTopTracks(stats.TopTracks.Take(5))},");
            dataObject.AppendLine($"            topAlbums: {SerializeTopAlbumsWithTracks(stats.TopAlbums.Take(5), playHistory, year)},");
            dataObject.AppendLine($"            longestStreak: {stats.LongestListeningStreak},");
            dataObject.AppendLine($"            mostActiveHour: {stats.MostActiveHour},");
            dataObject.AppendLine($"            dailyMinutes: {SerializeDailyMinutes(stats.DailyListeningMinutes)},");
            dataObject.AppendLine($"            dailyPlays: {SerializeDailyPlays(stats.DailyPlayCounts)},");
            dataObject.AppendLine($"            monthlyHours: {SerializeMonthlyHours(stats.MonthlyListeningHours)},");
            dataObject.AppendLine($"            weekendRatio: {stats.WeekendVsWeekdayRatio.ToString("F2", CultureInfo.InvariantCulture)},");
            dataObject.AppendLine($"            firstPlay: {SerializePlayData(firstPlay)},");
            dataObject.AppendLine($"            lastPlay: {SerializePlayData(lastPlay)}");
            dataObject.AppendLine("        };");

            return dataObject.ToString();
        }
    }
}
