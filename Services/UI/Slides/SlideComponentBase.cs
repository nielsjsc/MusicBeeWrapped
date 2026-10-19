using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Abstract base class for all slide components in the MusicBee Wrapped interface.
    /// Provides shared data extraction and clean HTML/JS helpers.
    /// </summary>
    public abstract class SlideComponentBase
    {
        protected static readonly ArtworkService Artwork = new ArtworkService();

        public abstract string SlideId { get; }
        public abstract string SlideTitle { get; }
        public abstract int SlideOrder { get; }
        public virtual bool RequiresChartRendering => false;
        public virtual int AnimationDelay => 0;
        public virtual string SlideClasses => "slide";
        public virtual string AccentTheme => "parchment";

        public abstract string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year);

        public virtual string GenerateCSS()
        {
            return string.Empty;
        }

        public virtual string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return string.Empty;
        }

        public virtual bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return stats != null && playHistory != null;
        }

        public virtual string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            return null;
        }

        public virtual string GetInsightTitle(WrappedStatistics stats)
        {
            return null;
        }

        /// <summary>
        /// Gets actual listened seconds for a play record, with fallback to track duration or 3.5 min.
        /// </summary>
        protected int GetActualSeconds(TrackPlay play)
        {
            if (play == null) return 0;
            if (play.PlayDuration > 0) return play.PlayDuration;
            if (play.Duration > 0) return play.Duration;
            return 210;
        }

        /// <summary>
        /// Gets all plays for the specified year, falling back to the most recent year in PlayHistory if needed.
        /// </summary>
        protected List<TrackPlay> GetYearPlays(PlayHistory playHistory, int year)
        {
            if (playHistory?.Plays == null || !playHistory.Plays.Any())
                return new List<TrackPlay>();

            var distinctYears = playHistory.Plays.Select(p => p.PlayedAt.Year).Distinct().ToList();
            if (distinctYears.Count == 1)
            {
                return playHistory.Plays.ToList();
            }

            var spanDays = (playHistory.Plays.Max(p => p.PlayedAt) - playHistory.Plays.Min(p => p.PlayedAt)).TotalDays;
            if (spanDays <= 45)
            {
                return playHistory.Plays.ToList();
            }

            var plays = playHistory.Plays.Where(p => p.PlayedAt.Year == year).ToList();
            if (!plays.Any())
            {
                var latestYear = distinctYears.Max();
                plays = playHistory.Plays.Where(p => p.PlayedAt.Year == latestYear).ToList();
            }
            return plays;
        }

        protected string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            return input.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#39;");
        }

        protected string EscapeJs(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("\\", "\\\\")
                        .Replace("'", "\\'")
                        .Replace("\"", "\\\"")
                        .Replace("\n", " ")
                        .Replace("\r", " ");
        }

        protected string FormatNumber(int number)
        {
            if (number >= 1000000)
                return $"{number / 1000000.0:F1}M";
            if (number >= 10000)
                return $"{number / 1000.0:F1}k";
            return number.ToString("N0");
        }

        protected string FormatDuration(double seconds)
        {
            var totalMins = Math.Round(seconds / 60.0);
            if (totalMins >= 60)
            {
                var hours = seconds / 3600.0;
                return $"{hours:F1} hrs";
            }
            return $"{totalMins:N0} mins";
        }

        protected string RenderCoverArt(string dataUri, string primaryText, string secondaryText = null, string wrapperClass = "sleeve-frame")
        {
            var monogram = EscapeHtml(ArtworkService.GetMonogram(primaryText, secondaryText));
            var imgTag = !string.IsNullOrEmpty(dataUri)
                ? $"<img src=\"{dataUri}\" alt=\"\" loading=\"lazy\" onerror=\"this.style.display='none'\">"
                : "";
            return $"<div class=\"{wrapperClass}\"><div class=\"sleeve-fallback\"><span>{monogram}</span></div>{imgTag}</div>";
        }

        protected string WrapInSlideContainer(string content, string accentOverride = null)
        {
            var accent = !string.IsNullOrEmpty(accentOverride) ? accentOverride : AccentTheme;
            return $@"
            <section class='{SlideClasses}' data-slide-id='{SlideId}' data-slide-title='{EscapeHtml(SlideTitle)}' data-accent='{accent}'>
                <div class='slide-stage'>
{content}
                </div>
            </section>";
        }
    }
}
