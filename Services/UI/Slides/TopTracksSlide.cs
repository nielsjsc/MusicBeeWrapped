using System;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Top Tracks slide component - Displays the user's most played tracks
    /// Shows track titles with artist information and play counts
    /// </summary>
    public class TopTracksSlide : SlideComponentBase
    {
        public override string SlideId => "top-5-songs";
        public override string SlideTitle => "Musical Constellation";
        public override int SlideOrder => 3;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var topTracks = stats.TopTracks.Take(5).ToList();
            
            var tracksHTML = string.Join("", topTracks.Select((track, index) => {
                var parts = track.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                string title, artist;
                
                if (parts.Length == 2)
                {
                    title = parts[0];
                    artist = parts[1];
                }
                else
                {
                    title = track.Key;
                    artist = "Unknown Artist";
                }

                var playCount = track.Value;
                
                return $@"
                    <div class='constellation-track' data-index='{index}' data-plays='{playCount}'>
                        <div class='track-star'></div>
                        <div class='track-content'>
                            <div class='track-rank'>#{index + 1}</div>
                            <div class='track-details'>
                                <div class='track-title'>{EscapeHtml(title)}</div>
                                <div class='track-artist'>{EscapeHtml(artist)}</div>
                                <div class='track-plays'>{playCount} plays</div>
                            </div>
                        </div>
                        <div class='constellation-line'></div>
                    </div>";
            }));

            var content = $@"
                <div class='constellation-container'>
                    <!-- Animated background -->
                    <div class='cosmic-background'>
                        <div class='floating-particles'></div>
                        <div class='nebula-glow'></div>
                    </div>
                    
                    <!-- Content -->
                    <div class='constellation-content'>
                        <div class='constellation-header'>
                            <h1 class='constellation-title'>Your Top Songs</h1>
                            <p class='constellation-subtitle'>The tracks that lit up your {year}</p>
                        </div>
                        
                        <div class='constellation-tracks'>
                            {tracksHTML}
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        private string CreateTopTracksList(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, int>> tracks)
        {
            if (tracks == null || !tracks.Any()) return "";

            var html = new StringBuilder();
            html.AppendLine("                <ul class='top-list'>");

            int rank = 1;
            foreach (var track in tracks)
            {
                var (title, artist) = ParseTrackString(track.Key);
                
                html.AppendLine("                    <li>");
                html.AppendLine($"                        <span class='rank'>#{rank}</span>");
                html.AppendLine($"                        <span class='name'>{EscapeHtml(title)} <small>by {EscapeHtml(artist)}</small></span>");
                html.AppendLine($"                        <span class='count'>{track.Value} plays</span>");
                html.AppendLine("                    </li>");
                rank++;
            }

            html.AppendLine("                </ul>");
            return html.ToString();
        }

        private (string title, string artist) ParseTrackString(string trackKey)
        {
            // Handle format "Artist - Title"
            var parts = trackKey.Split(new[] { " - " }, 2, System.StringSplitOptions.None);
            if (parts.Length == 2)
            {
                return (parts[1], parts[0]); // Return (title, artist)
            }
            return (trackKey, "Unknown Artist");
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            return GetTrackInsight(stats);
        }

        public override string GetInsightTitle(WrappedStatistics stats)
        {
            return "Your North Star";
        }

        private string GetTrackInsight(WrappedStatistics stats)
        {
            if (!stats.TopTracks.Any()) 
                return "No track data available for this year.";

            var topTrack = stats.TopTracks.First();
            var (title, artist) = ParseTrackString(topTrack.Key);
            var playCount = topTrack.Value;
            
            string cosmicText;
            if (playCount >= 50)
                cosmicText = "This stellar track dominated your musical universe!";
            else if (playCount >= 25)
                cosmicText = "A shining beacon in your playlist constellation!";
            else if (playCount >= 10)
                cosmicText = "This bright star guided your musical journey!";
            else
                cosmicText = "A luminous gem in your cosmic collection!";

            return $"<strong>'{EscapeHtml(title)}'</strong> by {EscapeHtml(artist)} " +
                   $"was your brightest star with {playCount} plays. {cosmicText}";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) && 
                   stats.TopTracks != null && 
                   stats.TopTracks.Any();
        }

        public override string GenerateCSS()
        {
            return @"
                .constellation-container {
                    position: relative;
                    width: 100%;
                    height: 100vh;
                    background: linear-gradient(135deg, #0a0a2e 0%, #16213e 50%, #1a1a3a 100%);
                    overflow: hidden;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }

                .cosmic-background {
                    position: absolute;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    z-index: 1;
                }

                .floating-particles::before {
                    content: '';
                    position: absolute;
                    width: 100%;
                    height: 100%;
                    background-image: 
                        radial-gradient(2px 2px at 20px 30px, rgba(255,255,255,0.8), transparent),
                        radial-gradient(2px 2px at 40px 70px, rgba(255,255,255,0.6), transparent),
                        radial-gradient(1px 1px at 90px 40px, rgba(255,255,255,0.9), transparent),
                        radial-gradient(1px 1px at 130px 80px, rgba(255,255,255,0.7), transparent),
                        radial-gradient(2px 2px at 160px 30px, rgba(255,255,255,0.8), transparent);
                    background-size: 200px 100px;
                    animation: float 20s linear infinite;
                }

                .nebula-glow {
                    position: absolute;
                    top: 20%;
                    left: 10%;
                    width: 300px;
                    height: 200px;
                    background: radial-gradient(ellipse, rgba(138, 43, 226, 0.3) 0%, transparent 70%);
                    border-radius: 50%;
                    animation: pulse 8s ease-in-out infinite;
                }

                .constellation-content {
                    position: relative;
                    z-index: 2;
                    max-width: 800px;
                    width: 90%;
                    margin: 0 auto;
                    text-align: center;
                }

                .constellation-header {
                    margin-bottom: 2rem;
                    animation: fadeInUp 1s ease-out;
                }

                .constellation-title {
                    font-size: 2.5rem;
                    font-weight: 300;
                    background: linear-gradient(45deg, #ffffff, #b19cd9, #ffffff);
                    background-size: 200% 200%;
                    -webkit-background-clip: text;
                    -webkit-text-fill-color: transparent;
                    background-clip: text;
                    animation: shimmer 3s ease-in-out infinite;
                    margin-bottom: 0.5rem;
                    letter-spacing: 2px;
                }

                .constellation-subtitle {
                    font-size: 1rem;
                    color: rgba(255, 255, 255, 0.7);
                    font-weight: 300;
                    letter-spacing: 1px;
                }

                .constellation-tracks {
                    display: flex;
                    flex-direction: column;
                    align-items: center;
                    gap: 1rem;
                    margin-top: 1.5rem;
                }

                .constellation-track {
                    position: relative;
                    display: flex;
                    align-items: center;
                    width: 100%;
                    max-width: 550px;
                    opacity: 0;
                    transform: translateX(-50px);
                    animation: slideInTrack 0.8s ease-out forwards;
                }

                .constellation-track:nth-child(1) { animation-delay: 0.2s; }
                .constellation-track:nth-child(2) { animation-delay: 0.4s; }
                .constellation-track:nth-child(3) { animation-delay: 0.6s; }
                .constellation-track:nth-child(4) { animation-delay: 0.8s; }
                .constellation-track:nth-child(5) { animation-delay: 1.0s; }

                .track-star {
                    width: 12px;
                    height: 12px;
                    background: linear-gradient(45deg, #ffd700, #ffed4e);
                    border-radius: 50%;
                    position: relative;
                    margin-right: 1rem;
                    box-shadow: 0 0 15px rgba(255, 215, 0, 0.6);
                    animation: twinkle 2s ease-in-out infinite;
                }

                .track-star::before,
                .track-star::after {
                    content: '';
                    position: absolute;
                    background: linear-gradient(45deg, #ffd700, #ffed4e);
                    border-radius: 50%;
                }

                .track-star::before {
                    width: 3px;
                    height: 15px;
                    top: -1.5px;
                    left: 4.5px;
                }

                .track-star::after {
                    width: 15px;
                    height: 3px;
                    top: 4.5px;
                    left: -1.5px;
                }

                .track-content {
                    flex: 1;
                    display: flex;
                    align-items: center;
                    gap: 1rem;
                    background: rgba(255, 255, 255, 0.05);
                    padding: 0.8rem 1.2rem;
                    border-radius: 15px;
                    border: 1px solid rgba(255, 255, 255, 0.1);
                    backdrop-filter: blur(10px);
                    transition: all 0.3s ease;
                }

                .track-content:hover {
                    background: rgba(255, 255, 255, 0.1);
                    border-color: rgba(255, 255, 255, 0.2);
                    transform: translateY(-2px);
                    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
                }

                .track-rank {
                    font-size: 1.4rem;
                    font-weight: 600;
                    color: #ffd700;
                    min-width: 40px;
                    text-align: center;
                    text-shadow: 0 0 10px rgba(255, 215, 0, 0.5);
                }

                .track-details {
                    flex: 1;
                    text-align: left;
                }

                .track-title {
                    font-size: 1rem;
                    font-weight: 600;
                    color: #ffffff;
                    margin-bottom: 0.2rem;
                    line-height: 1.2;
                }

                .track-artist {
                    font-size: 0.85rem;
                    color: rgba(255, 255, 255, 0.7);
                    margin-bottom: 0.2rem;
                    font-weight: 300;
                }

                .track-plays {
                    font-size: 0.75rem;
                    color: #b19cd9;
                    font-weight: 500;
                }

                .constellation-line {
                    position: absolute;
                    right: -30px;
                    top: 50%;
                    width: 60px;
                    height: 1px;
                    background: linear-gradient(90deg, rgba(255, 255, 255, 0.3), transparent);
                }

                .constellation-track:last-child .constellation-line {
                    display: none;
                }

                @keyframes slideInTrack {
                    to {
                        opacity: 1;
                        transform: translateX(0);
                    }
                }

                @keyframes twinkle {
                    0%, 100% { opacity: 1; transform: scale(1); }
                    50% { opacity: 0.7; transform: scale(1.1); }
                }

                @keyframes shimmer {
                    0% { background-position: 0% 50%; }
                    50% { background-position: 100% 50%; }
                    100% { background-position: 0% 50%; }
                }

                @keyframes float {
                    0% { transform: translateY(0px); }
                    50% { transform: translateY(-10px); }
                    100% { transform: translateY(0px); }
                }

                @keyframes pulse {
                    0%, 100% { opacity: 0.3; transform: scale(1); }
                    50% { opacity: 0.5; transform: scale(1.1); }
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

                /* Responsive design */
                @media (max-width: 768px) {
                    .constellation-title {
                        font-size: 2rem;
                    }
                    
                    .constellation-subtitle {
                        font-size: 0.9rem;
                    }
                    
                    .track-content {
                        padding: 0.6rem 1rem;
                        flex-direction: column;
                        text-align: center;
                        gap: 0.8rem;
                    }
                    
                    .track-details {
                        text-align: center;
                    }
                    
                    .track-rank {
                        font-size: 1.2rem;
                    }
                    
                    .track-title {
                        font-size: 0.9rem;
                    }
                }
            ";
        }
    }
}
