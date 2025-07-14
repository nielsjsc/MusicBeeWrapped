using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Obsession Periods slide - Analyzes weeks where the user was obsessed with specific artists
    /// Identifies periods of intense focus on particular artists and presents them as a multi-part story
    /// </summary>
    public class ObsessionPeriodsSlide : SlideComponentBase
    {
        public override string SlideId => "obsession-periods";
        public override string SlideTitle => "Musical Obsessions";
        public override int SlideOrder => 6;

        // Configuration constants - Made more lenient for testing
        private const double MIN_WEEKLY_HOURS = 1.5;          // Minimum total listening hours per week
        private const double MIN_ARTIST_HOURS = 1.0;          // Minimum hours for an artist to be considered obsession
        private const double DOMINANCE_THRESHOLD = 0.5;       // 50% of weekly listening must be this artist
        private const int MAX_OBSESSIONS_TO_SHOW = 3;         // Show top 3 obsession periods

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var obsessions = FindObsessionPeriods(playHistory, year);
            
            // Always show the slide, even if no obsessions found
            if (!obsessions.Any())
            {
                return WrapInSlideContainer(GenerateNoObsessionsHTML(playHistory, year));
            }

            var content = $@"
                <div class='obsession-container' data-obsession-count='{obsessions.Count}'>
                    <div class='obsession-background'>
                        <div class='obsession-particles'></div>
                        <div class='intensity-waves'></div>
                        <div class='focus-beams'></div>
                    </div>
                    
                    <div class='obsession-content'>
                        {GenerateObsessionOverview(obsessions, year)}
                        {GenerateObsessionDetails(obsessions)}
                        {GenerateObsessionInsights(obsessions)}
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var obsessions = FindObsessionPeriods(playHistory, year);
            return GenerateObsessionInsight(obsessions, year);
        }

        public override string GetInsightTitle(WrappedStatistics stats)
        {
            return "Deep Dives";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            if (!base.CanRender(stats, playHistory) || playHistory?.Plays == null)
                return false;

            // Basic check - we need at least some play data to analyze
            return playHistory.Plays.Any();
        }

        #region Obsession Detection Algorithm

        /// <summary>
        /// Main algorithm to find obsession periods from play history
        /// </summary>
        private List<ObsessionPeriod> FindObsessionPeriods(PlayHistory playHistory, int year)
        {
            if (playHistory?.Plays == null || !playHistory.Plays.Any())
                return new List<ObsessionPeriod>();

            // Debug: Check if we have data for the requested year
            var playsForYear = playHistory.Plays.Where(p => p.PlayedAt.Year == year).ToList();
            if (!playsForYear.Any())
            {
                // If no plays for specified year, try current year or most recent year with data
                var availableYears = playHistory.Plays.Select(p => p.PlayedAt.Year).Distinct().OrderByDescending(y => y).ToList();
                if (availableYears.Any())
                {
                    year = availableYears.First(); // Use the most recent year with data
                    playsForYear = playHistory.Plays.Where(p => p.PlayedAt.Year == year).ToList();
                }
            }

            if (!playsForYear.Any())
                return new List<ObsessionPeriod>();

            // Step 1: Group plays by week and artist
            var weeklyData = GroupPlaysByWeekAndArtist(playHistory, year);

            // Step 2: Find weeks with sufficient listening time
            var validWeeks = weeklyData.Where(w => w.TotalHours >= MIN_WEEKLY_HOURS).ToList();

            // Step 3: Identify obsession weeks
            var obsessionWeeks = FindObsessionWeeks(validWeeks);

            // Step 4: Merge consecutive weeks and calculate metrics
            var obsessionPeriods = MergeConsecutiveWeeks(obsessionWeeks);

            // Step 5: Sort by intensity and take top periods
            return obsessionPeriods
                .OrderByDescending(o => o.IntensityScore)
                .Take(MAX_OBSESSIONS_TO_SHOW)
                .ToList();
        }

        /// <summary>
        /// Groups plays by ISO week number and artist
        /// </summary>
        private List<WeeklyListeningData> GroupPlaysByWeekAndArtist(PlayHistory playHistory, int year)
        {
            var calendar = CultureInfo.InvariantCulture.Calendar;
            var weekGroups = new Dictionary<int, WeeklyListeningData>();

            foreach (var play in playHistory.Plays.Where(p => p.PlayedAt.Year == year))
            {
                var weekOfYear = calendar.GetWeekOfYear(play.PlayedAt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                
                if (!weekGroups.ContainsKey(weekOfYear))
                {
                    weekGroups[weekOfYear] = new WeeklyListeningData
                    {
                        WeekNumber = weekOfYear,
                        Year = year,
                        ArtistHours = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase),
                        WeekStart = GetWeekStart(year, weekOfYear)
                    };
                }

                var weekData = weekGroups[weekOfYear];
                var artist = !string.IsNullOrEmpty(play.Artist) ? play.Artist : "Unknown Artist";
                
                // Estimate 3.5 minutes per play, convert to hours
                var estimatedHours = 3.5 / 60.0;
                
                if (!weekData.ArtistHours.ContainsKey(artist))
                    weekData.ArtistHours[artist] = 0;
                    
                weekData.ArtistHours[artist] += estimatedHours;
            }

            // Calculate total hours for each week
            foreach (var week in weekGroups.Values)
            {
                week.TotalHours = week.ArtistHours.Values.Sum();
            }

            return weekGroups.Values.ToList();
        }

        /// <summary>
        /// Identifies weeks where one artist dominated listening
        /// </summary>
        private List<ObsessionWeek> FindObsessionWeeks(List<WeeklyListeningData> weeklyData)
        {
            var obsessionWeeks = new List<ObsessionWeek>();

            foreach (var week in weeklyData)
            {
                if (!week.ArtistHours.Any()) continue;

                var topArtist = week.ArtistHours.OrderByDescending(a => a.Value).First();
                var dominancePercentage = topArtist.Value / week.TotalHours;

                if (dominancePercentage >= DOMINANCE_THRESHOLD && topArtist.Value >= MIN_ARTIST_HOURS)
                {
                    obsessionWeeks.Add(new ObsessionWeek
                    {
                        WeekNumber = week.WeekNumber,
                        Year = week.Year,
                        WeekStart = week.WeekStart,
                        Artist = topArtist.Key,
                        ArtistHours = topArtist.Value,
                        TotalHours = week.TotalHours,
                        DominancePercentage = dominancePercentage,
                        AllArtistHours = new Dictionary<string, double>(week.ArtistHours)
                    });
                }
            }

            return obsessionWeeks;
        }

        /// <summary>
        /// Merges consecutive weeks with the same artist into obsession periods
        /// </summary>
        private List<ObsessionPeriod> MergeConsecutiveWeeks(List<ObsessionWeek> obsessionWeeks)
        {
            var periods = new List<ObsessionPeriod>();
            if (!obsessionWeeks.Any()) return periods;

            var sortedWeeks = obsessionWeeks.OrderBy(w => w.WeekNumber).ToList();
            var currentPeriod = new ObsessionPeriod
            {
                Artist = sortedWeeks[0].Artist,
                StartWeek = sortedWeeks[0].WeekNumber,
                EndWeek = sortedWeeks[0].WeekNumber,
                Year = sortedWeeks[0].Year,
                StartDate = sortedWeeks[0].WeekStart,
                Weeks = new List<ObsessionWeek> { sortedWeeks[0] }
            };

            for (int i = 1; i < sortedWeeks.Count; i++)
            {
                var week = sortedWeeks[i];
                
                // Check if this week continues the current obsession
                if (week.Artist.Equals(currentPeriod.Artist, StringComparison.OrdinalIgnoreCase) &&
                    week.WeekNumber == currentPeriod.EndWeek + 1)
                {
                    // Extend current period
                    currentPeriod.EndWeek = week.WeekNumber;
                    currentPeriod.Weeks.Add(week);
                }
                else
                {
                    // Finalize current period and start new one
                    CalculatePeriodMetrics(currentPeriod);
                    periods.Add(currentPeriod);
                    
                    currentPeriod = new ObsessionPeriod
                    {
                        Artist = week.Artist,
                        StartWeek = week.WeekNumber,
                        EndWeek = week.WeekNumber,
                        Year = week.Year,
                        StartDate = week.WeekStart,
                        Weeks = new List<ObsessionWeek> { week }
                    };
                }
            }

            // Don't forget the last period
            CalculatePeriodMetrics(currentPeriod);
            periods.Add(currentPeriod);

            return periods;
        }

        /// <summary>
        /// Calculates metrics for an obsession period
        /// </summary>
        private void CalculatePeriodMetrics(ObsessionPeriod period)
        {
            period.TotalHours = period.Weeks.Sum(w => w.ArtistHours);
            period.AverageDominance = period.Weeks.Average(w => w.DominancePercentage);
            period.PeakDominance = period.Weeks.Max(w => w.DominancePercentage);
            period.Duration = period.EndWeek - period.StartWeek + 1;
            period.EndDate = GetWeekStart(period.Year, period.EndWeek).AddDays(6);
            
            // Calculate intensity score (combination of hours, dominance, and duration)
            period.IntensityScore = period.TotalHours * period.AverageDominance * Math.Log(period.Duration + 1);
        }

        /// <summary>
        /// Gets the start date of a specific week in a year
        /// </summary>
        private DateTime GetWeekStart(int year, int weekOfYear)
        {
            // Create a date for January 4th of the year (always in week 1 of ISO week system)
            var jan4 = new DateTime(year, 1, 4);
            
            // Find the Monday of the week containing January 4th
            var daysFromMonday = ((int)jan4.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var firstMondayOfYear = jan4.AddDays(-daysFromMonday);
            
            // Calculate the Monday of the requested week
            return firstMondayOfYear.AddDays((weekOfYear - 1) * 7);
        }

        #endregion

        #region HTML Generation

        private string GenerateObsessionOverview(List<ObsessionPeriod> obsessions, int year)
        {
            var totalObsessionHours = obsessions.Sum(o => o.TotalHours);
            var avgDominance = obsessions.Average(o => o.AverageDominance);

            return $@"
                <div class='obsession-overview'>
                    <div class='overview-header'>
                        <div class='category-tag'>Deep Analysis</div>
                        <h2 class='obsession-title'>
                            <span class='title-line-1'>Your</span>
                            <span class='title-line-2'>Musical</span>
                            <span class='title-line-3'>Obsessions</span>
                        </h2>
                        <div class='obsession-subtitle'>
                            Periods when you couldn't stop listening to specific artists
                        </div>
                    </div>
                    
                    <div class='overview-stats'>
                        <div class='overview-stat'>
                            <div class='stat-number'>{obsessions.Count}</div>
                            <div class='stat-label'>Obsession{(obsessions.Count != 1 ? "s" : "")} Found</div>
                        </div>
                        <div class='overview-stat'>
                            <div class='stat-number'>{totalObsessionHours:F0}h</div>
                            <div class='stat-label'>Total Deep Listening</div>
                        </div>
                        <div class='overview-stat'>
                            <div class='stat-number'>{avgDominance:P0}</div>
                            <div class='stat-label'>Average Focus</div>
                        </div>
                    </div>
                </div>";
        }

        private string GenerateObsessionDetails(List<ObsessionPeriod> obsessions)
        {
            var detailsHTML = obsessions.Select((obsession, index) => $@"
                <div class='obsession-detail' data-obsession-index='{index}'>
                    <div class='obsession-timeline'>
                        <div class='timeline-marker'></div>
                        <div class='timeline-period'>
                            <div class='period-duration'>{obsession.Duration} week{(obsession.Duration != 1 ? "s" : "")}</div>
                            <div class='period-dates'>{obsession.StartDate:MMM d} - {obsession.EndDate:MMM d}</div>
                        </div>
                    </div>
                    
                    <div class='obsession-artist'>
                        <div class='artist-name'>{EscapeHtml(obsession.Artist)}</div>
                        <div class='artist-stats'>
                            <span class='hours-stat'>{obsession.TotalHours:F1}h</span>
                            <span class='dominance-stat'>{obsession.AverageDominance:P0} focus</span>
                            <span class='peak-stat'>Peak: {obsession.PeakDominance:P0}</span>
                        </div>
                    </div>
                    
                    <div class='obsession-intensity'>
                        <div class='intensity-bar'>
                            <div class='intensity-fill' style='width: {obsession.AverageDominance:P0}'></div>
                        </div>
                        <div class='intensity-label'>Intensity</div>
                    </div>
                </div>").ToArray();

            return $@"
                <div class='obsession-details'>
                    <div class='details-header'>
                        <h3>Your Deep Dives</h3>
                    </div>
                    <div class='details-list'>
                        {string.Join("", detailsHTML)}
                    </div>
                </div>";
        }

        private string GenerateObsessionInsights(List<ObsessionPeriod> obsessions)
        {
            var insight = GenerateObsessionInsightText(obsessions);
            
            return $@"
                <div class='obsession-insights'>
                    <div class='insight-content'>
                        <div class='insight-text'>{insight}</div>
                    </div>
                </div>";
        }

        private string GenerateNoObsessionsHTML(PlayHistory playHistory, int year)
        {
            // Provide some basic analysis even when no obsessions found
            var totalPlays = playHistory?.Plays?.Where(p => p.PlayedAt.Year == year).Count() ?? 0;
            var uniqueArtists = playHistory?.Plays?.Where(p => p.PlayedAt.Year == year)
                .Select(p => p.Artist)
                .Where(a => !string.IsNullOrEmpty(a))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() ?? 0;

            var diversityMessage = uniqueArtists > 50 
                ? "incredibly diverse" 
                : uniqueArtists > 25 
                ? "beautifully diverse" 
                : "nicely varied";

            return $@"
                <div class='no-obsessions-container'>
                    <div class='no-obsessions-content'>
                        <div class='category-tag'>Analysis</div>
                        <h2>Diverse Listening</h2>
                        <p>You maintained a {diversityMessage} listening style in {year}, exploring {uniqueArtists} different artists across {totalPlays:N0} plays. Rather than getting obsessed with any single artist, your musical curiosity kept you discovering new sounds!</p>
                        <div class='diversity-stats'>
                            <div class='diversity-stat'>
                                <div class='stat-number'>{uniqueArtists}</div>
                                <div class='stat-label'>Artists Explored</div>
                            </div>
                            <div class='diversity-stat'>
                                <div class='stat-number'>{totalPlays:N0}</div>
                                <div class='stat-label'>Total Plays</div>
                            </div>
                        </div>
                    </div>
                </div>";
        }

        #endregion

        #region Insight Generation

        private string GenerateObsessionInsight(List<ObsessionPeriod> obsessions, int year)
        {
            if (!obsessions.Any())
            {
                return $"You maintained impressively diverse listening habits in {year}, never getting too caught up in any single artist's orbit. Your musical curiosity kept you exploring!";
            }

            var topObsession = obsessions.First();
            var durationText = topObsession.Duration == 1 ? "week" : "weeks";
            
            return $"Your biggest musical obsession was <strong>{EscapeHtml(topObsession.Artist)}</strong> for {topObsession.Duration} {durationText} " +
                   $"in {topObsession.StartDate:MMMM}, consuming {topObsession.TotalHours:F1} hours and {topObsession.AverageDominance:P0} of your listening time. " +
                   GetObsessionPersonality(obsessions);
        }

        private string GenerateObsessionInsightText(List<ObsessionPeriod> obsessions)
        {
            if (!obsessions.Any()) return "";

            var totalObsessions = obsessions.Count;
            var avgDuration = obsessions.Average(o => o.Duration);
            
            if (totalObsessions == 1)
            {
                return $"You had one major musical obsession this year. When you find something you love, you really dive deep!";
            }
            else
            {
                return $"You experienced {totalObsessions} distinct obsession periods, averaging {avgDuration:F1} weeks each. " +
                       "You're the type who finds an artist and explores their entire universe before moving on.";
            }
        }

        private string GetObsessionPersonality(List<ObsessionPeriod> obsessions)
        {
            var avgDuration = obsessions.Average(o => o.Duration);
            var avgDominance = obsessions.Average(o => o.AverageDominance);

            if (avgDuration >= 3 && avgDominance >= 0.8)
                return "You're a deep-dive listener who commits fully to your musical discoveries.";
            else if (avgDuration >= 2)
                return "You like to really get to know an artist before moving on.";
            else if (avgDominance >= 0.75)
                return "When something clicks, it completely captures your attention.";
            else
                return "You have focused periods of musical exploration.";
        }

        #endregion

        #region CSS Generation

        public override string GenerateCSS()
        {
            return @"
        /* Obsession Periods Slide */
        .obsession-container {
            position: relative;
            width: 100%;
            height: 100vh;
            background: linear-gradient(135deg, #0a0a0a 0%, #1a0a2e 25%, #2d1b4e 50%, #1a0a2e 75%, #0a0a0a 100%);
            overflow: hidden;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .obsession-background {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 1;
        }

        .obsession-particles {
            position: absolute;
            width: 100%;
            height: 100%;
            background-image: 
                radial-gradient(circle at 15% 25%, rgba(147, 51, 234, 0.2) 1px, transparent 1px),
                radial-gradient(circle at 85% 75%, rgba(236, 72, 153, 0.15) 1px, transparent 1px),
                radial-gradient(circle at 45% 60%, rgba(59, 130, 246, 0.1) 1px, transparent 1px);
            background-size: 150px 150px, 200px 200px, 180px 180px;
            animation: obsession-float 25s linear infinite;
        }

        .intensity-waves {
            position: absolute;
            width: 100%;
            height: 100%;
            background: repeating-linear-gradient(
                45deg,
                transparent 0,
                rgba(147, 51, 234, 0.03) 1px,
                rgba(147, 51, 234, 0.03) 2px,
                transparent 3px,
                transparent 20px
            );
            animation: wave-pulse 4s ease-in-out infinite;
        }

        .focus-beams {
            position: absolute;
            width: 100%;
            height: 100%;
            background: 
                linear-gradient(90deg, transparent 0%, rgba(236, 72, 153, 0.1) 50%, transparent 100%),
                linear-gradient(0deg, transparent 0%, rgba(59, 130, 246, 0.08) 50%, transparent 100%);
            animation: beam-sweep 8s ease-in-out infinite;
        }

        .obsession-content {
            position: relative;
            z-index: 2;
            max-width: 90%;
            width: 1200px;
            padding: clamp(2rem, 4vw, 3rem);
        }

        /* Overview Section */
        .obsession-overview {
            text-align: center;
            margin-bottom: clamp(3rem, 6vh, 4rem);
        }

        .overview-header .category-tag {
            display: inline-block;
            padding: 0.5rem 1.5rem;
            background: rgba(147, 51, 234, 0.2);
            border: 1px solid rgba(147, 51, 234, 0.3);
            border-radius: 2rem;
            color: #a855f7;
            font-size: 0.9rem;
            font-weight: 500;
            letter-spacing: 1px;
            text-transform: uppercase;
            margin-bottom: 2rem;
            animation: tag-glow 3s ease-in-out infinite;
        }

        .obsession-title {
            font-size: clamp(2.5rem, 6vw, 4rem);
            font-weight: 700;
            line-height: 1.1;
            margin-bottom: 1.5rem;
            background: linear-gradient(135deg, #ffffff 0%, #e879f9 25%, #a855f7 50%, #3b82f6 75%, #ffffff 100%);
            background-clip: text;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-size: 200% 200%;
            animation: obsession-shimmer 4s ease-in-out infinite;
        }

        .title-line-1, .title-line-2, .title-line-3 {
            display: inline-block;
            animation: title-emerge 0.8s ease-out forwards;
            opacity: 0;
            transform: translateY(20px);
        }

        .title-line-1 { animation-delay: 0.2s; }
        .title-line-2 { animation-delay: 0.4s; }
        .title-line-3 { animation-delay: 0.6s; }

        .obsession-subtitle {
            font-size: clamp(1rem, 2.2vw, 1.3rem);
            color: rgba(255, 255, 255, 0.7);
            font-weight: 300;
            letter-spacing: 0.5px;
            margin-bottom: 3rem;
            animation: subtitle-fade 1s ease-out 1s forwards;
            opacity: 0;
        }

        .overview-stats {
            display: flex;
            justify-content: center;
            gap: clamp(2rem, 6vw, 4rem);
            flex-wrap: wrap;
        }

        .overview-stat {
            text-align: center;
            animation: stat-rise 0.6s ease-out forwards;
            opacity: 0;
            transform: translateY(20px);
        }

        .overview-stat:nth-child(1) { animation-delay: 1.2s; }
        .overview-stat:nth-child(2) { animation-delay: 1.4s; }
        .overview-stat:nth-child(3) { animation-delay: 1.6s; }

        .overview-stat .stat-number {
            font-size: clamp(2rem, 4vw, 2.5rem);
            font-weight: 700;
            color: #e879f9;
            margin-bottom: 0.5rem;
            text-shadow: 0 0 20px rgba(232, 121, 249, 0.5);
        }

        .overview-stat .stat-label {
            font-size: clamp(0.8rem, 1.5vw, 0.9rem);
            color: rgba(255, 255, 255, 0.6);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        /* Details Section */
        .obsession-details {
            margin: clamp(4rem, 8vh, 6rem) 0;
        }

        .details-header h3 {
            font-size: clamp(1.5rem, 3vw, 2rem);
            font-weight: 600;
            color: #ffffff;
            text-align: center;
            margin-bottom: 3rem;
            animation: details-header-fade 1s ease-out 2s forwards;
            opacity: 0;
        }

        .details-list {
            display: flex;
            flex-direction: column;
            gap: 2rem;
        }

        .obsession-detail {
            display: flex;
            align-items: center;
            background: rgba(255, 255, 255, 0.03);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 1rem;
            padding: 1.5rem;
            backdrop-filter: blur(10px);
            animation: detail-slide-in 0.8s ease-out forwards;
            opacity: 0;
            transform: translateX(-50px);
            transition: all 0.3s ease;
        }

        .obsession-detail:nth-child(1) { animation-delay: 2.2s; }
        .obsession-detail:nth-child(2) { animation-delay: 2.4s; }
        .obsession-detail:nth-child(3) { animation-delay: 2.6s; }

        .obsession-detail:hover {
            background: rgba(255, 255, 255, 0.05);
            border-color: rgba(147, 51, 234, 0.3);
            transform: translateY(-5px);
        }

        .obsession-timeline {
            display: flex;
            align-items: center;
            margin-right: 2rem;
            min-width: 120px;
        }

        .timeline-marker {
            width: 12px;
            height: 12px;
            border-radius: 50%;
            background: linear-gradient(45deg, #e879f9, #a855f7);
            margin-right: 1rem;
            box-shadow: 0 0 15px rgba(232, 121, 249, 0.5);
        }

        .timeline-period {
            text-align: left;
        }

        .period-duration {
            font-size: 0.9rem;
            font-weight: 600;
            color: #e879f9;
            margin-bottom: 0.2rem;
        }

        .period-dates {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.6);
        }

        .obsession-artist {
            flex: 1;
            margin-right: 2rem;
        }

        .artist-name {
            font-size: clamp(1.1rem, 2.5vw, 1.4rem);
            font-weight: 600;
            color: #ffffff;
            margin-bottom: 0.5rem;
        }

        .artist-stats {
            display: flex;
            gap: 1rem;
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.7);
        }

        .artist-stats span {
            padding: 0.2rem 0.6rem;
            background: rgba(147, 51, 234, 0.2);
            border-radius: 0.5rem;
        }

        .obsession-intensity {
            min-width: 150px;
            text-align: right;
        }

        .intensity-bar {
            width: 100%;
            height: 8px;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 4px;
            overflow: hidden;
            margin-bottom: 0.5rem;
        }

        .intensity-fill {
            height: 100%;
            background: linear-gradient(90deg, #3b82f6, #a855f7, #e879f9);
            border-radius: 4px;
            animation: intensity-grow 1s ease-out 3s forwards;
            transform: scaleX(0);
            transform-origin: left;
        }

        .intensity-label {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.6);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        /* Insights Section */
        .obsession-insights {
            text-align: center;
            margin-top: clamp(3rem, 5vh, 4rem);
            margin-bottom: clamp(2rem, 4vh, 3rem);
            animation: insights-fade 1s ease-out 3.5s forwards;
            opacity: 0;
        }

        .insight-content {
            max-width: 600px;
            margin: 0 auto;
            padding: 2rem;
            background: rgba(255, 255, 255, 0.02);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 1rem;
            backdrop-filter: blur(10px);
        }

        .insight-text {
            font-size: clamp(1rem, 2vw, 1.1rem);
            color: rgba(255, 255, 255, 0.8);
            line-height: 1.6;
        }

        /* No Obsessions */
        .no-obsessions-container {
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100vh;
            text-align: center;
        }

        .no-obsessions-content {
            max-width: 600px;
            padding: 3rem;
            background: rgba(255, 255, 255, 0.03);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 2rem;
            backdrop-filter: blur(20px);
        }

        .no-obsessions-content .category-tag {
            display: inline-block;
            padding: 0.5rem 1.5rem;
            background: rgba(59, 130, 246, 0.2);
            border: 1px solid rgba(59, 130, 246, 0.3);
            border-radius: 2rem;
            color: #60a5fa;
            font-size: 0.9rem;
            font-weight: 500;
            letter-spacing: 1px;
            text-transform: uppercase;
            margin-bottom: 2rem;
        }

        .no-obsessions-content h2 {
            font-size: clamp(2rem, 5vw, 3rem);
            font-weight: 700;
            color: #ffffff;
            margin-bottom: 1.5rem;
        }

        .no-obsessions-content p {
            font-size: clamp(1rem, 2vw, 1.2rem);
            color: rgba(255, 255, 255, 0.7);
            line-height: 1.6;
            margin-bottom: 2rem;
        }

        .diversity-stats {
            display: flex;
            justify-content: center;
            gap: 3rem;
            margin-top: 2rem;
        }

        .diversity-stat {
            text-align: center;
        }

        .diversity-stat .stat-number {
            font-size: clamp(2rem, 4vw, 2.5rem);
            font-weight: 700;
            color: #60a5fa;
            margin-bottom: 0.5rem;
            text-shadow: 0 0 20px rgba(96, 165, 250, 0.5);
        }

        .diversity-stat .stat-label {
            font-size: clamp(0.8rem, 1.5vw, 0.9rem);
            color: rgba(255, 255, 255, 0.6);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        /* Animations */
        @keyframes obsession-float {
            0% { transform: translateX(0) translateY(0); }
            25% { transform: translateX(10px) translateY(-5px); }
            50% { transform: translateX(-5px) translateY(-10px); }
            75% { transform: translateX(-10px) translateY(5px); }
            100% { transform: translateX(0) translateY(0); }
        }

        @keyframes wave-pulse {
            0%, 100% { opacity: 0.5; transform: scale(1); }
            50% { opacity: 0.8; transform: scale(1.05); }
        }

        @keyframes beam-sweep {
            0% { transform: translateX(-100%); }
            50% { transform: translateX(0%); }
            100% { transform: translateX(100%); }
        }

        @keyframes tag-glow {
            0%, 100% { box-shadow: 0 0 10px rgba(147, 51, 234, 0.3); }
            50% { box-shadow: 0 0 20px rgba(147, 51, 234, 0.6); }
        }

        @keyframes obsession-shimmer {
            0% { background-position: 0% 50%; }
            50% { background-position: 100% 50%; }
            100% { background-position: 0% 50%; }
        }

        @keyframes title-emerge {
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes subtitle-fade {
            to { opacity: 1; }
        }

        @keyframes stat-rise {
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes details-header-fade {
            to { opacity: 1; }
        }

        @keyframes detail-slide-in {
            to { opacity: 1; transform: translateX(0); }
        }

        @keyframes intensity-grow {
            to { transform: scaleX(1); }
        }

        @keyframes insights-fade {
            to { opacity: 1; }
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .obsession-content {
                max-width: 95%;
                padding: clamp(1rem, 3vw, 2rem);
            }

            .overview-stats {
                flex-direction: column;
                gap: 1.5rem;
            }

            .obsession-detail {
                flex-direction: column;
                text-align: center;
                gap: 1rem;
            }

            .obsession-timeline,
            .obsession-artist {
                margin-right: 0;
                margin-bottom: 1rem;
            }

            .obsession-intensity {
                min-width: auto;
                width: 100%;
            }

            .artist-stats {
                justify-content: center;
                flex-wrap: wrap;
            }
        }";
        }

        #endregion

        #region JavaScript Generation

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        // Obsession Periods Slide JavaScript
        function initializeObsessionSlide() {
            const obsessionSlide = document.querySelector('[data-slide-id=""obsession-periods""]');
            if (!obsessionSlide) return;

            // Initialize progressive animations
            initializeObsessionAnimations();
            
            // Add interactive elements
            addObsessionInteractions();
            
            // Animate intensity bars
            animateIntensityBars();
        }

        function initializeObsessionAnimations() {
            // Stagger animations for obsession details
            const details = document.querySelectorAll('.obsession-detail');
            details.forEach((detail, index) => {
                detail.style.animationDelay = `${2.2 + (index * 0.2)}s`;
            });

            // Add dynamic particle effects
            createObsessionParticles();
        }

        function addObsessionInteractions() {
            const details = document.querySelectorAll('.obsession-detail');
            
            details.forEach((detail, index) => {
                detail.addEventListener('mouseenter', () => {
                    // Highlight this obsession period
                    detail.style.transform = 'translateY(-8px) scale(1.02)';
                    detail.style.boxShadow = '0 10px 40px rgba(147, 51, 234, 0.3)';
                });

                detail.addEventListener('mouseleave', () => {
                    detail.style.transform = 'translateY(0) scale(1)';
                    detail.style.boxShadow = 'none';
                });

                // Add click interaction for future expansion
                detail.addEventListener('click', () => {
                    console.log(`Clicked obsession ${index + 1}`);
                    // Future: Could show detailed breakdown of that period
                });
            });
        }

        function animateIntensityBars() {
            const intensityFills = document.querySelectorAll('.intensity-fill');
            
            intensityFills.forEach((fill, index) => {
                setTimeout(() => {
                    fill.style.transformOrigin = 'left';
                    fill.style.transform = 'scaleX(1)';
                    fill.style.transition = 'transform 1s ease-out';
                }, 3000 + (index * 200));
            });
        }

        function createObsessionParticles() {
            const container = document.querySelector('.obsession-particles');
            if (!container) return;

            // Create floating focus particles
            for (let i = 0; i < 20; i++) {
                const particle = document.createElement('div');
                particle.className = 'focus-particle';
                particle.style.cssText = `
                    position: absolute;
                    width: ${Math.random() * 3 + 1}px;
                    height: ${Math.random() * 3 + 1}px;
                    background: linear-gradient(45deg, #e879f9, #a855f7);
                    border-radius: 50%;
                    left: ${Math.random() * 100}%;
                    top: ${Math.random() * 100}%;
                    animation: focus-drift ${Math.random() * 15 + 10}s linear infinite;
                    animation-delay: ${Math.random() * 5}s;
                    opacity: ${Math.random() * 0.7 + 0.3};
                `;
                container.appendChild(particle);
            }

            // Add CSS for focus drift
            const style = document.createElement('style');
            style.textContent = `
                @keyframes focus-drift {
                    0% { transform: translateY(100vh) rotate(0deg); }
                    100% { transform: translateY(-10vh) rotate(360deg); }
                }
            `;
            document.head.appendChild(style);
        }

        // Initialize when slide becomes active
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(initializeObsessionSlide, 100);
        });

        document.addEventListener('slideChanged', function(event) {
            setTimeout(() => {
                const activeSlide = document.querySelector('.slide.active');
                if (activeSlide && activeSlide.querySelector('[data-slide-id=""obsession-periods""]')) {
                    initializeObsessionSlide();
                }
            }, 100);
        });";
        }

        #endregion

        #region Data Models

        /// <summary>
        /// Represents listening data for a specific week
        /// </summary>
        private class WeeklyListeningData
        {
            public int WeekNumber { get; set; }
            public int Year { get; set; }
            public DateTime WeekStart { get; set; }
            public Dictionary<string, double> ArtistHours { get; set; }
            public double TotalHours { get; set; }
        }

        /// <summary>
        /// Represents a week where one artist dominated listening
        /// </summary>
        private class ObsessionWeek
        {
            public int WeekNumber { get; set; }
            public int Year { get; set; }
            public DateTime WeekStart { get; set; }
            public string Artist { get; set; }
            public double ArtistHours { get; set; }
            public double TotalHours { get; set; }
            public double DominancePercentage { get; set; }
            public Dictionary<string, double> AllArtistHours { get; set; }
        }

        /// <summary>
        /// Represents a complete obsession period (one or more consecutive weeks)
        /// </summary>
        private class ObsessionPeriod
        {
            public string Artist { get; set; }
            public int StartWeek { get; set; }
            public int EndWeek { get; set; }
            public int Year { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<ObsessionWeek> Weeks { get; set; }
            public double TotalHours { get; set; }
            public double AverageDominance { get; set; }
            public double PeakDominance { get; set; }
            public int Duration { get; set; }
            public double IntensityScore { get; set; }
        }

        #endregion
    }
}
