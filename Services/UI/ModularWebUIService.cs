using System;
using System.Collections.Generic;
using MusicBeeWrapped;
using MusicBeeWrapped.Services.UI;
using MusicBeeWrapped.Services.UI.Slides;
using MusicBeePlugin.Services;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Modern modular web UI service that orchestrates all UI components
    /// Replaces the monolithic WebUIService with a component-based architecture
    /// </summary>
    public class ModularWebUIService
    {
        private readonly SessionManager _sessionManager;
        private readonly BrowserLauncher _browserLauncher;
        private readonly YearSelectorService _yearSelectorService;
        private readonly HtmlTemplateBuilder _templateBuilder;
        private readonly CssStyleProvider _cssProvider;
        private readonly JavaScriptProvider _jsProvider;
        private readonly DataSerializer _dataSerializer;
        private readonly SlideManager _slideManager;
        private readonly YearBasedDataService _dataService;

        public ModularWebUIService(YearBasedDataService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            
            // Initialize all UI components
            _sessionManager = new SessionManager();
            _browserLauncher = new BrowserLauncher();
            _cssProvider = new CssStyleProvider();
            _jsProvider = new JavaScriptProvider();
            _dataSerializer = new DataSerializer();
            _templateBuilder = new HtmlTemplateBuilder(_cssProvider, _jsProvider, _dataSerializer);
            _slideManager = new SlideManager();
            _yearSelectorService = new YearSelectorService(_dataService, _templateBuilder, _sessionManager, _browserLauncher);
            
            // Cleanup old sessions on startup
            _sessionManager.CleanupOldSessions();
        }

        /// <summary>
        /// Generates and launches the wrapped UI for a specific year
        /// Uses the new modular slide-based architecture
        /// </summary>
        /// <param name="stats">Wrapped statistics</param>
        /// <param name="playHistory">Play history data</param>
        /// <param name="year">Target year</param>
        /// <returns>True if UI was generated and launched successfully</returns>
        public bool GenerateWrappedUI(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            try
            {
                // Validate that we can render slides with available data
                var validationResult = _slideManager.ValidateSlides(stats, playHistory);
                if (!validationResult.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot generate wrapped UI: {string.Join(", ", validationResult.ValidationErrors)}");
                    return false;
                }

                // Create session for this wrapped generation
                var session = _sessionManager.CreateSession();
                
                // Prepare slides based on available data
                var activeSlides = _slideManager.PrepareSlides(stats, playHistory);
                System.Diagnostics.Debug.WriteLine($"Prepared {activeSlides.Count} slides for year {year}");

                // Generate HTML using the modular template builder with slide integration
                var htmlContent = GenerateModularWrappedHTML(stats, playHistory, year, activeSlides);
                
                // Save the HTML file
                var htmlPath = _sessionManager.WriteSessionFile("index.html", htmlContent);
                
                // Launch in browser
                var launched = _browserLauncher.LaunchHtmlFile(htmlPath);
                
                if (launched)
                {
                    // Schedule cleanup after 2 hours
                    _sessionManager.ScheduleSessionCleanup(TimeSpan.FromHours(2));
                    System.Diagnostics.Debug.WriteLine($"Modular wrapped UI launched successfully for year {year}");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating modular wrapped UI: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates and launches the year selector UI
        /// </summary>
        /// <param name="availableYears">Years with data</param>
        /// <param name="metadata">Year metadata collection</param>
        /// <returns>True if year selector was launched successfully</returns>
        public bool GenerateYearSelectorUI(List<int> availableYears, YearMetadataCollection metadata)
        {
            try
            {
                return _yearSelectorService.GenerateAndLaunchYearSelector();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating year selector UI: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates the complete HTML for the wrapped interface using modular components
        /// </summary>
        private string GenerateModularWrappedHTML(WrappedStatistics stats, PlayHistory playHistory, int year, List<SlideComponentBase> slides)
        {
            // Create the base HTML structure
            var html = new System.Text.StringBuilder();
            
            // Document header with metadata and base CSS
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("    <meta name=\"description\" content=\"MusicBee Wrapped - Your personal music listening statistics and insights\">");
            html.AppendLine($"    <title>Your {year} Music Wrapped - MusicBee Wrapped</title>");
            
            // Embedded CSS - combine base styles with slide-specific styles
            html.AppendLine("    <style>");
            html.AppendLine(_cssProvider.GetMainInterfaceCSS());
            html.AppendLine(_slideManager.GenerateAllSlideCSS());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            
            // Body with app container
            html.AppendLine("<body>");
            html.AppendLine("    <div id=\"app\">");
            
            // Loading screen
            html.AppendLine("        <div id=\"loading\" class=\"slide active\">");
            html.AppendLine("            <div class=\"loading-content\">");
            html.AppendLine("                <h1>🎵 Generating Your Music Wrapped...</h1>");
            html.AppendLine("                <div class=\"loading-spinner\"></div>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            
            // Generate HTML for each active slide
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
                    // Continue with other slides
                }
            }
            
            html.AppendLine("    </div>");
            
            // Navigation controls
            html.AppendLine("    <div id=\"nav-controls\">");
            html.AppendLine("        <button id=\"prev-btn\" class=\"nav-btn\" disabled aria-label=\"Previous slide\">←</button>");
            html.AppendLine("        <div id=\"slide-indicator\" role=\"tablist\" aria-label=\"Slide navigation\"></div>");
            html.AppendLine("        <button id=\"next-btn\" class=\"nav-btn\" aria-label=\"Next slide\">→</button>");
            html.AppendLine("    </div>");
            
            // Embedded data and JavaScript
            html.AppendLine("    <script>");
            html.AppendLine(_dataSerializer.CreateWrappedDataObject(stats, playHistory, year));
            html.AppendLine(_slideManager.GenerateSlideNavigationData());
            html.AppendLine("    </script>");
            
            html.AppendLine("    <script>");
            html.AppendLine(GenerateModularJavaScript(stats, year));
            html.AppendLine("    </script>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates JavaScript that integrates with the slide system
        /// </summary>
        private string GenerateModularJavaScript(WrappedStatistics stats, int year)
        {
            var js = new System.Text.StringBuilder();
            
            // Base JavaScript functionality
            js.AppendLine(_jsProvider.GetMainInterfaceJS());
            
            // Slide-specific JavaScript
            js.AppendLine(_slideManager.GenerateAllSlideJavaScript(stats, year));
            
            // Enhanced navigation for modular slides
            js.AppendLine(@"
        // Enhanced modular slide navigation
        function initializeModularSlides() {
            const slideElements = document.querySelectorAll('.slide[data-slide-id]');
            const slideData = window.SLIDE_DATA || { slides: [], totalSlides: slideElements.length };
            
            // Update global slide tracking
            slides = Array.from(slideElements);
            totalSlides = slides.length;
            
            console.log(`Initialized ${totalSlides} modular slides`);
            
            // Ensure all slides are initially hidden except for setup
            slides.forEach((slide, index) => {
                slide.style.display = 'none';
                slide.classList.remove('active');
            });
            
            // Initialize any chart-requiring slides
            slideData.slides.forEach(slideInfo => {
                if (slideInfo.requiresChart) {
                    console.log(`Slide ${slideInfo.id} requires chart rendering`);
                }
            });
            
            updateSlideIndicator();
        }
        
        // Override the original initialization
        const originalInitializeApp = initializeApp;
        initializeApp = function() {
            setTimeout(() => {
                document.getElementById('loading').style.display = 'none';
                initializeModularSlides();
                initializeNavigation();
                // Small delay to ensure slides are properly initialized
                setTimeout(() => {
                    showSlide(0);
                }, 100);
            }, 2000);
        };");
            
            return js.ToString();
        }

        /// <summary>
        /// Cleans up old web UI sessions
        /// </summary>
        public void CleanupOldSessions()
        {
            _sessionManager.CleanupOldSessions();
        }

        /// <summary>
        /// Gets information about the currently supported slide types
        /// </summary>
        /// <returns>List of slide information</returns>
        public List<SlideInfo> GetSupportedSlides()
        {
            var slides = _slideManager.GetAllSlides();
            return slides.ConvertAll(slide => new SlideInfo
            {
                Id = slide.SlideId,
                Title = slide.SlideTitle,
                Order = slide.SlideOrder,
                RequiresCharts = slide.RequiresChartRendering
            });
        }
    }

    /// <summary>
    /// Information about a slide component
    /// </summary>
    public class SlideInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public bool RequiresCharts { get; set; }
    }
}
