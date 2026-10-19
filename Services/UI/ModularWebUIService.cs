using System;
using System.Collections.Generic;
using MusicBeeWrapped;
using MusicBeeWrapped.Services.UI;
using MusicBeeWrapped.Services.UI.Slides;
using MusicBeePlugin.Services;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Modern modular web UI service that orchestrates all UI components.
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

            _sessionManager = new SessionManager();
            _browserLauncher = new BrowserLauncher();
            _cssProvider = new CssStyleProvider();
            _jsProvider = new JavaScriptProvider();
            _dataSerializer = new DataSerializer();
            _templateBuilder = new HtmlTemplateBuilder(_cssProvider, _jsProvider, _dataSerializer);
            _slideManager = new SlideManager();
            _yearSelectorService = new YearSelectorService(_dataService, _templateBuilder, _sessionManager, _browserLauncher);

            _sessionManager.CleanupOldSessions();
        }

        public bool GenerateWrappedUI(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            try
            {
                var validationResult = _slideManager.ValidateSlides(stats, playHistory);
                if (!validationResult.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot generate wrapped UI: {string.Join(", ", validationResult.ValidationErrors)}");
                    return false;
                }

                _sessionManager.CreateSession();

                var activeSlides = _slideManager.PrepareSlides(stats, playHistory);
                System.Diagnostics.Debug.WriteLine($"Prepared {activeSlides.Count} slides for year {year}");

                var htmlContent = _templateBuilder.GenerateModularWrappedHTML(
                    stats,
                    playHistory,
                    year,
                    activeSlides,
                    _cssProvider,
                    _jsProvider,
                    _dataSerializer,
                    _slideManager);

                var htmlPath = _sessionManager.WriteSessionFile("index.html", htmlContent);
                var launched = _browserLauncher.LaunchHtmlFile(htmlPath);

                if (launched)
                {
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

        public void CleanupOldSessions()
        {
            _sessionManager.CleanupOldSessions();
        }

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

    public class SlideInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public bool RequiresCharts { get; set; }
    }
}
