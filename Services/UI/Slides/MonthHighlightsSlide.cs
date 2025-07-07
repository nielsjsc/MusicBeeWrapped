using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide showing month highlights with genre/artist changes
    /// </summary>
    public class MonthHighlightsSlide : SlideComponentBase
    {
        public override string SlideId => "month-highlights";
        public override string SlideTitle => "Month Highlights";
        public override int SlideOrder => 4;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var monthHighlights = GetMonthHighlights(playHistory, year);

            var content = $@"
                <div class='month-highlights-slide-container'>
                    <div class='slide-header'>
                        <h2>📅 Your Musical Journey Through {year}</h2>
                        <p class='slide-subtitle'>How your taste evolved month by month</p>
                    </div>
                    
                    <div class='month-highlights-main-content'>
                        {string.Join("", monthHighlights.Select(highlight => $@"
                            <div class='month-highlight'>
                                <div class='month-header'>
                                    <div class='month-name'>{highlight.MonthName}</div>
                                    <div class='month-icon'>{GetMonthIcon(highlight.MonthNumber)}</div>
                                </div>
                                <div class='month-content'>
                                    <div class='highlight-title'>{highlight.Title}</div>
                                    <div class='highlight-description'>{highlight.Description}</div>
                                    <div class='highlight-artist'>{EscapeHtml(highlight.TopArtist)}</div>
                                </div>
                            </div>
                        "))}
                    </div>
                    
                    <div class='month-insights'>
                        <p>{GetMonthInsight(monthHighlights, year)}</p>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var monthHighlights = GetMonthHighlights(playHistory, year);
            return GetMonthInsight(monthHighlights, year);
        }

        public override string GetInsightTitle(WrappedStatistics stats)
        {
            return "Monthly Evolution";
        }

        private MonthHighlight[] GetMonthHighlights(PlayHistory playHistory, int year)
        {
            if (playHistory?.Plays == null || !playHistory.Plays.Any())
            {
                return GetDefaultHighlights(year);
            }

            var monthlyData = playHistory.Plays
                .Where(t => t.PlayedAt.Year == year)
                .GroupBy(t => t.PlayedAt.Month)
                .ToDictionary(g => g.Key, g => g.ToList());

            var highlights = new List<MonthHighlight>();

            // Pick 3 interesting months based on different criteria
            var months = monthlyData.Keys.OrderBy(m => m).ToList();
            if (months.Count >= 3)
            {
                // Early year (Jan-Apr)
                var earlyMonth = months.Where(m => m <= 4).OrderByDescending(m => monthlyData[m].Count).FirstOrDefault();
                if (earlyMonth > 0)
                {
                    highlights.Add(CreateMonthHighlight(earlyMonth, monthlyData[earlyMonth], "New Year, New Vibes"));
                }

                // Mid year (May-Aug)  
                var midMonth = months.Where(m => m >= 5 && m <= 8).OrderByDescending(m => monthlyData[m].Count).FirstOrDefault();
                if (midMonth > 0)
                {
                    highlights.Add(CreateMonthHighlight(midMonth, monthlyData[midMonth], "Summer Soundtrack"));
                }

                // Late year (Sep-Dec)
                var lateMonth = months.Where(m => m >= 9).OrderByDescending(m => monthlyData[m].Count).FirstOrDefault();
                if (lateMonth > 0)
                {
                    highlights.Add(CreateMonthHighlight(lateMonth, monthlyData[lateMonth], "Year-End Vibes"));
                }
            }

            return highlights.Take(3).ToArray();
        }

        private MonthHighlight[] GetDefaultHighlights(int year)
        {
            return new[]
            {
                new MonthHighlight { MonthNumber = 3, MonthName = "March", Title = "Spring Awakening", Description = "Fresh sounds for a fresh season", TopArtist = "Various Artists" },
                new MonthHighlight { MonthNumber = 7, MonthName = "July", Title = "Summer Anthems", Description = "The perfect soundtrack for sunny days", TopArtist = "Various Artists" },
                new MonthHighlight { MonthNumber = 11, MonthName = "November", Title = "Autumn Reflections", Description = "Cozy vibes for shorter days", TopArtist = "Various Artists" }
            };
        }

        private MonthHighlight CreateMonthHighlight(int monthNumber, System.Collections.Generic.List<TrackPlay> tracks, string theme)
        {
            var topArtist = tracks
                .GroupBy(t => GetArtistFromTrack(t.Title + " - " + t.Artist))
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "Various Artists";

            var monthName = new DateTime(2000, monthNumber, 1).ToString("MMMM");
            
            return new MonthHighlight
            {
                MonthNumber = monthNumber,
                MonthName = monthName,
                Title = theme,
                Description = GetMonthDescription(monthNumber, tracks.Count, topArtist),
                TopArtist = topArtist
            };
        }

        private string GetArtistFromTrack(string trackName)
        {
            var parts = trackName.Split(new[] { " - " }, 2, StringSplitOptions.None);
            return parts.Length == 2 ? parts[1].Trim() : "Unknown Artist";
        }

        private string GetMonthDescription(int month, int trackCount, string topArtist)
        {
            var monthDescriptions = new Dictionary<int, string[]>
            {
                { 1, new[] { "Started the year strong", "New year, new playlist", "Fresh musical beginnings" } },
                { 2, new[] { "Love was in the air", "Romantic vibes dominated", "Heartfelt melodies" } },
                { 3, new[] { "Spring energy kicked in", "Fresh sounds emerged", "Musical awakening" } },
                { 4, new[] { "April showers, musical flowers", "Blooming playlist", "Fresh discoveries" } },
                { 5, new[] { "May melodies flourished", "Peak listening season", "Musical momentum" } },
                { 6, new[] { "Summer vibes started", "Upbeat energy", "Sunny soundtracks" } },
                { 7, new[] { "Peak summer anthems", "Hot tracks for hot days", "Summer soundtrack perfected" } },
                { 8, new[] { "Late summer classics", "Vacation vibes", "End of summer nostalgia" } },
                { 9, new[] { "Back to school beats", "Autumn transition", "New season, new sounds" } },
                { 10, new[] { "Halloween harmonies", "Spooky season sounds", "October atmosphere" } },
                { 11, new[] { "Thanksgiving tunes", "Gratitude for great music", "Cozy autumn vibes" } },
                { 12, new[] { "Holiday harmonies", "Year-end reflections", "Festive finale" } }
            };

            var descriptions = monthDescriptions.ContainsKey(month) ? monthDescriptions[month] : new[] { "Great music month" };
            var randomDesc = descriptions[new Random().Next(descriptions.Length)];
            
            return $"{randomDesc} • {trackCount} tracks";
        }

        private string GetMonthIcon(int month)
        {
            var icons = new[] { "❄️", "💕", "🌸", "🌧️", "🌺", "☀️", "🏖️", "🌻", "🍂", "🎃", "🦃", "🎄" };
            return icons[month - 1];
        }

        private string GetMonthInsight(MonthHighlight[] highlights, int year)
        {
            if (highlights.Length == 0)
                return $"Your musical journey through {year} was unique and personal. Every month brought its own soundtrack! 🎵";

            var monthNames = string.Join(", ", highlights.Take(2).Select(h => h.MonthName));
            if (highlights.Length > 2)
                monthNames += $", and {highlights.Last().MonthName}";

            return $"From {monthNames}, your musical taste evolved beautifully throughout {year}. Each season brought its own perfect soundtrack! 🎶";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory);
        }

        private class MonthHighlight
        {
            public int MonthNumber { get; set; }
            public string MonthName { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string TopArtist { get; set; }
        }
    }
}
