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
                    <div class='track-item' data-delay='{index * 150}' data-plays='{playCount}'>
                        <div class='track-rank'>#{index + 1}</div>
                        <div class='track-info'>
                            <div class='track-title'>{EscapeHtml(title)}</div>
                            <div class='track-artist'>{EscapeHtml(artist)}</div>
                        </div>
                        <div class='play-count-tooltip'>{playCount} plays</div>
                    </div>";
            }));

            var content = $@"
                <div class='night-sky-container'>
                    <!-- Night sky background -->
                    <div class='night-sky'>
                        <!-- Stars -->
                        <div class='star star-1'></div>
                        <div class='star star-2'></div>
                        <div class='star star-3'></div>
                        <div class='star star-4'></div>
                        <div class='star star-5'></div>
                        <div class='star star-6'></div>
                        <div class='star star-7'></div>
                        <div class='star star-8'></div>
                        <div class='star star-9'></div>
                        <div class='star star-10'></div>
                        
                        <!-- Comets -->
                        <div class='comet comet-1'></div>
                        <div class='comet comet-2'></div>
                        
                        <!-- Mountains silhouette -->
                        <div class='mountains'></div>
                    </div>
                    
                    <!-- Content -->
                    <div class='content-overlay'>
                        <div class='slide-header'>
                            <h2>Your Top Tracks</h2>
                            <p>The songs that soundtracked your year</p>
                        </div>
                        
                        <div class='tracks-list'>
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
    }
}
