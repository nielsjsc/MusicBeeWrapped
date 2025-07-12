using System;
using System.Collections.Generic;
using System.Linq;
using MusicBeePlugin.Services;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Specialized service for generating and managing year selector interface
    /// Handles year filtering, metadata preparation, and UI generation
    /// </summary>
    public class YearSelectorService
    {
        private readonly YearBasedDataService _dataService;
        private readonly HtmlTemplateBuilder _templateBuilder;
        private readonly SessionManager _sessionManager;
        private readonly BrowserLauncher _browserLauncher;

        public YearSelectorService(
            YearBasedDataService dataService,
            HtmlTemplateBuilder templateBuilder = null,
            SessionManager sessionManager = null,
            BrowserLauncher browserLauncher = null)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _templateBuilder = templateBuilder ?? new HtmlTemplateBuilder();
            _sessionManager = sessionManager ?? new SessionManager();
            _browserLauncher = browserLauncher ?? new BrowserLauncher();
        }

        /// <summary>
        /// Generates and launches the year selector interface
        /// </summary>
        /// <returns>True if generation and launch was successful</returns>
        public bool GenerateAndLaunchYearSelector()
        {
            try
            {
                // Get available years and validate them
                var availableYears = GetValidatedYears();
                
                if (availableYears.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No years with sufficient data found for year selector");
                    return false;
                }

                // If only one year available, skip selector and go directly to that year
                if (availableYears.Count == 1)
                {
                    return GenerateDirectWrapped(availableYears.First());
                }

                // Create session first - all files will be written to this session
                var sessionPath = _sessionManager.CreateSession();

                // Generate wrapped HTML for each year in background
                GenerateWrappedForAvailableYears(availableYears);

                // Create year selector HTML
                var metadata = _dataService.GetMetadata();
                var yearSelectorHtml = _templateBuilder.CreateYearSelectorTemplate(availableYears, metadata);

                // Write year selector file to the same session
                var yearSelectorFile = _sessionManager.WriteSessionFile("year_selector.html", yearSelectorHtml);

                // Schedule cleanup
                _sessionManager.ScheduleSessionCleanup(TimeSpan.FromHours(2));

                // Launch in browser
                bool launched = _browserLauncher.LaunchHtmlFile(yearSelectorFile);
                
                if (launched)
                {
                    System.Diagnostics.Debug.WriteLine($"Year selector launched successfully with {availableYears.Count} available years");
                }

                return launched;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating year selector: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates wrapped interface for a specific year
        /// </summary>
        /// <param name="year">Year to generate wrapped for</param>
        /// <returns>True if generation and launch was successful</returns>
        public bool GenerateDirectWrapped(int year)
        {
            try
            {
                var playHistory = _dataService.GetYearData(year);
                
                if (playHistory == null || playHistory.Plays.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"No data available for year {year}");
                    return false;
                }

                // Generate statistics
                var stats = new WrappedStatistics(playHistory, year);
                var slideManager = new MusicBeeWrapped.Services.UI.Slides.SlideManager();
                var activeSlides = slideManager.PrepareSlides(stats, playHistory);
                var wrappedHtml = _templateBuilder.GenerateModularWrappedHTML(
                    stats,
                    playHistory,
                    year,
                    activeSlides,
                    new CssStyleProvider(),
                    new JavaScriptProvider(),
                    new DataSerializer(),
                    slideManager
                );
                // Create session and write file
                var sessionPath = _sessionManager.CreateSession();
                var wrappedFile = _sessionManager.WriteSessionFile($"wrapped_{year}.html", wrappedHtml);
                
                // Schedule cleanup
                _sessionManager.ScheduleSessionCleanup(TimeSpan.FromHours(2));
                
                // Launch in browser
                bool launched = _browserLauncher.LaunchHtmlFile(wrappedFile);
                
                if (launched)
                {
                    System.Diagnostics.Debug.WriteLine($"Direct wrapped for year {year} launched successfully");
                }

                return launched;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating direct wrapped for year {year}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets years that have sufficient data for meaningful wrapped generation
        /// </summary>
        /// <param name="minimumPlays">Minimum number of plays required</param>
        /// <param name="minimumListeningTime">Minimum listening time in minutes</param>
        /// <returns>List of valid years with sufficient data</returns>
        public List<int> GetValidatedYears(int minimumPlays = 50, int minimumListeningTime = 60)
        {
            try
            {
                var allYears = _dataService.GetAvailableYears();
                var metadata = _dataService.GetMetadata();
                var validYears = new List<int>();

                foreach (var year in allYears)
                {
                    var yearMetadata = metadata.GetYearMetadata(year);
                    
                    if (yearMetadata != null && 
                        yearMetadata.TotalPlays >= minimumPlays && 
                        yearMetadata.TotalMinutes >= minimumListeningTime)
                    {
                        validYears.Add(year);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Year {year} filtered out - plays: {yearMetadata?.TotalPlays ?? 0}, minutes: {yearMetadata?.TotalMinutes ?? 0}");
                    }
                }

                return validYears.OrderByDescending(y => y).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating years: {ex.Message}");
                return new List<int>();
            }
        }

        /// <summary>
        /// Gets summary statistics for year selector display
        /// </summary>
        /// <param name="year">Year to get summary for</param>
        /// <returns>Year summary information</returns>
        public YearSummary GetYearSummary(int year)
        {
            try
            {
                var metadata = _dataService.GetYearMetadata(year);
                if (metadata == null)
                {
                    return null;
                }

                return new YearSummary
                {
                    Year = year,
                    TotalPlays = metadata.TotalPlays,
                    TotalHours = Math.Round(metadata.TotalMinutes / 60.0, 1),
                    TopArtist = metadata.TopArtist,
                    TopGenre = metadata.TopGenre,
                    FirstPlay = metadata.FirstPlay,
                    LastPlay = metadata.LastPlay,
                    IsCurrentYear = year == DateTime.Now.Year
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting year summary for {year}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Refreshes metadata for all available years
        /// Useful when data has been updated and metadata needs to be recalculated
        /// </summary>
        public void RefreshMetadata()
        {
            try
            {
                var years = _dataService.GetAvailableYears();
                
                foreach (var year in years)
                {
                    var playHistory = _dataService.GetYearData(year);
                    if (playHistory != null && playHistory.Plays.Count > 0)
                    {
                        // This will trigger metadata update in the data service
                        var stats = new WrappedStatistics(playHistory, year);
                        System.Diagnostics.Debug.WriteLine($"Refreshed metadata for year {year}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing metadata: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if a year has enough data for a meaningful wrapped experience
        /// </summary>
        /// <param name="year">Year to validate</param>
        /// <param name="minimumPlays">Minimum play count threshold</param>
        /// <param name="minimumHours">Minimum hours threshold</param>
        /// <returns>Validation result with details</returns>
        public YearValidationResult ValidateYear(int year, int minimumPlays = 50, double minimumHours = 1.0)
        {
            try
            {
                var metadata = _dataService.GetYearMetadata(year);
                
                if (metadata == null)
                {
                    return new YearValidationResult
                    {
                        IsValid = false,
                        Year = year,
                        Message = "No metadata found for this year"
                    };
                }

                var hours = metadata.TotalMinutes / 60.0;
                var hasEnoughPlays = metadata.TotalPlays >= minimumPlays;
                var hasEnoughTime = hours >= minimumHours;

                return new YearValidationResult
                {
                    IsValid = hasEnoughPlays && hasEnoughTime,
                    Year = year,
                    TotalPlays = metadata.TotalPlays,
                    TotalHours = hours,
                    Message = GetValidationMessage(hasEnoughPlays, hasEnoughTime, metadata.TotalPlays, hours, minimumPlays, minimumHours)
                };
            }
            catch (Exception ex)
            {
                return new YearValidationResult
                {
                    IsValid = false,
                    Year = year,
                    Message = $"Error validating year: {ex.Message}"
                };
            }
        }

        private void GenerateWrappedForAvailableYears(List<int> years)
        {
            foreach (var year in years)
            {
                try
                {
                    var playHistory = _dataService.GetYearData(year);
                    if (playHistory != null && playHistory.Plays.Count > 0)
                    {
                        var stats = new WrappedStatistics(playHistory, year);
                        var slideManager = new MusicBeeWrapped.Services.UI.Slides.SlideManager();
                        var activeSlides = slideManager.PrepareSlides(stats, playHistory);
                        var wrappedHtml = _templateBuilder.GenerateModularWrappedHTML(
                            stats,
                            playHistory,
                            year,
                            activeSlides,
                            new CssStyleProvider(),
                            new JavaScriptProvider(),
                            new DataSerializer(),
                            slideManager
                        );
                        _sessionManager.WriteSessionFile($"wrapped_{year}.html", wrappedHtml);
                        System.Diagnostics.Debug.WriteLine($"Generated wrapped for year {year} with {playHistory.Plays.Count} plays");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error generating wrapped for year {year}: {ex.Message}");
                }
            }
        }

        private string GetValidationMessage(bool hasEnoughPlays, bool hasEnoughTime, int plays, double hours, int minPlays, double minHours)
        {
            if (hasEnoughPlays && hasEnoughTime)
            {
                return $"Valid year with {plays} plays and {hours:F1} hours";
            }

            var issues = new List<string>();
            
            if (!hasEnoughPlays)
            {
                issues.Add($"insufficient plays ({plays} < {minPlays})");
            }
            
            if (!hasEnoughTime)
            {
                issues.Add($"insufficient listening time ({hours:F1}h < {minHours}h)");
            }

            return $"Year excluded: {string.Join(", ", issues)}";
        }
    }

    /// <summary>
    /// Summary information for a specific year
    /// </summary>
    public class YearSummary
    {
        public int Year { get; set; }
        public int TotalPlays { get; set; }
        public double TotalHours { get; set; }
        public string TopArtist { get; set; }
        public string TopGenre { get; set; }
        public DateTime FirstPlay { get; set; }
        public DateTime LastPlay { get; set; }
        public bool IsCurrentYear { get; set; }
    }

    /// <summary>
    /// Result of year validation
    /// </summary>
    public class YearValidationResult
    {
        public bool IsValid { get; set; }
        public int Year { get; set; }
        public int TotalPlays { get; set; }
        public double TotalHours { get; set; }
        public string Message { get; set; }
    }
}
