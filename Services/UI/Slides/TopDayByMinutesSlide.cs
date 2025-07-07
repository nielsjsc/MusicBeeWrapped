using System;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide showing the top day by minutes listened
    /// </summary>
    public class TopDayByMinutesSlide : SlideComponentBase
    {
        public override string SlideId => "top-day-by-minutes";
        public override string SlideTitle => "Top Day by Minutes";
        public override int SlideOrder => 1;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var topDay = GetTopDay(playHistory, year);
            
            var content = $@"
                <div class='top-day-container'>
                    <div class='background-elements'>
                        <div class='celestial-orb'></div>
                        <div class='orbital-rings'></div>
                        <div class='star-field'></div>
                        <div class='earth-rotation'></div>
                        <div class='aurora-lights'></div>
                    </div>
                    
                    <div class='content-layout'>
                        <div class='header-section'>
                            <div class='category-tag'>Peak Performance</div>
                            <h2 class='slide-title'>
                                <span class='title-line-1'>Your</span>
                                <span class='title-line-2'>Biggest</span>
                                <span class='title-line-3'>Music Day</span>
                            </h2>
                        </div>
                        
                        <div class='orbital-center-text'>
                            <div class='subtitle-text'>Your top day of the year</div>
                        </div>
                        
                        <div class='stats-right-column'>
                            <div class='stat-circle'>
                                <div class='stat-number'>{topDay.EstimatedMinutes:N0}</div>
                                <div class='stat-unit'>minutes</div>
                            </div>
                            
                            <div class='secondary-stat'>
                                <div class='stat-value'>{topDay.TrackCount}</div>
                                <div class='stat-label'>Songs Played</div>
                                <div class='stat-accent'></div>
                            </div>
                            
                            <div class='secondary-stat'>
                                <div class='stat-value'>{(topDay.EstimatedMinutes / 60.0):F1}h</div>
                                <div class='stat-label'>Total Time</div>
                                <div class='stat-accent'></div>
                            </div>
                        </div>
                        
                        <div class='main-showcase'>
                            <div class='date-showcase'>
                                <div class='day-indicator'>
                                    <div class='day-of-week'>{topDay.DayOfWeek}</div>
                                    <div class='day-connector'></div>
                                </div>
                                <div class='date-display'>
                                    <div class='month-day'>{topDay.Date:MMMM d}</div>
                                    <div class='year-display'>{topDay.Date:yyyy}</div>
                                </div>
                            </div>
                        </div>
                        

                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var topDay = GetTopDay(playHistory, year);
            return GetDayInsight(topDay, year);
        }

        public override string GetInsightTitle(WrappedStatistics stats)
        {
            return "Top Music Day";
        }

        private TopDayInfo GetTopDay(PlayHistory playHistory, int year)
        {
            if (playHistory?.Plays == null || !playHistory.Plays.Any())
            {
                return new TopDayInfo 
                { 
                    Date = new DateTime(year, 1, 1), 
                    TrackCount = 0, 
                    EstimatedMinutes = 0,
                    DayOfWeek = "Unknown"
                };
            }

            var dayGroups = playHistory.Plays
                .Where(t => t.PlayedAt.Year == year)
                .GroupBy(t => t.PlayedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TrackCount = g.Count(),
                    EstimatedMinutes = g.Count() * 3.5 // Average song length
                })
                .OrderByDescending(d => d.EstimatedMinutes)
                .FirstOrDefault();

            if (dayGroups == null)
            {
                return new TopDayInfo 
                { 
                    Date = new DateTime(year, 1, 1), 
                    TrackCount = 0, 
                    EstimatedMinutes = 0,
                    DayOfWeek = "Unknown"
                };
            }

            return new TopDayInfo
            {
                Date = dayGroups.Date,
                TrackCount = dayGroups.TrackCount,
                EstimatedMinutes = dayGroups.EstimatedMinutes,
                DayOfWeek = dayGroups.Date.DayOfWeek.ToString()
            };
        }

        private string GetDayInsight(TopDayInfo topDay, int year)
        {
            var dayName = topDay.Date.DayOfWeek.ToString();
            var minutes = topDay.EstimatedMinutes;

            if (minutes >= 300) // 5+ hours
                return $"On {topDay.Date:MMMM d}, you had an epic {minutes:N0}-minute music marathon! That's some serious dedication to your soundtrack. 🎧";
            else if (minutes >= 180) // 3+ hours  
                return $"You spent {minutes:N0} minutes jamming on {topDay.Date:MMMM d}. Sounds like the perfect {dayName} to us! 🎵";
            else if (minutes >= 120) // 2+ hours
                return $"Your top music day was {topDay.Date:MMMM d} with {minutes:N0} minutes of listening. Quality over quantity! ✨";
            else
                return $"Even your biggest music day shows you know how to savor good songs. {topDay.Date:MMMM d} was special! 🎶";
        }

        private string GetProfessionalInsight(TopDayInfo topDay, int year)
        {
            var dayName = topDay.Date.DayOfWeek.ToString();
            var minutes = topDay.EstimatedMinutes;
            var hours = minutes / 60.0;

            if (minutes >= 300) // 5+ hours
                return $"On {topDay.Date:MMMM d}, you experienced a {hours:F1}-hour musical journey that defined your year. This was pure dedication to your craft.";
            else if (minutes >= 180) // 3+ hours  
                return $"This {dayName} became legendary with {hours:F1} hours of continuous discovery. Some days are just meant for music.";
            else if (minutes >= 120) // 2+ hours
                return $"Quality listening defined {topDay.Date:MMMM d}. In {hours:F1} hours, you curated the perfect soundtrack for your day.";
            else
                return $"Even your peak day shows intentional listening. {topDay.Date:MMMM d} was about finding the right moments for the right songs.";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) && 
                   playHistory?.Plays != null && 
                   playHistory.Plays.Any();
        }

        public override string GenerateCSS()
        {
            return @"
        /* Top Day by Minutes - Professional Design */
        .top-day-container {
            position: relative;
            width: 100%;
            height: 100vh;
            overflow: hidden;
            background: linear-gradient(145deg, #0a0a0a 0%, #1a1a2e 30%, #16213e 60%, #2d1b69 100%);
            display: flex;
            align-items: center;
            justify-content: center;
        }

        /* Background Elements - Celestial Theme */
        .background-elements {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 1;
            overflow: hidden;
        }

        .celestial-orb {
            position: absolute;
            top: 10%;
            right: 15%;
            width: clamp(80px, 12vw, 150px);
            height: clamp(80px, 12vw, 150px);
            background: radial-gradient(circle at 30% 30%, #ffffff 0%, #e0e0e0 40%, #2d1b69 100%);
            border-radius: 50%;
            box-shadow: 
                0 0 clamp(40px, 6vw, 80px) rgba(255, 255, 255, 0.2),
                inset clamp(-15px, -2vw, -25px) clamp(-15px, -2vw, -25px) clamp(20px, 3vw, 40px) rgba(45, 27, 105, 0.3);
            animation: orb-glow 8s ease-in-out infinite;
        }

        .orbital-rings {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: clamp(300px, 40vw, 500px);
            height: clamp(300px, 40vw, 500px);
        }

        .orbital-rings::before,
        .orbital-rings::after {
            content: '';
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            border: 0.1vw solid rgba(255, 255, 255, 0.1);
            border-radius: 50%;
            animation: orbital-rotation 30s linear infinite;
        }

        .orbital-rings::before {
            width: clamp(220px, 30vw, 380px);
            height: clamp(220px, 30vw, 380px);
            border-style: dashed;
            animation-duration: 25s;
        }

        .orbital-rings::after {
            width: clamp(150px, 20vw, 250px);
            height: clamp(150px, 20vw, 250px);
            border-style: dotted;
            animation-duration: 35s;
            animation-direction: reverse;
        }

        .star-field {
            position: absolute;
            width: 100%;
            height: 100%;
            background-image: 
                radial-gradient(circle at 20% 30%, rgba(255, 255, 255, 0.8) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 70% 20%, rgba(255, 255, 255, 0.6) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 40% 70%, rgba(255, 255, 255, 0.7) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 90% 80%, rgba(255, 255, 255, 0.5) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 10% 90%, rgba(255, 255, 255, 0.9) 0.1vw, transparent 0.1vw);
            animation: star-twinkle 12s ease-in-out infinite;
        }

        .earth-rotation {
            position: absolute;
            bottom: 20%;
            left: 10%;
            width: clamp(60px, 8vw, 100px);
            height: clamp(60px, 8vw, 100px);
            background: conic-gradient(from 0deg, #1a4c96 0deg, #16213e 120deg, #0f1419 240deg, #1a4c96 360deg);
            border-radius: 50%;
            box-shadow: 
                0 0 clamp(30px, 4vw, 50px) rgba(26, 76, 150, 0.3),
                inset clamp(-10px, -1.5vw, -20px) clamp(-10px, -1.5vw, -20px) clamp(15px, 2vw, 25px) rgba(0, 0, 0, 0.3);
            animation: earth-spin 20s linear infinite;
        }

        .aurora-lights {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: linear-gradient(45deg, 
                transparent 0%, 
                rgba(45, 27, 105, 0.1) 25%, 
                transparent 50%, 
                rgba(22, 33, 62, 0.1) 75%, 
                transparent 100%);
            animation: aurora-wave 15s ease-in-out infinite;
        }

        /* Content Layout */
        .content-layout {
            position: relative;
            z-index: 2;
            width: 90%;
            max-width: 1000px;
            display: grid;
            grid-template-rows: auto 1fr;
            gap: clamp(2rem, 5vh, 4rem);
            height: 80vh;
        }

        /* Header Section */
        .header-section {
            text-align: center;
            animation: header-entrance 1.2s ease-out;
        }

        .category-tag {
            display: inline-block;
            padding: clamp(0.5rem, 1vw, 0.8rem) clamp(1rem, 2vw, 2rem);
            background: rgba(45, 27, 105, 0.3);
            border: 0.1vw solid rgba(45, 27, 105, 0.5);
            border-radius: 25px;
            font-size: clamp(0.8rem, 1.5vw, 1rem);
            font-weight: 500;
            color: rgba(255, 255, 255, 0.8);
            letter-spacing: 0.1vw;
            margin-bottom: clamp(1.5rem, 3vh, 3rem);
            backdrop-filter: blur(10px);
            animation: tag-glow 3s ease-in-out infinite;
        }

        .slide-title {
            font-size: clamp(2.5rem, 6vw, 4rem);
            font-weight: 700;
            line-height: 1.1;
            margin-bottom: clamp(1rem, 2vh, 2rem);
            background: linear-gradient(135deg, #ffffff 0%, #e0e0e0 50%, #2d1b69 100%);
            background-clip: text;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-size: 200% 200%;
            animation: title-shimmer 4s ease-in-out infinite;
        }

        .title-line-1,
        .title-line-2,
        .title-line-3 {
            display: block;
            opacity: 0;
            transform: translateY(clamp(20px, 3vh, 40px));
            animation: line-entrance 0.8s ease-out forwards;
        }

        .title-line-1 { animation-delay: 0.2s; }
        .title-line-2 { animation-delay: 0.4s; }
        .title-line-3 { animation-delay: 0.6s; }

        .subtitle-text {
            font-size: clamp(1rem, 2vw, 1.2rem);
            color: rgba(255, 255, 255, 0.6);
            font-weight: 300;
            letter-spacing: 0.1vw;
            opacity: 0;
            animation: subtitle-fade-in 1s ease-out 0.8s forwards;
        }

        /* Orbital Center Text */
        .orbital-center-text {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            z-index: 2;
            text-align: center;
        }

        /* Stats Right Column */
        .stats-right-column {
            position: absolute;
            top: 50%;
            left: 75%;
            transform: translateY(-50%);
            display: flex;
            flex-direction: column;
            gap: clamp(1.5rem, 3vh, 3rem);
            align-items: center;
            z-index: 3;
        }

        .stats-right-column .stat-circle {
            width: clamp(160px, 20vw, 240px);
            height: clamp(160px, 20vw, 240px);
            border-radius: 50%;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            position: relative;
            animation: circle-subtle-glow 4s ease-in-out infinite;
        }

        .stats-right-column .secondary-stat {
            text-align: center;
            opacity: 0;
            transform: translateY(clamp(15px, 2vh, 25px));
            animation: secondary-entrance 0.6s ease-out forwards;
        }

        .stats-right-column .secondary-stat:first-of-type { animation-delay: 1.8s; }
        .stats-right-column .secondary-stat:last-of-type { animation-delay: 2s; }

        .stats-right-column .stat-value {
            font-size: clamp(1.5rem, 3vw, 2rem);
            font-weight: 700;
            color: #ffffff;
            text-shadow: 0 0 clamp(15px, 2vw, 25px) rgba(255, 255, 255, 0.3);
            margin-bottom: clamp(0.3rem, 0.5vh, 0.5rem);
        }

        .stats-right-column .stat-label {
            font-size: clamp(0.8rem, 1.5vw, 1rem);
            color: rgba(255, 255, 255, 0.6);
            margin-bottom: clamp(0.6rem, 1vh, 1rem);
            letter-spacing: 0.1vw;
        }

        .stats-right-column .stat-accent {
            width: clamp(30px, 4vw, 50px);
            height: 0.2vh;
            background: linear-gradient(90deg, #2d1b69, transparent);
            border-radius: 1px;
            transform: scaleX(0);
            animation: accent-grow 0.8s ease-out forwards;
            margin: 0 auto;
        }

        .stats-right-column .secondary-stat:first-of-type .stat-accent { animation-delay: 2.2s; }
        .stats-right-column .secondary-stat:last-of-type .stat-accent { animation-delay: 2.4s; }

        /* Main Showcase */
        .main-showcase {
            display: grid;
            grid-template-columns: 1fr 1fr 1fr;
            gap: clamp(2rem, 4vw, 4rem);
            align-items: center;
            max-width: 1200px;
            margin: 0 auto;
        }

        /* Date Showcase */
        .date-showcase {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: clamp(1rem, 2vh, 2rem);
            opacity: 0;
            transform: translateX(clamp(-30px, -5vw, -60px));
            animation: date-entrance 1s ease-out 1s forwards;
        }

        .day-indicator {
            display: flex;
            align-items: center;
            gap: clamp(0.8rem, 1.5vw, 1.5rem);
        }

        .day-of-week {
            font-size: clamp(0.8rem, 1.5vw, 1rem);
            font-weight: 600;
            color: #2d1b69;
            letter-spacing: 0.2vw;
            background: rgba(45, 27, 105, 0.1);
            padding: clamp(0.6rem, 1vh, 1rem) clamp(1rem, 2vw, 1.5rem);
            border-radius: 8px;
            writing-mode: horizontal-tb;
        }

        .day-connector {
            width: clamp(30px, 4vw, 50px);
            height: 0.2vh;
            background: linear-gradient(to right, #2d1b69, transparent);
            animation: connector-grow 1s ease-out 1.5s forwards;
            transform: scaleX(0);
            transform-origin: left;
        }

        .date-display {
            text-align: center;
        }

        .month-day {
            font-size: clamp(2.5rem, 5vw, 3.5rem);
            font-weight: 700;
            color: #ffffff;
            line-height: 1;
            margin-bottom: clamp(0.5rem, 1vh, 1rem);
        }

        .year-display {
            font-size: clamp(1rem, 2vw, 1.3rem);
            color: rgba(255, 255, 255, 0.5);
            font-weight: 300;
            letter-spacing: 0.2vw;
        }

        /* Secondary Stats - Vertical Layout */
        .secondary-stats-vertical {
            display: flex;
            flex-direction: column;
            gap: 1.5rem;
            align-self: flex-start;
        }

        .secondary-stats-vertical .secondary-stat {
            text-align: left;
            opacity: 0;
            transform: translateY(20px);
            animation: secondary-entrance 0.6s ease-out forwards;
        }

        /* Secondary Stats - Side Layout */
        .secondary-stats-side {
            display: flex;
            flex-direction: column;
            gap: 2rem;
            align-items: center;
        }

        .secondary-stats-side .secondary-stat {
            text-align: center;
            opacity: 0;
            transform: translateY(20px);
            animation: secondary-entrance 0.6s ease-out forwards;
        }

        .secondary-stats-side .secondary-stat:first-child { animation-delay: 1.8s; }
        .secondary-stats-side .secondary-stat:last-child { animation-delay: 2s; }

        .secondary-stats-side .stat-value {
            font-size: 1.8rem;
            font-weight: 700;
            color: #ffffff;
            text-shadow: 0 0 20px rgba(255, 255, 255, 0.3);
            margin-bottom: 0.3rem;
        }

        .secondary-stats-side .stat-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.6);
            margin-bottom: 0.8rem;
            letter-spacing: 1px;
        }

        .secondary-stats-side .stat-accent {
            width: 40px;
            height: 2px;
            background: linear-gradient(90deg, #2d1b69, transparent);
            border-radius: 1px;
            transform: scaleX(0);
            animation: accent-grow 0.8s ease-out forwards;
            margin: 0 auto;
        }

        .secondary-stats-side .secondary-stat:first-child .stat-accent { animation-delay: 2.2s; }
        .secondary-stats-side .secondary-stat:last-child .stat-accent { animation-delay: 2.4s; }

        /* Stats Visualization */
        .stats-visualization {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: clamp(300px, 40vw, 500px);
            height: clamp(300px, 40vw, 500px);
            z-index: 3;
            opacity: 0;
            animation: stats-entrance 1s ease-out 1.2s forwards;
            background: none !important;
            border: none !important;
            box-shadow: none !important;
            outline: none !important;
            backdrop-filter: none !important;
        }

        .primary-stat {
            display: flex;
            justify-content: center;
            align-items: center;
            background: none !important;
            border: none !important;
            box-shadow: none !important;
            outline: none !important;
            backdrop-filter: none !important;
        }

        .stat-circle {
            width: clamp(160px, 20vw, 240px);
            height: clamp(160px, 20vw, 240px);
            border: 0.2vw solid rgba(255, 255, 255, 0.15);
            border-radius: 50%;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            position: relative;
            backdrop-filter: blur(20px);
            animation: circle-subtle-glow 4s ease-in-out infinite;
        }

        .stat-number {
            font-size: clamp(2.5rem, 5vw, 3.5rem);
            font-weight: 700;
            color: #ffffff;
            text-shadow: 0 0 clamp(20px, 3vw, 40px) rgba(255, 255, 255, 0.4);
            margin-bottom: clamp(0.5rem, 1vh, 1rem);
            animation: number-count 2s ease-out 1.5s forwards;
        }

        .stat-unit {
            font-size: clamp(0.9rem, 1.5vw, 1.1rem);
            color: rgba(255, 255, 255, 0.7);
            letter-spacing: 0.2vw;
            text-transform: uppercase;
            font-weight: 300;
        }

        /* Secondary Stats */
        .secondary-stats {
            display: flex;
            gap: clamp(2rem, 4vw, 4rem);
            justify-content: center;
        }

        .secondary-stat {
            text-align: center;
            opacity: 0;
            transform: translateY(clamp(15px, 2vh, 25px));
            animation: secondary-entrance 0.6s ease-out forwards;
        }

        .secondary-stat:first-child { animation-delay: 1.8s; }
        .secondary-stat:last-child { animation-delay: 2s; }

        .secondary-stat .stat-value {
            font-size: clamp(1.5rem, 3vw, 2rem);
            font-weight: 700;
            color: #ffffff;
            text-shadow: 0 0 clamp(15px, 2vw, 25px) rgba(255, 255, 255, 0.3);
            margin-bottom: clamp(0.4rem, 0.8vh, 0.6rem);
        }

        .secondary-stat .stat-label {
            font-size: clamp(0.8rem, 1.5vw, 1rem);
            color: rgba(255, 255, 255, 0.6);
            margin-bottom: clamp(0.8rem, 1.5vh, 1.2rem);
            letter-spacing: 0.1vw;
        }

        .stat-bar {
            width: clamp(50px, 6vw, 80px);
            height: 0.3vh;
            background: #2d1b69;
            margin: 0 auto;
            border-radius: 2px;
            transform: scaleX(0);
            animation: bar-grow 0.8s ease-out forwards;
        }

        .secondary-stat:first-child .stat-bar { animation-delay: 2.2s; }
        .secondary-stat:last-child .stat-bar { animation-delay: 2.4s; }

        /* Celestial Animations */
        @keyframes orb-glow {
            0%, 100% { 
                box-shadow: 0 0 60px rgba(255, 255, 255, 0.2), inset -20px -20px 30px rgba(45, 27, 105, 0.3); 
            }
            50% { 
                box-shadow: 0 0 80px rgba(255, 255, 255, 0.4), inset -20px -20px 30px rgba(45, 27, 105, 0.1); 
            }
        }

        @keyframes orbital-rotation {
            from { transform: translate(-50%, -50%) rotate(0deg); }
            to { transform: translate(-50%, -50%) rotate(360deg); }
        }

        @keyframes star-twinkle {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.6; }
        }

        @keyframes earth-spin {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
        }

        @keyframes aurora-wave {
            0%, 100% { 
                background: linear-gradient(45deg, transparent 0%, rgba(45, 27, 105, 0.1) 25%, transparent 50%, rgba(22, 33, 62, 0.1) 75%, transparent 100%); 
            }
            50% { 
                background: linear-gradient(45deg, transparent 0%, rgba(22, 33, 62, 0.1) 25%, transparent 50%, rgba(45, 27, 105, 0.1) 75%, transparent 100%); 
            }
        }

        @keyframes header-entrance {
            from { opacity: 0; transform: translateY(-30px); }
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes tag-glow {
            0%, 100% { box-shadow: 0 0 10px rgba(45, 27, 105, 0.3); }
            50% { box-shadow: 0 0 20px rgba(45, 27, 105, 0.6); }
        }

        @keyframes title-shimmer {
            0% { background-position: 0% 50%; }
            50% { background-position: 100% 50%; }
            100% { background-position: 0% 50%; }
        }

        @keyframes line-entrance {
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes subtitle-fade-in {
            to { opacity: 1; }
        }

        @keyframes date-entrance {
            to { opacity: 1; transform: translateX(0); }
        }

        @keyframes connector-grow {
            to { transform: scaleX(1); }
        }

        @keyframes accent-grow {
            to { transform: scaleX(1); }
        }

        @keyframes circle-subtle-glow {
            0%, 100% { 
                box-shadow: 0 0 30px rgba(255, 255, 255, 0.1); 
                border-color: rgba(255, 255, 255, 0.1);
            }
            50% { 
                box-shadow: 0 0 50px rgba(255, 255, 255, 0.2); 
                border-color: rgba(255, 255, 255, 0.2);
            }
        }

        @keyframes stats-entrance {
            to { opacity: 1; transform: translate(-50%, -50%); }
        }

        @keyframes number-count {
            from { transform: scale(0.8); opacity: 0; }
            to { transform: scale(1); opacity: 1; }
        }

        @keyframes secondary-entrance {
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes bar-grow {
            to { transform: scaleX(1); }
        }

        /* Responsive Design */
        @media (max-width: 60rem) {
            .main-showcase {
                grid-template-columns: 1fr;
                gap: clamp(1.5rem, 3vw, 2.5rem);
                text-align: center;
            }
            
            .stats-right-column {
                position: relative;
                top: auto;
                left: auto;
                transform: none;
                flex-direction: row;
                justify-content: center;
                gap: clamp(2rem, 4vw, 4rem);
            }
            
            .date-showcase {
                order: 1;
            }

            .stats-visualization {
                position: relative;
                top: auto;
                left: auto;
                transform: none;
                order: 3;
                width: clamp(250px, 35vw, 400px);
                height: clamp(250px, 35vw, 400px);
            }
        }

        @media (max-width: 48rem) {
            .stats-right-column {
                flex-direction: column;
                gap: clamp(1.2rem, 2.5vh, 2rem);
            }
            
            .stat-circle {
                width: clamp(140px, 18vw, 200px);
                height: clamp(140px, 18vw, 200px);
            }

            .stat-number {
                font-size: clamp(2rem, 4vw, 2.8rem);
            }

            .content-layout {
                gap: clamp(1.5rem, 3vh, 2.5rem);
            }
        }

        @media (max-width: 30rem) {
            .stat-circle {
                width: clamp(120px, 16vw, 160px);
                height: clamp(120px, 16vw, 160px);
            }

            .stat-number {
                font-size: clamp(1.8rem, 3.5vw, 2.2rem);
            }

            .stats-right-column {
                gap: clamp(1rem, 2vh, 1.5rem);
            }
        }";
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        // Professional Top Day Slide Enhancements
        function initializeTopDaySlide() {
            const topDaySlide = document.querySelector('[data-slide-id=""top-day-by-minutes""]');
            if (!topDaySlide) return;

            // Enhanced number counter animation
            animateTopDayCounters();
            
            // Dynamic progress bar animation
            animateProgressBars();
            
            // Interactive celestial particle system
            createTopDayParticles();
            
            // Celestial effect synchronization
            synchronizeCelestialEffects();
            
            // Smooth entrance orchestration
            orchestrateEntranceSequence();
        }

        function animateTopDayCounters() {
            const statNumbers = document.querySelectorAll('.top-day-container .stat-number');
            
            statNumbers.forEach((element, index) => {
                setTimeout(() => {
                    const finalValue = element.textContent;
                    const numericValue = parseInt(finalValue.replace(/[^\d]/g, ''));
                    
                    if (!isNaN(numericValue)) {
                        animateCounterWithEasing(element, 0, numericValue, 2000, finalValue);
                    }
                }, 1500 + (index * 200));
            });
        }

        function animateCounterWithEasing(element, start, end, duration, finalText) {
            const startTime = performance.now();
            
            function updateCounter(currentTime) {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / duration, 1);
                
                // Cubic ease-out for smooth deceleration
                const easeOut = 1 - Math.pow(1 - progress, 3);
                const current = Math.floor(start + (end - start) * easeOut);
                
                element.textContent = current.toLocaleString();
                element.style.transform = `scale(${1 + Math.sin(progress * Math.PI) * 0.1})`;
                
                if (progress < 1) {
                    requestAnimationFrame(updateCounter);
                } else {
                    element.textContent = finalText;
                    element.style.transform = 'scale(1)';
                }
            }
            
            requestAnimationFrame(updateCounter);
        }

        function animateProgressBars() {
            const statAccents = document.querySelectorAll('.stat-accent');
            
            // Animate accent bars with stagger
            statAccents.forEach((accent, index) => {
                setTimeout(() => {
                    accent.style.transform = 'scaleX(1)';
                    accent.style.boxShadow = `0 0 5px rgba(45, 27, 105, 0.4)`;
                }, 2200 + (index * 200));
            });
        }

        function createTopDayParticles() {
            const container = document.querySelector('.top-day-container');
            if (!container) return;

            // Create celestial stardust particles
            for (let i = 0; i < 15; i++) {
                const particle = document.createElement('div');
                particle.className = 'stardust-particle';
                particle.style.cssText = `
                    position: absolute;
                    width: ${Math.random() * 3 + 1}px;
                    height: ${Math.random() * 3 + 1}px;
                    background: radial-gradient(circle, rgba(255, 255, 255, 0.9), rgba(255, 255, 255, 0.3));
                    border-radius: 50%;
                    left: ${Math.random() * 100}%;
                    top: ${Math.random() * 100}%;
                    animation: stardust-float ${Math.random() * 20 + 15}s ease-in-out infinite;
                    animation-delay: ${Math.random() * 5}s;
                    pointer-events: none;
                    z-index: 1;
                    box-shadow: 0 0 4px rgba(255, 255, 255, 0.6);
                `;
                container.appendChild(particle);
            }

            // Add CSS for celestial particle animation
            const style = document.createElement('style');
            style.textContent = `
                @keyframes stardust-float {
                    0%, 100% { 
                        transform: translateY(0) translateX(0) rotate(0deg) scale(1); 
                        opacity: 0.4; 
                    }
                    25% { 
                        transform: translateY(-40px) translateX(30px) rotate(90deg) scale(1.2); 
                        opacity: 1; 
                    }
                    50% { 
                        transform: translateY(-20px) translateX(-25px) rotate(180deg) scale(0.8); 
                        opacity: 0.7; 
                    }
                    75% { 
                        transform: translateY(-50px) translateX(15px) rotate(270deg) scale(1.1); 
                        opacity: 0.9; 
                    }
                }
            `;
            document.head.appendChild(style);
        }

        function synchronizeCelestialEffects() {
            const celestialOrb = document.querySelector('.celestial-orb');
            const orbitalRings = document.querySelector('.orbital-rings');
            
            if (!celestialOrb || !orbitalRings) return;
            
            // Add dynamic celestial intensity based on stats
            const statCircle = document.querySelector('.stat-circle');
            if (statCircle) {
                let intensity = 1;
                
                function updateCelestialEffects() {
                    intensity = 0.7 + Math.sin(Date.now() * 0.001) * 0.3;
                    
                    // Pulsing celestial orb
                    celestialOrb.style.transform = `scale(${0.9 + intensity * 0.2})`;
                    celestialOrb.style.filter = `brightness(${0.8 + intensity * 0.4})`;
                    
                    // Orbital rings intensity
                    orbitalRings.style.opacity = 0.4 + intensity * 0.3;
                    
                    requestAnimationFrame(updateCelestialEffects);
                }
                
                setTimeout(updateCelestialEffects, 1500);
            }
        }

        function orchestrateEntranceSequence() {
            const elements = [
                { selector: '.category-tag', delay: 100 },
                { selector: '.title-line-1', delay: 200 },
                { selector: '.title-line-2', delay: 400 },
                { selector: '.title-line-3', delay: 600 },
                { selector: '.subtitle-text', delay: 800 },
                { selector: '.date-showcase', delay: 1000 },
                { selector: '.stats-visualization', delay: 1200 }
            ];
            
            elements.forEach(({ selector, delay }) => {
                const element = document.querySelector(selector);
                if (element) {
                    setTimeout(() => {
                        element.style.animation = element.style.animation.replace('paused', 'running');
                        element.classList.add('entrance-complete');
                    }, delay);
                }
            });
        }

        // Enhanced celestial mouse interaction effects
        function addTopDayInteractions() {
            const container = document.querySelector('.top-day-container');
            if (!container) return;
            
            container.addEventListener('mousemove', (e) => {
                const rect = container.getBoundingClientRect();
                const x = (e.clientX - rect.left) / rect.width;
                const y = (e.clientY - rect.top) / rect.height;
                
                // Celestial orb parallax movement
                const celestialOrb = container.querySelector('.celestial-orb');
                if (celestialOrb) {
                    const offsetX = (x - 0.5) * 15;
                    const offsetY = (y - 0.5) * 15;
                    celestialOrb.style.transform = `translate(${offsetX}px, ${offsetY}px) scale(${0.9 + (x + y) * 0.1})`;
                }
                
                // Aurora lights intensity based on mouse position
                const auroraLights = container.querySelector('.aurora-lights');
                if (auroraLights) {
                    const intensity = Math.sqrt(x * x + y * y) / Math.sqrt(2);
                    auroraLights.style.opacity = 0.3 + intensity * 0.4;
                }
                
                // Interactive glow on stat circle
                const statCircle = container.querySelector('.stat-circle');
                if (statCircle) {
                    const intensity = Math.sqrt(Math.pow(x - 0.7, 2) + Math.pow(y - 0.5, 2));
                    const glow = Math.max(0, 1 - intensity * 2);
                    statCircle.style.boxShadow = `0 0 ${30 + glow * 40}px rgba(255, 255, 255, ${0.2 + glow * 0.3})`;
                }
            });
        }

        // Initialize when the top day slide becomes active
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(() => {
                const activeSlide = document.querySelector('.slide.active');
                if (activeSlide && activeSlide.querySelector('[data-slide-id=""top-day-by-minutes""]')) {
                    initializeTopDaySlide();
                    addTopDayInteractions();
                }
            }, 100);
        });

        // Use event listener approach instead of function override to avoid conflicts
        document.addEventListener('slideChanged', function(event) {
            setTimeout(() => {
                const activeSlide = document.querySelector('.slide.active');
                if (activeSlide && activeSlide.querySelector('[data-slide-id=""top-day-by-minutes""]')) {
                    initializeTopDaySlide();
                    addTopDayInteractions();
                }
            }, 100);
        });";
        }

        private class TopDayInfo
        {
            public DateTime Date { get; set; }
            public int TrackCount { get; set; }
            public double EstimatedMinutes { get; set; }
            public string DayOfWeek { get; set; }
        }
    }
}
