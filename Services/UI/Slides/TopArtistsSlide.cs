using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Top Artists slide component - Displays the user's most played artists
    /// Shows rankings with play counts and provides artist-focused insights
    /// </summary>
    public class TopArtistsSlide : SlideComponentBase
    {
        public override string SlideId => "top-artist-of-year";
        public override string SlideTitle => "Your Top Artist of the Year";
        public override int SlideOrder => 5;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var topArtists = stats.TopArtists.Take(5).ToList();
            
            var topArtistsHTML = string.Join("", topArtists.Select((artist, index) => $@"
                <div class='top-item' data-delay='{index * 100}'>
                    <div class='top-rank'>#{index + 1}</div>
                    <div class='top-content'>
                        <div class='top-name'>{EscapeHtml(artist.Key)}</div>
                        <div class='top-subtitle'>Your favorite artist</div>
                    </div>
                    <div class='top-count'>{artist.Value} plays</div>
                </div>"));

            var content = $@"
                <h2>🎤 Your Top Artists</h2>
                <p style='margin-bottom: 1rem; opacity: 0.8;'>The artists that defined your {year}</p>
                <div class='top-list'>
                    {topArtistsHTML}
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            return GetArtistInsight(stats);
        }

        public override string GetInsightTitle(WrappedStatistics stats)
        {
            return "Artist Spotlight";
        }

        private string GetArtistInsight(WrappedStatistics stats)
        {
            if (!stats.TopArtists.Any()) 
                return "No artist data available for this year.";

            var topArtist = stats.TopArtists.First();
            var playCount = topArtist.Value;
            
            string intensityText;
            if (playCount >= 100)
                intensityText = "That's some serious dedication!";
            else if (playCount >= 50)
                intensityText = "You really connected with their music!";
            else if (playCount >= 20)
                intensityText = "A solid favorite this year!";
            else
                intensityText = "They clearly made an impression!";

            return $"You played <strong>{EscapeHtml(topArtist.Key)}</strong> " +
                   $"{playCount} times this year. {intensityText}";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) && 
                   stats.TopArtists != null && 
                   stats.TopArtists.Any();
        }
    }
}
