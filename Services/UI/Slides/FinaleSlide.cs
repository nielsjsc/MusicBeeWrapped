using System;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Finale slide component - Wrap-up slide with year summary and thanks
    /// The final slide that concludes the Wrapped experience
    /// </summary>
    public class FinaleSlide : SlideComponentBase
    {
        public override string SlideId => "finale";
        public override string SlideTitle => "That's a Wrap!";
        public override int SlideOrder => 8;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
{
    var topArtist = stats.TopArtists.FirstOrDefault();
    var topTrack = stats.TopTracks.FirstOrDefault();
    var totalMinutes = Math.Round(stats.TotalTracks * 3.5); // Assuming 3.5 min average
    var totalHours = Math.Round(totalMinutes / 60);

    // Group plays by FileUrl, count occurrences, and select top 50
    var top50Tracks = playHistory.Plays
        .Where(p => !string.IsNullOrEmpty(p.FileUrl))
        .GroupBy(p => p.FileUrl)
        .OrderByDescending(g => g.Count())
        .Take(50)
        .Select(g => {
            // Use the first play for metadata (artist, title, duration)
            var play = g.First();
            return new {
                artist = EscapeJs(play.Artist ?? "Unknown"),
                title = EscapeJs(play.Title ?? "Unknown"),
                duration = play.Duration,
                filePath = EscapeJs(play.FileUrl)
            };
        })
        .ToList();

    // Serialize to JS array
    string jsArray = "[" + string.Join(",",
        top50Tracks.Select(t => $"{{artist:'{t.artist}',title:'{t.title}',duration:{t.duration},filePath:'{t.filePath}'}}")) + "]";

    var content = $@"
        <script>
            window.top50Tracks = {jsArray};
        </script>
        <div class='hyperspace-background'>
            <div class='star-field-container'>
                <div class='star-layer' data-speed='1'></div>
                <div class='star-layer' data-speed='2'></div>
                <div class='star-layer' data-speed='3'></div>
                <div class='star-layer' data-speed='4'></div>
                <div class='star-layer' data-speed='5'></div>
            </div>
            <div class='velocity-lines'></div>
            <div class='central-vortex'></div>
            <div class='space-debris'></div>
        </div>
        <div class='finale-content'>
            <div class='finale-header'>
                <h1 class='finale-title'>THAT'S A WRAP</h1>
                <div class='finale-year'>{year}</div>
            </div>
            
            <div class='stats-constellation'>
                <div class='stat-node' data-delay='0'>
                    <div class='stat-value'>{stats.TotalTracks}</div>
                    <div class='stat-label'>TOTAL PLAYS</div>
                </div>
                <div class='stat-node' data-delay='200'>
                    <div class='stat-value'>{totalHours}</div>
                    <div class='stat-label'>HOURS LISTENED</div>
                </div>
                <div class='stat-node' data-delay='400'>
                    <div class='stat-value'>{stats.TopArtists.Count()}</div>
                    <div class='stat-label'>ARTISTS EXPLORED</div>
                </div>
            </div>

            <div class='highlights-section'>
                <div class='highlight-beam' data-delay='600'>
                    <div class='beam-label'>TOP ARTIST</div>
                    <div class='beam-value'>{EscapeHtml(topArtist.Key ?? "UNKNOWN")}</div>
                </div>
                <div class='highlight-beam' data-delay='800'>
                    <div class='beam-label'>MOST PLAYED</div>
                    <div class='beam-value'>{EscapeHtml(topTrack.Key ?? "UNKNOWN")}</div>
                </div>
            </div>

            <div class='finale-action' data-delay='1000'>
                <button class='export-btn' onclick='exportTop50Playlist({year})'>
                    <span class='btn-glow'></span>
                    <span class='btn-text'>EXPORT TOP 50 PLAYLIST</span>
                </button>
                <div class='action-subtitle'>RELIVE YOUR YEAR • DOWNLOAD YOUR SONIC JOURNEY</div>
            </div>
        </div>";

    return WrapInSlideContainer(content);
}
        // Escapes for JS string literals
        private string EscapeJs(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", " ").Replace("\r", " ");
        }
        

        public override string GenerateCSS()
{
    return @"
        .hyperspace-background {
            position: absolute;
            width: 100%;
            height: 100%;
            z-index: 0;
            pointer-events: none;
            overflow: hidden;
            background: radial-gradient(ellipse at center, #0a0a1a 0%, #000000 70%);
        }

        .star-field-container {
            position: absolute;
            width: 100%;
            height: 100%;
            perspective: 1000px;
            transform-style: preserve-3d;
        }

        .star-layer {
            position: absolute;
            width: 200%;
            height: 200%;
            top: -50%;
            left: -50%;
            background-image: 
                radial-gradient(circle at 12% 23%, #ffffff 0.5px, transparent 0.5px),
                radial-gradient(circle at 89% 45%, #ffffff 0.8px, transparent 0.8px),
                radial-gradient(circle at 34% 67%, #ffffff 0.3px, transparent 0.3px),
                radial-gradient(circle at 78% 12%, #ffffff 0.6px, transparent 0.6px),
                radial-gradient(circle at 45% 89%, #ffffff 0.4px, transparent 0.4px),
                radial-gradient(circle at 67% 34%, #ffffff 0.7px, transparent 0.7px),
                radial-gradient(circle at 23% 78%, #ffffff 0.5px, transparent 0.5px),
                radial-gradient(circle at 56% 11%, #ffffff 0.9px, transparent 0.9px),
                radial-gradient(circle at 91% 78%, #ffffff 0.3px, transparent 0.3px),
                radial-gradient(circle at 15% 56%, #ffffff 0.6px, transparent 0.6px),
                radial-gradient(circle at 73% 23%, #ffffff 0.4px, transparent 0.4px),
                radial-gradient(circle at 29% 91%, #ffffff 0.8px, transparent 0.8px),
                radial-gradient(circle at 84% 67%, #ffffff 0.5px, transparent 0.5px),
                radial-gradient(circle at 41% 15%, #ffffff 0.7px, transparent 0.7px),
                radial-gradient(circle at 62% 84%, #ffffff 0.3px, transparent 0.3px);
            background-size: 300px 300px;
            animation: hyperspaceJump 4s cubic-bezier(0.25, 0.46, 0.45, 0.94) infinite;
        }

        .star-layer[data-speed='1'] {
            animation-delay: 0s;
            opacity: 0.9;
        }

        .star-layer[data-speed='2'] {
            animation-delay: 0.2s;
            opacity: 0.7;
            background-size: 250px 250px;
        }

        .star-layer[data-speed='3'] {
            animation-delay: 0.4s;
            opacity: 0.5;
            background-size: 200px 200px;
        }

        .star-layer[data-speed='4'] {
            animation-delay: 0.6s;
            opacity: 0.3;
            background-size: 150px 150px;
        }

        .star-layer[data-speed='5'] {
            animation-delay: 0.8s;
            opacity: 0.2;
            background-size: 100px 100px;
        }

        .velocity-lines {
            position: absolute;
            width: 100%;
            height: 100%;
            background: repeating-linear-gradient(
                0deg,
                transparent 0px,
                transparent 48px,
                rgba(255, 255, 255, 0.03) 50px,
                rgba(255, 255, 255, 0.03) 52px,
                transparent 54px
            );
            animation: velocityStream 0.5s linear infinite;
        }

        .central-vortex {
            position: absolute;
            top: 50%;
            left: 50%;
            width: 300px;
            height: 300px;
            transform: translate(-50%, -50%);
            background: radial-gradient(circle, 
                rgba(255, 255, 255, 0.1) 0%,
                rgba(173, 216, 230, 0.05) 30%,
                transparent 70%
            );
            border-radius: 50%;
            animation: vortexPulse 3s ease-in-out infinite;
        }

        .space-debris {
            position: absolute;
            width: 100%;
            height: 100%;
            background-image: 
                radial-gradient(circle at 25% 30%, rgba(255, 223, 0, 0.6) 2px, transparent 2px),
                radial-gradient(circle at 75% 70%, rgba(255, 140, 0, 0.4) 1px, transparent 1px),
                radial-gradient(circle at 40% 80%, rgba(255, 69, 0, 0.5) 1.5px, transparent 1.5px),
                radial-gradient(circle at 80% 20%, rgba(255, 165, 0, 0.3) 1px, transparent 1px),
                radial-gradient(circle at 60% 40%, rgba(255, 215, 0, 0.4) 1.5px, transparent 1.5px);
            background-size: 400px 400px;
            animation: debrisStream 6s linear infinite;
        }

        .finale-content {
            position: relative;
            z-index: 10;
            width: 100%;
            height: auto;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            padding: 2rem;
            text-align: center;
        }

        .finale-header {
            margin-bottom: 4rem;
            animation: headerMaterialize 2s cubic-bezier(0.165, 0.84, 0.44, 1) forwards;
            opacity: 0;
        }

        .finale-title {
            font-size: 1rem;
            font-weight: 900;
            background: linear-gradient(135deg, 
                #ffffff 0%,
                #e2e8f0 25%,
                #cbd5e1 50%,
                #94a3b8 75%,
                #64748b 100%
            );
            background-clip: text;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 1rem;
            letter-spacing: 0.2em;
            text-shadow: 0 0 50px rgba(255, 255, 255, 0.3);
            filter: drop-shadow(0 0 20px rgba(255, 255, 255, 0.2));
        }

        .finale-year {
            font-size: 5rem;
            font-weight: 100;
            color: rgba(255, 255, 255, 0.1);
            margin: -2rem 0;
            letter-spacing: 0.3em;
            text-shadow: 0 0 100px rgba(255, 255, 255, 0.1);
        }

        .finale-subtitle {
            font-size: 1.2rem;
            color: rgba(255, 255, 255, 0.7);
            font-weight: 300;
            letter-spacing: 0.3em;
            margin-top: 1rem;
        }

        .stats-constellation {
            display: flex;
            justify-content: center;
            gap: 6rem;
            margin-bottom: 4rem;
        }

        .stat-node {
            position: relative;
            padding: 2rem;
            animation: nodeAppear 1.5s cubic-bezier(0.165, 0.84, 0.44, 1) forwards;
            opacity: 0;
            transform: translateY(50px);
        }

        .stat-node[data-delay='0'] { animation-delay: 0.5s; }
        .stat-node[data-delay='200'] { animation-delay: 0.7s; }
        .stat-node[data-delay='400'] { animation-delay: 0.9s; }

        .stat-node::before {
            content: '';
            position: absolute;
            top: 0;
            left: 50%;
            width: 2px;
            height: 3rem;
            background: linear-gradient(to bottom, transparent, rgba(255, 255, 255, 0.3), transparent);
            transform: translateX(-50%);
        }

        .stat-node::after {
            content: '';
            position: absolute;
            top: 3rem;
            left: 50%;
            width: 8px;
            height: 8px;
            background: radial-gradient(circle, #ffffff 0%, transparent 70%);
            border-radius: 50%;
            transform: translateX(-50%);
            box-shadow: 0 0 20px rgba(255, 255, 255, 0.8);
        }

        .stat-value {
            font-size: 3rem;
            font-weight: 700;
            color: #ffffff;
            margin-bottom: 0.5rem;
            text-shadow: 0 0 30px rgba(255, 255, 255, 0.5);
        }

        .stat-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.6);
            font-weight: 300;
            letter-spacing: 0.2em;
        }

        .highlights-section {
            display: flex;
            justify-content: center;
            gap: 2rem;
            margin-bottom: 4rem;
        }

        .highlight-beam {
            position: relative;
            padding: 1rem 1.5rem;
            background: linear-gradient(135deg, 
                rgba(255, 255, 255, 0.02) 0%,
                rgba(255, 255, 255, 0.05) 50%,
                rgba(255, 255, 255, 0.02) 100%
            );
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 2px;
            animation: beamActivate 1.5s cubic-bezier(0.165, 0.84, 0.44, 1) forwards;
            opacity: 0;
            transform: scaleX(0);
        }

        .highlight-beam[data-delay='600'] { animation-delay: 1.1s; }
        .highlight-beam[data-delay='800'] { animation-delay: 1.3s; }

        .highlight-beam::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: linear-gradient(90deg, 
                transparent 0%,
                rgba(255, 255, 255, 0.1) 50%,
                transparent 100%
            );
            animation: beamSweep 2s ease-in-out infinite;
        }

        .beam-label {
            font-size: 0.8rem;
            color: rgba(255, 255, 255, 0.5);
            font-weight: 300;
            letter-spacing: 0.2em;
            margin-bottom: 0.5rem;
        }

        .beam-value {
            font-size: 1.5rem;
            color: #ffffff;
            font-weight: 600;
            text-shadow: 0 0 20px rgba(255, 255, 255, 0.3);
        }

        .finale-action {
            animation: actionReveal 1.5s cubic-bezier(0.165, 0.84, 0.44, 1) forwards;
            opacity: 0;
            transform: translateY(30px);
        }

        .finale-action[data-delay='1000'] { animation-delay: 1.5s; }

        .export-btn {
            position: relative;
            background: transparent;
            border: 2px solid rgba(255, 255, 255, 0.3);
            color: #ffffff;
            font-size: .9rem;
            font-weight: 600;
            letter-spacing: 0.1em;
            padding: 1rem 3rem;
            margin-bottom: 1rem;
            cursor: pointer;
            transition: all 0.3s ease;
            overflow: hidden;
        }

        .export-btn::before {
            content: '';
            position: absolute;
            top: 0;
            left: -100%;
            width: 100%;
            height: 100%;
            background: linear-gradient(90deg, 
                transparent 0%,
                rgba(255, 255, 255, 0.1) 50%,
                transparent 100%
            );
            transition: left 0.5s ease;
        }

        .export-btn:hover::before {
            left: 100%;
        }

        .export-btn:hover {
            border-color: rgba(255, 255, 255, 0.6);
            box-shadow: 0 0 30px rgba(255, 255, 255, 0.2);
        }

        .btn-glow {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: radial-gradient(circle, rgba(255, 255, 255, 0.1) 0%, transparent 70%);
            opacity: 0;
            transition: opacity 0.3s ease;
        }

        .export-btn:hover .btn-glow {
            opacity: 1;
        }

        .btn-text {
            position: relative;
            z-index: 1;
        }

        .action-subtitle {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.4);
            font-weight: 300;
            letter-spacing: 0.2em;
        }

        @keyframes hyperspaceJump {
            0% {
                transform: scale(1) translateZ(0);
                opacity: 1;
            }
            50% {
                transform: scale(1.2) translateZ(100px);
                opacity: 0.8;
            }
            100% {
                transform: scale(50) translateZ(2000px);
                opacity: 0;
            }
        }

        @keyframes velocityStream {
            0% {
                transform: translateY(0);
            }
            100% {
                transform: translateY(-100px);
            }
        }

        @keyframes vortexPulse {
            0%, 100% {
                transform: translate(-50%, -50%) scale(1);
                opacity: 0.1;
            }
            50% {
                transform: translate(-50%, -50%) scale(1.3);
                opacity: 0.3;
            }
        }

        @keyframes debrisStream {
            0% {
                transform: scale(1) rotate(0deg);
                opacity: 0.6;
            }
            100% {
                transform: scale(0.1) rotate(360deg);
                opacity: 0;
            }
        }

        @keyframes headerMaterialize {
            0% {
                opacity: 0;
                transform: translateY(-50px);
                filter: blur(10px);
            }
            100% {
                opacity: 1;
                transform: translateY(0);
                filter: blur(0);
            }
        }

        @keyframes nodeAppear {
            0% {
                opacity: 0;
                transform: translateY(50px);
            }
            100% {
                opacity: 1;
                transform: translateY(0);
            }
        }

        @keyframes beamActivate {
            0% {
                opacity: 0;
                transform: scaleX(0);
            }
            100% {
                opacity: 1;
                transform: scaleX(1);
            }
        }

        @keyframes beamSweep {
            0% {
                transform: translateX(-100%);
            }
            100% {
                transform: translateX(100%);
            }
        }

        @keyframes actionReveal {
            0% {
                opacity: 0;
                transform: translateY(30px);
            }
            100% {
                opacity: 1;
                transform: translateY(0);
            }
        }

        @media (max-width: 768px) {
            .finale-title {
                font-size: 3rem;
            }
            
            .finale-year {
                font-size: 5rem;
            }
            
            .stats-constellation {
                flex-direction: column;
                gap: 2rem;
            }
            
            .highlights-section {
                flex-direction: column;
                gap: 2rem;
            }
        }
    ";
}
        

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var totalMinutes = Math.Round(stats.TotalTracks * 3.5);
            var totalHours = Math.Round(totalMinutes / 60);
            
            return $"You spent {totalHours} hours exploring music in {year} - that's a soundtrack worth celebrating!";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return stats.TotalTracks > 0; // Always show finale if we have any data
        }
    }
}