using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Top Albums slide component - Displays the user's most played albums
    /// Shows album names with artist information and top tracks from each album
    /// </summary>
    public class TopAlbumsSlide : SlideComponentBase
    {
        public override string SlideId => "top-albums";
        public override string SlideTitle => "Your Top Albums";
        public override int SlideOrder => 14;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var topAlbum = stats.TopAlbums.FirstOrDefault();
            var albumName = topAlbum.Key ?? "No album data";
            var albumPlays = topAlbum.Value;
            var albumPercentage = stats.TotalTracks > 0 ? Math.Round((double)albumPlays / stats.TotalTracks * 100) : 0;

            // Get top tracks from this album (if available)
            var albumTracks = GetTopTracksFromAlbum(topAlbum.Key, playHistory, year);

            var content = $@"
                <div class='album-slide-container'>
                    <div class='slide-header'>
                        <h2>💿 Favorite Album</h2>
                        <p class='slide-subtitle'>The album that dominated your {year}</p>
                    </div>
                    
                    <div class='album-main-content'>
                        <div class='featured-album'>
                            <div class='album-hero'>
                                <div class='album-icon-large'>💿</div>
                                <div class='album-info'>
                                    <div class='album-name'>{EscapeHtml(albumName)}</div>
                                    <div class='album-meta'>
                                        <span class='album-plays'>{albumPlays} plays</span>
                                        <span class='album-percentage'>{albumPercentage}% of your music</span>
                                    </div>
                                </div>
                            </div>
                            {(albumTracks.Any() ? $@"
                                <div class='album-tracks-section'>
                                    <h4 class='tracks-title'>Top tracks from this album</h4>
                                    <div class='tracks-list'>
                                        {string.Join("", albumTracks.Take(3).Select((track, index) => $@"
                                            <div class='track-item'>
                                                <span class='track-number'>{index + 1}</span>
                                                <div class='track-details'>
                                                    <span class='track-title'>{EscapeHtml(track.Title)}</span>
                                                    <span class='track-count'>{track.PlayCount} plays</span>
                                                </div>
                                            </div>
                                        "))}
                                    </div>
                                </div>
                            " : "")}
                        </div>
                        
                        {(stats.TopAlbums.Count() > 1 ? @"
                            <div class='other-albums-section'>
                                <h3 class='section-title'>Other Favorites</h3>
                                <div class='albums-grid'>
                                </div>
                            </div>
                        " : "")}
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        private class AlbumTrack
        {
            public string Title { get; set; }
            public int PlayCount { get; set; }
        }

        private List<AlbumTrack> GetTopTracksFromAlbum(string albumName, PlayHistory playHistory, int year)
        {
            if (string.IsNullOrEmpty(albumName))
                return new List<AlbumTrack>();

            var yearPlays = playHistory.GetPlaysByYear(year);
            var albumPlays = yearPlays.Where(p => p.Album?.Equals(albumName, StringComparison.OrdinalIgnoreCase) == true);
            
            return albumPlays
                .GroupBy(p => p.Title)
                .Select(g => new AlbumTrack { Title = g.Key, PlayCount = g.Count() })
                .OrderByDescending(t => t.PlayCount)
                .ToList();
        }

        private string CreateTopAlbumsList(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, int>> albums, PlayHistory playHistory, int year)
        {
            if (albums == null || !albums.Any()) return "";

            var html = new StringBuilder();
            html.AppendLine("                <ul class='top-list'>");

            int rank = 1;
            foreach (var album in albums)
            {
                var (albumName, artistName) = ParseAlbumString(album.Key);
                var topTracks = GetTopTracksFromAlbum(albumName, artistName, playHistory, year);
                
                html.AppendLine("                    <li>");
                html.AppendLine($"                        <span class='rank'>#{rank}</span>");
                html.AppendLine($"                        <span class='name'>");
                html.AppendLine($"                            {EscapeHtml(albumName)} <small>by {EscapeHtml(artistName)}</small>");
                
                if (!string.IsNullOrEmpty(topTracks))
                {
                    html.AppendLine($"                            <br><small style='opacity: 0.6;'>Top tracks: {topTracks}</small>");
                }
                
                html.AppendLine($"                        </span>");
                html.AppendLine($"                        <span class='count'>{album.Value} plays</span>");
                html.AppendLine("                    </li>");
                rank++;
            }

            html.AppendLine("                </ul>");
            return html.ToString();
        }

        private (string albumName, string artistName) ParseAlbumString(string albumKey)
        {
            // Handle format "Album - Artist"
            var parts = albumKey.Split(new[] { " - " }, 2, System.StringSplitOptions.None);
            if (parts.Length == 2)
            {
                return (parts[0], parts[1]); // Return (album, artist)
            }
            return (albumKey, "Unknown Artist");
        }

        private string GetTopTracksFromAlbum(string albumName, string artistName, PlayHistory playHistory, int year)
        {
            var albumTracks = playHistory.GetPlaysByYear(year)
                .Where(p => p.Album == albumName && p.Artist == artistName)
                .GroupBy(p => p.Title)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .OrderByDescending(t => t.Count)
                .Take(3)
                .Select(t => EscapeHtml(t.Title))
                .ToList();

            return string.Join(", ", albumTracks);
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            return GetAlbumInsight(stats);
        }

        public override string GetInsightTitle(WrappedStatistics stats)
        {
            return "Album Deep Dive";
        }

        private string GetAlbumInsight(WrappedStatistics stats)
        {
            if (!stats.TopAlbums.Any()) 
                return "No album data available for this year.";

            var topAlbum = stats.TopAlbums.First();
            var (albumName, artistName) = ParseAlbumString(topAlbum.Key);
            var playCount = topAlbum.Value;
            
            string connectionText;
            if (playCount >= 30)
                connectionText = "You had a deep connection with this complete body of work!";
            else if (playCount >= 15)
                connectionText = "This album really spoke to you this year!";
            else if (playCount >= 8)
                connectionText = "You found yourself returning to this album regularly!";
            else
                connectionText = "This album made a lasting impression!";

            return $"You really connected with <strong>'{EscapeHtml(albumName)}'</strong> " +
                   $"by {EscapeHtml(artistName)}, playing it {playCount} times! {connectionText}";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) && 
                   stats.TopAlbums != null && 
                   stats.TopAlbums.Any();
        }
    }
}
