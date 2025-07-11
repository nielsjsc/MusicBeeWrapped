using System;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Builds HTML templates for the MusicBee Wrapped web interface
    /// Provides structured HTML generation with proper separation of concerns
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

        /// <summary>
        /// Creates the complete HTML document for the main wrapped interface
        /// Embeds all necessary CSS, JavaScript, and data for offline viewing
        /// </summary>
        /// <param name="stats">Wrapped statistics data</param>
        /// <param name="playHistory">Play history for additional context</param>
        /// <param name="year">Target year for the wrapped</param>
        /// <returns>Complete HTML document as string</returns>


        /// <summary>
        /// Creates the HTML document for the year selector interface
        /// Provides a grid-based layout for choosing which year to view
        /// </summary>
        /// <param name="availableYears">List of years with data</param>
        /// <param name="metadata">Metadata collection for year summaries</param>
        /// <returns>Complete year selector HTML document</returns>
        public string CreateYearSelectorTemplate(System.Collections.Generic.List<int> availableYears, YearMetadataCollection metadata)
        {
            var html = new StringBuilder();

            html.AppendLine(CreateDocumentHeader(DateTime.Now.Year, "Choose Your Year", true));
            html.AppendLine(CreateYearSelectorBody(availableYears, metadata));
            html.AppendLine(CreateYearSelectorScript());
            html.AppendLine(CreateDocumentFooter());

            return html.ToString();
        }

        /// <summary>
        /// Creates the complete HTML document for the main wrapped interface using the slide system
        /// This is the modern version that uses SlideManager for component-based slides
        /// </summary>
        /// <param name="stats">Wrapped statistics data</param>
        /// <param name="playHistory">Play history for additional context</param>
        /// <param name="year">Target year for the wrapped</param>
        /// <returns>Complete HTML document as string</returns>
       public string CreateSlideBasedTemplate(WrappedStatistics stats, PlayHistory playHistory, int year)
            {
                var slideManager = new MusicBeeWrapped.Services.UI.Slides.SlideManager();
                var activeSlides = slideManager.PrepareSlides(stats, playHistory);
                
                var html = new StringBuilder();

                // Create document header with embedded slide CSS
                html.AppendLine(CreateSlideBasedDocumentHeader(year, slideManager));
                
                // Use CreateMainBodyWithSlides instead of CreateMainBody + separate slide generation
                html.AppendLine(CreateMainBodyWithSlides(activeSlides, stats, playHistory, year));
                
                html.AppendLine(CreateNavigationControls());
                html.AppendLine(CreateDataScript(stats, playHistory, year));
                html.AppendLine(CreateSlideBasedMainScript(slideManager, stats, year));

                // Inject a JS error display at the end of the body for debugging
                html.AppendLine("    <script>\nwindow.onerror = function(msg, url, line, col, error) {\n    var errBox = document.getElementById('js-error-box');\n    if (!errBox) {\n        errBox = document.createElement('pre');\n        errBox.id = 'js-error-box';\n        errBox.style.background = 'rgba(255,0,0,0.15)';\n        errBox.style.color = '#ff3333';\n        errBox.style.padding = '1em';\n        errBox.style.margin = '1em';\n        errBox.style.fontSize = '1em';\n        errBox.style.whiteSpace = 'pre-wrap';\n        errBox.style.zIndex = 9999;\n        document.body.appendChild(errBox);\n    }\n    errBox.textContent = 'JS Error: ' + msg + '\n' + url + ':' + line + ':' + col + (error ? ('\n' + error.stack) : '');\n    return false;\n};\n</script>");

                html.AppendLine(CreateDocumentFooter());

                return html.ToString();
            }

        /// <summary>
        /// Creates the HTML document header with meta tags, title, and embedded CSS
        /// </summary>
        /// <param name="year">Year for title context</param>
        /// <param name="titleSuffix">Additional title text</param>
        /// <param name="isYearSelector">Whether this is for year selector</param>
        /// <returns>HTML document header</returns>
        private string CreateDocumentHeader(int year, string titleSuffix, bool isYearSelector = false)
        {
            var header = new StringBuilder();

            header.AppendLine("<!DOCTYPE html>");
            header.AppendLine("<html lang=\"en\">");
            header.AppendLine("<head>");
            header.AppendLine("    <meta charset=\"UTF-8\">");
            header.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            header.AppendLine("    <meta name=\"description\" content=\"MusicBee Wrapped - Your personal music listening statistics and insights\">");
            header.AppendLine("    <meta name=\"author\" content=\"MusicBee Wrapped Plugin\">");
            header.AppendLine($"    <title>{year} {titleSuffix} - MusicBee Wrapped</title>");
            
            // Embedded CSS
            header.AppendLine("    <style>");
            if (isYearSelector)
            {
                header.AppendLine(_cssProvider.GetYearSelectorCSS());
            }
            else
            {
                header.AppendLine(_cssProvider.GetMainInterfaceCSS());
            }
            header.AppendLine("    </style>");
            
            header.AppendLine("</head>");

            return header.ToString();
        }

        /// <summary>
        /// Creates the document header specifically for slide-based templates
        /// Includes both base CSS and slide-specific CSS
        /// </summary>
        private string CreateSlideBasedDocumentHeader(int year, MusicBeeWrapped.Services.UI.Slides.SlideManager slideManager)
        {
            var header = new StringBuilder();

            header.AppendLine("<!DOCTYPE html>");
            header.AppendLine("<html lang=\"en\">");
            header.AppendLine("<head>");
            header.AppendLine("    <meta charset=\"UTF-8\">");
            header.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            header.AppendLine("    <meta name=\"description\" content=\"MusicBee Wrapped - Your personal music listening statistics and insights\">");
            header.AppendLine("    <meta name=\"author\" content=\"MusicBee Wrapped Plugin\">");
            header.AppendLine($"    <title>{year} Your Music Wrapped - MusicBee Wrapped</title>");
            
            // Embedded CSS - combine base styles with slide-specific styles
            header.AppendLine("    <style>");
            header.AppendLine(_cssProvider.GetMainInterfaceCSS());
            header.AppendLine(slideManager.GenerateAllSlideCSS());
            header.AppendLine("    </style>");
            header.AppendLine("</head>");

            return header.ToString();
        }

        /// <summary>
        /// Creates the main body structure for the wrapped interface
        /// Includes app container, loading screen, and initial slide structure
        /// </summary>
        /// <returns>HTML body structure</returns>
        private string CreateMainBody()
        {
            var body = new StringBuilder();

            body.AppendLine("<body>");
            body.AppendLine("    <div id=\"app\">");
            body.AppendLine("        <div id=\"loading\" class=\"slide active\">");
            body.AppendLine("            <div class=\"loading-content\">");
            body.AppendLine("                <h1>🎵 Generating Your Music Wrapped...</h1>");
            body.AppendLine("                <div class=\"loading-spinner\"></div>");
            body.AppendLine("            </div>");
            body.AppendLine("        </div>");
            body.AppendLine("    </div>");

            return body.ToString();
        }

        /// <summary>
        /// Creates the main body with slide HTML included
        /// </summary>
        private string CreateMainBodyWithSlides(System.Collections.Generic.List<MusicBeeWrapped.Services.UI.Slides.SlideComponentBase> activeSlides, WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var body = new StringBuilder();

            body.AppendLine("<body>");
            body.AppendLine("    <div id=\"app\">");
            
            // Loading screen - mark it as temporary and not part of navigation
            body.AppendLine("        <div id=\"loading\" class=\"slide loading-slide\" style=\"display: none;\">");
            body.AppendLine("            <div class=\"loading-content\">");
            body.AppendLine("                <h1>🎵 Generating Your Music Wrapped...</h1>");
            body.AppendLine("                <div class=\"loading-spinner\"></div>");
            body.AppendLine("            </div>");
            body.AppendLine("        </div>");
            
            // Generate slide HTML - make first slide active
            bool isFirstSlide = true;
            foreach (var slide in activeSlides)
            {
                try
                {
                    var slideHtml = slide.GenerateHTML(stats, playHistory, year);
                    
                    // Ensure first slide is active
                    if (isFirstSlide)
                    {
                        slideHtml = slideHtml.Replace("class=\"slide\"", "class=\"slide active\"");
                        isFirstSlide = false;
                    }
                    
                    body.AppendLine(slideHtml);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error generating HTML for slide {slide.SlideId}: {ex.Message}");
                }
            }
            
            body.AppendLine("    </div>");

            return body.ToString();
        }

        /// <summary>
        /// Creates the navigation controls for slide progression
        /// Includes previous/next buttons and slide indicators
        /// </summary>
        /// <returns>Navigation controls HTML</returns>
        private string CreateNavigationControls()
        {
            var nav = new StringBuilder();

            nav.AppendLine("    <div id=\"nav-controls\">");
            nav.AppendLine("        <button id=\"prev-btn\" class=\"nav-btn\" disabled aria-label=\"Previous slide\">←</button>");
            nav.AppendLine("        <div id=\"slide-indicator\" role=\"tablist\" aria-label=\"Slide navigation\"></div>");
            nav.AppendLine("        <button id=\"next-btn\" class=\"nav-btn\" aria-label=\"Next slide\">→</button>");
            nav.AppendLine("    </div>");

            return nav.ToString();
        }

        /// <summary>
        /// Creates the year selector body with grid layout of available years
        /// </summary>
        /// <param name="availableYears">Years to display</param>
        /// <param name="metadata">Metadata for each year</param>
        /// <returns>Year selector body HTML</returns>
        private string CreateYearSelectorBody(System.Collections.Generic.List<int> availableYears, YearMetadataCollection metadata)
        {
            var body = new StringBuilder();

            body.AppendLine("<body>");
            body.AppendLine("    <div class=\"year-selector-container\">");
            body.AppendLine("        <div class=\"year-selector-header\">");
            body.AppendLine("            <h1>Choose Your Music Year</h1>");
            body.AppendLine("            <p class=\"year-selector-subtitle\">Select a year to explore your listening history</p>");
            body.AppendLine("        </div>");
            body.AppendLine("        <div class=\"years-grid\">");

            foreach (var year in availableYears)
            {
                var yearMeta = metadata.GetYearMetadata(year);
                if (yearMeta != null && yearMeta.TotalPlays > 0)
                {
                    body.AppendLine(CreateYearCard(year, yearMeta));
                }
            }

            body.AppendLine("        </div>");
            body.AppendLine("    </div>");

            return body.ToString();
        }

        /// <summary>
        /// Creates an individual year card for the year selector
        /// </summary>
        /// <param name="year">Year to create card for</param>
        /// <param name="metadata">Year metadata for stats</param>
        /// <returns>Year card HTML</returns>
        private string CreateYearCard(int year, YearMetadata metadata)
        {
            var card = new StringBuilder();

            // Use explicit navigation to force a full page reload and avoid SPA/JS navigation issues
            card.AppendLine($"        <a href=\"wrapped_{year}.html\" class=\"year-card\" data-year=\"{year}\" tabindex=\"0\" target=\"_self\" rel=\"noopener\">" );
            card.AppendLine($"            <div class=\"year-title\">{year}</div>");
            card.AppendLine("            <div class=\"year-stats\">");
            card.AppendLine("                <div class=\"year-stat\">");
            card.AppendLine($"                    <div class=\"year-stat-number\">{FormatNumber(metadata.TotalPlays)}</div>");
            card.AppendLine("                    <div class=\"year-stat-label\">Tracks</div>");
            card.AppendLine("                </div>");
            card.AppendLine("                <div class=\"year-stat\">");
            card.AppendLine($"                    <div class=\"year-stat-number\">{Math.Round(metadata.TotalMinutes / 60.0, 1)}</div>");
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

            card.AppendLine($"                <div class=\"year-highlight\">Period: {metadata.FirstPlay:MMM} - {metadata.LastPlay:MMM}</div>");
            card.AppendLine("            </div>");
            card.AppendLine("        </a>");

            return card.ToString();
        }

        /// <summary>
        /// Creates the script tag containing serialized data for the wrapped interface
        /// </summary>
        /// <param name="stats">Statistics to serialize</param>
        /// <param name="playHistory">Play history for context</param>
        /// <param name="year">Target year</param>
        /// <returns>Script tag with embedded data</returns>
        private string CreateDataScript(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var script = new StringBuilder();

            script.AppendLine("    <script>");
            script.AppendLine(_dataSerializer.CreateWrappedDataObject(stats, playHistory, year));
            script.AppendLine("    </script>");

            return script.ToString();
        }

        /// <summary>
        /// Creates the main JavaScript script tag for wrapped functionality
        /// </summary>
        /// <returns>Script tag with main JavaScript</returns>
        private string CreateMainScript()
        {
            var script = new StringBuilder();

            script.AppendLine("    <script>");
            script.AppendLine(_jsProvider.GetMainInterfaceJS());
            script.AppendLine("    </script>");

            return script.ToString();
        }

        /// <summary>
        /// Creates the year selector JavaScript script tag
        /// </summary>
        /// <returns>Script tag with year selector JavaScript</returns>
        private string CreateYearSelectorScript()
        {
            var script = new StringBuilder();

            script.AppendLine("    <script>");
            script.AppendLine(_jsProvider.GetYearSelectorJS());
            script.AppendLine("    </script>");

            return script.ToString();
        }

        /// <summary>
        /// Creates the document footer
        /// </summary>
        /// <returns>HTML document footer</returns>
        private string CreateDocumentFooter()
        {
            var footer = new StringBuilder();

            footer.AppendLine("</body>");
            footer.AppendLine("</html>");

            return footer.ToString();
        }

        /// <summary>
        /// Escapes HTML characters for safe display
        /// </summary>
        /// <param name="input">Raw text input</param>
        /// <returns>HTML-escaped text</returns>
        private string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            return input.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#39;");
        }

        /// <summary>
        /// Formats numbers for display (e.g., 1500 -> 1.5k)
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <returns>Formatted number string</returns>
        private string FormatNumber(int number)
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
        /// Creates a minimal HTML template for error scenarios
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        /// <param name="year">Year context for title</param>
        /// <returns>Error page HTML</returns>
        public string CreateErrorTemplate(string errorMessage, int year)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine($"    <title>Error - MusicBee Wrapped {year}</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: sans-serif; padding: 2rem; background: #1a1a2e; color: white; text-align: center; }");
            html.AppendLine("        .error-container { max-width: 600px; margin: 0 auto; }");
            html.AppendLine("        h1 { color: #ff6b6b; margin-bottom: 1rem; }");
            html.AppendLine("        .error-message { background: rgba(255,255,255,0.1); padding: 1rem; border-radius: 8px; margin: 1rem 0; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class=\"error-container\">");
            html.AppendLine("        <h1>Unable to Generate Wrapped</h1>");
            html.AppendLine($"        <div class=\"error-message\">{EscapeHtml(errorMessage)}</div>");
            html.AppendLine("        <p>Please try again or check your data.</p>");
            html.AppendLine("    </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// Creates document header for slide-based templates
        /// </summary>
        private string CreateDocumentHeaderWithSlides(int year, MusicBeeWrapped.Services.UI.Slides.SlideManager slideManager)
        {
            var header = new StringBuilder();

            header.AppendLine("<!DOCTYPE html>");
            header.AppendLine("<html lang=\"en\">");
            header.AppendLine("<head>");
            header.AppendLine("    <meta charset=\"UTF-8\">");
            header.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            header.AppendLine("    <meta name=\"description\" content=\"MusicBee Wrapped - Your personal music listening statistics and insights\">");
            header.AppendLine("    <meta name=\"author\" content=\"MusicBee Wrapped Plugin\">");
            header.AppendLine($"    <title>{year} Your Music Wrapped - MusicBee Wrapped</title>");
            
            // Embedded CSS - base styles plus slide-specific styles
            header.AppendLine("    <style>");
            header.AppendLine(_cssProvider.GetMainInterfaceCSS());
            header.AppendLine(slideManager.GenerateAllSlideCSS());
            header.AppendLine("    </style>");
            header.AppendLine("</head>");

            return header.ToString();
        }

        /// <summary>
        /// Creates script section for slide-based templates
        /// </summary>
        private string CreateSlideBasedMainScript(MusicBeeWrapped.Services.UI.Slides.SlideManager slideManager, WrappedStatistics stats, int year)
        {
            var script = new StringBuilder();

            script.AppendLine("    <script>");
            script.AppendLine(slideManager.GenerateSlideNavigationData());
            script.AppendLine(_jsProvider.GetMainInterfaceJS());
            script.AppendLine(slideManager.GenerateAllSlideJavaScript(stats, year));
            script.AppendLine("    </script>");

            return script.ToString();
        }
    }
}
