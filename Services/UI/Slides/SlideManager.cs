using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Manages the collection of slide components and orchestrates slide generation
    /// Provides centralized control over slide registration, ordering, and rendering
    /// </summary>
    public class SlideManager
    {
        private readonly List<SlideComponentBase> _registeredSlides;
        private readonly List<SlideComponentBase> _activeSlides;

        public SlideManager()
        {
            _registeredSlides = new List<SlideComponentBase>();
            _activeSlides = new List<SlideComponentBase>();
            
            RegisterDefaultSlides();
        }

        /// <summary>
        /// Registers all default slide components
        /// </summary>
        private void RegisterDefaultSlides()
        {
            // Welcome slide comes first
            RegisterSlide(new WelcomeSlide());                  // 0. Welcome
            
            // Spotify-style slides (ordered 1-5)
            RegisterSlide(new TopDayByMinutesSlide());          // 1. Top day by minutes  
            RegisterSlide(new TopSongSlide());                  // 2. Top song - # of plays, day with most plays
            RegisterSlide(new TopTracksSlide());                // 3. Top 5 songs
            RegisterSlide(new TopArtistsSlide());               // 4. Top 5 artists
            RegisterSlide(new TopArtistSlide());                // 5. Interactive top artist showcase
            
            // Additional slides
            RegisterSlide(new TopAlbumsSlide());                // 6. Top Albums
            RegisterSlide(new DailyChartSlide());               // 7. Daily Chart
            RegisterSlide(new FinaleSlide());                   // 8. Finale
        }

        /// <summary>
        /// Registers a new slide component
        /// </summary>
        /// <param name="slide">Slide component to register</param>
        public void RegisterSlide(SlideComponentBase slide)
        {
            if (slide == null) throw new ArgumentNullException(nameof(slide));
            
            // Prevent duplicate registration
            if (_registeredSlides.Any(s => s.SlideId == slide.SlideId))
            {
                throw new InvalidOperationException($"Slide with ID '{slide.SlideId}' is already registered");
            }
            
            _registeredSlides.Add(slide);
        }

        /// <summary>
        /// Unregisters a slide component by ID
        /// </summary>
        /// <param name="slideId">ID of slide to unregister</param>
        /// <returns>True if slide was found and removed</returns>
        public bool UnregisterSlide(string slideId)
        {
            var slide = _registeredSlides.FirstOrDefault(s => s.SlideId == slideId);
            if (slide != null)
            {
                _registeredSlides.Remove(slide);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Prepares slides for rendering based on available data
        /// Only includes slides that can render with the provided data
        /// </summary>
        /// <param name="stats">Wrapped statistics</param>
        /// <param name="playHistory">Play history data</param>
        /// <returns>List of slides that can be rendered</returns>
        public List<SlideComponentBase> PrepareSlides(WrappedStatistics stats, PlayHistory playHistory)
        {
            _activeSlides.Clear();
            
            var availableSlides = _registeredSlides
                .Where(slide => slide.CanRender(stats, playHistory))
                .OrderBy(slide => slide.SlideOrder)
                .ToList();
            
            _activeSlides.AddRange(availableSlides);
            return new List<SlideComponentBase>(_activeSlides);
        }

        /// <summary>
        /// Generates JavaScript for all active slides
        /// </summary>
        /// <param name="stats">Statistics for data binding</param>
        /// <param name="year">Target year</param>
        /// <returns>Combined JavaScript for all slides</returns>
        public string GenerateAllSlideJavaScript(WrappedStatistics stats, int year)
        {
            var js = new StringBuilder();
            
            js.AppendLine("// Generated slide-specific JavaScript");
            js.AppendLine();

            foreach (var slide in _activeSlides)
            {
                var slideJs = slide.GenerateJavaScript(stats, year);
                if (!string.IsNullOrEmpty(slideJs))
                {
                    js.AppendLine($"// JavaScript for {slide.SlideId} slide");
                    js.AppendLine(slideJs);
                    js.AppendLine();
                }
            }

            return js.ToString();
        }

        /// <summary>
        /// Generates CSS for all active slides
        /// </summary>
        /// <returns>Combined CSS for all slides</returns>
        public string GenerateAllSlideCSS()
        {
            var css = new StringBuilder();
            
            css.AppendLine("/* Generated slide-specific CSS */");

            foreach (var slide in _activeSlides)
            {
                var slideCSS = slide.GenerateCSS();
                if (!string.IsNullOrEmpty(slideCSS))
                {
                    css.AppendLine($"/* CSS for {slide.SlideId} slide */");
                    css.AppendLine(slideCSS);
                    css.AppendLine();
                }
            }

            return css.ToString();
        }

        /// <summary>
        /// Generates the slide navigation data for JavaScript
        /// </summary>
        /// <returns>JavaScript object with slide navigation information</returns>
        public string GenerateSlideNavigationData()
        {
            var slides = _activeSlides.Select((slide, index) => new
            {
                id = slide.SlideId,
                title = slide.SlideTitle,
                order = slide.SlideOrder,
                index = index,
                requiresChart = slide.RequiresChartRendering
            }).ToList();

            var slideData = string.Join(",\n            ", slides.Select(s => 
                $"{{ id: '{s.id}', title: '{EscapeJsString(s.title)}', order: {s.order}, index: {s.index}, requiresChart: {s.requiresChart.ToString().ToLower()} }}"));

            return $@"
        window.SLIDE_DATA = {{
            slides: [
            {slideData}
            ],
            totalSlides: {_activeSlides.Count}
        }};";
        }

        /// <summary>
        /// Gets a slide by its ID
        /// </summary>
        /// <param name="slideId">Slide identifier</param>
        /// <returns>Slide component or null if not found</returns>
        public SlideComponentBase GetSlide(string slideId)
        {
            return _registeredSlides.FirstOrDefault(s => s.SlideId == slideId);
        }

        /// <summary>
        /// Gets all registered slides
        /// </summary>
        /// <returns>List of all registered slides</returns>
        public List<SlideComponentBase> GetAllSlides()
        {
            return new List<SlideComponentBase>(_registeredSlides);
        }

        /// <summary>
        /// Gets the currently active slides (those that will be rendered)
        /// </summary>
        /// <returns>List of active slides</returns>
        public List<SlideComponentBase> GetActiveSlides()
        {
            return new List<SlideComponentBase>(_activeSlides);
        }

        /// <summary>
        /// Checks if any registered slides require chart rendering
        /// </summary>
        /// <returns>True if charts are needed</returns>
        public bool RequiresChartRendering()
        {
            return _activeSlides.Any(slide => slide.RequiresChartRendering);
        }

        /// <summary>
        /// Validates that all required slides can be rendered
        /// </summary>
        /// <param name="stats">Statistics to validate against</param>
        /// <param name="playHistory">Play history to validate against</param>
        /// <returns>Validation result with details</returns>
        public SlideValidationResult ValidateSlides(WrappedStatistics stats, PlayHistory playHistory)
        {
            var result = new SlideValidationResult();
            
            foreach (var slide in _registeredSlides)
            {
                if (slide.CanRender(stats, playHistory))
                {
                    result.ValidSlides.Add(slide.SlideId);
                }
                else
                {
                    result.InvalidSlides.Add(slide.SlideId);
                    result.ValidationErrors.Add($"Slide '{slide.SlideId}' cannot render with available data");
                }
            }
            
            result.IsValid = result.ValidSlides.Count > 0;
            return result;
        }

        private string EscapeJsString(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }

    /// <summary>
    /// Result of slide validation operation
    /// </summary>
    public class SlideValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidSlides { get; set; } = new List<string>();
        public List<string> InvalidSlides { get; set; } = new List<string>();
        public List<string> ValidationErrors { get; set; } = new List<string>();
        
        public int ValidSlideCount => ValidSlides.Count;
        public int InvalidSlideCount => InvalidSlides.Count;
        public bool HasErrors => ValidationErrors.Count > 0;
    }
}
