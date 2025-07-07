using System;
using System.Collections.Generic;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Abstract base class for all slide components in the MusicBee Wrapped interface
    /// Provides common functionality for HTML/CSS/JS generation and data binding
    /// </summary>
    public abstract class SlideComponentBase
    {
        /// <summary>
        /// Unique identifier for this slide type
        /// </summary>
        public abstract string SlideId { get; }

        /// <summary>
        /// Display name for this slide (used in navigation)
        /// </summary>
        public abstract string SlideTitle { get; }

        /// <summary>
        /// Order in which this slide appears (lower numbers appear first)
        /// </summary>
        public abstract int SlideOrder { get; }

        /// <summary>
        /// Indicates if this slide requires chart rendering
        /// </summary>
        public virtual bool RequiresChartRendering => false;

        /// <summary>
        /// Animation delay for slide entrance (in milliseconds)
        /// </summary>
        public virtual int AnimationDelay => 0;

        /// <summary>
        /// CSS classes to apply to the slide container
        /// </summary>
        public virtual string SlideClasses => "slide";

        /// <summary>
        /// Generates the HTML content for this slide
        /// </summary>
        /// <param name="stats">Wrapped statistics data</param>
        /// <param name="playHistory">Play history for additional context</param>
        /// <param name="year">Target year</param>
        /// <returns>HTML content for the slide</returns>
        public abstract string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year);

        /// <summary>
        /// Generates slide-specific CSS (optional)
        /// </summary>
        /// <returns>CSS specific to this slide type</returns>
        public virtual string GenerateCSS()
        {
            return string.Empty;
        }

        /// <summary>
        /// Generates slide-specific JavaScript (optional)
        /// </summary>
        /// <param name="stats">Wrapped statistics for data binding</param>
        /// <param name="year">Target year</param>
        /// <returns>JavaScript specific to this slide type</returns>
        public virtual string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return string.Empty;
        }

        /// <summary>
        /// Validates that required data is available for this slide
        /// </summary>
        /// <param name="stats">Statistics to validate</param>
        /// <param name="playHistory">Play history to validate</param>
        /// <returns>True if slide can be rendered with available data</returns>
        public virtual bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return stats != null && playHistory != null;
        }

        /// <summary>
        /// Gets the insight text for this slide (main narrative content)
        /// </summary>
        /// <param name="stats">Statistics data</param>
        /// <param name="playHistory">Play history data</param>
        /// <param name="year">Target year</param>
        /// <returns>Insight text or null if no insight</returns>
        public virtual string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            return null;
        }

        /// <summary>
        /// Gets the insight title for this slide
        /// </summary>
        /// <param name="stats">Statistics data</param>
        /// <returns>Insight title or null if no insight</returns>
        public virtual string GetInsightTitle(WrappedStatistics stats)
        {
            return null;
        }

        /// <summary>
        /// Creates a standard insight box with title and content
        /// </summary>
        /// <param name="title">Insight title</param>
        /// <param name="content">Insight content</param>
        /// <param name="cssClass">Additional CSS class for styling</param>
        /// <returns>HTML for insight box</returns>
        protected string CreateInsightBox(string title, string content, string cssClass = "")
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;

            var additionalClass = !string.IsNullOrEmpty(cssClass) ? $" {cssClass}" : "";
            
            return $@"
                <div class='insight-box{additionalClass}'>
                    {(!string.IsNullOrEmpty(title) ? $"<h3>{EscapeHtml(title)}</h3>" : "")}
                    <p class='insight-text'>{content}</p>
                </div>";
        }

        /// <summary>
        /// Creates a stats grid container with multiple stat cards
        /// </summary>
        /// <param name="stats">Dictionary of label -> value pairs</param>
        /// <returns>HTML for stats grid</returns>
        protected string CreateStatsGrid(Dictionary<string, string> stats)
        {
            if (stats == null || stats.Count == 0) return string.Empty;

            var html = new StringBuilder();
            html.AppendLine("                <div class='stats-grid'>");

            foreach (var stat in stats)
            {
                html.AppendLine("                    <div class='stat-card'>");
                html.AppendLine($"                        <div class='stat-number'>{EscapeHtml(stat.Value)}</div>");
                html.AppendLine($"                        <div class='stat-label'>{EscapeHtml(stat.Key)}</div>");
                html.AppendLine("                    </div>");
            }

            html.AppendLine("                </div>");
            return html.ToString();
        }

        /// <summary>
        /// Creates a top list (rankings) with rank, name, and count
        /// </summary>
        /// <param name="items">List of items with name and count</param>
        /// <param name="formatCount">Function to format the count display</param>
        /// <returns>HTML for top list</returns>
        protected string CreateTopList<T>(IEnumerable<T> items, Func<T, string> getName, Func<T, int> getCount, Func<int, string> formatCount = null)
        {
            if (items == null) return string.Empty;

            formatCount = formatCount ?? (count => $"{count} plays");
            var html = new StringBuilder();

            int rank = 1;
            foreach (var item in items)
            {
                var name = getName(item);
                var count = getCount(item);
                
                html.AppendLine($"                <div class='top-item' data-delay='{(rank - 1) * 100}'>");
                html.AppendLine($"                    <div class='top-rank'>#{rank}</div>");
                html.AppendLine("                    <div class='top-content'>");
                html.AppendLine($"                        <div class='top-name'>{EscapeHtml(name)}</div>");
                html.AppendLine("                    </div>");
                html.AppendLine($"                    <div class='top-count'>{formatCount(count)}</div>");
                html.AppendLine("                </div>");
                rank++;
            }

            return html.ToString();
        }

        /// <summary>
        /// Creates a chart container placeholder
        /// </summary>
        /// <param name="chartId">Unique ID for the chart canvas</param>
        /// <param name="height">Height of the chart container</param>
        /// <returns>HTML for chart container</returns>
        protected string CreateChartContainer(string chartId, int height = 400)
        {
            return $@"
                <div class='chart-container'>
                    <canvas id='{chartId}' width='800' height='{height}'></canvas>
                </div>";
        }

        /// <summary>
        /// Creates a slide header with title and optional subtitle
        /// </summary>
        /// <param name="title">Main title</param>
        /// <param name="subtitle">Optional subtitle</param>
        /// <param name="titleClass">CSS class for title</param>
        /// <returns>HTML for slide header</returns>
        protected string CreateSlideHeader(string title, string subtitle = null, string titleClass = "gradient-text")
        {
            var html = new StringBuilder();
            
            if (title.Length > 30)
            {
                html.AppendLine($"                <h2 class='{titleClass}'>{EscapeHtml(title)}</h2>");
            }
            else
            {
                html.AppendLine($"                <h1 class='{titleClass}'>{EscapeHtml(title)}</h1>");
            }

            if (!string.IsNullOrEmpty(subtitle))
            {
                html.AppendLine($"                <h3>{EscapeHtml(subtitle)}</h3>");
            }

            return html.ToString();
        }

        /// <summary>
        /// Escapes HTML characters for safe output
        /// </summary>
        /// <param name="input">Raw text</param>
        /// <returns>HTML-escaped text</returns>
        protected string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            return input.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#39;");
        }

        /// <summary>
        /// Formats a number for display (e.g., 1500 -> 1.5k)
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <returns>Formatted number string</returns>
        protected string FormatNumber(int number)
        {
            if (number >= 1000000)
            {
                return $"{number / 1000000.0:F1}M";
            }
            else if (number >= 1000)
            {
                return $"{number / 1000.0:F1}k";
            }
            return number.ToString();
        }

        /// <summary>
        /// Formats duration in seconds to readable format
        /// </summary>
        /// <param name="seconds">Duration in seconds</param>
        /// <returns>Formatted duration string</returns>
        protected string FormatDuration(double seconds)
        {
            var hours = Math.Floor(seconds / 3600);
            var minutes = Math.Floor((seconds % 3600) / 60);

            if (hours > 0)
            {
                return $"{hours}h {minutes}m";
            }
            return $"{minutes}m";
        }

        /// <summary>
        /// Creates animation CSS for slide entrance effects
        /// </summary>
        /// <param name="delay">Delay in milliseconds</param>
        /// <param name="animationType">Type of animation (fade-in-up, slide-up, etc.)</param>
        /// <returns>CSS class string for animation</returns>
        protected string GetAnimationClass(int delay = 0, string animationType = "fade-in-up")
        {
            var delayStyle = delay > 0 ? $" style='animation-delay: {delay}ms;'" : "";
            return $"class='{animationType}'{delayStyle}";
        }

        /// <summary>
        /// Wraps content in the standard slide container
        /// </summary>
        /// <param name="content">Inner HTML content</param>
        /// <returns>Complete slide HTML</returns>
        protected string WrapInSlideContainer(string content)
        {
            return $@"
            <div class='{SlideClasses}' data-slide-id='{SlideId}'>
{content}
            </div>";
        }
    }
}
