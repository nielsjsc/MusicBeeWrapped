using System;
using System.Linq;
using System.Collections.Generic;
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
            var artistTracks = GetArtistTracks(stats, playHistory, year);
            
            var topArtistsHTML = string.Join("", topArtists.Select((artist, index) => {
                var tracks = GetTracksForArtist(artist.Key, artistTracks);
                var tracksHTML = "";
                
                if (tracks.Any())
                {
                    tracksHTML = string.Join("", tracks.Select((track, trackIndex) => $@"
                        <div class='orbit-track' data-track-delay='{trackIndex * 50}'>
                            <div class='planet-core'></div>
                            <div class='track-info'>
                                <div class='track-title'>{EscapeHtml(track.Title)}</div>
                                <div class='track-plays'>{track.PlayCount} plays</div>
                            </div>
                            <div class='orbital-trail'></div>
                        </div>
                    "));
                }
                else
                {
                    // Enhanced debug info when no tracks found
                    var availableArtists = string.Join(", ", artistTracks.Keys.Take(3));
                    var totalTracksFound = artistTracks.Values.Sum(list => list.Count);
                    
                    tracksHTML = $@"
                        <div class='orbit-track no-tracks-debug'>
                            <div class='track-info'>
                                <div class='track-title'>No tracks found for ""{EscapeHtml(artist.Key)}""</div>
                                <div class='track-plays'>Found {totalTracksFound} total tracks for artists: {EscapeHtml(availableArtists)}...</div>
                            </div>
                        </div>";
                }

                return $@"
                    <div class='stellar-system' data-artist='{EscapeHtml(artist.Key)}' data-delay='{index * 100}'>
                        <div class='star-core clickable-star' 
                             data-artist-id='artist-{index}' 
                             role='button' 
                             tabindex='0'
                             aria-expanded='false'
                             aria-controls='tracks-{index}'>
                            <div class='stellar-nucleus'></div>
                            <div class='corona-glow'></div>
                            <div class='stellar-rings'></div>
                            
                            <div class='star-info'>
                                <div class='star-rank'>#{index + 1}</div>
                                <div class='star-content'>
                                    <div class='artist-name'>{EscapeHtml(artist.Key)}</div>
                                </div>
                                <div class='play-count-badge'>{artist.Value}</div>
                            </div>
                            
                            <div class='expand-indicator'>
                                <div class='expand-icon'></div>
                            </div>
                        </div>
                        
                        <div class='orbital-system' id='tracks-{index}' aria-hidden='true'>
                            <div class='orbital-tracks'>
                                {tracksHTML}
                            </div>
                        </div>
                    </div>";
            }));

            var content = $@"
                <div class='artists-galaxy-container'>
                    <div class='cosmic-background'>
                        <div class='distant-stars'></div>
                        <div class='galactic-dust'></div>
                        <div class='space-nebula'></div>
                    </div>
                    
                    <div class='galaxy-content'>
                        <div class='galaxy-header'>
                            <div class='category-tag'>Musical Galaxy</div>
                            <h2 class='galaxy-title'>
                                <span class='title-line-1'>Your</span>
                                <span class='title-line-2'>Artist</span>
                                <span class='title-line-3'>Universe</span>
                            </h2>
                            <div class='galaxy-subtitle'>The stellar artists that shaped your {year}</div>
                        </div>
                        
                        <div class='stellar-field'>
                            {topArtistsHTML}
                        </div>
                        
                        <div class='interaction-hint'>
                            <div class='hint-glow'></div>
                            <span class='hint-text'>Click any artist to explore their top tracks</span>
                        </div>
                    </div>
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

        /// <summary>
        /// Groups tracks by artist and returns the top tracks for each artist
        /// </summary>
        private Dictionary<string, List<TrackInfo>> GetArtistTracks(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var artistTracks = new Dictionary<string, List<TrackInfo>>(StringComparer.OrdinalIgnoreCase);

            if (stats.TopTracks == null || !stats.TopTracks.Any())
                return artistTracks;

            // Group tracks by artist from the TopTracks data
            // TopTracks format appears to be "Title - Artist" based on other slides
            foreach (var track in stats.TopTracks)
            {
                var trackInfo = ParseTrackInfo(track.Key, track.Value);
                if (!string.IsNullOrEmpty(trackInfo.Artist) && trackInfo.Artist != "Unknown Artist")
                {
                    // Use case-insensitive key matching
                    var artistKey = trackInfo.Artist;
                    if (!artistTracks.ContainsKey(artistKey))
                    {
                        artistTracks[artistKey] = new List<TrackInfo>();
                    }
                    artistTracks[artistKey].Add(trackInfo);
                }
            }

            // Sort tracks within each artist by play count and take top 5
            foreach (var artist in artistTracks.Keys.ToList())
            {
                artistTracks[artist] = artistTracks[artist]
                    .OrderByDescending(t => t.PlayCount)
                    .Take(5)
                    .ToList();
            }

            return artistTracks;
        }

        /// <summary>
        /// Parses track string into structured track information
        /// Based on TopTracks format: "Artist - Title" (NOT "Title - Artist")
        /// </summary>
        private TrackInfo ParseTrackInfo(string trackString, int playCount)
        {
            if (string.IsNullOrEmpty(trackString))
            {
                return new TrackInfo
                {
                    Title = "Unknown Track",
                    Artist = "Unknown Artist",
                    PlayCount = playCount
                };
            }

            var parts = trackString.Split(new[] { " - " }, 2, StringSplitOptions.None);
            
            if (parts.Length == 2)
            {
                // Format: "Artist - Title" (from WrappedStatistics.cs line 114)
                return new TrackInfo
                {
                    Title = parts[1].Trim(),     // Second part is Title
                    Artist = parts[0].Trim(),   // First part is Artist
                    PlayCount = playCount
                };
            }
            
            // Fallback for unexpected format
            return new TrackInfo
            {
                Title = trackString.Trim(),
                Artist = "Unknown Artist",
                PlayCount = playCount
            };
        }

        /// <summary>
        /// Gets the top tracks for a specific artist with flexible matching
        /// </summary>
        private List<TrackInfo> GetTracksForArtist(string artistName, Dictionary<string, List<TrackInfo>> artistTracks)
        {
            // Try exact match first (case-insensitive)
            if (artistTracks.TryGetValue(artistName, out var tracks))
            {
                return tracks;
            }
            
            // Try fuzzy matching - look for any key that contains the artist name or vice versa
            foreach (var kvp in artistTracks)
            {
                var key = kvp.Key;
                if (string.Equals(key, artistName, StringComparison.OrdinalIgnoreCase) ||
                    key.ToLowerInvariant().Contains(artistName.ToLowerInvariant()) ||
                    artistName.ToLowerInvariant().Contains(key.ToLowerInvariant()))
                {
                    return kvp.Value;
                }
            }
            
            return new List<TrackInfo>();
        }

        /// <summary>
        /// Data structure for track information
        /// </summary>
        private class TrackInfo
        {
            public string Title { get; set; }
            public string Artist { get; set; }
            public int PlayCount { get; set; }
        }

        public override string GenerateCSS()
        {
            return @"
        /* Artists Galaxy - Cosmic Theme with Expandable Functionality */
        .artists-galaxy-container {
            position: relative;
            width: 100%;
            height: 100vh;
            overflow: hidden;
            background: linear-gradient(135deg, #0c0c0c 0%, #1a1a2e 25%, #16213e 50%, #0f3460 75%, #533483 100%);
        }

        /* Cosmic Background */
        .cosmic-background {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 1;
        }

        .distant-stars {
            position: absolute;
            width: 100%;
            height: 100%;
            background-image: 
                radial-gradient(circle at 15% 25%, rgba(255, 255, 255, 0.8) 1px, transparent 1px),
                radial-gradient(circle at 85% 15%, rgba(83, 52, 131, 0.6) 1px, transparent 1px),
                radial-gradient(circle at 45% 85%, rgba(15, 52, 96, 0.5) 1px, transparent 1px),
                radial-gradient(circle at 75% 65%, rgba(255, 255, 255, 0.4) 1px, transparent 1px);
            background-size: 25vw 25vh, 30vw 30vh, 20vw 20vh, 35vw 35vh;
            animation: star-twinkle 4s ease-in-out infinite;
        }

        .galactic-dust {
            position: absolute;
            width: 100%;
            height: 100%;
            background: 
                radial-gradient(ellipse at 20% 40%, rgba(83, 52, 131, 0.1) 0%, transparent 50%),
                radial-gradient(ellipse at 80% 60%, rgba(15, 52, 96, 0.1) 0%, transparent 50%);
            animation: dust-drift 15s linear infinite;
        }

        .space-nebula {
            position: absolute;
            width: 100%;
            height: 100%;
            background: radial-gradient(ellipse at 50% 30%, rgba(83, 52, 131, 0.05) 0%, transparent 70%);
            filter: blur(2vw);
            animation: nebula-pulse 8s ease-in-out infinite;
        }

        /* Content Layout */
        .galaxy-content {
            position: relative;
            z-index: 2;
            display: flex;
            flex-direction: column;
            justify-content: flex-start;
            align-items: center;
            height: 100vh;
            padding: clamp(0.1rem, 0.3vw, 0.3rem);
            overflow-y: auto;
            padding-top: clamp(0.1rem, 0.2vh, 0.2rem);
            gap: clamp(0.1rem, 0.2vh, 0.2rem);
        }

        .galaxy-header {
            text-align: center;
            margin-bottom: 0;
            flex-shrink: 0;
            margin-top: 0;
        }

        .category-tag {
            font-size: clamp(0.6rem, 1vw, 0.7rem);
            color: rgba(83, 52, 131, 0.8);
            text-transform: uppercase;
            letter-spacing: 0.15vw;
            margin-bottom: 0;
            margin-top: 0;
            font-weight: 500;
        }

        .galaxy-title {
            font-size: clamp(1.5rem, 3.5vw, 2rem);
            font-weight: 700;
            line-height: 0.9;
            margin-bottom: 0;
            margin-top: 0;
            background: linear-gradient(135deg, #ffffff 0%, #b8b8b8 50%, #533483 100%);
            background-clip: text;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }

        .title-line-1, .title-line-2, .title-line-3 {
            display: block;
            animation: title-entrance 0.8s ease-out forwards;
            opacity: 0;
            transform: translateY(1vh);
        }

        .title-line-1 { animation-delay: 0.2s; }
        .title-line-2 { animation-delay: 0.4s; }
        .title-line-3 { animation-delay: 0.6s; }

        .galaxy-subtitle {
            font-size: clamp(0.7rem, 1.2vw, 0.8rem);
            color: rgba(255, 255, 255, 0.7);
            font-weight: 300;
            letter-spacing: 0.08vw;
            margin-top: 0;
            margin-bottom: 0;
        }

        /* Stellar Field */
        .stellar-field {
            display: flex;
            flex-direction: column;
            gap: clamp(0.1rem, 0.3vh, 0.2rem);
            width: 100%;
            max-width: 45rem;
            flex: 1;
            min-height: 0;
            margin-top: clamp(0.1rem, 0.2vh, 0.1rem);
        }

        /* Stellar System (Individual Artist) */
        .stellar-system {
            position: relative;
            width: 100%;
            opacity: 0;
            transform: translateY(1vh);
            animation: stellar-entrance 0.8s ease-out forwards;
        }

        .stellar-system[data-delay='0'] { animation-delay: 0.8s; }
        .stellar-system[data-delay='100'] { animation-delay: 1s; }
        .stellar-system[data-delay='200'] { animation-delay: 1.2s; }
        .stellar-system[data-delay='300'] { animation-delay: 1.4s; }
        .stellar-system[data-delay='400'] { animation-delay: 1.6s; }

        /* Star Core (Artist Card) */
        .star-core {
            position: relative;
            background: rgba(16, 28, 44, 0.8);
            border: 1px solid rgba(83, 52, 131, 0.3);
            border-radius: 0.8rem;
            padding: clamp(0.6rem, 1.2vw, 0.9rem);
            cursor: pointer;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            overflow: hidden;
            backdrop-filter: blur(1rem);
        }

        .clickable-star:hover {
            transform: translateY(-0.3vh) scale(1.01);
            border-color: rgba(83, 52, 131, 0.6);
            box-shadow: 
                0 0.8vh 2vh rgba(83, 52, 131, 0.3),
                0 0 1.5vh rgba(83, 52, 131, 0.2);
        }

        .clickable-star:focus {
            outline: 2px solid rgba(83, 52, 131, 0.8);
            outline-offset: 2px;
        }

        .clickable-star[aria-expanded='true'] {
            transform: scale(1.05);
            border-color: rgba(83, 52, 131, 0.8);
            box-shadow: 
                0 1.5vh 4vh rgba(83, 52, 131, 0.4),
                0 0 3vh rgba(83, 52, 131, 0.3);
        }

        /* Stellar Effects */
        .stellar-nucleus {
            position: absolute;
            top: 50%;
            left: 1rem;
            width: clamp(8px, 1vw, 12px);
            height: clamp(8px, 1vw, 12px);
            background: radial-gradient(circle, #533483, #16213e);
            border-radius: 50%;
            transform: translateY(-50%);
            animation: nucleus-pulse 2s ease-in-out infinite;
        }

        .corona-glow {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: radial-gradient(ellipse at 10% 50%, rgba(83, 52, 131, 0.1) 0%, transparent 50%);
            border-radius: 1rem;
            opacity: 0;
            transition: opacity 0.4s ease;
        }

        .clickable-star:hover .corona-glow,
        .clickable-star[aria-expanded='true'] .corona-glow {
            opacity: 1;
        }

        .stellar-rings {
            position: absolute;
            top: 50%;
            left: 2.5rem;
            width: clamp(20px, 3vw, 30px);
            height: clamp(20px, 3vw, 30px);
            border: 1px solid rgba(83, 52, 131, 0.3);
            border-radius: 50%;
            transform: translateY(-50%);
            opacity: 0;
            transition: all 0.4s ease;
        }

        .clickable-star:hover .stellar-rings,
        .clickable-star[aria-expanded='true'] .stellar-rings {
            opacity: 1;
            animation: ring-rotation 4s linear infinite;
        }

        /* Star Info Layout */
        .star-info {
            display: flex;
            align-items: center;
            gap: clamp(0.8rem, 1.5vw, 1rem);
            margin-left: clamp(2.5rem, 4vw, 3.5rem);
        }

        .star-rank {
            font-size: clamp(1rem, 2vw, 1.2rem);
            font-weight: 600;
            color: rgba(83, 52, 131, 0.8);
            min-width: clamp(1.5rem, 3vw, 2.5rem);
        }

        .star-content {
            flex: 1;
        }

        .artist-name {
            font-size: clamp(0.95rem, 1.8vw, 1.1rem);
            font-weight: 600;
            color: #ffffff;
            margin-bottom: 0;
            line-height: 1.2;
        }

        .artist-subtitle {
            font-size: clamp(0.7rem, 1.2vw, 0.8rem);
            color: rgba(255, 255, 255, 0.6);
            line-height: 1.3;
        }

        .play-count-badge {
            background: linear-gradient(135deg, rgba(83, 52, 131, 0.8), rgba(15, 52, 96, 0.8));
            color: #ffffff;
            padding: clamp(0.4rem, 0.8vw, 0.6rem) clamp(0.6rem, 1.2vw, 0.9rem);
            border-radius: 1.5rem;
            font-size: clamp(0.7rem, 1.2vw, 0.8rem);
            font-weight: 600;
            text-shadow: 0 1px 2px rgba(0, 0, 0, 0.3);
        }

        /* Expand Indicator */
        .expand-indicator {
            margin-left: clamp(0.8rem, 1.5vw, 1.2rem);
        }

        .expand-icon {
            width: clamp(18px, 2.5vw, 22px);
            height: clamp(18px, 2.5vw, 22px);
            border: 2px solid rgba(83, 52, 131, 0.6);
            border-radius: 50%;
            position: relative;
            transition: all 0.3s ease;
        }

        .expand-icon::before,
        .expand-icon::after {
            content: '';
            position: absolute;
            top: 50%;
            left: 50%;
            background: rgba(83, 52, 131, 0.6);
            transition: all 0.3s ease;
        }

        .expand-icon::before {
            width: 60%;
            height: 2px;
            transform: translate(-50%, -50%);
        }

        .expand-icon::after {
            width: 2px;
            height: 60%;
            transform: translate(-50%, -50%);
        }

        .clickable-star[aria-expanded='true'] .expand-icon {
            border-color: rgba(83, 52, 131, 1);
            transform: rotate(45deg);
        }

        .clickable-star[aria-expanded='true'] .expand-icon::before,
        .clickable-star[aria-expanded='true'] .expand-icon::after {
            background: rgba(83, 52, 131, 1);
        }

        /* Active System Enhancement */
        .stellar-system.active-system {
            z-index: 10;
            margin-bottom: clamp(1.5rem, 3vh, 2.5rem);
        }

        .stellar-system.active-system .star-core {
            background: rgba(16, 28, 44, 0.95);
            border-color: rgba(83, 52, 131, 1);
            box-shadow: 
                0 2vh 4vh rgba(83, 52, 131, 0.4),
                0 0 3vh rgba(83, 52, 131, 0.3),
                inset 0 0 2vh rgba(83, 52, 131, 0.1);
        }

        /* Orbital System (Track List) */
        .orbital-system {
            max-height: 0;
            overflow: hidden;
            transition: max-height 0.6s cubic-bezier(0.4, 0, 0.2, 1);
            margin-top: 0;
            position: relative;
            z-index: 20;
        }

        .orbital-system.expanded {
            max-height: 40vh;
            margin-top: clamp(0.5rem, 1vh, 0.8rem);
            margin-bottom: clamp(1rem, 2vh, 1.5rem);
        }

        .orbital-tracks {
            padding: clamp(0.6rem, 1.2vw, 0.9rem);
            background: rgba(12, 12, 12, 0.6);
            border: 1px solid rgba(83, 52, 131, 0.2);
            border-radius: 0.8rem;
            backdrop-filter: blur(0.5rem);
            margin-top: clamp(0.4rem, 0.8vh, 0.6rem);
        }

        /* Individual Track (Planet) */
        .orbit-track {
            display: flex;
            align-items: center;
            gap: clamp(0.6rem, 1.2vw, 0.8rem);
            padding: clamp(0.4rem, 0.8vw, 0.6rem) 0;
            border-bottom: 1px solid rgba(83, 52, 131, 0.1);
            opacity: 0;
            transform: translateX(-1.5vw);
            animation: orbit-entrance 0.5s ease-out forwards;
        }

        .orbit-track:last-child {
            border-bottom: none;
        }

        .orbit-track[data-track-delay='0'] { animation-delay: 0.1s; }
        .orbit-track[data-track-delay='50'] { animation-delay: 0.15s; }
        .orbit-track[data-track-delay='100'] { animation-delay: 0.2s; }
        .orbit-track[data-track-delay='150'] { animation-delay: 0.25s; }
        .orbit-track[data-track-delay='200'] { animation-delay: 0.3s; }

        .no-tracks-debug {
            color: rgba(255, 255, 0, 0.8) !important;
            font-style: italic;
        }

        .planet-core {
            width: clamp(6px, 1vw, 8px);
            height: clamp(6px, 1vw, 8px);
            background: linear-gradient(135deg, #533483, #16213e);
            border-radius: 50%;
            animation: planet-orbit 3s linear infinite;
            flex-shrink: 0;
        }

        .track-info {
            flex: 1;
        }

        .track-title {
            font-size: clamp(0.85rem, 1.6vw, 0.95rem);
            color: #ffffff;
            font-weight: 500;
            margin-bottom: clamp(0.2rem, 0.4vh, 0.3rem);
            line-height: 1.3;
        }

        .track-plays {
            font-size: clamp(0.7rem, 1.3vw, 0.8rem);
            color: rgba(255, 255, 255, 0.5);
            line-height: 1.2;
        }

        .orbital-trail {
            width: clamp(15px, 2vw, 20px);
            height: 1px;
            background: linear-gradient(90deg, transparent, rgba(83, 52, 131, 0.3), transparent);
            opacity: 0.6;
        }

        /* Interaction Hint */
        .interaction-hint {
            position: fixed;
            top: 50%;
            right: clamp(1rem, 3vw, 2rem);
            transform: translateY(-50%);
            display: flex;
            align-items: center;
            gap: clamp(0.6rem, 1.2vw, 0.8rem);
            opacity: 0;
            animation: hint-fade-in 1s ease-out 2s forwards;
            z-index: 5;
            background: rgba(12, 12, 12, 0.7);
            padding: clamp(0.6rem, 1.2vw, 0.8rem) clamp(0.8rem, 1.5vw, 1rem);
            border-radius: 2rem;
            border: 1px solid rgba(83, 52, 131, 0.3);
            backdrop-filter: blur(0.5rem);
        }

        .hint-glow {
            width: clamp(5px, 0.8vw, 6px);
            height: clamp(5px, 0.8vw, 6px);
            background: #533483;
            border-radius: 50%;
            animation: glow-pulse 2s ease-in-out infinite;
        }

        .hint-text {
            font-size: clamp(0.7rem, 1.2vw, 0.8rem);
            color: rgba(255, 255, 255, 0.6);
            font-style: italic;
            white-space: nowrap;
        }

        /* Animations */
        @keyframes star-twinkle {
            0%, 100% { opacity: 0.8; }
            50% { opacity: 1; }
        }

        @keyframes dust-drift {
            0% { transform: translateX(0); }
            100% { transform: translateX(-10vw); }
        }

        @keyframes nebula-pulse {
            0%, 100% { opacity: 0.3; }
            50% { opacity: 0.6; }
        }

        @keyframes title-entrance {
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes stellar-entrance {
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes nucleus-pulse {
            0%, 100% { transform: translateY(-50%) scale(1); box-shadow: 0 0 1vh rgba(83, 52, 131, 0.5); }
            50% { transform: translateY(-50%) scale(1.2); box-shadow: 0 0 2vh rgba(83, 52, 131, 0.8); }
        }

        @keyframes ring-rotation {
            from { transform: translateY(-50%) rotate(0deg); }
            to { transform: translateY(-50%) rotate(360deg); }
        }

        @keyframes orbit-entrance {
            to { opacity: 1; transform: translateX(0); }
        }

        @keyframes planet-orbit {
            0% { transform: rotate(0deg) scale(1); }
            50% { transform: rotate(180deg) scale(1.1); }
            100% { transform: rotate(360deg) scale(1); }
        }

        @keyframes hint-fade-in {
            to { opacity: 1; }
        }

        @keyframes glow-pulse {
            0%, 100% { box-shadow: 0 0 0.5vh rgba(83, 52, 131, 0.5); }
            50% { box-shadow: 0 0 1.5vh rgba(83, 52, 131, 1), 0 0 2vh rgba(83, 52, 131, 0.5); }
        }

        /* Responsive Design */
        @media (max-width: 48rem) {
            .stellar-field { gap: clamp(0.1rem, 0.2vh, 0.15rem); }
            .star-info { margin-left: clamp(2rem, 3.5vw, 2.5rem); gap: clamp(0.6rem, 1.2vw, 0.8rem); }
            .orbital-system.expanded { max-height: 35vh; }
            .orbit-track { gap: clamp(0.5rem, 1vw, 0.7rem); }
            .galaxy-content { 
                padding: clamp(0.1rem, 0.3vw, 0.3rem); 
                padding-top: clamp(0.05rem, 0.1vh, 0.1rem);
                gap: clamp(0.05rem, 0.1vh, 0.1rem);
            }
            .stellar-system.active-system { margin-bottom: clamp(1rem, 2vh, 1.5rem); }
            .interaction-hint {
                right: clamp(0.5rem, 2vw, 1rem);
                padding: clamp(0.4rem, 0.8vw, 0.6rem) clamp(0.6rem, 1.2vw, 0.8rem);
            }
        }

        @media (max-width: 30rem) {
            .galaxy-content { 
                padding: clamp(0.1rem, 0.2vw, 0.2rem); 
                padding-top: clamp(0.05rem, 0.1vh, 0.1rem);
                gap: clamp(0.05rem, 0.1vh, 0.1rem);
            }
            .star-info { 
                flex-direction: column; 
                align-items: flex-start; 
                gap: clamp(0.4rem, 0.8vw, 0.6rem);
                margin-left: clamp(1.5rem, 3vw, 2rem);
            }
            .star-rank { align-self: flex-start; }
            .stellar-field { gap: clamp(0.1rem, 0.15vh, 0.15rem); }
            .orbital-system.expanded { max-height: 30vh; }
            .stellar-system.active-system { margin-bottom: clamp(0.8rem, 1.5vh, 1.2rem); }
            .interaction-hint {
                position: fixed;
                top: auto;
                bottom: clamp(1rem, 3vh, 2rem);
                right: clamp(0.5rem, 2vw, 1rem);
                transform: none;
                font-size: clamp(0.6rem, 1vw, 0.7rem);
            }
        }";
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        (function() {
            'use strict';
            
            // State management
            let currentlyExpanded = null;
            
            // Initialize after DOM is ready
            document.addEventListener('DOMContentLoaded', function() {
                initializeArtistExpansion();
            });
            
            function initializeArtistExpansion() {
                const artistStars = document.querySelectorAll('.clickable-star');
                
                artistStars.forEach(function(star) {
                    // Add click event listener
                    star.addEventListener('click', function(e) {
                        e.preventDefault();
                        handleArtistClick(star);
                    });
                    
                    // Add keyboard support
                    star.addEventListener('keydown', function(e) {
                        if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            handleArtistClick(star);
                        }
                    });
                });
                
                // Add escape key handler for closing expanded artists
                document.addEventListener('keydown', function(e) {
                    if (e.key === 'Escape' && currentlyExpanded) {
                        collapseArtist(currentlyExpanded);
                    }
                });
                
                // Add click outside handler
                document.addEventListener('click', function(e) {
                    if (currentlyExpanded && !e.target.closest('.stellar-system')) {
                        collapseArtist(currentlyExpanded);
                    }
                });
            }
            
            function handleArtistClick(star) {
                const artistId = star.getAttribute('data-artist-id');
                const orbitalSystem = document.getElementById(star.getAttribute('aria-controls'));
                const isExpanded = star.getAttribute('aria-expanded') === 'true';
                
                if (isExpanded) {
                    // Collapse current artist
                    collapseArtist(star);
                } else {
                    // Collapse any previously expanded artist
                    if (currentlyExpanded && currentlyExpanded !== star) {
                        collapseArtist(currentlyExpanded);
                    }
                    
                    // Expand current artist
                    expandArtist(star, orbitalSystem);
                }
            }
            
            function expandArtist(star, orbitalSystem) {
                // Update ARIA attributes
                star.setAttribute('aria-expanded', 'true');
                orbitalSystem.setAttribute('aria-hidden', 'false');
                
                // Add CSS classes for animation
                orbitalSystem.classList.add('expanded');
                
                // Update state
                currentlyExpanded = star;
                
                // Animate track entrance with staggered delays
                const tracks = orbitalSystem.querySelectorAll('.orbit-track');
                tracks.forEach(function(track, index) {
                    track.style.animationDelay = (index * 50) + 'ms';
                    track.style.opacity = '0';
                    track.style.transform = 'translateX(-2vw)';
                    
                    // Trigger animation
                    setTimeout(function() {
                        track.style.opacity = '1';
                        track.style.transform = 'translateX(0)';
                        track.style.transition = 'all 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
                    }, index * 50 + 100);
                });
                
                // Add cosmic effects
                addCosmicEffects(star);
                
                // Scroll into view if needed
                setTimeout(function() {
                    const stellarSystem = star.closest('.stellar-system');
                    if (stellarSystem) {
                        stellarSystem.scrollIntoView({ 
                            behavior: 'smooth', 
                            block: 'center' 
                        });
                    }
                }, 300);
            }
            
            function collapseArtist(star) {
                const orbitalSystem = document.getElementById(star.getAttribute('aria-controls'));
                const stellarSystem = star.closest('.stellar-system');
                
                // Update ARIA attributes
                star.setAttribute('aria-expanded', 'false');
                orbitalSystem.setAttribute('aria-hidden', 'true');
                
                // Remove CSS classes for animation
                orbitalSystem.classList.remove('expanded');
                
                // Remove active-system class immediately
                if (stellarSystem) {
                    stellarSystem.classList.remove('active-system');
                }
                
                // Update state
                if (currentlyExpanded === star) {
                    currentlyExpanded = null;
                }
                
                // Remove cosmic effects and reset all styles
                removeCosmicEffects(star);
                
                // Reset any inline styles that might have been applied
                star.style.transform = '';
                if (stellarSystem) {
                    stellarSystem.style.marginBottom = '';
                    stellarSystem.style.opacity = '';
                    stellarSystem.style.transform = '';
                    stellarSystem.style.transition = '';
                }
            }
            
            function addCosmicEffects(star) {
                // Add enhanced glow effect
                const stellarSystem = star.closest('.stellar-system');
                stellarSystem.classList.add('active-system');
                
                // Dim other artists for focus
                const allSystems = document.querySelectorAll('.stellar-system');
                allSystems.forEach(function(system) {
                    if (system !== stellarSystem) {
                        system.style.opacity = '0.6';
                        system.style.transform = 'scale(0.98)';
                        system.style.transition = 'all 0.4s ease';
                    }
                });
            }
            
            function removeCosmicEffects(star) {
                // Note: active-system class should already be removed by collapseArtist
                
                // Restore all artists opacity and clear any inline styles
                const allSystems = document.querySelectorAll('.stellar-system');
                allSystems.forEach(function(system) {
                    system.style.opacity = '';
                    system.style.transform = '';
                    system.style.transition = '';
                    system.style.marginBottom = '';
                });
            }
            
            // Enhanced hover effects for better interactivity
            function initializeHoverEffects() {
                const artistStars = document.querySelectorAll('.clickable-star');
                
                artistStars.forEach(function(star) {
                    star.addEventListener('mouseenter', function() {
                        if (star.getAttribute('aria-expanded') !== 'true') {
                            star.style.transform = 'translateY(-0.5vh) scale(1.02)';
                        }
                    });
                    
                    star.addEventListener('mouseleave', function() {
                        if (star.getAttribute('aria-expanded') !== 'true') {
                            star.style.transform = '';
                        }
                    });
                });
            }
            
            // Initialize hover effects
            document.addEventListener('DOMContentLoaded', function() {
                initializeHoverEffects();
            });
            
            // Public API for external control (if needed)
            window.ArtistGalaxy = {
                expandArtist: function(artistIndex) {
                    const star = document.querySelector(`[data-artist-id='artist-${artistIndex}']`);
                    if (star) handleArtistClick(star);
                },
                collapseAll: function() {
                    if (currentlyExpanded) {
                        collapseArtist(currentlyExpanded);
                    }
                },
                getCurrentlyExpanded: function() {
                    return currentlyExpanded ? currentlyExpanded.getAttribute('data-artist-id') : null;
                }
            };
        })();";
        }
    }
}
