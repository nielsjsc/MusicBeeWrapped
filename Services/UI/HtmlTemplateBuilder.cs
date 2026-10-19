using System;
using System.Collections.Generic;
using System.Text;
using MusicBeeWrapped;
using MusicBeeWrapped.Services.UI.Slides;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Builds HTML documents for the MusicBee Wrapped web interface and Year Selector.
    /// </summary>
    public class HtmlTemplateBuilder
    {
        private readonly CssStyleProvider _cssProvider;
        private readonly JavaScriptProvider _jsProvider;
        private readonly DataSerializer _dataSerializer;

        public HtmlTemplateBuilder(
            CssStyleProvider cssProvider = null,
            JavaScriptProvider jsProvider = null,
            DataSerializer dataSerializer = null)
        {
            _cssProvider = cssProvider ?? new CssStyleProvider();
            _jsProvider = jsProvider ?? new JavaScriptProvider();
            _dataSerializer = dataSerializer ?? new DataSerializer();
        }

        public string CreateYearSelectorTemplate(
            List<int> availableYears,
            YearMetadataCollection metadata,
            WrappedStatistics weeklyStats = null,
            WrappedStatistics monthlyStats = null)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("    <title>MusicBee Wrapped</title>");
            html.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">");
            html.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin>");
            html.AppendLine("    <link href=\"https://fonts.googleapis.com/css2?family=Instrument+Serif:ital@0;1&family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500;600&display=swap\" rel=\"stylesheet\">");
            html.AppendLine("    <style>");
            html.AppendLine(_cssProvider.GetYearSelectorCSS());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class=\"year-selector-container\">");
            html.AppendLine("        <div class=\"year-selector-header\">");
            html.AppendLine("            <h1>MusicBee Wrapped</h1>");
            html.AppendLine("            <p class=\"year-selector-subtitle\">Select a report</p>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"years-grid\">");

            if (weeklyStats != null && weeklyStats.TotalTracks > 0)
            {
                html.AppendLine(CreatePeriodCard("wrapped_weekly.html", "This Week", "Last 7 Days", weeklyStats));
            }

            if (monthlyStats != null && monthlyStats.TotalTracks > 0)
            {
                html.AppendLine(CreatePeriodCard("wrapped_monthly.html", "This Month", "Last 30 Days", monthlyStats));
            }

            foreach (var year in availableYears)
            {
                var yearMeta = metadata?.GetYearMetadata(year);
                if (yearMeta != null && yearMeta.TotalPlays > 0)
                {
                    html.AppendLine(CreateYearCard(year, yearMeta));
                }
            }

            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("    <script>");
            html.AppendLine(_jsProvider.GetYearSelectorJS());
            html.AppendLine("    </script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        public string CreateSlideBasedTemplate(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var slideManager = new SlideManager();
            var activeSlides = slideManager.PrepareSlides(stats, playHistory);
            return GenerateModularWrappedHTML(
                stats,
                playHistory,
                year,
                activeSlides,
                _cssProvider,
                _jsProvider,
                _dataSerializer,
                slideManager);
        }

        public string GenerateModularWrappedHTML(
            WrappedStatistics stats,
            PlayHistory playHistory,
            int year,
            List<SlideComponentBase> slides,
            CssStyleProvider cssProvider,
            JavaScriptProvider jsProvider,
            DataSerializer dataSerializer,
            SlideManager slideManager)
        {
            string periodLabel = !string.IsNullOrEmpty(stats?.PeriodLabel) ? stats.PeriodLabel : year.ToString();
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("    <meta name=\"description\" content=\"MusicBee Wrapped\">");
            html.AppendLine($"    <title>{EscapeHtml(periodLabel)} — MusicBee Wrapped</title>");
            html.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">");
            html.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin>");
            html.AppendLine("    <link href=\"https://fonts.googleapis.com/css2?family=Instrument+Serif:ital@0;1&family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500;600&display=swap\" rel=\"stylesheet\">");
            html.AppendLine("    <style>");
            html.AppendLine(cssProvider.GetMainInterfaceCSS());
            html.AppendLine(slideManager.GenerateAllSlideCSS());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body data-theme=\"parchment\">");

            // Minimal Top Margin Bar
            html.AppendLine("    <header class=\"app-topbar\">");
            html.AppendLine("        <div class=\"brand-group\">");
            html.AppendLine("            <span class=\"brand-name\">MusicBee Wrapped</span>");
            html.AppendLine("            <span class=\"brand-divider\">·</span>");
            html.AppendLine($"            <span class=\"brand-catno\">{EscapeHtml(periodLabel)}</span>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"topbar-right\">");
            html.AppendLine("            <span id=\"topbar-slide-title\" class=\"active-slide-label\">Overview</span>");
            html.AppendLine($"            <span id=\"topbar-slide-counter\" class=\"slide-counter-badge\">01 / {slides.Count:00}</span>");
            html.AppendLine("        </div>");
            html.AppendLine("    </header>");

            // Main Slide Viewport
            html.AppendLine("    <main id=\"app\">");
            foreach (var slide in slides)
            {
                try
                {
                    var slideHtml = slide.GenerateHTML(stats, playHistory, year);
                    html.AppendLine(slideHtml);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error generating HTML for slide {slide.SlideId}: {ex.Message}");
                }
            }
            html.AppendLine("    </main>");

            // Bottom Navigation Bar
            html.AppendLine("    <footer id=\"nav-controls\">");
            html.AppendLine("        <button id=\"prev-btn\" class=\"nav-btn\" disabled aria-label=\"Previous\" title=\"Previous (Left Arrow)\">←</button>");
            html.AppendLine("        <div id=\"slide-indicator\" role=\"tablist\" aria-label=\"Navigation\"></div>");
            html.AppendLine("        <button id=\"next-btn\" class=\"nav-btn\" aria-label=\"Next\" title=\"Next (Right Arrow)\">→</button>");
            html.AppendLine("    </footer>");

            // Embedded Data & Scripts
            html.AppendLine("    <script>");
            html.AppendLine(dataSerializer.CreateWrappedDataObject(stats, playHistory, year));
            html.AppendLine(slideManager.GenerateSlideNavigationData());
            html.AppendLine("    </script>");
            html.AppendLine("    <script>");
            html.AppendLine(jsProvider.GetMainInterfaceJS());
            html.AppendLine(slideManager.GenerateAllSlideJavaScript(stats, year));
            html.AppendLine("    </script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string CreatePeriodCard(string href, string title, string subtitle, WrappedStatistics stats)
        {
            var card = new StringBuilder();
            card.AppendLine($"        <a href=\"{href}\" class=\"year-card\" tabindex=\"0\" target=\"_self\" rel=\"noopener\">");
            card.AppendLine($"            <div class=\"year-title\">{EscapeHtml(title)}</div>");
            card.AppendLine("            <div class=\"year-stats\">");
            card.AppendLine("                <div class=\"year-stat\">");
            card.AppendLine($"                    <div class=\"year-stat-number\">{FormatNumber(stats.TotalTracks)}</div>");
            card.AppendLine("                    <div class=\"year-stat-label\">Tracks</div>");
            card.AppendLine("                </div>");
            card.AppendLine("                <div class=\"year-stat\">");
            card.AppendLine($"                    <div class=\"year-stat-number\">{stats.TotalHours:N1}</div>");
            card.AppendLine("                    <div class=\"year-stat-label\">Hours</div>");
            card.AppendLine("                </div>");
            card.AppendLine("            </div>");
            card.AppendLine("            <div class=\"year-highlights\">");

            var topArtist = stats.TopArtists != null && stats.TopArtists.Count > 0 ? stats.TopArtists[0].Key : null;
            if (!string.IsNullOrEmpty(topArtist))
            {
                card.AppendLine($"                <div class=\"year-highlight\">Top Artist: <strong>{EscapeHtml(topArtist)}</strong></div>");
            }

            var topGenre = stats.TopGenres != null && stats.TopGenres.Count > 0 ? stats.TopGenres[0].Key : null;
            if (!string.IsNullOrEmpty(topGenre))
            {
                card.AppendLine($"                <div class=\"year-highlight\">Top Genre: <strong>{EscapeHtml(topGenre)}</strong></div>");
            }

            card.AppendLine($"                <div class=\"year-highlight\">{EscapeHtml(subtitle)}</div>");
            card.AppendLine("            </div>");
            card.AppendLine("        </a>");

            return card.ToString();
        }

        private string CreateYearCard(int year, YearMetadata metadata)
        {
            var card = new StringBuilder();
            card.AppendLine($"        <a href=\"wrapped_{year}.html\" class=\"year-card\" data-year=\"{year}\" tabindex=\"0\" target=\"_self\" rel=\"noopener\">");
            card.AppendLine($"            <div class=\"year-title\">{year}</div>");
            card.AppendLine("            <div class=\"year-stats\">");
            card.AppendLine("                <div class=\"year-stat\">");
            card.AppendLine($"                    <div class=\"year-stat-number\">{FormatNumber(metadata.TotalPlays)}</div>");
            card.AppendLine("                    <div class=\"year-stat-label\">Tracks</div>");
            card.AppendLine("                </div>");
            card.AppendLine("                <div class=\"year-stat\">");
            card.AppendLine($"                    <div class=\"year-stat-number\">{Math.Round(metadata.TotalMinutes / 60.0, 1):N1}</div>");
            card.AppendLine("                    <div class=\"year-stat-label\">Hours</div>");
            card.AppendLine("                </div>");
            card.AppendLine("            </div>");
            card.AppendLine("            <div class=\"year-highlights\">");

            if (!string.IsNullOrEmpty(metadata.TopArtist))
            {
                card.AppendLine($"                <div class=\"year-highlight\">Top Artist: <strong>{EscapeHtml(metadata.TopArtist)}</strong></div>");
            }

            if (!string.IsNullOrEmpty(metadata.TopGenre))
            {
                card.AppendLine($"                <div class=\"year-highlight\">Top Genre: <strong>{EscapeHtml(metadata.TopGenre)}</strong></div>");
            }

            card.AppendLine($"                <div class=\"year-highlight\">Active: {metadata.FirstPlay:MMM d} – {metadata.LastPlay:MMM d}</div>");
            card.AppendLine("            </div>");
            card.AppendLine("        </a>");

            return card.ToString();
        }

        public string CreateErrorTemplate(string errorMessage, int year)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine($"    <title>Error — MusicBee Wrapped {year}</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: system-ui, sans-serif; padding: 3rem; background: #090a0f; color: #f4f4f6; text-align: center; }");
            html.AppendLine("        .error-container { max-width: 560px; margin: 0 auto; background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.1); border-radius: 16px; padding: 2rem; }");
            html.AppendLine("        h1 { color: #f43f5e; margin-bottom: 1rem; }");
            html.AppendLine("        .error-message { background: rgba(0,0,0,0.3); padding: 1rem; border-radius: 8px; margin: 1rem 0; color: #a1a1aa; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class=\"error-container\">");
            html.AppendLine("        <h1>Unable to Generate Wrapped</h1>");
            html.AppendLine($"        <div class=\"error-message\">{EscapeHtml(errorMessage)}</div>");
            html.AppendLine("        <p>Please try again once you have recorded listening history.</p>");
            html.AppendLine("    </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            return html.ToString();
        }

        private string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#39;");
        }

        private string FormatNumber(int number)
        {
            if (number >= 1000000) return $"{number / 1000000.0:F1}M";
            if (number >= 10000) return $"{number / 1000.0:F1}k";
            return number.ToString("N0");
        }
    }
}
