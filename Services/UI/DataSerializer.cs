using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Handles serialization of wrapped statistics data to JSON format for web UI consumption.
    /// Provides type-safe serialization with proper escaping and accurate duration calculations.
    /// </summary>
    public class DataSerializer
    {
        private static int GetActualSeconds(TrackPlay p)
        {
            if (p == null) return 0;
            if (p.PlayDuration > 0) return p.PlayDuration;
            if (p.Duration > 0) return p.Duration;
            return 210;
        }

        public string SerializeTopList(IEnumerable<KeyValuePair<string, int>> items)
        {
            if (items == null) return "[]";

            var jsonItems = items.Select(item =>
                $"{{\"name\":\"{EscapeJsonString(item.Key)}\",\"count\":{item.Value}}}");
            return $"[{string.Join(",", jsonItems)}]";
        }

        public string SerializeTopTracks(IEnumerable<KeyValuePair<string, int>> tracks)
        {
            if (tracks == null) return "[]";

            var jsonItems = tracks.Select(track =>
            {
                var parts = track.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    return $"{{\"title\":\"{EscapeJsonString(parts[1])}\",\"artist\":\"{EscapeJsonString(parts[0])}\",\"count\":{track.Value}}}";
                }
                return $"{{\"title\":\"{EscapeJsonString(track.Key)}\",\"artist\":\"Unknown\",\"count\":{track.Value}}}";
            });
            return $"[{string.Join(",", jsonItems)}]";
        }

        public string SerializeTopAlbumsWithTracks(IEnumerable<KeyValuePair<string, int>> albums, PlayHistory playHistory, int year)
        {
            if (albums == null) return "[]";

            var yearPlays = playHistory != null ? playHistory.GetPlaysByYear(year).ToList() : new List<TrackPlay>();

            var jsonItems = albums.Select(album =>
            {
                var parts = album.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                var albumName = parts.Length >= 1 ? parts[0] : album.Key;
                var artistName = parts.Length >= 2 ? parts[1] : "Unknown";

                var albumTracks = yearPlays
                    .Where(p => string.Equals(p.Album, albumName, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(p.Artist, artistName, StringComparison.OrdinalIgnoreCase))
                    .GroupBy(p => p.Title ?? "Unknown")
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

        public string SerializeDailyMinutes(Dictionary<string, double> dailyMinutes)
        {
            if (dailyMinutes == null) return "{}";

            var jsonItems = dailyMinutes.OrderBy(x => x.Key).Select(item =>
                $"\"{item.Key}\":{item.Value.ToString("F1", CultureInfo.InvariantCulture)}");
            return $"{{{string.Join(",", jsonItems)}}}";
        }

        public string SerializeDailyPlays(Dictionary<string, int> dailyPlays)
        {
            if (dailyPlays == null) return "{}";

            var jsonItems = dailyPlays.OrderBy(x => x.Key).Select(item =>
                $"\"{item.Key}\":{item.Value}");
            return $"{{{string.Join(",", jsonItems)}}}";
        }

        public string SerializeMonthlyHours(Dictionary<string, double> monthlyHours)
        {
            if (monthlyHours == null) return "{}";

            var jsonItems = monthlyHours.OrderBy(x => x.Key).Select(item =>
                $"\"{item.Key}\":{item.Value.ToString("F1", CultureInfo.InvariantCulture)}");
            return $"{{{string.Join(",", jsonItems)}}}";
        }

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

        private string SerializeDailyDetails(List<TrackPlay> yearPlays)
        {
            if (yearPlays == null || !yearPlays.Any()) return "{}";

            var grouped = yearPlays
                .GroupBy(p => p.PlayedAt.ToString("yyyy-MM-dd"))
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var plays = g.Count();
                    var mins = Math.Round(g.Sum(p => GetActualSeconds(p)) / 60.0, 1);
                    var topArtist = g.Where(p => !string.IsNullOrEmpty(p.Artist))
                                     .GroupBy(p => p.Artist)
                                     .OrderByDescending(ag => ag.Count())
                                     .Select(ag => ag.Key)
                                     .FirstOrDefault() ?? "Unknown";
                    var topTrackGroup = g.Where(p => !string.IsNullOrEmpty(p.Title))
                                         .GroupBy(p => p.Title)
                                         .OrderByDescending(tg => tg.Count())
                                         .FirstOrDefault();
                    var topTrack = topTrackGroup != null ? topTrackGroup.Key : "";
                    return $"\"{g.Key}\":{{\"plays\":{plays},\"minutes\":{mins.ToString("F1", CultureInfo.InvariantCulture)},\"topArtist\":\"{EscapeJsonString(topArtist)}\",\"topTrack\":\"{EscapeJsonString(topTrack)}\"}}";
                });

            return $"{{{string.Join(",", grouped)}}}";
        }

        private string SerializeMonthlySummaries(List<TrackPlay> yearPlays, int year)
        {
            var items = new List<string>();
            for (int m = 1; m <= 12; m++)
            {
                var monthPlays = yearPlays != null
                    ? yearPlays.Where(p => p.PlayedAt.Month == m).ToList()
                    : new List<TrackPlay>();
                int count = monthPlays.Count;
                double hours = Math.Round(monthPlays.Sum(p => GetActualSeconds(p)) / 3600.0, 1);
                string topArtist = monthPlays
                    .Where(p => !string.IsNullOrEmpty(p.Artist))
                    .GroupBy(p => p.Artist)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "—";
                string monthName = new DateTime(year, m, 1).ToString("MMM", CultureInfo.InvariantCulture);

                items.Add($"{{\"month\":{m},\"name\":\"{monthName}\",\"plays\":{count},\"hours\":{hours.ToString("F1", CultureInfo.InvariantCulture)},\"topArtist\":\"{EscapeJsonString(topArtist)}\"}}");
            }
            return $"[{string.Join(",", items)}]";
        }

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

        public string CreateWrappedDataObject(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            List<TrackPlay> yearPlays;
            if (playHistory == null || playHistory.Plays == null)
            {
                yearPlays = new List<TrackPlay>();
            }
            else if (stats != null && stats.IsCustomDateRange && stats.RangeStartDate.HasValue && stats.RangeEndDate.HasValue)
            {
                yearPlays = playHistory.Plays
                    .Where(p => p.PlayedAt.Date >= stats.RangeStartDate.Value.Date && p.PlayedAt.Date <= stats.RangeEndDate.Value.Date)
                    .OrderBy(p => p.PlayedAt)
                    .ToList();
            }
            else
            {
                yearPlays = playHistory.GetPlaysByYear(year).OrderBy(p => p.PlayedAt).ToList();
            }

            var firstPlay = yearPlays.FirstOrDefault();
            var lastPlay = yearPlays.LastOrDefault();

            string rangeStartStr = stats != null && stats.IsCustomDateRange && stats.RangeStartDate.HasValue
                ? $"\"{stats.RangeStartDate.Value:yyyy-MM-dd}\""
                : "null";
            string rangeEndStr = stats != null && stats.IsCustomDateRange && stats.RangeEndDate.HasValue
                ? $"\"{stats.RangeEndDate.Value:yyyy-MM-dd}\""
                : "null";

            var dataObject = new StringBuilder();
            dataObject.AppendLine("        window.WRAPPED_DATA = {");
            dataObject.AppendLine($"            year: {year},");
            dataObject.AppendLine($"            rangeStart: {rangeStartStr},");
            dataObject.AppendLine($"            rangeEnd: {rangeEndStr},");
            dataObject.AppendLine($"            totalTracks: {stats.TotalTracks},");
            dataObject.AppendLine($"            totalHours: {stats.TotalHours.ToString("F1", CultureInfo.InvariantCulture)},");
            dataObject.AppendLine($"            totalArtists: {stats.TotalUniqueArtists},");
            dataObject.AppendLine($"            totalAlbums: {stats.TotalUniqueAlbums},");
            dataObject.AppendLine($"            topArtists: {SerializeTopList(stats.TopArtists.Take(10))},");
            dataObject.AppendLine($"            topTracks: {SerializeTopTracks(stats.TopTracks.Take(10))},");
            dataObject.AppendLine($"            topAlbums: {SerializeTopAlbumsWithTracks(stats.TopAlbums.Take(8), playHistory, year)},");
            dataObject.AppendLine($"            longestStreak: {stats.LongestListeningStreak},");
            dataObject.AppendLine($"            mostActiveHour: {stats.MostActiveHour},");
            dataObject.AppendLine($"            dailyMinutes: {SerializeDailyMinutes(stats.DailyListeningMinutes)},");
            dataObject.AppendLine($"            dailyPlays: {SerializeDailyPlays(stats.DailyPlayCounts)},");
            dataObject.AppendLine($"            dailyDetails: {SerializeDailyDetails(yearPlays)},");
            dataObject.AppendLine($"            monthlySummaries: {SerializeMonthlySummaries(yearPlays, year)},");
            dataObject.AppendLine($"            monthlyHours: {SerializeMonthlyHours(stats.MonthlyListeningHours)},");
            dataObject.AppendLine($"            weekendRatio: {stats.WeekendVsWeekdayRatio.ToString("F2", CultureInfo.InvariantCulture)},");
            dataObject.AppendLine($"            firstPlay: {SerializePlayData(firstPlay)},");
            dataObject.AppendLine($"            lastPlay: {SerializePlayData(lastPlay)}");
            dataObject.AppendLine("        };");
            dataObject.AppendLine($"        window.top50Tracks = {SerializeTop50ForExport(yearPlays)};");

            return dataObject.ToString();
        }

        private string SerializeTop50ForExport(List<TrackPlay> yearPlays)
        {
            if (yearPlays == null || !yearPlays.Any()) return "[]";

            var items = yearPlays
                .Where(p => !string.IsNullOrEmpty(p.Title))
                .GroupBy(p => $"{(p.Artist ?? "Unknown").Trim()}|||{p.Title.Trim()}", StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var first = g.First();
                    var fileUrl = g.Select(x => x.FileUrl).FirstOrDefault(u => !string.IsNullOrEmpty(u)) ?? "";
                    var dur = first.Duration > 0 ? first.Duration : GetActualSeconds(first);
                    return new
                    {
                        Title = first.Title ?? "Unknown",
                        Artist = first.Artist ?? "Unknown",
                        FilePath = fileUrl,
                        Duration = dur,
                        Count = g.Count(),
                        TotalSeconds = g.Sum(x => GetActualSeconds(x))
                    };
                })
                .OrderByDescending(x => x.Count)
                .ThenByDescending(x => x.TotalSeconds)
                .Take(50)
                .Select(t =>
                    $"{{\"title\":\"{EscapeJsonString(t.Title)}\",\"artist\":\"{EscapeJsonString(t.Artist)}\",\"filePath\":\"{EscapeJsonString(t.FilePath)}\",\"duration\":{t.Duration},\"count\":{t.Count}}}");

            return $"[{string.Join(",", items)}]";
        }
    }
}
