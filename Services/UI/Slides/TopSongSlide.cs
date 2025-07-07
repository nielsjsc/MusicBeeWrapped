using System;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide showing the top song with details about plays and best day
    /// </summary>
    public class TopSongSlide : SlideComponentBase
    {
        public override string SlideId => "top-song";
        public override string SlideTitle => "Top Song";
        public override int SlideOrder => 2;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            if (!stats.TopTracks.Any()) 
            {
                return WrapInSlideContainer("<div class='error'>No track data available</div>");
            }

            var topTrack = stats.TopTracks.First();
            var trackInfo = ParseTrackInfo(topTrack.Key);
            var bestDay = GetBestDayForTrack(topTrack.Key, playHistory, year);
            var playsOnBestDay = bestDay.HasValue ? GetPlaysOnDay(topTrack.Key, bestDay.Value, playHistory) : 0;
            var estimatedMinutes = Math.Round(topTrack.Value * 3.5); // Average song length of 3.5 minutes
            var estimatedHours = Math.Round(estimatedMinutes / 60.0, 1);

            var content = $@"
                <div class='cosmic-hit-container'>
                    <div class='deep-space-background'>
                        <div class='distant-galaxies'></div>
                        <div class='nebula-clouds'></div>
                        <div class='star-field-distant'></div>
                        <div class='meteor-shower'></div>
                    </div>
                    
                    <div class='cosmic-content'>
                        <div class='mission-header'>
                            <div class='mission-tag'>Top Track</div>
                            <h2 class='cosmic-title'>
                                <span class='title-line-1'>Your Most</span>
                                <span class='title-line-2'>Played Song</span>
                                <span class='title-line-3'>{year}</span>
                            </h2>
                            <div class='mission-subtitle'>The track that defined your year</div>
                        </div>
                        
                        <div class='supernova-system'>
                            <div class='central-star' data-play-count='{topTrack.Value}'>
                                <div class='stellar-core'></div>
                                <div class='energy-rings'></div>
                                <div class='stellar-wind'></div>
                                <div class='corona-glow'></div>
                            </div>
                            
                            <div class='orbital-rings' data-play-count='{topTrack.Value}'>
                                <div class='orbit-ring orbit-25'></div>
                                <div class='orbit-ring orbit-50'></div>
                                <div class='orbit-ring orbit-100'></div>
                            </div>
                            
                            <div class='constellation-info'>
                                <div class='artist-name-main'>{EscapeHtml(trackInfo.Artist)}</div>
                                <div class='song-title-hero'>{EscapeHtml(trackInfo.Title)}</div>
                            </div>
                            
                            <!-- Left Side Hours Counter -->
                            <div class='side-hours-counter'>
                                <div class='hours-count-circle'>
                                    <div class='hours-number'>{(estimatedHours >= 1 ? $"{estimatedHours}" : $"{estimatedMinutes}")}</div>
                                    <div class='hours-label'>{(estimatedHours >= 1 ? "hours" : "mins")}</div>
                                </div>
                            </div>
                            
                            <!-- Side Play Counter -->
                            <div class='side-play-counter'>
                                <div class='play-count-circle'>
                                    <div class='play-number'>{topTrack.Value}</div>
                                    <div class='play-label'>plays</div>
                                </div>
                            </div>
                            
                            {(bestDay.HasValue ? $@"
                                <div class='satellite-comet' data-best-day='{bestDay.Value:yyyy-MM-dd}'>
                                    <div class='comet-tail'></div>
                                    <div class='comet-core'></div>
                                    <div class='comet-info'>
                                        <div class='comet-label'>Peak Intensity</div>
                                        <div class='comet-date'>{bestDay.Value:MMM d}</div>
                                        <div class='comet-plays'>{playsOnBestDay} plays</div>
                                    </div>
                                </div>
                            " : "")}
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            if (!stats.TopTracks.Any()) return "No track data available.";
            
            var topTrack = stats.TopTracks.First();
            var trackInfo = ParseTrackInfo(topTrack.Key);
            return GetTopSongInsight(trackInfo, topTrack.Value, year);
        }

        public override string GetInsightTitle(WrappedStatistics stats)
        {
            return "Top Song";
        }

        private TrackInfo ParseTrackInfo(string trackString)
        {
            var parts = trackString.Split(new[] { " - " }, 2, StringSplitOptions.None);
            
            if (parts.Length == 2)
            {
                return new TrackInfo
                {
                    Title = parts[0].Trim(),
                    Artist = parts[1].Trim()
                };
            }
            
            return new TrackInfo
            {
                Title = trackString.Trim(),
                Artist = "Unknown Artist"
            };
        }

        private DateTime? GetBestDayForTrack(string trackName, PlayHistory playHistory, int year)
        {
            if (playHistory?.Plays == null || !playHistory.Plays.Any())
                return null;

            var bestDay = playHistory.Plays
                .Where(t => (t.Title + " - " + t.Artist) == trackName && t.PlayedAt.Year == year)
                .GroupBy(t => t.PlayedAt.Date)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return bestDay?.Key;
        }

        private int GetPlaysOnDay(string trackName, DateTime date, PlayHistory playHistory)
        {
            if (playHistory?.Plays == null) return 0;

            return playHistory.Plays
                .Count(t => (t.Title + " - " + t.Artist) == trackName && t.PlayedAt.Date == date.Date);
        }

        private string GetCosmicInsight(TrackInfo track, int playCount, int year)
        {
            if (playCount >= 100)
                return $"With {playCount} orbital cycles, \"{track.Title}\" by {track.Artist} became a massive supernova in your musical galaxy. This cosmic phenomenon dominated your universe throughout {year}.";
            else if (playCount >= 50)
                return $"Registering {playCount} orbital cycles, \"{track.Title}\" by {track.Artist} achieved stellar magnitude in {year}. This celestial body exerted powerful gravitational pull on your playlist.";
            else if (playCount >= 25)
                return $"\"{track.Title}\" by {track.Artist} reached bright star status with {playCount} orbital cycles. This luminous presence consistently illuminated your musical constellation in {year}.";
            else
                return $"With {playCount} orbital cycles, \"{track.Title}\" by {track.Artist} emerged as your primary star system in {year}. A perfect example of stellar quality over cosmic quantity.";
        }

        private string GetTopSongInsight(TrackInfo track, int playCount, int year)
        {
            return GetCosmicInsight(track, playCount, year);
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) && 
                   stats.TopTracks != null && 
                   stats.TopTracks.Any();
        }

        private class TrackInfo
        {
            public string Title { get; set; }
            public string Artist { get; set; }
        }

        public override string GenerateCSS()
        {
            return @"
        /* Cosmic Hit - Supernova Theme CSS */
        .cosmic-hit-container {
            position: relative;
            width: 100%;
            height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            overflow: hidden;
            background: radial-gradient(ellipse at center, #0a0a1a 0%, #000000 100%);
        }

        /* Deep Space Background */
        .deep-space-background {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 1;
        }

        .distant-galaxies {
            position: absolute;
            width: 100%;
            height: 100%;
            background-image: 
                radial-gradient(ellipse at 20% 30%, rgba(138, 43, 226, 0.1) 0%, transparent 50%),
                radial-gradient(ellipse at 80% 70%, rgba(0, 191, 255, 0.08) 0%, transparent 50%),
                radial-gradient(ellipse at 60% 20%, rgba(255, 105, 180, 0.06) 0%, transparent 50%);
            animation: galaxy-rotation 60s linear infinite;
        }

        .nebula-clouds {
            position: absolute;
            width: 100%;
            height: 100%;
            background: linear-gradient(45deg, 
                transparent 0%, 
                rgba(138, 43, 226, 0.03) 25%, 
                transparent 50%, 
                rgba(0, 191, 255, 0.04) 75%, 
                transparent 100%);
            animation: nebula-drift 40s ease-in-out infinite;
        }

        .star-field-distant {
            position: absolute;
            width: 100%;
            height: 100%;
            background-image: 
                radial-gradient(circle at 15% 25%, rgba(255, 255, 255, 0.8) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 85% 15%, rgba(255, 255, 255, 0.6) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 35% 75%, rgba(255, 255, 255, 0.7) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 75% 85%, rgba(255, 255, 255, 0.5) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 95% 45%, rgba(255, 255, 255, 0.9) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 25% 55%, rgba(255, 255, 255, 0.4) 0.1vw, transparent 0.1vw);
            animation: star-twinkle-distant 15s ease-in-out infinite;
        }

        .meteor-shower {
            position: absolute;
            width: 100%;
            height: 100%;
            overflow: hidden;
        }

        /* Main Content Layout */
        .cosmic-content {
            position: relative;
            z-index: 2;
            width: 90%;
            max-width: clamp(800px, 80vw, 1400px);
            height: 70vh;
            display: grid;
            grid-template-rows: auto 1fr auto;
            gap: clamp(1.5rem, 3vh, 3rem);
            align-items: center;
        }

        /* Mission Header */
        .mission-header {
            text-align: center;
            animation: mission-deployment 1.5s ease-out;
        }

        .mission-tag {
            display: inline-block;
            padding: clamp(0.4rem, 1vh, 0.7rem) clamp(1.5rem, 3vw, 2.5rem);
            background: rgba(0, 191, 255, 0.2);
            border: 0.1vw solid rgba(0, 191, 255, 0.4);
            border-radius: clamp(20px, 3vw, 30px);
            font-size: clamp(0.8rem, 1.5vw, 1rem);
            font-weight: 500;
            color: rgba(255, 255, 255, 0.9);
            letter-spacing: 0.2vw;
            margin-bottom: clamp(1.5rem, 3vh, 2.5rem);
            backdrop-filter: blur(10px);
            animation: mission-pulse 4s ease-in-out infinite;
            text-transform: uppercase;
        }

        .cosmic-title {
            font-size: clamp(2.5rem, 6vw, 4.5rem);
            font-weight: 700;
            line-height: 1.1;
            margin-bottom: clamp(1rem, 2vh, 2rem);
            background: linear-gradient(135deg, #ffffff 0%, #87ceeb 30%, #8a2be2 70%, #ff69b4 100%);
            background-clip: text;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-size: 300% 300%;
            animation: cosmic-shimmer 6s ease-in-out infinite;
        }

        .title-line-1,
        .title-line-2,
        .title-line-3 {
            display: block;
            opacity: 0;
            transform: translateY(clamp(30px, 4vh, 50px));
            animation: stellar-emergence 1s ease-out forwards;
        }

        .title-line-1 { animation-delay: 0.3s; }
        .title-line-2 { animation-delay: 0.6s; }
        .title-line-3 { animation-delay: 0.9s; }

        .mission-subtitle {
            font-size: clamp(1rem, 2.2vw, 1.3rem);
            color: rgba(255, 255, 255, 0.7);
            font-weight: 300;
            letter-spacing: 0.1vw;
            opacity: 0;
            animation: subtitle-materialize 1s ease-out 1.2s forwards;
        }

        /* Supernova System */
        .supernova-system {
            position: relative;
            width: 100%;
            height: clamp(400px, 50vh, 600px);
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .central-star {
            position: relative;
            width: clamp(140px, 18vw, 220px);
            height: clamp(140px, 18vw, 220px);
            z-index: 10;
        }

        .stellar-core {
            width: 100%;
            height: 100%;
            border-radius: 50%;
            background: radial-gradient(circle at 30% 30%, #ffffff 0%, #ffd700 40%, #ff8c00 80%, #ff4500 100%);
            box-shadow: 
                0 0 clamp(40px, 6vw, 80px) rgba(255, 215, 0, 0.8),
                0 0 clamp(80px, 12vw, 150px) rgba(255, 140, 0, 0.6),
                0 0 clamp(120px, 18vw, 220px) rgba(255, 69, 0, 0.4);
            animation: stellar-pulse 3s ease-in-out infinite;
        }

        .energy-rings {
            position: absolute;
            top: clamp(-15px, -2vw, -25px);
            left: clamp(-15px, -2vw, -25px);
            right: clamp(-15px, -2vw, -25px);
            bottom: clamp(-15px, -2vw, -25px);
        }

        .energy-rings::before,
        .energy-rings::after {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            border: 0.2vw solid rgba(255, 215, 0, 0.3);
            border-radius: 50%;
            animation: energy-wave 2s ease-out infinite;
        }

        .energy-rings::after {
            animation-delay: 1s;
            border-color: rgba(255, 140, 0, 0.2);
        }

        .stellar-wind {
            position: absolute;
            top: clamp(-30px, -4vw, -50px);
            left: clamp(-30px, -4vw, -50px);
            right: clamp(-30px, -4vw, -50px);
            bottom: clamp(-30px, -4vw, -50px);
            border-radius: 50%;
            background: conic-gradient(from 0deg, 
                transparent 0deg, 
                rgba(255, 215, 0, 0.1) 30deg, 
                transparent 60deg,
                rgba(255, 140, 0, 0.1) 90deg,
                transparent 120deg,
                rgba(255, 215, 0, 0.1) 150deg,
                transparent 180deg,
                rgba(255, 140, 0, 0.1) 210deg,
                transparent 240deg,
                rgba(255, 215, 0, 0.1) 270deg,
                transparent 300deg,
                rgba(255, 140, 0, 0.1) 330deg,
                transparent 360deg);
            animation: stellar-wind-rotation 8s linear infinite;
        }

        .corona-glow {
            position: absolute;
            top: clamp(-45px, -6vw, -75px);
            left: clamp(-45px, -6vw, -75px);
            right: clamp(-45px, -6vw, -75px);
            bottom: clamp(-45px, -6vw, -75px);
            border-radius: 50%;
            background: radial-gradient(circle, rgba(255, 215, 0, 0.1) 0%, transparent 70%);
            animation: corona-fluctuation 5s ease-in-out infinite;
        }

        /* Orbital Rings */
        .orbital-rings {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            z-index: 5;
        }

        .orbit-ring {
            position: absolute;
            border: 0.1vw dashed rgba(255, 255, 255, 0.2);
            border-radius: 50%;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
        }

        .orbit-25 {
            width: clamp(220px, 28vw, 350px);
            height: clamp(220px, 28vw, 350px);
            animation: orbit-rotation 20s linear infinite;
        }

        .orbit-50 {
            width: clamp(280px, 36vw, 450px);
            height: clamp(280px, 36vw, 450px);
            border-style: dotted;
            animation: orbit-rotation 30s linear infinite reverse;
        }

        .orbit-100 {
            width: clamp(340px, 44vw, 550px);
            height: clamp(340px, 44vw, 550px);
            border-style: solid;
            border-color: rgba(0, 191, 255, 0.3);
            animation: orbit-rotation 40s linear infinite;
        }

        /* Show appropriate orbits based on play count */
        [data-play-count] .orbit-25 { opacity: 1; }
        [data-play-count] .orbit-50 { opacity: 0; }
        [data-play-count] .orbit-100 { opacity: 0; }

        [data-play-count*='5'] .orbit-50,
        [data-play-count*='6'] .orbit-50,
        [data-play-count*='7'] .orbit-50,
        [data-play-count*='8'] .orbit-50,
        [data-play-count*='9'] .orbit-50 { opacity: 1; }

        /* Constellation Info */
        .constellation-info {
            position: absolute;
            top: clamp(-180px, -20vh, -250px);
            left: 50%;
            transform: translateX(-50%);
            text-align: center;
            z-index: 15;
            opacity: 0;
            animation: constellation-reveal 1.5s ease-out 2s forwards;
        }

        .artist-name-main {
            font-size: clamp(2rem, 4vw, 3rem);
            color: #ffd700;
            text-shadow: 0 0 clamp(15px, 2vw, 25px) rgba(255, 215, 0, 0.8);
            font-weight: 400;
            letter-spacing: 0.1vw;
            margin-bottom: clamp(20px, 3vh, 30px);
            text-transform: uppercase;
        }

        .song-title-hero {
            font-size: clamp(1rem, 2.2vw, 1.4rem);
            font-weight: 700;
            color: #ffffff;
            text-shadow: 0 0 clamp(15px, 2vw, 25px) rgba(255, 215, 0, 0.5);
            letter-spacing: 0.1vw;
            line-height: 1.6;
            margin-bottom: clamp(15px, 2vh, 25px);
            animation: title-shimmer 3s ease-in-out infinite;
        }

        /* Side Play Counter */
        .side-play-counter {
            position: absolute;
            top: 50%;
            right: clamp(-100px, -12vw, -140px);
            transform: translateY(-50%);
            z-index: 15;
            opacity: 0;
            animation: side-counter-slide 1.5s ease-out 2.8s forwards;
        }

        .play-count-circle {
            width: clamp(80px, 10vw, 120px);
            height: clamp(80px, 10vw, 120px);
            border-radius: 50%;
            background: radial-gradient(circle, rgba(255, 215, 0, 0.2) 0%, rgba(255, 215, 0, 0.05) 70%);
            border: 0.2vw solid rgba(255, 215, 0, 0.6);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            backdrop-filter: blur(10px);
            animation: circle-glow 2s ease-in-out infinite alternate;
        }

        .play-number {
            font-size: clamp(1.5rem, 2.8vw, 2.2rem);
            font-weight: 800;
            color: #ffd700;
            text-shadow: 0 0 clamp(10px, 1.5vw, 20px) rgba(255, 215, 0, 0.8);
            margin-bottom: clamp(2px, 0.3vh, 4px);
        }

        .play-label {
            font-size: clamp(0.6rem, 1.2vw, 0.8rem);
            color: rgba(255, 255, 255, 0.9);
            text-transform: uppercase;
            letter-spacing: 0.1vw;
            font-weight: 600;
        }

        /* Left Side Hours Counter */
        .side-hours-counter {
            position: absolute;
            top: 50%;
            left: clamp(-100px, -12vw, -140px);
            transform: translateY(-50%);
            z-index: 15;
            opacity: 0;
            animation: side-counter-slide 1.5s ease-out 2.5s forwards;
        }

        .hours-count-circle {
            width: clamp(80px, 10vw, 120px);
            height: clamp(80px, 10vw, 120px);
            border-radius: 50%;
            background: radial-gradient(circle, rgba(0, 191, 255, 0.2) 0%, rgba(0, 191, 255, 0.05) 70%);
            border: 0.2vw solid rgba(0, 191, 255, 0.6);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            backdrop-filter: blur(10px);
            animation: hours-circle-glow 2s ease-in-out infinite alternate;
        }

        .hours-number {
            font-size: clamp(1.5rem, 2.8vw, 2.2rem);
            font-weight: 800;
            color: #00bfff;
            text-shadow: 0 0 clamp(10px, 1.5vw, 20px) rgba(0, 191, 255, 0.8);
            margin-bottom: clamp(2px, 0.3vh, 4px);
        }

        .hours-label {
            font-size: clamp(0.6rem, 1.2vw, 0.8rem);
            color: rgba(255, 255, 255, 0.9);
            text-transform: uppercase;
            letter-spacing: 0.1vw;
            font-weight: 600;
        }

        .star-designation {
            display: flex;
            align-items: center;
            gap: 1rem;
            margin-bottom: 0.5rem;
        }

        .constellation-line {
            width: 40px;
            height: 1px;
            background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.6), transparent);
        }

        .song-title {
            font-size: 1.8rem;
            font-weight: 600;
            color: #ffffff;
            text-shadow: 0 0 20px rgba(255, 255, 255, 0.5);
            letter-spacing: 1px;
        }

        .artist-classification {
            margin-top: 0.5rem;
        }

        .artist-name {
            font-size: 1.2rem;
            color: rgba(255, 255, 255, 0.8);
            font-weight: 300;
            letter-spacing: 2px;
        }

        /* Play Counter */
        .play-counter-display {
            position: absolute;
            bottom: -100px;
            left: 50%;
            transform: translateX(-50%);
            z-index: 15;
            opacity: 0;
            animation: counter-activation 1.5s ease-out 2.5s forwards;
            text-align: center;
        }

        .primary-stat {
            margin-bottom: 15px;
            padding: 15px 20px;
            background: rgba(255, 215, 0, 0.1);
            border: 1px solid rgba(255, 215, 0, 0.3);
            border-radius: 12px;
            backdrop-filter: blur(10px);
        }

        .primary-number {
            font-size: 3rem;
            font-weight: 800;
            color: #ffd700;
            text-shadow: 0 0 20px rgba(255, 215, 0, 0.6);
            margin-bottom: 5px;
            animation: number-glow 2s ease-in-out infinite alternate;
        }

        .primary-label {
            font-size: 1rem;
            color: rgba(255, 255, 255, 0.9);
            text-transform: uppercase;
            letter-spacing: 2px;
            font-weight: 600;
        }

        .stats-container {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 15px;
            padding: 10px 15px;
            background: rgba(0, 0, 0, 0.3);
            border: 1px solid rgba(0, 191, 255, 0.3);
            border-radius: 15px;
            backdrop-filter: blur(20px);
        }

        .stat-item {
            text-align: center;
        }

        .stat-number {
            font-size: 1.5rem;
            font-weight: 700;
            color: #ffffff;
            margin-bottom: 3px;
        }

        .stat-label {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .stat-divider {
            color: rgba(255, 255, 255, 0.5);
            font-size: 1.2rem;
        }

        @keyframes number-glow {
            from { text-shadow: 0 0 20px rgba(255, 215, 0, 0.6); }
            to { text-shadow: 0 0 30px rgba(255, 215, 0, 0.9), 0 0 40px rgba(255, 215, 0, 0.4); }
        }

        .cosmic-stat {
            position: relative;
            text-align: center;
            padding: 1rem 2rem;
            background: rgba(0, 0, 0, 0.3);
            border: 1px solid rgba(0, 191, 255, 0.3);
            border-radius: 15px;
            backdrop-filter: blur(20px);
        }

        .stat-glow {
            position: absolute;
            top: -2px;
            left: -2px;
            right: -2px;
            bottom: -2px;
            background: linear-gradient(45deg, #00bfff, #8a2be2, #00bfff);
            border-radius: 15px;
            z-index: -1;
            animation: stat-border-glow 3s ease-in-out infinite;
        }

        .stat-number {
            font-size: 2.5rem;
            font-weight: 700;
            color: #ffffff;
            text-shadow: 0 0 20px rgba(255, 255, 255, 0.4);
            margin-bottom: 0.5rem;
        }

        .stat-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            letter-spacing: 2px;
            text-transform: uppercase;
        }

        /* Satellite Comet (Best Day) */
        .satellite-comet {
            position: absolute;
            top: 20%;
            right: -80px;
            z-index: 12;
            opacity: 0;
            animation: comet-approach 2s ease-out 3s forwards;
        }

        .comet-core {
            width: 40px;
            height: 40px;
            background: radial-gradient(circle, #ffffff 0%, #00bfff 50%, #8a2be2 100%);
            border-radius: 50%;
            box-shadow: 0 0 30px rgba(0, 191, 255, 0.8);
            animation: comet-glow 2s ease-in-out infinite;
        }

        .comet-tail {
            position: absolute;
            top: 50%;
            right: 100%;
            width: 80px;
            height: 2px;
            background: linear-gradient(90deg, rgba(0, 191, 255, 0.8), transparent);
            transform: translateY(-50%);
            animation: tail-flicker 1.5s ease-in-out infinite;
        }

        .comet-info {
            position: absolute;
            top: -60px;
            left: 50%;
            transform: translateX(-50%);
            text-align: center;
            white-space: nowrap;
        }

        .comet-label {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.6);
            margin-bottom: 0.25rem;
            letter-spacing: 1px;
        }

        .comet-date {
            font-size: 1.1rem;
            font-weight: 600;
            color: #00bfff;
            margin-bottom: 0.25rem;
        }

        .comet-plays {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.8);
        }

        /* Cosmic Animations */
        @keyframes galaxy-rotation {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
        }

        @keyframes nebula-drift {
            0%, 100% { transform: translateX(0) translateY(0); }
            25% { transform: translateX(20px) translateY(-10px); }
            50% { transform: translateX(-15px) translateY(15px); }
            75% { transform: translateX(10px) translateY(-5px); }
        }

        @keyframes star-twinkle-distant {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.7; }
        }

        @keyframes mission-deployment {
            from { opacity: 0; transform: translateY(-50px); }
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes mission-pulse {
            0%, 100% { box-shadow: 0 0 20px rgba(0, 191, 255, 0.3); }
            50% { box-shadow: 0 0 30px rgba(0, 191, 255, 0.6); }
        }

        @keyframes cosmic-shimmer {
            0% { background-position: 0% 50%; }
            50% { background-position: 100% 50%; }
            100% { background-position: 0% 50%; }
        }

        @keyframes stellar-emergence {
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes subtitle-materialize {
            to { opacity: 1; }
        }

        @keyframes stellar-pulse {
            0%, 100% { 
                transform: scale(1); 
                box-shadow: 0 0 60px rgba(255, 215, 0, 0.8), 0 0 120px rgba(255, 140, 0, 0.6), 0 0 180px rgba(255, 69, 0, 0.4); 
            }
            50% { 
                transform: scale(1.05); 
                box-shadow: 0 0 80px rgba(255, 215, 0, 1), 0 0 160px rgba(255, 140, 0, 0.8), 0 0 240px rgba(255, 69, 0, 0.6); 
            }
        }

        @keyframes energy-wave {
            0% { transform: scale(1); opacity: 1; }
            100% { transform: scale(1.5); opacity: 0; }
        }

        @keyframes stellar-wind-rotation {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
        }

        @keyframes corona-fluctuation {
            0%, 100% { opacity: 0.8; transform: scale(1); }
            50% { opacity: 1; transform: scale(1.1); }
        }

        @keyframes orbit-rotation {
            from { transform: translate(-50%, -50%) rotate(0deg); }
            to { transform: translate(-50%, -50%) rotate(360deg); }
        }

        @keyframes constellation-reveal {
            to { opacity: 1; }
        }

        @keyframes counter-activation {
            to { opacity: 1; }
        }

        @keyframes stat-border-glow {
            0%, 100% { opacity: 0.5; }
            50% { opacity: 1; }
        }

        @keyframes comet-approach {
            from { transform: translateX(100px); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }

        @keyframes comet-glow {
            0%, 100% { box-shadow: 0 0 30px rgba(0, 191, 255, 0.8); }
            50% { box-shadow: 0 0 40px rgba(0, 191, 255, 1); }
        }

        @keyframes tail-flicker {
            0%, 100% { opacity: 0.8; }
            50% { opacity: 1; }
        }

        @keyframes control-panel-online {
            to { opacity: 1; }
        }

        @keyframes status-blink {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.3; }
        }

        @keyframes title-shimmer {
            0% { background-position: 0% 50%; }
            50% { background-position: 100% 50%; }
            100% { background-position: 0% 50%; }
        }

        @keyframes side-counter-slide {
            from { 
                opacity: 0; 
                transform: translateY(-50%) translateX(30px); 
            }
            to { 
                opacity: 1; 
                transform: translateY(-50%) translateX(0); 
            }
        }

        @keyframes circle-glow {
            from { 
                box-shadow: 0 0 20px rgba(255, 215, 0, 0.4); 
                border-color: rgba(255, 215, 0, 0.6); 
            }
            to { 
                box-shadow: 0 0 30px rgba(255, 215, 0, 0.8); 
                border-color: rgba(255, 215, 0, 0.9); 
            }
        }

        @keyframes hours-circle-glow {
            from { 
                box-shadow: 0 0 20px rgba(0, 191, 255, 0.4); 
                border-color: rgba(0, 191, 255, 0.6); 
            }
            to { 
                box-shadow: 0 0 30px rgba(0, 191, 255, 0.8); 
                border-color: rgba(0, 191, 255, 0.9); 
            }
        }

        /* Responsive Design */
        @media (max-width: 60rem) {
            .cosmic-content {
                width: 95%;
                gap: clamp(1rem, 2vh, 1.5rem);
            }
            
            .supernova-system {
                height: clamp(350px, 45vh, 500px);
            }
            
            .central-star {
                width: clamp(120px, 15vw, 160px);
                height: clamp(120px, 15vw, 160px);
            }
            
            .orbit-25 { 
                width: clamp(180px, 25vw, 250px); 
                height: clamp(180px, 25vw, 250px); 
            }
            .orbit-50 { 
                width: clamp(220px, 32vw, 320px); 
                height: clamp(220px, 32vw, 320px); 
            }
            .orbit-100 { 
                width: clamp(260px, 40vw, 400px); 
                height: clamp(260px, 40vw, 400px); 
            }
            
            .constellation-info {
                position: static;
                transform: none;
                margin-top: clamp(1.5rem, 3vh, 2.5rem);
            }
            
            .side-play-counter,
            .side-hours-counter {
                position: static;
                transform: none;
                margin: clamp(1rem, 2vh, 1.5rem) auto;
                display: flex;
                justify-content: center;
            }
            
            .satellite-comet {
                position: static;
                margin: clamp(1.5rem, 3vh, 2.5rem) auto;
                display: flex;
                align-items: center;
                gap: clamp(0.8rem, 1.5vw, 1.2rem);
            }
        }

        @media (max-width: 48rem) {
            .cosmic-content {
                gap: clamp(0.8rem, 1.5vh, 1.2rem);
            }
            
            .supernova-system {
                height: clamp(300px, 40vh, 400px);
            }
            
            .central-star {
                width: clamp(100px, 12vw, 140px);
                height: clamp(100px, 12vw, 140px);
            }
            
            .artist-name-main {
                font-size: clamp(1.5rem, 3vw, 2rem);
                margin-bottom: clamp(15px, 2vh, 20px);
            }
            
            .song-title-hero {
                font-size: clamp(0.9rem, 1.8vw, 1.1rem);
                margin-bottom: clamp(10px, 1.5vh, 15px);
            }
        }

        @media (max-width: 30rem) {
            .play-count-circle,
            .hours-count-circle {
                width: clamp(70px, 8vw, 90px);
                height: clamp(70px, 8vw, 90px);
            }
            
            .play-number,
            .hours-number {
                font-size: clamp(1.2rem, 2.2vw, 1.6rem);
            }
            
            .orbit-25 { 
                width: clamp(150px, 22vw, 200px); 
                height: clamp(150px, 22vw, 200px); 
            }
            .orbit-50 { 
                width: clamp(180px, 28vw, 250px); 
                height: clamp(180px, 28vw, 250px); 
            }
            .orbit-100 { 
                width: clamp(210px, 35vw, 300px); 
                height: clamp(210px, 35vw, 300px); 
            }
        }
        ";
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        // Cosmic Hit - Supernova JavaScript
        function initializeCosmicHit() {
            const cosmicSlide = document.querySelector('[data-slide-id=""top-song""]');
            if (!cosmicSlide) return;

            // Enhanced supernova effects
            initializeSupernovaCore();
            
            // Dynamic orbital system
            activateOrbitalSystem();
            
            // Stellar particle system
            createStellarParticles();
            
            // Meteor shower effects
            launchMeteorShower();
            
            // Interactive cosmic environment
            addCosmicInteractions();
            
            // Stellar data visualization
            animateCosmicStats();
        }

        function initializeSupernovaCore() {
            const stellarCore = document.querySelector('.stellar-core');
            const centralStar = document.querySelector('.central-star');
            
            if (!stellarCore || !centralStar) return;
            
            const playCount = parseInt(centralStar.dataset.playCount) || 0;
            
            // Adjust supernova intensity based on play count
            let intensity = Math.min(playCount / 100, 2); // Max intensity at 100+ plays
            if (intensity < 0.5) intensity = 0.5; // Minimum intensity
            
            // Dynamic stellar core properties
            stellarCore.style.transform = `scale(${0.8 + intensity * 0.4})`;
            stellarCore.style.filter = `brightness(${0.8 + intensity * 0.6}) saturate(${1 + intensity * 0.5})`;
            
            // Pulsation based on popularity
            const pulseDuration = Math.max(1.5, 4 - intensity);
            stellarCore.style.animationDuration = `${pulseDuration}s`;
        }

        function activateOrbitalSystem() {
            const orbitalRings = document.querySelector('.orbital-rings');
            if (!orbitalRings) return;
            
            const playCount = parseInt(orbitalRings.dataset.playCount) || 0;
            
            // Show appropriate orbital rings based on play count
            const orbit25 = orbitalRings.querySelector('.orbit-25');
            const orbit50 = orbitalRings.querySelector('.orbit-50');
            const orbit100 = orbitalRings.querySelector('.orbit-100');
            
            // Always show first orbit
            if (orbit25) orbit25.style.opacity = '1';
            
            // Show second orbit for 50+ plays
            if (orbit50 && playCount >= 50) {
                orbit50.style.opacity = '0.7';
                orbit50.style.borderColor = 'rgba(255, 215, 0, 0.4)';
            }
            
            // Show third orbit for 100+ plays  
            if (orbit100 && playCount >= 100) {
                orbit100.style.opacity = '1';
                orbit100.style.borderColor = 'rgba(0, 191, 255, 0.6)';
                orbit100.style.borderWidth = '2px';
            }
            
            // Add orbital particles
            createOrbitalParticles(playCount);
        }

        function createOrbitalParticles(playCount) {
            const supernovaSystem = document.querySelector('.supernova-system');
            if (!supernovaSystem) return;
            
            const particleCount = Math.min(Math.floor(playCount / 10), 20);
            
            for (let i = 0; i < particleCount; i++) {
                const particle = document.createElement('div');
                particle.className = 'orbital-particle';
                
                const orbit = 150 + Math.random() * 100; // Random orbit radius
                const speed = 10 + Math.random() * 20; // Random orbit speed
                
                particle.style.cssText = `
                    position: absolute;
                    width: 3px;
                    height: 3px;
                    background: radial-gradient(circle, rgba(255, 255, 255, 1), rgba(0, 191, 255, 0.5));
                    border-radius: 50%;
                    top: 50%;
                    left: 50%;
                    transform-origin: 0 0;
                    animation: orbital-particle-${i} ${speed}s linear infinite;
                    box-shadow: 0 0 4px rgba(255, 255, 255, 0.8);
                    z-index: 8;
                `;
                
                // Create unique orbit animation for each particle
                const style = document.createElement('style');
                style.textContent = `
                    @keyframes orbital-particle-${i} {
                        from { transform: translate(-50%, -50%) rotate(0deg) translateX(${orbit}px) rotate(0deg); }
                        to { transform: translate(-50%, -50%) rotate(360deg) translateX(${orbit}px) rotate(-360deg); }
                    }
                `;
                document.head.appendChild(style);
                
                supernovaSystem.appendChild(particle);
            }
        }

        function createStellarParticles() {
            const cosmicContainer = document.querySelector('.cosmic-hit-container');
            if (!cosmicContainer) return;

            // Create ambient stellar dust
            for (let i = 0; i < 25; i++) {
                const particle = document.createElement('div');
                particle.className = 'stellar-dust';
                particle.style.cssText = `
                    position: absolute;
                    width: ${Math.random() * 2 + 1}px;
                    height: ${Math.random() * 2 + 1}px;
                    background: radial-gradient(circle, rgba(255, 255, 255, 0.9), rgba(135, 206, 235, 0.3));
                    border-radius: 50%;
                    left: ${Math.random() * 100}%;
                    top: ${Math.random() * 100}%;
                    animation: stellar-drift ${Math.random() * 30 + 20}s ease-in-out infinite;
                    animation-delay: ${Math.random() * 10}s;
                    pointer-events: none;
                    z-index: 3;
                    box-shadow: 0 0 3px rgba(255, 255, 255, 0.6);
                `;
                cosmicContainer.appendChild(particle);
            }

            // Add stellar drift animation
            const style = document.createElement('style');
            style.textContent = `
                @keyframes stellar-drift {
                    0%, 100% { 
                        transform: translateY(0) translateX(0) rotate(0deg) scale(1); 
                        opacity: 0.3; 
                    }
                    25% { 
                        transform: translateY(-50px) translateX(30px) rotate(90deg) scale(1.2); 
                        opacity: 0.8; 
                    }
                    50% { 
                        transform: translateY(-30px) translateX(-40px) rotate(180deg) scale(0.8); 
                        opacity: 0.6; 
                    }
                    75% { 
                        transform: translateY(-70px) translateX(20px) rotate(270deg) scale(1.1); 
                        opacity: 0.9; 
                    }
                }
            `;
            document.head.appendChild(style);
        }

        function launchMeteorShower() {
            const meteorShower = document.querySelector('.meteor-shower');
            if (!meteorShower) return;
            
            function createMeteor() {
                const meteor = document.createElement('div');
                meteor.className = 'meteor';
                
                const startX = Math.random() * 100;
                const startY = -10;
                const angle = Math.random() * 30 + 45; // 45-75 degree angle
                const speed = Math.random() * 3 + 2; // 2-5 second duration
                const size = Math.random() * 2 + 1;
                
                meteor.style.cssText = `
                    position: absolute;
                    width: ${size}px;
                    height: ${size * 20}px;
                    background: linear-gradient(to bottom, rgba(255, 255, 255, 1), transparent);
                    left: ${startX}%;
                    top: ${startY}%;
                    transform: rotate(${angle}deg);
                    animation: meteor-fall ${speed}s linear forwards;
                    pointer-events: none;
                    z-index: 4;
                    box-shadow: 0 0 6px rgba(255, 255, 255, 0.8);
                `;
                
                meteorShower.appendChild(meteor);
                
                // Remove meteor after animation
                setTimeout(() => {
                    if (meteor.parentNode) {
                        meteor.parentNode.removeChild(meteor);
                    }
                }, speed * 1000);
            }
            
            // Add meteor fall animation
            const style = document.createElement('style');
            style.textContent = `
                @keyframes meteor-fall {
                    from { transform: translateY(0) translateX(0) rotate(45deg); opacity: 1; }
                    to { transform: translateY(120vh) translateX(60vw) rotate(45deg); opacity: 0; }
                }
            `;
            document.head.appendChild(style);
            
            // Launch meteors periodically
            setInterval(createMeteor, 3000 + Math.random() * 5000);
            
            // Launch initial meteors
            setTimeout(createMeteor, 2000);
            setTimeout(createMeteor, 4000);
            setTimeout(createMeteor, 7000);
        }

        function addCosmicInteractions() {
            const cosmicContainer = document.querySelector('.cosmic-hit-container');
            const centralStar = document.querySelector('.central-star');
            const nebulaCloud = document.querySelector('.nebula-clouds');
            
            if (!cosmicContainer || !centralStar) return;
            
            cosmicContainer.addEventListener('mousemove', (e) => {
                const rect = cosmicContainer.getBoundingClientRect();
                const x = (e.clientX - rect.left) / rect.width;
                const y = (e.clientY - rect.top) / rect.height;
                
                // Parallax effect on nebula clouds
                if (nebulaCloud) {
                    const offsetX = (x - 0.5) * 30;
                    const offsetY = (y - 0.5) * 30;
                    nebulaCloud.style.transform = `translate(${offsetX}px, ${offsetY}px)`;
                }
                
                // Enhanced stellar glow based on mouse proximity to star
                const starRect = centralStar.getBoundingClientRect();
                const containerRect = cosmicContainer.getBoundingClientRect();
                const starCenterX = (starRect.left + starRect.width / 2 - containerRect.left) / containerRect.width;
                const starCenterY = (starRect.top + starRect.height / 2 - containerRect.top) / containerRect.height;
                
                const distance = Math.sqrt(Math.pow(x - starCenterX, 2) + Math.pow(y - starCenterY, 2));
                const intensity = Math.max(0, 1 - distance * 3);
                
                if (intensity > 0) {
                    const stellarCore = centralStar.querySelector('.stellar-core');
                    if (stellarCore) {
                        const baseGlow = '0 0 60px rgba(255, 215, 0, 0.8), 0 0 120px rgba(255, 140, 0, 0.6), 0 0 180px rgba(255, 69, 0, 0.4)';
                        const enhancedGlow = `${baseGlow}, 0 0 ${120 + intensity * 100}px rgba(255, 255, 255, ${intensity * 0.4})`;
                        stellarCore.style.boxShadow = enhancedGlow;
                    }
                }
            });
        }

        function animateCosmicStats() {
            const statNumber = document.querySelector('.cosmic-stat .stat-number');
            if (!statNumber) return;
            
            const finalValue = parseInt(statNumber.textContent) || 0;
            let currentValue = 0;
            const duration = 2000; // 2 seconds
            const startTime = performance.now();
            
            function updateCounter(currentTime) {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / duration, 1);
                
                // Easing function for smooth animation
                const easedProgress = 1 - Math.pow(1 - progress, 3);
                currentValue = Math.floor(finalValue * easedProgress);
                
                statNumber.textContent = currentValue;
                
                if (progress < 1) {
                    requestAnimationFrame(updateCounter);
                }
            }
            
            // Start counter animation after a delay
            setTimeout(() => {
                requestAnimationFrame(updateCounter);
            }, 2500);
        }

        // Initialize when cosmic hit slide becomes active
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(() => {
                const activeSlide = document.querySelector('.slide.active');
                if (activeSlide && activeSlide.querySelector('[data-slide-id=""top-song""]')) {
                    initializeCosmicHit();
                }
            }, 100);
        });

        // Use event listener approach for slide changes
        document.addEventListener('slideChanged', function(event) {
            setTimeout(() => {
                const activeSlide = document.querySelector('.slide.active');
                if (activeSlide && activeSlide.querySelector('[data-slide-id=""top-song""]')) {
                    initializeCosmicHit();
                }
            }, 100);
        });";
        }
    }
}
