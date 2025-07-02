using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using MusicBeePlugin.Services;

namespace MusicBeeWrapped.Services
{
    /// <summary>
    /// Generates and manages the web-based UI for MusicBee Wrapped
    /// Creates HTML files with embedded CSS/JS and launches in browser
    /// </summary>
    public class WebUIService
    {
        private readonly string _tempBasePath;
        private string _currentSessionPath;
        private readonly YearBasedDataService _dataService;

        public WebUIService(YearBasedDataService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _tempBasePath = Path.Combine(Path.GetTempPath(), "MusicBeeWrapped");
            EnsureTempDirectoryExists();
        }

        /// <summary>
        /// Main entry point - generates the complete Wrapped UI and launches browser
        /// </summary>
        public bool GenerateWrappedUI(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            try
            {
                // Create unique session folder
                _currentSessionPath = CreateSessionFolder();
                
                // Generate the main HTML file with embedded assets
                string htmlContent = CreateHTMLTemplate(stats, playHistory, year);
                string indexPath = Path.Combine(_currentSessionPath, "index.html");
                File.WriteAllText(indexPath, htmlContent, Encoding.UTF8);

                // Launch in default browser
                bool launched = LaunchBrowser(indexPath);
                
                if (launched)
                {
                    // Schedule cleanup after 2 hours
                    ScheduleCleanup(_currentSessionPath, TimeSpan.FromHours(2));
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // Log error but don't throw - fallback to trace logging
                System.Diagnostics.Trace.WriteLine($"WebUIService Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates a year selector UI for choosing which year's stats to view
        /// </summary>
        public bool GenerateYearSelectorUI(System.Collections.Generic.List<int> availableYears, YearMetadataCollection metadata)
        {
            try
            {
                // Create unique session folder
                _currentSessionPath = CreateSessionFolder();
                
                // Generate wrapped HTML for each available year
                foreach (var year in availableYears)
                {
                    // First, ensure year data is loaded (this will also load metadata)
                    var yearPlayHistory = _dataService.GetYearData(year);
                    
                    // Now get the metadata (should be loaded)
                    var yearMetadata = metadata.GetYearMetadata(year);
                    if (yearMetadata != null && yearMetadata.TotalPlays > 0)
                    {
                        // Generate wrapped data for this year
                        GenerateWrappedForYear(year);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Year {year} filtered out - metadata: {yearMetadata?.TotalPlays ?? 0} plays");
                    }
                }
                
                // Generate the year selector HTML
                string htmlContent = CreateYearSelectorHTML(availableYears, metadata);
                string indexPath = Path.Combine(_currentSessionPath, "year_selector.html");
                File.WriteAllText(indexPath, htmlContent, Encoding.UTF8);

                // Launch in default browser
                bool launched = LaunchBrowser(indexPath);
                
                if (launched)
                {
                    // Schedule cleanup after 2 hours
                    ScheduleCleanup(_currentSessionPath, TimeSpan.FromHours(2));
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating year selector UI: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Generates wrapped HTML for a specific year and saves it to the session folder
        /// </summary>
        private void GenerateWrappedForYear(int year)
        {
            try
            {
                // Get play history for the specified year
                var yearPlayHistory = _dataService.GetYearData(year);
                
                if (yearPlayHistory != null && yearPlayHistory.Plays.Count > 0)
                {
                    // Generate wrapped statistics for this year
                    var wrappedStats = new WrappedStatistics(yearPlayHistory, year);
                    
                    // Create HTML content for this year
                    string htmlContent = CreateHTMLTemplate(wrappedStats, yearPlayHistory, year);
                    string wrappedPath = Path.Combine(_currentSessionPath, $"wrapped_{year}.html");
                    File.WriteAllText(wrappedPath, htmlContent, Encoding.UTF8);
                    
                    System.Diagnostics.Debug.WriteLine($"Generated wrapped for year {year} with {yearPlayHistory.Plays.Count} plays");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No data available for year {year}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating wrapped for year {year}: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the main HTML template with embedded CSS, JS, and data
        /// </summary>
        private string CreateHTMLTemplate(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine($"    <title>Your {year} Music Wrapped</title>");
            
            // Embed CSS
            html.AppendLine("    <style>");
            html.AppendLine(GetEmbeddedCSS());
            html.AppendLine("    </style>");
            
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Main app container
            html.AppendLine("    <div id=\"app\">");
            html.AppendLine("        <div id=\"loading\" class=\"slide active\">");
            html.AppendLine("            <div class=\"loading-content\">");
            html.AppendLine("                <h1>🎵 Generating Your Music Wrapped...</h1>");
            html.AppendLine("                <div class=\"loading-spinner\"></div>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            // Navigation controls
            html.AppendLine("    <div id=\"nav-controls\">");
            html.AppendLine("        <button id=\"prev-btn\" class=\"nav-btn\" disabled>←</button>");
            html.AppendLine("        <div id=\"slide-indicator\"></div>");
            html.AppendLine("        <button id=\"next-btn\" class=\"nav-btn\">→</button>");
            html.AppendLine("    </div>");
            
            // Get first and last plays of the year
            var yearPlays = playHistory.GetPlaysByYear(year).OrderBy(p => p.PlayedAt).ToList();
            var firstPlay = yearPlays.FirstOrDefault();
            var lastPlay = yearPlays.LastOrDefault();
            
            // Embed statistics data as JSON
            html.AppendLine("    <script>");
            html.AppendLine("        window.WRAPPED_DATA = {");
            html.AppendLine($"            year: {year},");
            html.AppendLine($"            totalTracks: {stats.TotalTracks},");
            html.AppendLine($"            totalHours: {stats.TotalHours.ToString("F1", CultureInfo.InvariantCulture)},");
            html.AppendLine($"            totalArtists: {stats.TotalUniqueArtists},");
            html.AppendLine($"            totalAlbums: {stats.TotalUniqueAlbums},");
            html.AppendLine($"            topArtists: {SerializeTopList(stats.TopArtists.Take(5))},");
            html.AppendLine($"            topTracks: {SerializeTopTracks(stats.TopTracks.Take(5))},");
            html.AppendLine($"            topAlbums: {SerializeTopAlbumsWithTracks(stats.TopAlbums.Take(5), playHistory, year)},");
            html.AppendLine($"            longestStreak: {stats.LongestListeningStreak},");
            html.AppendLine($"            mostActiveHour: {stats.MostActiveHour},");
            html.AppendLine($"            dailyMinutes: {SerializeDailyMinutes(stats.DailyListeningMinutes)},");
            html.AppendLine($"            dailyPlays: {SerializeDailyPlays(stats.DailyPlayCounts)},");
            html.AppendLine($"            monthlyHours: {SerializeMonthlyHours(stats.MonthlyListeningHours)},");
            html.AppendLine($"            weekendRatio: {stats.WeekendVsWeekdayRatio.ToString("F2", CultureInfo.InvariantCulture)},");
            html.AppendLine($"            firstPlay: {SerializePlayData(firstPlay)},");
            html.AppendLine($"            lastPlay: {SerializePlayData(lastPlay)}");
            html.AppendLine("        };");
            html.AppendLine("    </script>");
            
            // Embed JavaScript
            html.AppendLine("    <script>");
            html.AppendLine(GetEmbeddedJavaScript());
            html.AppendLine("    </script>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        /// <summary>
        /// Serializes top artists/tracks list to JSON format
        /// </summary>
        private string SerializeTopList(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, int>> items)
        {
            var jsonItems = items.Select(item => 
                $"{{\"name\":\"{EscapeJsonString(item.Key)}\",\"count\":{item.Value}}}");
            return $"[{string.Join(",", jsonItems)}]";
        }

        /// <summary>
        /// Serializes top tracks with additional formatting
        /// </summary>
        private string SerializeTopTracks(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, int>> tracks)
        {
            var jsonItems = tracks.Select(track => 
            {
                // Try to split track into title and artist if format is "Artist - Title"
                var parts = track.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    return $"{{\"title\":\"{EscapeJsonString(parts[1])}\",\"artist\":\"{EscapeJsonString(parts[0])}\",\"count\":{track.Value}}}";
                }
                else
                {
                    return $"{{\"title\":\"{EscapeJsonString(track.Key)}\",\"artist\":\"Unknown\",\"count\":{track.Value}}}";
                }
            });
            return $"[{string.Join(",", jsonItems)}]";
        }

        /// <summary>
        /// Escapes string for safe JSON embedding
        /// </summary>
        private string EscapeJsonString(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }

        /// <summary>
        /// Serializes daily listening minutes for chart data
        /// </summary>
        private string SerializeDailyMinutes(System.Collections.Generic.Dictionary<string, double> dailyMinutes)
        {
            if (dailyMinutes == null) return "{}";
            
            var jsonItems = dailyMinutes.OrderBy(x => x.Key).Select(item => 
                $"\"{item.Key}\":{item.Value.ToString("F1", CultureInfo.InvariantCulture)}");
            return $"{{{string.Join(",", jsonItems)}}}";
        }

        /// <summary>
        /// Serializes daily play counts for chart data
        /// </summary>
        private string SerializeDailyPlays(System.Collections.Generic.Dictionary<string, int> dailyPlays)
        {
            if (dailyPlays == null) return "{}";
            
            var jsonItems = dailyPlays.OrderBy(x => x.Key).Select(item => 
                $"\"{item.Key}\":{item.Value}");
            return $"{{{string.Join(",", jsonItems)}}}";
        }

        /// <summary>
        /// Serializes monthly listening hours for overview data
        /// </summary>
        private string SerializeMonthlyHours(System.Collections.Generic.Dictionary<string, double> monthlyHours)
        {
            if (monthlyHours == null) return "{}";
            
            var jsonItems = monthlyHours.OrderBy(x => x.Key).Select(item => 
                $"\"{item.Key}\":{item.Value.ToString("F1", CultureInfo.InvariantCulture)}");
            return $"{{{string.Join(",", jsonItems)}}}";
        }

        /// <summary>
        /// Serializes TrackPlay data for JSON embedding
        /// </summary>
        private string SerializePlayData(TrackPlay play)
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

        /// <summary>
        /// Returns embedded CSS for the complete UI styling
        /// </summary>
        private string GetEmbeddedCSS()
        {
            return @"
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0f0f23 0%, #1a1a2e 50%, #16213e 100%);
            color: white;
            overflow: hidden;
            height: 100vh;
        }

        #app {
            position: relative;
            width: 100%;
            height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .slide {
            position: absolute;
            width: 100%;
            height: 100%;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            opacity: 0;
            transform: translateX(100%);
            transition: all 0.6s cubic-bezier(0.4, 0, 0.2, 1);
            padding: 2rem;
            text-align: center;
        }

        .slide.active {
            opacity: 1;
            transform: translateX(0);
        }

        .slide.prev {
            transform: translateX(-100%);
        }

        .loading-content h1 {
            font-size: 3rem;
            margin-bottom: 2rem;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1, #96ceb4);
            background-size: 300% 300%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            animation: gradientShift 3s ease-in-out infinite;
        }

        .loading-spinner {
            width: 60px;
            height: 60px;
            border: 3px solid rgba(255, 255, 255, 0.1);
            border-top: 3px solid #4ecdc4;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto;
        }

        #nav-controls {
            position: fixed;
            bottom: 2rem;
            left: 50%;
            transform: translateX(-50%);
            display: flex;
            align-items: center;
            gap: 1rem;
            z-index: 1000;
        }

        .nav-btn {
            width: 50px;
            height: 50px;
            border: none;
            background: rgba(255, 255, 255, 0.1);
            color: white;
            border-radius: 50%;
            font-size: 1.5rem;
            cursor: pointer;
            transition: all 0.3s ease;
            backdrop-filter: blur(10px);
        }

        .nav-btn:hover:not(:disabled) {
            background: rgba(255, 255, 255, 0.2);
            transform: scale(1.1);
        }

        .nav-btn:disabled {
            opacity: 0.3;
            cursor: not-allowed;
        }

        #slide-indicator {
            display: flex;
            gap: 0.5rem;
        }

        .indicator-dot {
            width: 10px;
            height: 10px;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.3);
            transition: all 0.3s ease;
            cursor: pointer;
        }

        .indicator-dot:hover {
            background: rgba(255, 255, 255, 0.5);
        }

        .indicator-dot.active {
            background: #4ecdc4;
            transform: scale(1.2);
        }

        /* Enhanced Statistics Styling */
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 3rem;
            margin-top: 3rem;
            max-width: 1000px;
            width: 100%;
        }

        .stat-card {
            background: rgba(255, 255, 255, 0.05);
            border-radius: 20px;
            padding: 2rem;
            border: 1px solid rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
            transition: all 0.3s ease;
            transform: translateY(20px);
            opacity: 0;
        }

        .stat-card.animate {
            transform: translateY(0);
            opacity: 1;
        }

        .stat-card:hover {
            transform: translateY(-5px);
            border-color: rgba(78, 205, 196, 0.3);
            box-shadow: 0 10px 30px rgba(78, 205, 196, 0.1);
        }

        .stat-number {
            font-size: 3.5rem;
            font-weight: 700;
            color: #4ecdc4;
            margin: 0.5rem 0;
            counter-reset: stat-counter 0;
        }

        .stat-number.animate {
            animation: countUp 2s ease-out;
        }

        .stat-label {
            font-size: 1.1rem;
            color: rgba(255, 255, 255, 0.8);
            text-transform: uppercase;
            letter-spacing: 2px;
            font-weight: 500;
        }

        .slide h1 {
            font-size: 3.5rem;
            margin-bottom: 1rem;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            animation: fadeInUp 1s ease-out;
        }

        .slide h2 {
            font-size: 2.5rem;
            margin-bottom: 2rem;
            color: #ff6b6b;
            animation: fadeInUp 1s ease-out 0.2s both;
        }

        .slide p {
            font-size: 1.3rem;
            opacity: 0.8;
            max-width: 600px;
            line-height: 1.6;
            animation: fadeInUp 1s ease-out 0.4s both;
        }

        /* Top Lists Styling */
        .top-list {
            max-width: 700px;
            width: 100%;
            margin-top: 3rem;
        }

        .top-item {
            display: flex;
            align-items: center;
            padding: 1.5rem;
            margin: 1rem 0;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 15px;
            border-left: 4px solid #4ecdc4;
            transition: all 0.3s ease;
            transform: translateX(-50px);
            opacity: 0;
        }

        .top-item.animate {
            transform: translateX(0);
            opacity: 1;
        }

        .top-item:hover {
            background: rgba(255, 255, 255, 0.1);
            transform: translateX(10px);
        }

        .top-rank {
            font-size: 2rem;
            font-weight: bold;
            color: #4ecdc4;
            width: 4rem;
            text-align: center;
        }

        .top-content {
            flex: 1;
            text-align: left;
            margin-left: 1.5rem;
        }

        .top-name {
            font-size: 1.4rem;
            font-weight: 600;
            margin-bottom: 0.3rem;
        }

        .top-subtitle {
            font-size: 1rem;
            color: rgba(255, 255, 255, 0.6);
        }

        .top-count {
            font-size: 1.2rem;
            color: rgba(255, 255, 255, 0.7);
            font-weight: 500;
        }

        /* Pattern Visualizations */
        .pattern-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 2rem;
            margin-top: 3rem;
            max-width: 800px;
            width: 100%;
        }

        .pattern-card {
            background: rgba(255, 255, 255, 0.05);
            border-radius: 15px;
            padding: 2rem 1.5rem;
            text-align: center;
            border: 1px solid rgba(255, 255, 255, 0.1);
            transition: all 0.3s ease;
            opacity: 0;
            transform: translateY(30px) scale(0.9);
            backdrop-filter: blur(10px);
        }

        .pattern-card.animate {
            opacity: 1;
            transform: translateY(0) scale(1);
        }

        .pattern-card:hover {
            background: rgba(255, 255, 255, 0.1);
            transform: translateY(-5px) scale(1.05);
            box-shadow: 0 15px 40px rgba(78, 205, 196, 0.15);
        }

        .pattern-icon {
            font-size: 2.5rem;
            margin-bottom: 1rem;
        }

        .pattern-value {
            font-size: 2.5rem;
            font-weight: 700;
            color: #45b7d1;
            margin: 0.5rem 0;
            animation: pulse 2s ease-in-out infinite;
        }

        .pattern-value:hover {
            animation-play-state: paused;
            color: #4ecdc4;
            transform: scale(1.1);
        }

        .pattern-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        /* Progress Bars */
        .progress-bar {
            width: 100%;
            height: 8px;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 4px;
            overflow: hidden;
            margin: 1rem 0;
        }

        .progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #4ecdc4, #45b7d1);
            border-radius: 4px;
            width: 0%;
            transition: width 2s ease-out;
        }

        .progress-fill.animate {
            width: var(--progress-width);
        }

        /* Music Notes Animation */
        .music-notes {
            position: absolute;
            width: 100%;
            height: 100%;
            pointer-events: none;
            overflow: hidden;
        }

        .music-note {
            position: absolute;
            font-size: 2rem;
            color: rgba(78, 205, 196, 0.3);
            animation: floatNote 8s infinite linear;
        }

        /* Animations */
        @keyframes gradientShift {
            0%, 100% { background-position: 0% 50%; }
            50% { background-position: 100% 50%; }
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        @keyframes countUp {
            from { transform: scale(0.8); opacity: 0; }
            to { transform: scale(1); opacity: 1; }
        }

        @keyframes pulse {
            0%, 100% { 
                transform: scale(1); 
                opacity: 1; 
            }
            50% { 
                transform: scale(1.05); 
                opacity: 0.8; 
            }
        }

        @keyframes slideInRight {
            from {
                opacity: 0;
                transform: translateX(100%);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }

        @keyframes slideInLeft {
            from {
                opacity: 0;
                transform: translateX(-100%);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }

        .slide.entering {
            animation: slideInRight 0.6s cubic-bezier(0.4, 0, 0.2, 1) forwards;
        }

        .slide.entering.prev {
            animation: slideInLeft 0.6s cubic-bezier(0.4, 0, 0.2, 1) forwards;
        }

        @keyframes floatNote {
            0% {
                transform: translateY(100vh) rotate(0deg);
                opacity: 0;
            }
            10% {
                opacity: 1;
            }
            90% {
                opacity: 1;
            }
            100% {
                transform: translateY(-100px) rotate(360deg);
                opacity: 0;
            }
        }

        /* Chart Styles */
        .chart-container {
            width: 100%;
            max-width: 1200px;
            height: 500px;
            margin: 2rem auto;
            position: relative;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 15px;
            padding: 1rem;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
        }

        .chart-container canvas {
            width: 100%;
            height: 100%;
            border-radius: 10px;
        }

        .chart-stats {
            display: flex;
            gap: 3rem;
            justify-content: center;
            margin-top: 2rem;
        }

        .chart-stat {
            text-align: center;
        }

        .chart-stat .stat-number {
            font-size: 2.5rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 0.5rem;
        }

        .chart-stat .stat-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        @media (max-width: 768px) {
            .slide h1 { font-size: 2.5rem; }
            .slide h2 { font-size: 2rem; }
            .stats-grid { grid-template-columns: 1fr; gap: 2rem; }
            .stat-number { font-size: 2.5rem; }
            .top-item { padding: 1rem; }
            .top-rank { font-size: 1.5rem; }
            .top-name { font-size: 1.2rem; }
            .chart-container { height: 300px; }
            .chart-stats { gap: 2rem; }
        }

        @media (max-width: 480px) {
            .slide { padding: 1rem; }
            .slide h1 { font-size: 2rem; }
            .stats-grid { gap: 1.5rem; }
            .stat-card { padding: 1.5rem; }
            .chart-container { height: 250px; padding: 0.5rem; }
            .chart-stats { flex-direction: column; gap: 1rem; }
        }

        /* Song Soulmate Slide Styles */
        .soulmate-container {
            text-align: center;
            position: relative;
            overflow: hidden;
        }

        .soulmate-title {
            font-size: 3rem;
            font-weight: 700;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1);
            background-size: 300% 300%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            animation: gradientShift 3s ease-in-out infinite;
            margin-bottom: 1rem;
        }

        .soulmate-subtitle {
            font-size: 1.3rem;
            color: rgba(255, 255, 255, 0.8);
            margin-bottom: 3rem;
            font-style: italic;
        }

        .soulmate-card {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 4rem;
            max-width: 1000px;
            margin: 0 auto 3rem;
            padding: 2rem;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 20px;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.2);
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
        }

        .soulmate-vinyl {
            flex-shrink: 0;
        }

        .vinyl-disc {
            width: 200px;
            height: 200px;
            background: radial-gradient(circle, #1a1a1a 20%, #333 21%, #1a1a1a 22%, #333 40%, #1a1a1a 41%);
            border-radius: 50%;
            position: relative;
            animation: vinylSpin 8s linear infinite;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
        }

        .vinyl-center {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 60px;
            height: 60px;
            background: linear-gradient(45deg, #4ecdc4, #45b7d1);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5rem;
            color: white;
            font-weight: bold;
        }

        .vinyl-groove {
            position: absolute;
            border-radius: 50%;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .vinyl-groove:nth-child(2) {
            top: 30px; left: 30px; right: 30px; bottom: 30px;
        }

        .vinyl-groove:nth-child(3) {
            top: 50px; left: 50px; right: 50px; bottom: 50px;
        }

        .vinyl-groove:nth-child(4) {
            top: 70px; left: 70px; right: 70px; bottom: 70px;
        }

        .soulmate-info {
            flex: 1;
            text-align: left;
        }

        .soulmate-track {
            font-size: 2.5rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 0.5rem;
            line-height: 1.2;
        }

        .soulmate-artist {
            font-size: 1.5rem;
            color: rgba(255, 255, 255, 0.8);
            margin-bottom: 2rem;
            font-style: italic;
        }

        .soulmate-stat-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 2rem;
        }

        .soulmate-stat {
            text-align: center;
            padding: 1rem;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 10px;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .soulmate-stat .stat-number {
            font-size: 2rem;
            font-weight: 700;
            color: #ff6b6b;
            display: block;
            margin-bottom: 0.5rem;
        }

        .soulmate-stat .stat-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .soulmate-message {
            max-width: 600px;
            margin: 0 auto;
            font-size: 1.2rem;
            line-height: 1.6;
            color: rgba(255, 255, 255, 0.9);
        }

        .soulmate-message p {
            margin-bottom: 1rem;
        }

        .soulmate-empty {
            padding: 4rem 2rem;
            text-align: center;
        }

        .empty-vinyl {
            font-size: 8rem;
            color: rgba(255, 255, 255, 0.3);
            margin-bottom: 2rem;
            animation: pulse 2s ease-in-out infinite;
        }

        .floating-hearts {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            overflow: hidden;
        }

        .floating-hearts::before,
        .floating-hearts::after {
            content: '💕';
            position: absolute;
            font-size: 2rem;
            animation: floatHeart 6s infinite linear;
            opacity: 0.6;
        }

        .floating-hearts::before {
            left: 10%;
            animation-delay: 0s;
        }

        .floating-hearts::after {
            right: 10%;
            animation-delay: 3s;
        }

        @keyframes vinylSpin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        @keyframes floatHeart {
            0% {
                transform: translateY(100vh) rotate(0deg);
                opacity: 0;
            }
            10% {
                opacity: 0.6;
            }
            90% {
                opacity: 0.6;
            }
            100% {
                transform: translateY(-100px) rotate(360deg);
                opacity: 0;
            }
        }

        /* Professional Musical Journey Horizontal Timeline Styles */
        .journey-slide-container {
            width: 100%;
            max-width: 1200px;
            margin: 0 auto;
            padding: 1rem;
            height: 100%;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }

        .journey-header {
            text-align: center;
            margin-bottom: 3rem;
        }

        .journey-header .journey-title {
            font-size: 2.5rem;
            font-weight: 700;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1, #96ceb4);
            background-size: 300% 300%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            animation: gradientShift 4s ease-in-out infinite;
            margin-bottom: 0.5rem;
        }

        .journey-subtitle {
            font-size: 1.1rem;
            color: rgba(255, 255, 255, 0.8);
            font-style: italic;
            margin: 0;
        }

        .journey-timeline-horizontal {
            display: flex;
            align-items: center;
            justify-content: space-between;
            width: 100%;
            margin: 2rem 0;
            position: relative;
            padding: 2rem 0;
        }

        .timeline-milestone {
            display: flex;
            flex-direction: column;
            align-items: center;
            position: relative;
            flex: 0 0 auto;
            z-index: 2;
        }

        .milestone-marker {
            width: 80px;
            height: 80px;
            border-radius: 50%;
            background: linear-gradient(135deg, rgba(255, 255, 255, 0.15), rgba(255, 255, 255, 0.05));
            border: 3px solid rgba(78, 205, 196, 0.4);
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 1rem;
            transition: all 0.4s ease;
            backdrop-filter: blur(10px);
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
        }

        .milestone-marker:hover {
            transform: translateY(-5px);
            border-color: rgba(78, 205, 196, 0.8);
            box-shadow: 0 15px 40px rgba(0, 0, 0, 0.4);
        }

        .peak-marker {
            border-color: rgba(255, 107, 107, 0.4);
            background: linear-gradient(135deg, rgba(255, 107, 107, 0.15), rgba(255, 107, 107, 0.05));
        }

        .peak-marker:hover {
            border-color: rgba(255, 107, 107, 0.8);
        }

        .milestone-icon {
            font-size: 2rem;
            filter: drop-shadow(0 0 10px rgba(255, 255, 255, 0.3));
        }

        .milestone-card {
            background: rgba(255, 255, 255, 0.08);
            border-radius: 12px;
            padding: 1.2rem;
            min-width: 180px;
            max-width: 220px;
            border: 1px solid rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(15px);
            text-align: center;
            transition: all 0.3s ease;
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.2);
        }

        .milestone-card:hover {
            background: rgba(255, 255, 255, 0.12);
            transform: translateY(-3px);
            box-shadow: 0 12px 35px rgba(0, 0, 0, 0.3);
        }

        .peak-card {
            border-color: rgba(255, 107, 107, 0.2);
            background: rgba(255, 107, 107, 0.08);
        }

        .peak-card:hover {
            background: rgba(255, 107, 107, 0.12);
        }

        .milestone-date {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 0.5rem;
        }

        .milestone-card h4 {
            font-size: 1.1rem;
            color: #4ecdc4;
            font-weight: 600;
            margin-bottom: 0.8rem;
        }

        .peak-card h4 {
            color: #ff6b6b;
        }

        .milestone-song {
            margin-top: 0.5rem;
        }

        .song-name {
            font-size: 1rem;
            font-weight: 600;
            color: white;
            margin-bottom: 0.3rem;
            line-height: 1.2;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
        }

        .song-artist {
            font-size: 0.85rem;
            color: rgba(255, 255, 255, 0.7);
            font-style: italic;
        }

        .peak-info {
            margin-top: 0.5rem;
        }

        .peak-value {
            font-size: 1.3rem;
            font-weight: 700;
            color: #ff6b6b;
            margin-bottom: 0.3rem;
        }

        .peak-desc {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.7);
        }

        .timeline-connector {
            flex: 1;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            margin: 0 1rem;
        }

        .connector-line {
            width: 100%;
            height: 3px;
            background: linear-gradient(90deg, rgba(78, 205, 196, 0.3), rgba(69, 183, 209, 0.3));
            border-radius: 2px;
            position: relative;
            overflow: hidden;
        }

        .connector-line::before {
            content: '';
            position: absolute;
            top: 0;
            left: -100%;
            width: 100%;
            height: 100%;
            background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.4), transparent);
            animation: shimmer 3s infinite;
        }

        .journey-stats-compact {
            position: absolute;
            top: -60px;
            left: 50%;
            transform: translateX(-50%);
            display: flex;
            gap: 1rem;
            background: rgba(0, 0, 0, 0.8);
            padding: 0.8rem 1.2rem;
            border-radius: 20px;
            border: 1px solid rgba(255, 255, 255, 0.2);
            backdrop-filter: blur(15px);
            white-space: nowrap;
        }

        .stat-bubble {
            text-align: center;
        }

        .stat-num {
            display: block;
            font-size: 1rem;
            font-weight: 700;
            color: #4ecdc4;
            line-height: 1;
        }

        .stat-lbl {
            font-size: 0.7rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .journey-flow-indicator {
            position: absolute;
            top: -35px;
            left: 50%;
            transform: translateX(-50%);
            background: rgba(150, 206, 180, 0.2);
            padding: 0.4rem 1rem;
            border-radius: 15px;
            border: 1px solid rgba(150, 206, 180, 0.4);
            backdrop-filter: blur(10px);
        }

        .flow-text {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.8);
            font-style: italic;
        }

        .journey-insights {
            margin-top: 2rem;
            display: flex;
            justify-content: center;
        }

        .insight-card {
            background: rgba(255, 255, 255, 0.08);
            border-radius: 15px;
            padding: 1.2rem 2rem;
            border: 1px solid rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(15px);
            display: flex;
            align-items: center;
            gap: 1rem;
            max-width: 600px;
            transition: all 0.3s ease;
        }

        .insight-card:hover {
            background: rgba(255, 255, 255, 0.12);
            transform: translateY(-2px);
        }

        .insight-icon {
            font-size: 1.5rem;
            flex-shrink: 0;
        }

        .insight-message {
            color: rgba(255, 255, 255, 0.9);
            line-height: 1.5;
            font-size: 1rem;
        }

        /* Animations */
        @keyframes shimmer {
            0% { left: -100%; }
            100% { left: 100%; }
        }

        /* Responsive Design for Musical Journey */
        @media (max-width: 768px) {
            .journey-timeline-horizontal {
                flex-direction: column;
                gap: 2rem;
                padding: 1rem 0;
            }

            .timeline-connector {
                flex: none;
                width: 80px;
                height: 60px;
                margin: 0;
                transform: rotate(90deg);
            }

            .connector-line {
                width: 60px;
                height: 3px;
            }

            .journey-stats-compact {
                position: static;
                transform: none;
                flex-direction: column;
                gap: 0.5rem;
                margin: 1rem 0;
            }

            .journey-flow-indicator {
                position: static;
                transform: none;
                margin: 1rem 0;
            }

            .journey-header .journey-title {
                font-size: 2rem;
            }

            .milestone-marker {
                width: 60px;
                height: 60px;
            }

            .milestone-icon {
                font-size: 1.5rem;
            }

            .milestone-card {
                min-width: 160px;
                max-width: 200px;
                padding: 1rem;
            }
        }

        @media (max-width: 480px) {
            .journey-slide-container {
                padding: 0.5rem;
            }

            .journey-header .journey-title {
                font-size: 1.8rem;
            }

            .journey-subtitle {
                font-size: 1rem;
            }

            .milestone-card {
                min-width: 140px;
                max-width: 180px;
                padding: 0.8rem;
            }

            .milestone-card h4 {
                font-size: 1rem;
            }

            .song-name {
                font-size: 0.9rem;
            }

            .insight-card {
                padding: 1rem;
                margin: 0 1rem;
            }

            .insight-message {
                font-size: 0.9rem;
            }
        }

        /* Legacy Musical Journey Flow Styles */
        .journey-container {
            text-align: center;
            position: relative;
            padding: 2rem 0;
            max-width: 1200px;
            margin: 0 auto;
        }

        .journey-title {
            font-size: 3rem;
            font-weight: 700;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1, #96ceb4);
            background-size: 300% 300%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            animation: gradientShift 4s ease-in-out infinite;
            margin-bottom: 1rem;
        }

        .journey-subtitle {
            font-size: 1.3rem;
            color: rgba(255, 255, 255, 0.8);
            margin-bottom: 4rem;
            font-style: italic;
        }

        .journey-flow {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 3rem;
            width: 100%;
        }

        .journey-milestone {
            width: 100%;
            max-width: 800px;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 20px;
            border: 1px solid rgba(255, 255, 255, 0.2);
            backdrop-filter: blur(15px);
            padding: 2.5rem;
            transition: all 0.4s ease;
            animation: slideInFromSide 0.8s ease-out;
        }

        .journey-milestone:hover {
            background: rgba(255, 255, 255, 0.15);
            transform: translateY(-8px);
            box-shadow: 0 20px 50px rgba(0, 0, 0, 0.3);
            border-color: rgba(78, 205, 196, 0.4);
        }

        .first-song {
            animation-delay: 0.2s;
        }

        .peak-period {
            animation-delay: 0.6s;
        }

        .current-obsession {
            animation-delay: 1s;
        }

        .milestone-header {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 1.5rem;
            margin-bottom: 2rem;
        }

        .milestone-icon {
            font-size: 3rem;
            width: 80px;
            height: 80px;
            border-radius: 50%;
            background: linear-gradient(135deg, rgba(255, 255, 255, 0.2), rgba(255, 255, 255, 0.1));
            border: 3px solid rgba(255, 255, 255, 0.3);
            display: flex;
            align-items: center;
            justify-content: center;
            animation: pulse 3s ease-in-out infinite;
        }

        .milestone-date {
            font-size: 1.1rem;
            color: rgba(255, 255, 255, 0.8);
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .milestone-content h3 {
            font-size: 2rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 1.5rem;
        }

        .song-info {
            background: rgba(78, 205, 196, 0.15);
            border-radius: 15px;
            padding: 1.5rem;
            margin: 1.5rem 0;
            border-left: 4px solid #4ecdc4;
        }

        .song-title {
            font-size: 1.4rem;
            font-weight: 600;
            color: white;
            margin-bottom: 0.5rem;
        }

        .song-artist {
            font-size: 1.1rem;
            color: rgba(255, 255, 255, 0.7);
            font-style: italic;
        }

        .milestone-description {
            font-size: 1rem;
            color: rgba(255, 255, 255, 0.8);
            line-height: 1.6;
            margin: 0;
        }

        .peak-stats {
            text-align: center;
            margin: 1rem 0;
        }

        .peak-number {
            font-size: 2.5rem;
            font-weight: 700;
            color: #ff6b6b;
            display: block;
            margin-bottom: 0.5rem;
        }

        .peak-description {
            font-size: 1rem;
            color: rgba(255, 255, 255, 0.8);
        }

        .journey-flow-line {
            width: 4px;
            height: 60px;
            background: linear-gradient(180deg, #4ecdc4, #45b7d1);
            border-radius: 2px;
            position: relative;
            animation: flowPulse 2s ease-in-out infinite;
        }

        .flow-stats {
            position: absolute;
            left: 50%;
            top: 50%;
            transform: translate(-50%, -50%);
            display: flex;
            gap: 2rem;
            background: rgba(0, 0, 0, 0.7);
            padding: 1rem 2rem;
            border-radius: 25px;
            border: 1px solid rgba(255, 255, 255, 0.2);
            backdrop-filter: blur(10px);
            white-space: nowrap;
        }

        .flow-stat {
            text-align: center;
        }

        .flow-stat .stat-number {
            font-size: 1.2rem;
            font-weight: 700;
            color: #4ecdc4;
            display: block;
        }

        .flow-stat .stat-label {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .discovery-highlight {
            position: absolute;
            left: 50%;
            top: 50%;
            transform: translate(-50%, -50%);
            background: rgba(150, 206, 180, 0.2);
            padding: 0.5rem 1.5rem;
            border-radius: 20px;
            border: 1px solid rgba(150, 206, 180, 0.4);
            backdrop-filter: blur(10px);
        }

        .discovery-text {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.8);
            font-style: italic;
        }

        .journey-summary {
            margin-top: 3rem;
            max-width: 600px;
            margin-left: auto;
            margin-right: auto;
        }

        .summary-insight {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 1rem;
            padding: 1.5rem;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 15px;
            border: 1px solid rgba(255, 255, 255, 0.2);
            backdrop-filter: blur(10px);
        }

        .insight-icon {
            font-size: 1.5rem;
        }

        .insight-text {
            color: rgba(255, 255, 255, 0.9);
            line-height: 1.5;
            text-align: center;
            font-size: 1rem;
        }

        /* Animations */
        @keyframes slideInFromSide {
            from {
                opacity: 0;
                transform: translateX(-50px);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }

        @keyframes flowPulse {
            0%, 100% {
                opacity: 0.7;
                transform: scaleY(1);
            }
            50% {
                opacity: 1;
                transform: scaleY(1.1);
            }
        }

        /* Responsive design */
        @media (max-width: 768px) {
            .journey-title {
                font-size: 2.5rem;
            }
            
            .journey-milestone {
                margin: 0 1rem;
                padding: 2rem 1.5rem;
            }
            
            .milestone-header {
                flex-direction: column;
                gap: 1rem;
            }
            
            .milestone-icon {
                font-size: 2.5rem;
                width: 60px;
                height: 60px;
            }
            
            .milestone-content h3 {
                font-size: 1.5rem;
            }
            
            .song-title {
                font-size: 1.2rem;
            }
            
            .flow-stats {
                flex-direction: column;
                gap: 0.5rem;
                padding: 1rem;
            }
        }

        @media (max-width: 480px) {
            .journey-container {
                padding: 1rem 0;
            }
            
            .journey-title {
                font-size: 2rem;
            }
            
            .journey-subtitle {
                font-size: 1.1rem;
                margin-bottom: 2rem;
            }
            
            .journey-milestone {
                margin: 0 0.5rem;
                padding: 1.5rem 1rem;
            }
            
            .song-info {
                padding: 1rem;
            }
        }

        /* Favorite Artist Slide Styles */
        .artist-showcase {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 3rem;
            max-width: 600px;
            margin: 0 auto;
        }

        .artist-card {
            background: linear-gradient(145deg, rgba(255, 255, 255, 0.1), rgba(255, 255, 255, 0.05));
            border-radius: 20px;
            padding: 3rem 2rem;
            text-align: center;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
            transition: transform 0.3s ease, box-shadow 0.3s ease;
            min-width: 300px;
        }

        .artist-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 25px 50px rgba(0, 0, 0, 0.4);
        }

        .artist-icon {
            font-size: 4rem;
            margin-bottom: 1rem;
            filter: drop-shadow(0 0 20px rgba(255, 215, 0, 0.5));
        }

        .artist-name {
            font-size: 2.5rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 0.5rem;
            text-shadow: 0 0 20px rgba(78, 205, 196, 0.3);
        }

        .artist-plays {
            font-size: 1.3rem;
            opacity: 0.8;
            font-weight: 500;
        }

        .artist-stats {
            display: flex;
            gap: 3rem;
            justify-content: center;
        }

        .stat-item {
            text-align: center;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 15px;
            padding: 2rem 1.5rem;
            backdrop-filter: blur(5px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            min-width: 120px;
            transition: transform 0.3s ease;
        }

        .stat-item:hover {
            transform: scale(1.05);
        }

        .stat-value {
            font-size: 2.5rem;
            font-weight: 700;
            color: #ff6b6b;
            margin-bottom: 0.5rem;
            text-shadow: 0 0 15px rgba(255, 107, 107, 0.3);
        }

        .stat-label {
            font-size: 1rem;
            opacity: 0.7;
            text-transform: uppercase;
            letter-spacing: 1px;
            font-weight: 500;
        }

        @media (max-width: 768px) {
            .artist-showcase {
                gap: 2rem;
            }

            .artist-card {
                padding: 2rem 1.5rem;
                min-width: 250px;
            }

            .artist-icon {
                font-size: 3rem;
            }

            .artist-name {
                font-size: 2rem;
            }

            .artist-plays {
                font-size: 1.1rem;
            }

            .artist-stats {
                gap: 1.5rem;
            }

            .stat-item {
                padding: 1.5rem 1rem;
                min-width: 100px;
            }

            .stat-value {
                font-size: 2rem;
            }

            .stat-label {
                font-size: 0.9rem;
            }
        }

        @media (max-width: 480px) {
            .artist-stats {
                flex-direction: column;
                gap: 1rem;
            }

            .stat-item {
                min-width: auto;
                width: 100%;
                max-width: 200px;
            }
        }

        /* Professional Favorite Album Slide Styles */
        .album-slide-container {
            width: 100%;
            max-width: 1100px;
            margin: 0 auto;
            padding: 1rem;
            height: 100%;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }

        .slide-header {
            text-align: center;
            margin-bottom: 2rem;
        }

        .slide-header h2 {
            font-size: 2.5rem;
            margin-bottom: 0.5rem;
            color: #ff6b6b;
        }

        .slide-subtitle {
            font-size: 1.1rem;
            opacity: 0.8;
            margin: 0;
        }

        .album-main-content {
            display: flex;
            gap: 2rem;
            flex: 1;
            align-items: flex-start;
        }

        .featured-album {
            flex: 1;
            max-width: 500px;
        }

        .album-hero {
            display: flex;
            align-items: center;
            gap: 1.5rem;
            background: rgba(255, 255, 255, 0.08);
            border-radius: 15px;
            padding: 1.5rem;
            margin-bottom: 1.5rem;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .album-icon-large {
            font-size: 3.5rem;
            width: 80px;
            height: 80px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: rgba(78, 205, 196, 0.2);
            border-radius: 50%;
            border: 2px solid rgba(78, 205, 196, 0.4);
            flex-shrink: 0;
        }

        .album-info {
            flex: 1;
            min-width: 0;
        }

        .album-info .album-name {
            font-size: 1.8rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 0.5rem;
            line-height: 1.2;
            word-wrap: break-word;
        }

        .album-meta {
            display: flex;
            flex-direction: column;
            gap: 0.3rem;
        }

        .album-plays {
            font-size: 1.1rem;
            color: rgba(255, 255, 255, 0.9);
            font-weight: 600;
        }

        .album-percentage {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.6);
            background: rgba(255, 255, 255, 0.1);
            padding: 0.2rem 0.6rem;
            border-radius: 10px;
            align-self: flex-start;
        }

        .album-tracks-section {
            background: rgba(255, 255, 255, 0.05);
            border-radius: 12px;
            padding: 1.2rem;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .tracks-title {
            font-size: 1.2rem;
            color: #45b7d1;
            margin-bottom: 1rem;
            font-weight: 600;
        }

        .tracks-list {
            display: flex;
            flex-direction: column;
            gap: 0.6rem;
        }

        .track-item {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 0.8rem;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 8px;
            border-left: 3px solid #45b7d1;
            transition: all 0.3s ease;
        }

        .track-item:hover {
            background: rgba(255, 255, 255, 0.1);
            transform: translateX(5px);
        }

        .track-number {
            font-size: 1rem;
            font-weight: bold;
            color: #45b7d1;
            min-width: 1.5rem;
            text-align: center;
        }

        .track-details {
            flex: 1;
            display: flex;
            justify-content: space-between;
            align-items: center;
            min-width: 0;
        }

        .track-title {
            font-size: 1rem;
            font-weight: 500;
            color: white;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            flex: 1;
            margin-right: 1rem;
        }

        .track-count {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.6);
            font-weight: 500;
            flex-shrink: 0;
        }

        .other-albums-section {
            flex: 1;
            max-width: 500px;
        }

        .section-title {
            font-size: 1.5rem;
            color: #4ecdc4;
            margin-bottom: 1rem;
            font-weight: 600;
        }

        .albums-grid {
            display: flex;
            flex-direction: column;
            gap: 0.8rem;
        }

        .other-album-card {
            background: rgba(255, 255, 255, 0.06);
            border-radius: 10px;
            padding: 1rem;
            border: 1px solid rgba(255, 255, 255, 0.1);
            transition: all 0.3s ease;
            opacity: 0;
            transform: translateY(20px);
        }

        .other-album-card.animate {
            opacity: 1;
            transform: translateY(0);
        }

        .other-album-card:hover {
            background: rgba(255, 255, 255, 0.1);
            transform: translateY(-3px);
        }

        .other-album-header {
            display: flex;
            align-items: center;
            gap: 0.8rem;
            margin-bottom: 0.8rem;
        }

        .other-album-header .album-icon {
            font-size: 1.5rem;
            width: 35px;
            height: 35px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: rgba(78, 205, 196, 0.15);
            border-radius: 50%;
            border: 1px solid rgba(78, 205, 196, 0.3);
            flex-shrink: 0;
        }

        .other-album-info {
            flex: 1;
            min-width: 0;
        }

        .other-album-info .album-name {
            font-size: 1rem;
            font-weight: 600;
            color: #4ecdc4;
            margin-bottom: 0.2rem;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .other-album-info .album-plays {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.6);
        }

        .other-album-tracks {
            display: flex;
            flex-direction: column;
            gap: 0.4rem;
        }

        .other-album-tracks .track-item {
            padding: 0.5rem 0.6rem;
            font-size: 0.85rem;
        }

        .other-album-tracks .track-number {
            font-size: 0.8rem;
            min-width: 1.2rem;
        }

        .other-album-tracks .track-title {
            font-size: 0.85rem;
        }

        .other-album-tracks .track-count {
            font-size: 0.75rem;
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .album-main-content {
                flex-direction: column;
                gap: 1.5rem;
            }

            .slide-header h2 {
                font-size: 2rem;
            }

            .album-hero {
                flex-direction: column;
                text-align: center;
                gap: 1rem;
            }

            .album-info .album-name {
                font-size: 1.5rem;
            }

            .featured-album,
            .other-albums-section {
                max-width: none;
            }

            .track-details {
                flex-direction: column;
                align-items: flex-start;
                gap: 0.2rem;
            }

            .track-title {
                margin-right: 0;
            }
        }

        @media (max-width: 480px) {
            .album-slide-container {
                padding: 0.5rem;
            }

            .slide-header h2 {
                font-size: 1.8rem;
            }

            .album-icon-large {
                font-size: 2.5rem;
                width: 60px;
                height: 60px;
            }

            .album-info .album-name {
                font-size: 1.3rem;
            }

            .album-hero {
                padding: 1rem;
            }

            .album-tracks-section {
                padding: 1rem;
            }

            .track-item {
                padding: 0.6rem;
                gap: 0.8rem;
            }
        }

        /* Legacy Album Slide Styles */
        .album-showcase {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 3rem;
            max-width: 600px;
            margin: 0 auto;
        }

        .album-card {
            background: linear-gradient(145deg, rgba(255, 255, 255, 0.1), rgba(255, 255, 255, 0.05));
            border-radius: 20px;
            padding: 3rem 2rem;
            text-align: center;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
            transition: transform 0.3s ease, box-shadow 0.3s ease;
            min-width: 300px;
        }

        .album-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 25px 50px rgba(0, 0, 0, 0.4);
        }

        .album-icon {
            font-size: 4rem;
            margin-bottom: 1rem;
            filter: drop-shadow(0 0 20px rgba(255, 215, 0, 0.5));
        }

        .album-name {
            font-size: 2.5rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 0.5rem;
            text-shadow: 0 0 20px rgba(78, 205, 196, 0.3);
        }

        .album-plays {
            font-size: 1.3rem;
            opacity: 0.8;
            font-weight: 500;
        }

        .album-stats {
            display: flex;
            gap: 3rem;
            justify-content: center;
        }

        .top-albums-list {
            max-width: 600px;
            width: 100%;
            margin: 0 auto;
        }

        .album-item {
            display: flex;
            align-items: center;
            padding: 1.5rem;
            margin: 1rem 0;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 15px;
            border-left: 4px solid #4ecdc4;
            transition: all 0.3s ease;
            transform: translateX(-50px);
            opacity: 0;
        }

        .album-item.animate {
            transform: translateX(0);
            opacity: 1;
        }

        .album-item:hover {
            background: rgba(255, 255, 255, 0.1);
            transform: translateX(10px);
        }

        .album-rank {
            font-size: 2rem;
            font-weight: bold;
            color: #4ecdc4;
            width: 4rem;
            text-align: center;
        }

        .album-content {
            flex: 1;
            text-align: left;
            margin-left: 1.5rem;
        }

        .album-title {
            font-size: 1.4rem;
            font-weight: 600;
            margin-bottom: 0.3rem;
        }

        .album-count {
            font-size: 1.2rem;
            color: rgba(255, 255, 255, 0.7);
            font-weight: 500;
        }

        .top-album-tracks {
            margin-top: 1.5rem;
            padding-top: 1.5rem;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
        }

        .album-track-item {
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 0.75rem;
            margin: 0.5rem 0;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 10px;
            border-left: 3px solid #45b7d1;
            opacity: 0;
            transform: translateY(20px);
            transition: all 0.4s ease-out;
        }

        .track-rank {
            font-size: 1rem;
            font-weight: bold;
            color: #45b7d1;
            min-width: 2rem;
        }

        .track-name {
            flex: 1;
            font-size: 1rem;
            font-weight: 600;
            margin: 0 1rem;
            text-align: left;
        }

        .track-plays {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.6);
            font-weight: 500;
        }

        .album-top-track {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.6);
            font-style: italic;
            margin-top: 0.5rem;
        }

        @media (max-width: 768px) {
            .album-showcase {
                gap: 2rem;
            }

            .album-card {
                padding: 2rem 1.5rem;
                min-width: 250px;
            }

            .album-icon {
                font-size: 3rem;
            }

            .album-name {
                font-size: 2rem;
            }

            .album-plays {
                font-size: 1.1rem;
            }

            .album-stats {
                gap: 1.5rem;
            }
        }

        @media (max-width: 480px) {
            .album-stats {
                flex-direction: column;
                gap: 1rem;
            }
        }
        ";
        }

        /// <summary>
        /// Returns embedded JavaScript for slide navigation and animations
        /// </summary>
        private string GetEmbeddedJavaScript()
        {
            return @"
        class WrappedApp {
            constructor() {
                this.currentSlide = 0;
                this.slides = [];
                this.isAnimating = false;
                
                this.init();
            }

            init() {
                // Wait for DOM to load, then generate slides
                if (document.readyState === 'loading') {
                    document.addEventListener('DOMContentLoaded', () => this.start());
                } else {
                    this.start();
                }
            }

            start() {
                // Generate all slides
                this.generateSlides();
                
                // Setup navigation
                this.setupNavigation();
                
                // Show first slide after brief delay
                setTimeout(() => {
                    this.showSlide(1, 'next'); // Skip loading slide
                }, 2000);
            }

            generateSlides() {
                const app = document.getElementById('app');
                const data = window.WRAPPED_DATA;

                // Slide 1: Welcome with animated background
                const welcomeSlide = this.createSlide('welcome', `
                    <div class='music-notes'></div>
                    <h1>🎵 Your ${data.year} Music Wrapped</h1>
                    <p style='font-size: 1.5rem; margin-top: 2rem; opacity: 0.8;'>
                        Discover your musical journey through ${data.totalTracks.toLocaleString()} tracks
                    </p>
                    <p style='font-size: 1rem; margin-top: 1rem; opacity: 0.6;'>
                        Use arrow keys or click to navigate
                    </p>
                `);

                // Slide 2: Overview with animated cards
                const overviewSlide = this.createSlide('overview', `
                    <h2>📊 Your Year in Numbers</h2>
                    <div class='stats-grid'>
                        <div class='stat-card' data-delay='0'>
                            <div class='stat-number' data-target='${data.totalTracks}'>${data.totalTracks.toLocaleString()}</div>
                            <div class='stat-label'>Tracks Played</div>
                            <div class='progress-bar'>
                                <div class='progress-fill' style='--progress-width: 100%'></div>
                            </div>
                        </div>
                        <div class='stat-card' data-delay='200'>
                            <div class='stat-number' data-target='${Math.round(data.totalHours)}'>${data.totalHours}</div>
                            <div class='stat-label'>Hours Listened</div>
                            <div class='progress-bar'>
                                <div class='progress-fill' style='--progress-width: ${Math.min(data.totalHours / 1000 * 100, 100)}%'></div>
                            </div>
                        </div>
                        <div class='stat-card' data-delay='400'>
                            <div class='stat-number' data-target='${data.totalArtists}'>${data.totalArtists.toLocaleString()}</div>
                            <div class='stat-label'>Unique Artists</div>
                            <div class='progress-bar'>
                                <div class='progress-fill' style='--progress-width: ${Math.min(data.totalArtists / 500 * 100, 100)}%'></div>
                            </div>
                        </div>
                    </div>
                `);

                // Slide 3: Top Artists with enhanced styling
                const topArtistsHTML = data.topArtists.map((artist, index) => `
                    <div class='top-item' data-delay='${index * 100}'>
                        <div class='top-rank'>#${index + 1}</div>
                        <div class='top-content'>
                            <div class='top-name'>${artist.name}</div>
                            <div class='top-subtitle'>Your favorite artist</div>
                        </div>
                        <div class='top-count'>${artist.count} plays</div>
                    </div>
                `).join('');

                const artistsSlide = this.createSlide('artists', `
                    <h2>🎤 Your Top Artists</h2>
                    <p style='margin-bottom: 1rem; opacity: 0.8;'>The artists that defined your ${data.year}</p>
                    <div class='top-list'>
                        ${topArtistsHTML}
                    </div>
                `);

                // Slide 4: Top Tracks
                const topTracksHTML = data.topTracks.map((track, index) => `
                    <div class='top-item' data-delay='${index * 100}'>
                        <div class='top-rank'>#${index + 1}</div>
                        <div class='top-content'>
                            <div class='top-name'>${track.title}</div>
                            <div class='top-subtitle'>by ${track.artist}</div>
                        </div>
                        <div class='top-count'>${track.count} plays</div>
                    </div>
                `).join('');

                const tracksSlide = this.createSlide('tracks', `
                    <h2>🎧 Your Top Tracks</h2>
                    <p style='margin-bottom: 1rem; opacity: 0.8;'>Songs you couldn't stop playing</p>
                    <div class='top-list'>
                        ${topTracksHTML}
                    </div>
                `);

                // Slide 5: Song Soulmate
                const topTrack = data.topTracks && data.topTracks.length > 0 ? data.topTracks[0] : null;
                const soulmatePlays = topTrack ? topTrack.count : 0;
                const soulmatePercentage = data.totalTracks > 0 ? Math.round((soulmatePlays / data.totalTracks) * 100) : 0;
                const averagePlaysPerDay = data.totalTracks > 0 ? Math.round(soulmatePlays / 365 * 10) / 10 : 0;
                
                const songSoulmateSlide = this.createSlide('song-soulmate', `
                    <div class='soulmate-container'>
                        <div class='floating-hearts'></div>
                        <h2 class='soulmate-title'>💕 Your Song Soulmate</h2>
                        <div class='soulmate-subtitle'>The song that understood you the most in ${data.year}</div>
                        
                        ${topTrack ? `
                            <div class='soulmate-card'>
                                <div class='soulmate-vinyl'>
                                    <div class='vinyl-disc'>
                                        <div class='vinyl-center'>♪</div>
                                        <div class='vinyl-groove'></div>
                                        <div class='vinyl-groove'></div>
                                        <div class='vinyl-groove'></div>
                                    </div>
                                </div>
                                
                                <div class='soulmate-info'>
                                    <div class='soulmate-track'>${topTrack.title}</div>
                                    <div class='soulmate-artist'>by ${topTrack.artist}</div>
                                    
                                    <div class='soulmate-stats'>
                                        <div class='soulmate-stat-grid'>
                                            <div class='soulmate-stat'>
                                                <div class='stat-number'>${soulmatePlays}</div>
                                                <div class='stat-label'>Times Played</div>
                                            </div>
                                            <div class='soulmate-stat'>
                                                <div class='stat-number'>${soulmatePercentage}%</div>
                                                <div class='stat-label'>of Total Listening</div>
                                            </div>
                                            <div class='soulmate-stat'>
                                                <div class='stat-number'>${averagePlaysPerDay}</div>
                                                <div class='stat-label'>Plays Per Day</div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            
                            <div class='soulmate-message'>
                                <p>This song was there for you through ${soulmatePlays} moments.</p>
                                <p>Whether you were happy, sad, or somewhere in between - it was your constant companion.</p>
                            </div>
                        ` : `
                            <div class='soulmate-empty'>
                                <div class='empty-vinyl'>♪</div>
                                <p>Your musical journey is just beginning!</p>
                                <p>Keep listening to find your song soulmate.</p>
                            </div>
                        `}
                    </div>
                `);

                // Slide 6: Musical Journey - Time Machine + First/Last Songs
                const monthlyData = data.monthlyHours || {};
                const peakMonth = Object.keys(monthlyData).reduce((a, b) => monthlyData[a] > monthlyData[b] ? a : b, 'January');
                const quietMonth = Object.keys(monthlyData).reduce((a, b) => monthlyData[a] < monthlyData[b] ? a : b, 'January');
                
                // Simple month name converter
                const monthNames = {
                    '01': 'January', '02': 'February', '03': 'March', '04': 'April',
                    '05': 'May', '06': 'June', '07': 'July', '08': 'August',
                    '09': 'September', '10': 'October', '11': 'November', '12': 'December'
                };
                const formatMonth = (monthStr) => {
                    if (monthStr.includes('-')) {
                        const monthNum = monthStr.split('-')[1];
                        return monthNames[monthNum] || monthStr;
                    }
                    return monthStr;
                };
                
                const musicalJourneySlide = this.createSlide('musical-journey', `
                    <div class='journey-slide-container'>
                        <div class='journey-header'>
                            <h2 class='journey-title'>🚀 Your Musical Journey Through ${data.year}</h2>
                            <p class='journey-subtitle'>From your first song to your latest discovery</p>
                        </div>
                        
                        <div class='journey-timeline-horizontal'>
                            ${data.firstPlay ? `
                                <div class='timeline-milestone start-milestone'>
                                    <div class='milestone-marker'>
                                        <div class='milestone-icon'>🎬</div>
                                    </div>
                                    <div class='milestone-card'>
                                        <div class='milestone-date'>${data.firstPlay.month} ${data.firstPlay.day}</div>
                                        <h4>Journey Began</h4>
                                        <div class='milestone-song'>
                                            <div class='song-name'>${data.firstPlay.title}</div>
                                            <div class='song-artist'>by ${data.firstPlay.artist}</div>
                                        </div>
                                    </div>
                                </div>
                            ` : ''}
                            
                            <div class='timeline-connector'>
                                <div class='connector-line'></div>
                                
                            </div>
                            
                            <div class='timeline-milestone peak-milestone'>
                                <div class='milestone-marker peak-marker'>
                                    <div class='milestone-icon'>🔥</div>
                                </div>
                                <div class='milestone-card peak-card'>
                                    <div class='milestone-date'>${formatMonth(peakMonth)} ${data.year}</div>
                                    <h4>Peak Period</h4>
                                    <div class='peak-info'>
                                        <div class='peak-value'>${Math.round(monthlyData[peakMonth] || 0)} hours</div>
                                        <div class='peak-desc'>Most active month</div>
                                    </div>
                                </div>
                            </div>
                            
                            <div class='timeline-connector'>
                                <div class='connector-line'></div>
                            </div>
                            
                            ${data.lastPlay ? `
                                <div class='timeline-milestone end-milestone'>
                                    <div class='milestone-marker'>
                                        <div class='milestone-icon'>🌟</div>
                                    </div>
                                    <div class='milestone-card'>
                                        <div class='milestone-date'>${data.lastPlay.month} ${data.lastPlay.day}</div>
                                        <h4>Latest Discovery</h4>
                                        <div class='milestone-song'>
                                            <div class='song-name'>${data.lastPlay.title}</div>
                                            <div class='song-artist'>by ${data.lastPlay.artist}</div>
                                        </div>
                                    </div>
                                </div>
                            ` : ''}
                        </div>
                        
                        <div class='journey-insights'>
                            <div class='insight-card'>
                                <span class='insight-icon'>🎯</span>
                                <span class='insight-message'>Your musical taste evolved beautifully from ${formatMonth(peakMonth)} peaks to ${formatMonth(quietMonth)} quiet moments</span>
                            </div>
                        </div>
                    </div>
                `);

                // Slide 7: Favorite Artist
                const artistSlide = this.createSlide('favorite-artist', `
                    <h2>🎤 Your Musical Soulmate</h2>
                    <p style='margin-bottom: 2rem; opacity: 0.8;'>Your most-played artist of ${data.year}</p>
                    <div class='artist-showcase'>
                        <div class='artist-card'>
                            <div class='artist-icon'>🌟</div>
                            <div class='artist-name'>${data.topArtists && data.topArtists[0] ? data.topArtists[0].name : 'Unknown Artist'}</div>
                            <div class='artist-plays'>${data.topArtists && data.topArtists[0] ? data.topArtists[0].count : 0} plays</div>
                        </div>
                        <div class='artist-stats'>
                            <div class='stat-item'>
                                <div class='stat-value'>${data.topArtists && data.topArtists[0] ? Math.round(data.topArtists[0].count / data.totalTracks * 100) : 0}%</div>
                                <div class='stat-label'>Of Your Music</div>
                            </div>
                            <div class='stat-item'>
                                <div class='stat-value'>${data.topArtists && data.topArtists[0] ? Math.round(data.topArtists[0].count * 3.5 / 60) : 0}</div>
                                <div class='stat-label'>Hours Together</div>
                            </div>
                        </div>
                    </div>
                `);

                // Slide 8: Daily Listening Chart
                const dailyChartSlide = this.createSlide('daily-chart', `
                    <h2>📅 Your Daily Listening Journey</h2>
                    <p style='margin-bottom: 2rem; opacity: 0.8;'>Track plays each day throughout ${data.year}</p>
                    <div class='chart-container'>
                        <canvas id='dailyChart' width='1200' height='500'></canvas>
                    </div>
                    <div class='chart-stats'>
                        <div class='chart-stat'>
                            <div class='stat-number'>${Math.max(...Object.values(data.dailyPlays || {})) || 0}</div>
                            <div class='stat-label'>Peak Day (Plays)</div>
                        </div>
                        <div class='chart-stat'>
                            <div class='stat-number'>${data.weekendRatio}x</div>
                            <div class='stat-label'>Weekend vs Weekday</div>
                        </div>
                        <div class='chart-stat'>
                            <div class='stat-number'>${Object.keys(data.dailyPlays || {}).length}</div>
                            <div class='stat-label'>Active Days</div>
                        </div>
                    </div>
                `);

                const favoriteAlbumSlide = this.createSlide('favorite-album', `
                    <div class='album-slide-container'>
                        <div class='slide-header'>
                            <h2>💿 Favorite Album</h2>
                            <p class='slide-subtitle'>The album that dominated your ${data.year}</p>
                        </div>
                        
                        <div class='album-main-content'>
                            <div class='featured-album'>
                                <div class='album-hero'>
                                    <div class='album-icon-large'>💿</div>
                                    <div class='album-info'>
                                        <div class='album-name'>${data.topAlbums && data.topAlbums.length > 0 ? data.topAlbums[0].name : 'No album data'}</div>
                                        <div class='album-meta'>
                                            <span class='album-plays'>${data.topAlbums && data.topAlbums.length > 0 ? data.topAlbums[0].count : 0} plays</span>
                                            <span class='album-percentage'>${data.topAlbums && data.topAlbums.length > 0 ? Math.round((data.topAlbums[0].count / data.totalTracks) * 100) : 0}% of your music</span>
                                        </div>
                                    </div>
                                </div>
                                ${data.topAlbums && data.topAlbums.length > 0 && data.topAlbums[0].topTracks && data.topAlbums[0].topTracks.length > 0 ? `
                                    <div class='album-tracks-section'>
                                        <h4 class='tracks-title'>Top tracks from this album</h4>
                                        <div class='tracks-list'>
                                            ${data.topAlbums[0].topTracks.slice(0, 3).map((track, index) => `
                                                <div class='track-item'>
                                                    <span class='track-number'>${index + 1}</span>
                                                    <div class='track-details'>
                                                        <span class='track-title'>${track.title}</span>
                                                        <span class='track-count'>${track.count} plays</span>
                                                    </div>
                                                </div>
                                            `).join('')}
                                        </div>
                                    </div>
                                ` : ''}
                            </div>
                            
                            ${data.topAlbums && data.topAlbums.length > 1 ? `
                                <div class='other-albums-section'>
                                    <h3 class='section-title'>Other Favorites</h3>
                                    <div class='albums-grid'>
                                    </div>
                                </div>
                            ` : ''}
                        </div>
                    </div>
                `);

                // Slide 10: Enhanced Thank You
                const thankYouSlide = this.createSlide('thankyou', `
                    <div class='music-notes'></div>
                    <h1>🎉 That's Your ${data.year} Wrapped!</h1>
                    <div class='stats-grid' style='margin-top: 3rem;'>
                        <div class='stat-card'>
                            <div class='stat-number'>${data.totalTracks.toLocaleString()}</div>
                            <div class='stat-label'>Total Tracks</div>
                        </div>
                        <div class='stat-card'>
                            <div class='stat-number'>${data.totalHours}</div>
                            <div class='stat-label'>Hours Enjoyed</div>
                        </div>
                    </div>
                    <p style='font-size: 1.3rem; margin-top: 3rem; opacity: 0.8;'>
                        Thanks for using MusicBee Wrapped
                    </p>
                    <p style='margin-top: 1rem; opacity: 0.6;'>
                        Press F5 to view again or close this tab
                    </p>
                `);

                // Add all slides to DOM
                [welcomeSlide, overviewSlide, artistsSlide, tracksSlide, songSoulmateSlide, musicalJourneySlide, artistSlide, favoriteAlbumSlide, dailyChartSlide, thankYouSlide].forEach(slide => {
                    app.appendChild(slide);
                });

                this.slides = [
                    document.getElementById('loading'),
                    welcomeSlide, overviewSlide, artistsSlide, tracksSlide, songSoulmateSlide, musicalJourneySlide, artistSlide, favoriteAlbumSlide, dailyChartSlide, thankYouSlide
                ];

                // Generate slide indicators
                this.generateIndicators();
                
                // Add floating music notes to welcome and thank you slides
                this.addFloatingNotes(welcomeSlide);
                this.addFloatingNotes(thankYouSlide);
            }

            createSlide(id, content) {
                const slide = document.createElement('div');
                slide.className = 'slide';
                slide.id = id;
                slide.innerHTML = content;
                return slide;
            }

            generateIndicators() {
                const indicator = document.getElementById('slide-indicator');
                // Skip loading slide in indicators
                for (let i = 1; i < this.slides.length; i++) {
                    const dot = document.createElement('div');
                    dot.className = 'indicator-dot';
                    dot.addEventListener('click', () => {
                        const direction = i > this.currentSlide ? 'next' : 'prev';
                        this.showSlide(i, direction);
                    });
                    indicator.appendChild(dot);
                }
            }

            setupNavigation() {
                const prevBtn = document.getElementById('prev-btn');
                const nextBtn = document.getElementById('next-btn');

                prevBtn.addEventListener('click', () => this.previousSlide());
                nextBtn.addEventListener('click', () => this.nextSlide());

                // Keyboard navigation with enhanced shortcuts
                document.addEventListener('keydown', (e) => {
                    if (e.key === 'ArrowLeft' || e.key === 'a' || e.key === 'A') {
                        e.preventDefault();
                        this.previousSlide();
                    }
                    if (e.key === 'ArrowRight' || e.key === 'd' || e.key === 'D') {
                        e.preventDefault();
                        this.nextSlide();
                    }
                    if (e.key === ' ' || e.key === 'Enter') {
                        e.preventDefault();
                        this.nextSlide();
                    }
                    if (e.key === 'Home') {
                        e.preventDefault();
                        this.showSlide(1, 'prev');
                    }
                    if (e.key === 'End') {
                        e.preventDefault();
                        this.showSlide(this.slides.length - 1, 'next');
                    }
                    if (e.key === 'Escape') {
                        e.preventDefault();
                        // Optional: Show help or close
                        this.showKeyboardHelp();
                    }
                });
            }

            updateNavigation() {
                const prevBtn = document.getElementById('prev-btn');
                const nextBtn = document.getElementById('next-btn');
                const indicators = document.querySelectorAll('.indicator-dot');

                // Update buttons
                prevBtn.disabled = this.currentSlide <= 1;
                nextBtn.disabled = this.currentSlide >= this.slides.length - 1;

                // Update indicators (offset by 1 to skip loading slide)
                indicators.forEach((dot, index) => {
                    dot.classList.toggle('active', index === this.currentSlide - 1);
                });
            }

            nextSlide() {
                if (this.currentSlide < this.slides.length - 1) {
                    this.showSlide(this.currentSlide + 1, 'next');
                }
            }

            previousSlide() {
                if (this.currentSlide > 1) {
                    this.showSlide(this.currentSlide - 1, 'prev');
                }
            }

            addFloatingNotes(slideElement) {
                const notesContainer = slideElement.querySelector('.music-notes');
                if (!notesContainer) return;

                const notes = ['♪', '♫', '♬', '♭', '♯', '𝄞'];
                const createNote = () => {
                    const note = document.createElement('div');
                    note.className = 'music-note';
                    note.textContent = notes[Math.floor(Math.random() * notes.length)];
                    note.style.left = Math.random() * 100 + '%';
                    note.style.animationDelay = Math.random() * 8 + 's';
                    note.style.animationDuration = (8 + Math.random() * 4) + 's';
                    notesContainer.appendChild(note);

                    // Remove note after animation
                    setTimeout(() => {
                        if (note.parentNode) {
                            note.parentNode.removeChild(note);
                        }
                    }, 12000);
                };

                // Create initial notes
                for (let i = 0; i < 8; i++) {
                    setTimeout(createNote, i * 1000);
                }

                // Continue creating notes
                const noteInterval = setInterval(() => {
                    if (slideElement.classList.contains('active')) {
                        createNote();
                    }
                }, 2000);

                // Clean up interval when slide is no longer active
                const observer = new MutationObserver((mutations) => {
                    mutations.forEach((mutation) => {
                        if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                            if (!slideElement.classList.contains('active')) {
                                clearInterval(noteInterval);
                                observer.disconnect();
                            }
                        }
                    });
                });
                observer.observe(slideElement, { attributes: true });
            }

            animateSlideContent(slideElement) {
                // Special handling for favorite album slide
                if (slideElement.id === 'favorite-album') {
                    // Animate the albums grid with other favorite albums
                    const albumsGrid = slideElement.querySelector('.albums-grid');
                    if (albumsGrid && window.WRAPPED_DATA.topAlbums && window.WRAPPED_DATA.topAlbums.length > 1) {
                        // Clear existing content
                        albumsGrid.innerHTML = '';
                        
                        // Generate other favorite albums (albums 2-4)
                        const otherAlbums = window.WRAPPED_DATA.topAlbums.slice(1, 4);
                        otherAlbums.forEach((album, index) => {
                            const albumCard = document.createElement('div');
                            albumCard.className = 'other-album-card';
                            
                            let topTracksHtml = '';
                            if (album.topTracks && album.topTracks.length > 0) {
                                topTracksHtml = `
                                    <div class='other-album-tracks'>
                                        ${album.topTracks.slice(0, 2).map((track, trackIndex) => `
                                            <div class='track-item'>
                                                <span class='track-number'>${trackIndex + 1}</span>
                                                <div class='track-details'>
                                                    <span class='track-title'>${track.title}</span>
                                                    <span class='track-count'>${track.count}</span>
                                                </div>
                                            </div>
                                        `).join('')}
                                    </div>
                                `;
                            }
                            
                            albumCard.innerHTML = `
                                <div class='other-album-header'>
                                    <div class='album-icon'>💿</div>
                                    <div class='other-album-info'>
                                        <div class='album-name'>${album.name}</div>
                                        <div class='album-plays'>${album.count} plays</div>
                                    </div>
                                </div>
                                ${topTracksHtml}
                            `;
                            
                            albumsGrid.appendChild(albumCard);
                            
                            // Animate in with staggered timing
                            setTimeout(() => {
                                albumCard.classList.add('animate');
                            }, 800 + (index * 200));
                        });
                    }
                    
                    // Animate the main album tracks
                    const trackItems = slideElement.querySelectorAll('.track-item');
                    trackItems.forEach((item, index) => {
                        item.style.opacity = '0';
                        item.style.transform = 'translateY(15px)';
                        
                        setTimeout(() => {
                            item.style.transition = 'all 0.4s ease-out';
                            item.style.opacity = '1';
                            item.style.transform = 'translateY(0)';
                        }, 500 + (index * 100));
                    });

                    // Animate the main album hero section
                    const albumHero = slideElement.querySelector('.album-hero');
                    if (albumHero) {
                        albumHero.style.opacity = '0';
                        albumHero.style.transform = 'translateY(20px)';
                        
                        setTimeout(() => {
                            albumHero.style.transition = 'all 0.6s cubic-bezier(0.4, 0, 0.2, 1)';
                            albumHero.style.opacity = '1';
                            albumHero.style.transform = 'translateY(0)';
                        }, 200);
                    }

                    // Animate the tracks section
                    const tracksSection = slideElement.querySelector('.album-tracks-section');
                    if (tracksSection) {
                        tracksSection.style.opacity = '0';
                        tracksSection.style.transform = 'translateY(20px)';
                        
                        setTimeout(() => {
                            tracksSection.style.transition = 'all 0.6s cubic-bezier(0.4, 0, 0.2, 1)';
                            tracksSection.style.opacity = '1';
                            tracksSection.style.transform = 'translateY(0)';
                        }, 400);
                    }

                    // Animate the section titles
                    const sectionTitles = slideElement.querySelectorAll('.section-title');
                    sectionTitles.forEach((title, index) => {
                        title.style.opacity = '0';
                        title.style.transform = 'translateY(10px)';
                        
                        setTimeout(() => {
                            title.style.transition = 'all 0.4s ease-out';
                            title.style.opacity = '1';
                            title.style.transform = 'translateY(0)';
                        }, 600 + (index * 100));
                    });
                }

                // Special handling for daily chart slide
                if (slideElement.id === 'daily-chart') {
                    setTimeout(() => {
                        this.drawDailyChart();
                    }, 500);
                }

                // Special handling for musical journey timeline
                if (slideElement.id === 'musical-journey') {
                    // Animate timeline milestones from left to right
                    const milestones = slideElement.querySelectorAll('.timeline-milestone');
                    milestones.forEach((milestone, index) => {
                        const marker = milestone.querySelector('.milestone-marker');
                        const card = milestone.querySelector('.milestone-card');
                        
                        // Initial state
                        marker.style.opacity = '0';
                        marker.style.transform = 'scale(0.3) translateY(30px)';
                        card.style.opacity = '0';
                        card.style.transform = 'translateY(40px) scale(0.8)';
                        
                        // Animate marker first
                        setTimeout(() => {
                            marker.style.transition = 'all 0.8s cubic-bezier(0.175, 0.885, 0.32, 1.275)';
                            marker.style.opacity = '1';
                            marker.style.transform = 'scale(1) translateY(0)';
                            
                            // Then animate card
                            setTimeout(() => {
                                card.style.transition = 'all 0.6s cubic-bezier(0.4, 0, 0.2, 1)';
                                card.style.opacity = '1';
                                card.style.transform = 'translateY(0) scale(1)';
                            }, 300);
                        }, index * 600 + 400);
                    });

                    // Animate timeline connectors
                    const connectors = slideElement.querySelectorAll('.timeline-connector');
                    connectors.forEach((connector, index) => {
                        const line = connector.querySelector('.connector-line');
                        if (line) {
                            line.style.opacity = '0';
                            line.style.transform = 'scaleX(0)';
                            
                            setTimeout(() => {
                                line.style.transition = 'all 1.2s ease-out';
                                line.style.opacity = '1';
                                line.style.transform = 'scaleX(1)';
                            }, index * 600 + 800);
                        }
                    });

                    // Animate stats bubbles
                    const statBubbles = slideElement.querySelectorAll('.stat-bubble');
                    statBubbles.forEach((bubble, index) => {
                        bubble.style.opacity = '0';
                        bubble.style.transform = 'translateY(20px) scale(0.8)';
                        
                        setTimeout(() => {
                            bubble.style.transition = 'all 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
                            bubble.style.opacity = '1';
                            bubble.style.transform = 'translateY(0) scale(1)';
                        }, 1200 + index * 100);
                    });

                    // Animate insight card
                    const insightCards = slideElement.querySelectorAll('.insight-card');
                    insightCards.forEach((card, index) => {
                        card.style.opacity = '0';
                        card.style.transform = 'translateY(30px) scale(0.95)';
                        
                        setTimeout(() => {
                            card.style.transition = 'all 0.6s cubic-bezier(0.4, 0, 0.2, 1)';
                            card.style.opacity = '1';
                            card.style.transform = 'translateY(0) scale(1)';
                        }, 2500 + index * 200);
                    });

                    // Animate flow indicators
                    const flowIndicators = slideElement.querySelectorAll('.journey-flow-indicator');
                    flowIndicators.forEach((indicator, index) => {
                        indicator.style.opacity = '0';
                        indicator.style.transform = 'translateY(-10px) scale(0.9)';
                        
                        setTimeout(() => {
                            indicator.style.transition = 'all 0.4s ease-out';
                            indicator.style.opacity = '1';
                            indicator.style.transform = 'translateY(0) scale(1)';
                        }, 1800 + index * 200);
                    });
                }

                // Animate stat cards with enhanced timing
                const statCards = slideElement.querySelectorAll('.stat-card');
                statCards.forEach((card, index) => {
                    const delay = parseInt(card.dataset.delay) || index * 150;
                    card.style.opacity = '0';
                    card.style.transform = 'translateY(30px) scale(0.95)';
                    
                    setTimeout(() => {
                        card.style.transition = 'all 0.6s cubic-bezier(0.4, 0, 0.2, 1)';
                        card.style.opacity = '1';
                        card.style.transform = 'translateY(0) scale(1)';
                        
                        // Animate progress bars
                        const progressFill = card.querySelector('.progress-fill');
                        if (progressFill) {
                            setTimeout(() => {
                                progressFill.classList.add('animate');
                            }, 400);
                        }

                        // Animate numbers with enhanced effect
                        const numberElement = card.querySelector('.stat-number[data-target]');
                        if (numberElement) {
                            setTimeout(() => {
                                this.animateNumber(numberElement, parseInt(numberElement.dataset.target) || 0);
                            }, 200);
                        }
                    }, delay);
                });

                // Animate top lists with staggered effect
                const topItems = slideElement.querySelectorAll('.top-item');
                topItems.forEach((item, index) => {
                    const delay = parseInt(item.dataset.delay) || index * 120;
                    item.style.opacity = '0';
                    item.style.transform = 'translateX(-30px)';
                    
                    setTimeout(() => {
                        item.style.transition = 'all 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
                        item.style.opacity = '1';
                        item.style.transform = 'translateX(0)';
                        item.classList.add('animate');
                    }, delay);
                });

                // Animate pattern cards with enhanced scaling
                const patternCards = slideElement.querySelectorAll('.pattern-card');
                patternCards.forEach((card, index) => {
                    card.style.opacity = '0';
                    card.style.transform = 'translateY(40px) scale(0.8)';
                    
                    setTimeout(() => {
                        card.style.transition = 'all 0.6s cubic-bezier(0.175, 0.885, 0.32, 1.275)';
                        card.style.opacity = '1';
                        card.style.transform = 'translateY(0) scale(1)';
                        card.classList.add('animate');
                        
                        // Add subtle bounce effect
                        setTimeout(() => {
                            card.style.transform = 'translateY(0) scale(1.02)';
                            setTimeout(() => {
                                card.style.transform = 'translateY(0) scale(1)';
                            }, 150);
                        }, 300);
                    }, index * 180 + 300);
                });

                // Special animations for headings
                const headings = slideElement.querySelectorAll('h1, h2');
                headings.forEach((heading, index) => {
                    heading.style.opacity = '0';
                    heading.style.transform = 'translateY(-20px)';
                    setTimeout(() => {
                        heading.style.transition = 'all 0.8s cubic-bezier(0.4, 0, 0.2, 1)';
                        heading.style.opacity = '1';
                        heading.style.transform = 'translateY(0)';
                    }, index * 100);
                });
            }

            animateNumber(element, target) {
                const start = 0;
                const duration = 2000;
                const startTime = performance.now();

                const updateNumber = (currentTime) => {
                    const elapsed = currentTime - startTime;
                    const progress = Math.min(elapsed / duration, 1);
                    
                    // Easing function for smooth animation
                    const easeOut = 1 - Math.pow(1 - progress, 3);
                    const current = Math.floor(start + (target - start) * easeOut);
                    
                    if (target > 1000) {
                        element.textContent = current.toLocaleString();
                    } else {
                        element.textContent = current;
                    }

                    if (progress < 1) {
                        requestAnimationFrame(updateNumber);
                    } else {
                        // Ensure final value is exact
                        if (target > 1000) {
                            element.textContent = target.toLocaleString();
                        } else {
                            element.textContent = target;
                        }
                        element.style.animation = 'countUp 0.3s ease-out';
                    }
                };

                requestAnimationFrame(updateNumber);
            }

            showSlide(index, direction = 'next') {
                if (this.isAnimating || index === this.currentSlide) return;
                
                this.isAnimating = true;
                const previousIndex = this.currentSlide;

                // Hide current slide with exit animation
                if (this.slides[this.currentSlide]) {
                    const currentSlide = this.slides[this.currentSlide];
                    currentSlide.classList.remove('active');
                    
                    // Add exit animation based on direction
                    if (direction === 'prev') {
                        currentSlide.style.transform = 'translateX(100%)';
                    } else {
                        currentSlide.style.transform = 'translateX(-100%)';
                    }
                }

                // Show new slide with entrance animation
                this.currentSlide = index;
                if (this.slides[this.currentSlide]) {
                    const newSlide = this.slides[this.currentSlide];
                    
                    // Set initial position based on direction
                    if (direction === 'prev') {
                        newSlide.style.transform = 'translateX(-100%)';
                    } else {
                        newSlide.style.transform = 'translateX(100%)';
                    }
                    
                    // Force reflow and then animate in
                    newSlide.offsetHeight;
                    newSlide.classList.add('active');
                    newSlide.style.transform = 'translateX(0)';
                    
                    // Trigger slide-specific animations after slide transition
                    setTimeout(() => {
                        this.animateSlideContent(newSlide);
                    }, 400);
                }

                // Update navigation
                this.updateNavigation();

                // Re-enable animation after transition completes
                setTimeout(() => {
                    this.isAnimating = false;
                    
                    // Clean up transform styles
                    if (this.slides[previousIndex]) {
                        this.slides[previousIndex].style.transform = '';
                    }
                }, 700);
            }

            showKeyboardHelp() {
                // Create a temporary help overlay
                const helpOverlay = document.createElement('div');
                helpOverlay.style.cssText = 
                    'position: fixed; top: 0; left: 0; width: 100%; height: 100%; ' +
                    'background: rgba(0, 0, 0, 0.8); display: flex; align-items: center; ' +
                    'justify-content: center; z-index: 9999; backdrop-filter: blur(10px);';
                
                const helpContent = document.createElement('div');
                helpContent.style.cssText = 
                    'background: rgba(255, 255, 255, 0.1); border-radius: 20px; ' +
                    'padding: 2rem; text-align: center; border: 1px solid rgba(255, 255, 255, 0.2); ' +
                    'max-width: 400px; width: 90%;';
                
                helpContent.innerHTML = 
                    '<h3 style=""margin-bottom: 1.5rem; color: #4ecdc4;"">⌨️ Keyboard Shortcuts</h3>' +
                    '<div style=""text-align: left; line-height: 2;"">' +
                        '<div><strong>→ / D</strong> - Next slide</div>' +
                        '<div><strong>← / A</strong> - Previous slide</div>' +
                        '<div><strong>Space / Enter</strong> - Next slide</div>' +
                        '<div><strong>Home</strong> - First slide</div>' +
                        '<div><strong>End</strong> - Last slide</div>' +
                        '<div><strong>Esc</strong> - Show this help</div>' +
                    '</div>' +
                    '<p style=""margin-top: 1.5rem; opacity: 0.7; font-size: 0.9rem;"">' +
                        'Click anywhere to close' +
                    '</p>';
                    
                helpOverlay.appendChild(helpContent);
                
                helpOverlay.addEventListener('click', () => {
                    document.body.removeChild(helpOverlay);
                });
                
                document.body.appendChild(helpOverlay);
                
                // Auto-hide after 5 seconds
                setTimeout(() => {
                    if (document.body.contains(helpOverlay)) {
                        document.body.removeChild(helpOverlay);
                    }                    }, 5000);
            }

            drawDailyChart() {
                const canvas = document.getElementById('dailyChart');
                if (!canvas) return;
                
                const ctx = canvas.getContext('2d');
                const data = window.WRAPPED_DATA;
                const dailyPlays = data.dailyPlays || {};
                
                // Set canvas size with high DPI support
                const rect = canvas.getBoundingClientRect();
                const dpr = window.devicePixelRatio || 1;
                canvas.width = rect.width * dpr;
                canvas.height = rect.height * dpr;
                ctx.scale(dpr, dpr);
                
                const width = rect.width;
                const height = rect.height;
                const padding = { top: 40, right: 40, bottom: 80, left: 80 };
                
                // Create full year of dates for the chart
                const year = data.year;
                const startDate = new Date(year, 0, 1);
                const endDate = new Date(year, 11, 31);
                const allDates = [];
                
                // Generate all dates in the year
                for (let d = new Date(startDate); d <= endDate; d.setDate(d.getDate() + 1)) {
                    allDates.push(new Date(d));
                }
                
                // Create data array with play counts for each day
                const chartData = allDates.map(date => {
                    const dateStr = date.toISOString().split('T')[0];
                    return {
                        date: date,
                        dateStr: dateStr,
                        plays: dailyPlays[dateStr] || 0,
                        month: date.getMonth(),
                        dayOfYear: Math.floor((date - startDate) / (24 * 60 * 60 * 1000))
                    };
                });
                
                const maxPlays = Math.max(...chartData.map(d => d.plays), 1);
                const chartWidth = width - padding.left - padding.right;
                const chartHeight = height - padding.top - padding.bottom;
                const barWidth = Math.max(1, chartWidth / chartData.length);
                
                // Clear canvas
                ctx.clearRect(0, 0, width, height);
                
                // Set up styling
                ctx.fillStyle = '#ffffff';
                ctx.strokeStyle = '#ffffff';
                ctx.font = '12px sans-serif';
                
                // Draw background grid lines
                ctx.strokeStyle = 'rgba(255, 255, 255, 0.1)';
                ctx.lineWidth = 1;
                
                // Horizontal grid lines
                const gridLines = 5;
                for (let i = 0; i <= gridLines; i++) {
                    const y = padding.top + (chartHeight * i / gridLines);
                    ctx.beginPath();
                    ctx.moveTo(padding.left, y);
                    ctx.lineTo(width - padding.right, y);
                    ctx.stroke();
                }
                
                // Draw month separators and labels
                ctx.strokeStyle = 'rgba(255, 255, 255, 0.3)';
                ctx.lineWidth = 1;
                ctx.fillStyle = 'rgba(255, 255, 255, 0.8)';
                ctx.textAlign = 'center';
                
                const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 
                                   'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
                let lastMonth = -1;
                
                chartData.forEach((dataPoint, index) => {
                    const x = padding.left + (index * barWidth);
                    
                    if (dataPoint.month !== lastMonth) {
                        ctx.beginPath();
                        ctx.moveTo(x, padding.top);
                        ctx.lineTo(x, height - padding.bottom);
                        ctx.stroke();
                        
                        const monthLabelX = x + (barWidth * 15);
                        ctx.fillText(monthNames[dataPoint.month], monthLabelX, height - padding.bottom + 25);
                        
                        lastMonth = dataPoint.month;
                    }
                });
                
                // Draw bars
                chartData.forEach((dataPoint, index) => {
                    const x = padding.left + (index * barWidth);
                    const barHeight = (dataPoint.plays / maxPlays) * chartHeight;
                    const y = height - padding.bottom - barHeight;
                    
                    if (dataPoint.plays > 0) {
                        const gradient = ctx.createLinearGradient(0, y, 0, y + barHeight);
                        gradient.addColorStop(0, '#4ecdc4');
                        gradient.addColorStop(1, 'rgba(78, 205, 196, 0.6)');
                        
                        ctx.fillStyle = gradient;
                        ctx.fillRect(x, y, Math.max(1, barWidth - 0.5), barHeight);
                        
                        if (dataPoint.plays > maxPlays * 0.7) {
                            ctx.shadowColor = '#4ecdc4';
                            ctx.shadowBlur = 3;
                            ctx.fillRect(x, y, Math.max(1, barWidth - 0.5), barHeight);
                            ctx.shadowBlur = 0;
                        }
                    }
                });
                
                // Draw Y-axis labels
                ctx.fillStyle = 'rgba(255, 255, 255, 0.8)';
                ctx.textAlign = 'right';
                ctx.textBaseline = 'middle';
                
                for (let i = 0; i <= gridLines; i++) {
                    const y = padding.top + (chartHeight * i / gridLines);
                    const value = Math.round(maxPlays * (gridLines - i) / gridLines);
                    ctx.fillText(value.toString(), padding.left - 15, y);
                }
                
                // Draw axis labels
                ctx.fillStyle = 'rgba(255, 255, 255, 0.9)';
                ctx.font = 'bold 14px sans-serif';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                
                // Y-axis label
                ctx.save();
                ctx.translate(20, height / 2);
                ctx.rotate(-Math.PI / 2);
                ctx.fillText('Track Plays', 0, 0);
                ctx.restore();
                
                // X-axis label
                ctx.fillText('Days in ' + year, width / 2, height - 20);
                
                // Draw chart title
                ctx.font = 'bold 16px sans-serif';
                ctx.fillStyle = '#ffffff';
                ctx.fillText('Daily Listening Activity', width / 2, 25);
            }
        }

        // Initialize the app
        new WrappedApp();
        ";
        }

        /// <summary>
        /// Creates the year selector HTML template
        /// </summary>
        private string CreateYearSelectorHTML(System.Collections.Generic.List<int> availableYears, YearMetadataCollection metadata)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("    <title>MusicBee Wrapped - Select Year</title>");
            
            // Embed Year Selector CSS
            html.AppendLine("    <style>");
            html.AppendLine(GetYearSelectorCSS());
            html.AppendLine("    </style>");
            
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Main container
            html.AppendLine("    <div id=\"year-selector\">");
            html.AppendLine("        <div class=\"header\">");
            html.AppendLine("            <h1>🎵 Choose Your Year</h1>");
            html.AppendLine("            <p>Select a year to view your MusicBee Wrapped statistics</p>");
            html.AppendLine("        </div>");
            
            html.AppendLine("        <div class=\"years-grid\">");
            
            foreach (var year in availableYears)
            {
                var yearMeta = metadata.GetYearMetadata(year);
                bool hasData = yearMeta != null && yearMeta.TotalPlays > 0;
                string cardClass = hasData ? "year-card" : "year-card no-data";
                
                html.AppendLine($"            <div class=\"{cardClass}\" data-year=\"{year}\">");
                html.AppendLine($"                <div class=\"year-number\">{year}</div>");
                
                if (hasData && yearMeta != null)
                {
                    html.AppendLine("                <div class=\"year-stats\">");
                    html.AppendLine($"                    <div class=\"stat-item\">");
                    html.AppendLine($"                        <span class=\"stat-number\">{yearMeta.TotalPlays:N0}</span>");
                    html.AppendLine($"                        <span class=\"stat-label\">Plays</span>");
                    html.AppendLine($"                    </div>");
                    html.AppendLine($"                    <div class=\"stat-item\">");
                    html.AppendLine($"                        <span class=\"stat-number\">{(yearMeta.TotalMinutes / 60.0):F0}h</span>");
                    html.AppendLine($"                        <span class=\"stat-label\">Hours</span>");
                    html.AppendLine($"                    </div>");
                    html.AppendLine("                </div>");
                    html.AppendLine($"                <div class=\"year-highlights\">");
                    html.AppendLine($"                    <div class=\"highlight\">Top: {EscapeHtml(yearMeta.TopArtist)}</div>");
                    html.AppendLine($"                </div>");
                }
                else
                {
                    html.AppendLine("                <div class=\"no-data-message\">");
                    html.AppendLine("                    <span>No listening data</span>");
                    html.AppendLine("                </div>");
                }
                
                html.AppendLine("            </div>");
            }
            
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            // Embed JavaScript for year selection
            html.AppendLine("    <script>");
            html.AppendLine(GetYearSelectorJavaScript());
            html.AppendLine("    </script>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        /// <summary>
        /// Returns CSS styling for the year selector
        /// </summary>
        private string GetYearSelectorCSS()
        {
            return @"
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0f0f23 0%, #1a1a2e 50%, #16213e 100%);
            color: white;
            min-height: 100vh;
            padding: 2rem;
        }

        #year-selector {
            max-width: 1200px;
            margin: 0 auto;
            text-align: center;
        }

        .header {
            margin-bottom: 4rem;
        }

        .header h1 {
            font-size: 3.5rem;
            margin-bottom: 1rem;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            animation: fadeInUp 1s ease-out;
        }

        .header p {
            font-size: 1.3rem;
            color: rgba(255, 255, 255, 0.8);
            animation: fadeInUp 1s ease-out 0.2s both;
        }

        .years-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 2rem;
            margin-top: 3rem;
        }

        .year-card {
            background: rgba(255, 255, 255, 0.05);
            border-radius: 20px;
            padding: 2rem;
            border: 2px solid rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
            transition: all 0.3s ease;
            cursor: pointer;
            transform: translateY(20px);
            opacity: 0;
            animation: slideIn 0.6s ease-out forwards;
        }

        .year-card:nth-child(1) { animation-delay: 0.1s; }
        .year-card:nth-child(2) { animation-delay: 0.2s; }
        .year-card:nth-child(3) { animation-delay: 0.3s; }
        .year-card:nth-child(4) { animation-delay: 0.4s; }
        .year-card:nth-child(5) { animation-delay: 0.5s; }

        .year-card:hover {
            transform: translateY(-10px) scale(1.05);
            border-color: rgba(78, 205, 196, 0.5);
            box-shadow: 0 20px 40px rgba(78, 205, 196, 0.2);
            background: rgba(255, 255, 255, 0.1);
        }

        .year-card.no-data {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .year-card.no-data:hover {
            transform: translateY(-5px) scale(1.02);
            border-color: rgba(255, 255, 255, 0.2);
            box-shadow: 0 10px 20px rgba(255, 255, 255, 0.1);
        }

        .year-number {
            font-size: 3rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 1.5rem;
        }

        .year-stats {
            display: flex;
            justify-content: space-around;
            margin: 1.5rem 0;
            padding: 1rem 0;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
            border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        }

        .stat-item {
            display: flex;
            flex-direction: column;
            align-items: center;
        }

        .stat-number {
            font-size: 1.8rem;
            font-weight: 600;
            color: #45b7d1;
            margin-bottom: 0.3rem;
        }

        .stat-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .year-highlights {
            margin-top: 1rem;
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.8);
        }

        .highlight {
            background: rgba(78, 205, 196, 0.2);
            padding: 0.5rem 1rem;
            border-radius: 15px;
            margin: 0.5rem 0;
        }

        .no-data-message {
            margin-top: 1rem;
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.6);
        }

        /* Add visual indicator for clickable cards */
        .year-card:not(.no-data)::after {
            content: '→';
            position: absolute;
            top: 1rem;
            right: 1rem;
            font-size: 1.5rem;
            color: #4ecdc4;
            opacity: 0;
            transition: all 0.3s ease;
        }

        .year-card:not(.no-data) {
            position: relative;
        }

        .year-card:not(.no-data):hover::after {
            opacity: 1;
            transform: translateX(5px);
        }

        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        @keyframes slideIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        ";
        }

        /// <summary>
        /// Returns embedded JavaScript for the year selector functionality
        /// </summary>
        private string GetYearSelectorJavaScript()
        {
            return @"
        document.addEventListener('DOMContentLoaded', () => {
            const yearCards = document.querySelectorAll('.year-card:not(.no-data)');
            
            yearCards.forEach(card => {
                card.addEventListener('click', () => {
                    const year = card.getAttribute('data-year');
                    // Navigate to the wrapped HTML for the selected year
                    window.location.href = `wrapped_${year}.html`;
                });
                
                // Add hover effect
                card.addEventListener('mouseenter', () => {
                    card.style.transform = 'translateY(-5px)';
                });
                
                card.addEventListener('mouseleave', () => {
                    card.style.transform = 'translateY(0)';
                });
            });
            
            // Disable clicks on no-data cards
            const noDataCards = document.querySelectorAll('.year-card.no-data');
            noDataCards.forEach(card => {
                card.addEventListener('click', (e) => {
                    e.preventDefault();
                    // Could show a message that no data is available
                });
            });
        });
        ";
        }

        /// <summary>
        /// Creates a unique session folder for this Wrapped generation
        /// </summary>
        private string CreateSessionFolder()
        {
            string sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
            string sessionPath = Path.Combine(_tempBasePath, $"session_{sessionId}");
            Directory.CreateDirectory(sessionPath);
            return sessionPath;
        }

        /// <summary>
        /// Ensures the base temp directory exists
        /// </summary>
        private void EnsureTempDirectoryExists()
        {
            if (!Directory.Exists(_tempBasePath))
            {
                Directory.CreateDirectory(_tempBasePath);
            }
        }

        /// <summary>
        /// Launches the HTML file in the default browser
        /// </summary>
        private bool LaunchBrowser(string htmlFilePath)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = htmlFilePath,
                    UseShellExecute = true
                };
                
                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to launch browser: {ex.Message}");
                
                // Fallback: try to copy path to clipboard
                try
                {
                    System.Windows.Forms.Clipboard.SetText(htmlFilePath);
                    System.Diagnostics.Trace.WriteLine($"Browser launch failed, path copied to clipboard: {htmlFilePath}");
                }
                catch
                {
                    // Silent fail on clipboard
                }
                
                return false;
            }
        }

        /// <summary>
        /// Schedules cleanup of temporary files after specified delay
        /// </summary>
        private void ScheduleCleanup(string folderPath, TimeSpan delay)
        {
            Task.Run(async () =>
            {
                await Task.Delay(delay);
                
                try
                {
                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                        System.Diagnostics.Trace.WriteLine($"Cleaned up temp folder: {folderPath}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Cleanup failed for {folderPath}: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Cleans up old session folders (called during initialization)
        /// </summary>
        public void CleanupOldSessions()
        {
            try
            {
                if (!Directory.Exists(_tempBasePath)) return;

                var directories = Directory.GetDirectories(_tempBasePath, "session_*");
                var cutoffTime = DateTime.Now.AddHours(-6); // Remove sessions older than 6 hours

                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (dirInfo.CreationTime < cutoffTime)
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                        }
 catch
                        {
                            // Silent fail on individual cleanup
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Cleanup old sessions failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Escapes HTML characters for safe display
        /// </summary>
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
        /// Serializes album data with top tracks for each album
        /// </summary>
        private string SerializeTopAlbumsWithTracks(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, int>> albums, PlayHistory playHistory, int year)
        {
            var yearPlays = playHistory.GetPlaysByYear(year).ToList();
            
            var albumData = albums.Select(album => 
            {
                // Get top 3 tracks from this album
                var albumTracks = yearPlays
                    .Where(p => string.Equals(p.Album, album.Key, StringComparison.OrdinalIgnoreCase))
                    .GroupBy(p => p.Title)
                    .Select(g => new { Title = g.Key, Artist = g.First().Artist, Count = g.Count() })
                    .OrderByDescending(t => t.Count)
                    .Take(3)
                    .ToList();

                var topTracksJson = string.Join(",", albumTracks.Select(t => 
                    $"{{\"title\":\"{EscapeJsonString(t.Title)}\",\"artist\":\"{EscapeJsonString(t.Artist)}\",\"count\":{t.Count}}}"));

                return $"{{\"name\":\"{EscapeJsonString(album.Key)}\",\"count\":{album.Value},\"topTracks\":[{topTracksJson}]}}";
            });
            
            return $"[{string.Join(",", albumData)}]";
        }
    }
}
