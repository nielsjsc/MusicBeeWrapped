using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Top Artist slide component - Interactive showcase of the user's #1 most played artist
    /// Features hover effects, clickable albums, and detailed track information
    /// </summary>
    public class TopArtistSlide : SlideComponentBase
    {
        public override string SlideId => "top-artist-showcase";
        public override string SlideTitle => "Your Musical Obsession";
        public override int SlideOrder => 6;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            if (!stats.TopArtists.Any()) return WrapInSlideContainer("<p>No artist data available.</p>");

            var topArtist = stats.TopArtists.First();
            var artistName = topArtist.Key;
            var totalPlays = topArtist.Value;

            // Get all plays for this artist
            var artistPlays = playHistory.Plays.Where(p => p.Artist == artistName).ToList();
            
            // Calculate total minutes
            var totalMinutes = artistPlays.Sum(p => p.PlayDuration) / 60.0;
            var totalHours = totalMinutes / 60.0;

            // Get top tracks for this artist
            var topTracks = artistPlays
                .GroupBy(p => p.Title)
                .Select(g => new { Track = g.Key, Plays = g.Count(), Minutes = g.Sum(p => p.PlayDuration) / 60.0 })
                .OrderByDescending(t => t.Plays)
                .Take(5)
                .ToList();

            // Get top albums for this artist
            var topAlbums = artistPlays
                .Where(p => !string.IsNullOrEmpty(p.Album))
                .GroupBy(p => p.Album)
                .Select(g => new { 
                    Album = g.Key, 
                    Plays = g.Count(), 
                    Minutes = g.Sum(p => p.PlayDuration) / 60.0,
                    Tracks = g.GroupBy(t => t.Title).Select(tg => new { 
                        Title = tg.Key, 
                        Plays = tg.Count() 
                    }).OrderByDescending(t => t.Plays).Take(3).ToList()
                })
                .OrderByDescending(a => a.Plays)
                .Take(4)
                .ToList();

            // Generate track list HTML
            var tracksHtml = string.Join("", topTracks.Select((track, index) => $@"
                <div class='favorite-track' data-index='{index}'>
                    <div class='track-number'>#{index + 1}</div>
                    <div class='track-info'>
                        <div class='track-name'>{EscapeHtml(track.Track)}</div>
                        <div class='track-stats'>{track.Plays} plays • {track.Minutes:F1} min</div>
                    </div>
                </div>"));

            // Generate album list HTML
            var albumsHtml = string.Join("", topAlbums.Select((album, index) => {
                var albumTracksHtml = string.Join("", album.Tracks.Select(t => $@"
                    <div class='album-track'>
                        <span class='album-track-name'>{EscapeHtml(t.Title)}</span>
                        <span class='album-track-plays'>{t.Plays} plays</span>
                    </div>"));

                return $@"
                    <div class='favorite-album' data-index='{index}' data-album='{EscapeHtml(album.Album)}'>
                        <div class='album-cover'>
                            <div class='album-vinyl'></div>
                            <div class='album-hover-overlay'>
                                <div class='album-play-icon'>▶</div>
                            </div>
                        </div>
                        <div class='album-info'>
                            <div class='album-name'>{EscapeHtml(album.Album)}</div>
                            <div class='album-stats'>{album.Plays} plays • {album.Minutes:F1} min</div>
                        </div>
                        <div class='album-tracks-popup'>
                            <div class='album-tracks-header'>
                                <h4>{EscapeHtml(album.Album)}</h4>
                                <button class='close-popup'>×</button>
                            </div>
                            <div class='album-tracks-list'>
                                {albumTracksHtml}
                            </div>
                        </div>
                    </div>";
            }));

            var content = $@"
                <div class='top-artist-showcase'>
                    <!-- Animated background -->
                    <div class='artist-background'>
                        <div class='sound-waves'></div>
                        <div class='floating-notes'></div>
                    </div>
                    
                    <!-- Main content -->
                    <div class='artist-hero'>
                        <div class='artist-spotlight'>
                            <div class='artist-avatar'>
                                <div class='avatar-circle'>
                                    <div class='music-icon'>♪</div>
                                    <div class='pulse-ring'></div>
                                </div>
                            </div>
                            
                            <div class='artist-details'>
                                <h1 class='artist-name clickable-name'>{EscapeHtml(artistName)}</h1>
                                <div class='artist-subtitle'>Your #{year} Musical Obsession</div>
                                <div class='click-hint'>Click the name to explore more</div>
                                <div class='artist-stats'>
                                    <div class='stat-item'>
                                        <div class='stat-number'>{totalPlays}</div>
                                        <div class='stat-label'>Total Plays</div>
                                    </div>
                                    <div class='stat-item'>
                                        <div class='stat-number'>{totalHours:F1}h</div>
                                        <div class='stat-label'>Hours Listened</div>
                                    </div>
                                    <div class='stat-item'>
                                        <div class='stat-number'>{topTracks.Count}</div>
                                        <div class='stat-label'>Top Tracks</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Interactive sections - Hidden by default -->
                        <div class='artist-content hidden'>
                            <!-- Favorite tracks section -->
                            <div class='content-section tracks-section'>
                                <div class='section-header'>
                                    <h3>Your Favorite Tracks</h3>
                                    <p>The songs you couldn't stop playing</p>
                                </div>
                                <div class='favorite-tracks-list'>
                                    {tracksHtml}
                                </div>
                            </div>
                            
                            <!-- Favorite albums section -->
                            <div class='content-section albums-section'>
                                <div class='section-header'>
                                    <h3>Your Favorite Albums</h3>
                                    <p>Click an album to see your top tracks from it</p>
                                </div>
                                <div class='favorite-albums-grid'>
                                    {albumsHtml}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GenerateCSS()
        {
            return @"
                .top-artist-showcase {
                    position: relative;
                    width: 100%;
                    height: 100vh;
                    background: linear-gradient(135deg, #0f0f23 0%, #2d1b69 50%, #11203b 100%);
                    overflow: hidden;
                    color: #ffffff;
                    font-family: 'Segoe UI', sans-serif;
                }

                .artist-background {
                    position: absolute;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    z-index: 1;
                }

                .sound-waves {
                    position: absolute;
                    bottom: 0;
                    left: 0;
                    width: 100%;
                    height: 30%;
                    background: linear-gradient(transparent, rgba(138, 43, 226, 0.2));
                    animation: waveAnimation 4s ease-in-out infinite;
                }

                .floating-notes::before,
                .floating-notes::after {
                    content: '♪ ♫ ♪ ♫ ♪';
                    position: absolute;
                    font-size: 1.5rem;
                    color: rgba(255, 255, 255, 0.1);
                    animation: floatNotes 15s linear infinite;
                }

                .floating-notes::before {
                    top: 20%;
                    left: -100px;
                }

                .floating-notes::after {
                    top: 60%;
                    left: -100px;
                    animation-delay: -7s;
                }

                .artist-hero {
                    position: relative;
                    z-index: 2;
                    height: 100vh;
                    display: flex;
                    flex-direction: column;
                    padding: 2rem;
                    overflow-y: auto;
                }

                .artist-spotlight {
                    display: flex;
                    align-items: center;
                    margin-bottom: 3rem;
                    animation: slideInFromLeft 1s ease-out;
                }

                .artist-avatar {
                    margin-right: 3rem;
                    position: relative;
                }

                .avatar-circle {
                    position: relative;
                    width: 120px;
                    height: 120px;
                    border-radius: 50%;
                    background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    cursor: pointer;
                    transition: all 0.3s ease;
                }

                .avatar-circle:hover {
                    transform: scale(1.1);
                    box-shadow: 0 0 30px rgba(255, 107, 107, 0.5);
                }

                .music-icon {
                    font-size: 3rem;
                    color: white;
                    text-shadow: 2px 2px 4px rgba(0,0,0,0.5);
                }

                .pulse-ring {
                    position: absolute;
                    top: -10px;
                    left: -10px;
                    right: -10px;
                    bottom: -10px;
                    border: 2px solid rgba(255, 255, 255, 0.3);
                    border-radius: 50%;
                    animation: pulse 2s infinite;
                }

                .artist-details {
                    flex: 1;
                }

                .artist-name {
                    font-size: 3.5rem;
                    font-weight: 300;
                    margin-bottom: 0.5rem;
                    background: linear-gradient(45deg, #ffffff, #ff6b6b, #4ecdc4);
                    background-size: 200% 200%;
                    -webkit-background-clip: text;
                    -webkit-text-fill-color: transparent;
                    background-clip: text;
                    animation: shimmer 3s ease-in-out infinite;
                    cursor: pointer;
                    transition: all 0.3s ease;
                }

                .artist-name:hover {
                    transform: scale(1.05);
                    filter: drop-shadow(0 0 20px rgba(255, 107, 107, 0.5));
                }

                .click-hint {
                    font-size: 0.9rem;
                    color: rgba(255, 255, 255, 0.5);
                    margin-bottom: 2rem;
                    font-style: italic;
                    opacity: 1;
                    transition: opacity 0.3s ease;
                }

                .click-hint.hidden {
                    opacity: 0;
                }

                .artist-subtitle {
                    font-size: 1.2rem;
                    color: rgba(255, 255, 255, 0.7);
                    margin-bottom: 1rem;
                    font-weight: 300;
                }

                .artist-stats {
                    display: flex;
                    gap: 2rem;
                }

                .stat-item {
                    text-align: center;
                    padding: 1rem;
                    background: rgba(255, 255, 255, 0.1);
                    border-radius: 15px;
                    backdrop-filter: blur(10px);
                    border: 1px solid rgba(255, 255, 255, 0.2);
                    transition: all 0.3s ease;
                }

                .stat-item:hover {
                    background: rgba(255, 255, 255, 0.15);
                    transform: translateY(-5px);
                }

                .stat-number {
                    font-size: 2rem;
                    font-weight: bold;
                    color: #4ecdc4;
                    margin-bottom: 0.5rem;
                }

                .stat-label {
                    font-size: 0.9rem;
                    color: rgba(255, 255, 255, 0.8);
                    text-transform: uppercase;
                    letter-spacing: 1px;
                }

                .artist-content {
                    display: flex;
                    gap: 3rem;
                    flex: 1;
                    margin-top: 2rem;
                    opacity: 1;
                    transform: translateY(0);
                    transition: all 0.5s ease;
                }

                .artist-content.hidden {
                    opacity: 0;
                    transform: translateY(30px);
                    pointer-events: none;
                    height: 0;
                    overflow: hidden;
                }

                .content-section {
                    flex: 1;
                    animation: fadeInUp 1s ease-out;
                }

                .tracks-section {
                    animation-delay: 0.3s;
                }

                .albums-section {
                    animation-delay: 0.6s;
                }

                .section-header {
                    margin-bottom: 2rem;
                }

                .section-header h3 {
                    font-size: 1.5rem;
                    margin-bottom: 0.5rem;
                    color: #ff6b6b;
                }

                .section-header p {
                    color: rgba(255, 255, 255, 0.7);
                    font-size: 0.9rem;
                }

                .favorite-tracks-list {
                    display: flex;
                    flex-direction: column;
                    gap: 1rem;
                }

                .favorite-track {
                    display: flex;
                    align-items: center;
                    padding: 1rem;
                    background: rgba(255, 255, 255, 0.05);
                    border-radius: 12px;
                    border: 1px solid rgba(255, 255, 255, 0.1);
                    transition: all 0.3s ease;
                    opacity: 0;
                    transform: translateX(-20px);
                    animation: slideInTrack 0.6s ease-out forwards;
                }

                .favorite-track:nth-child(1) { animation-delay: 0.1s; }
                .favorite-track:nth-child(2) { animation-delay: 0.2s; }
                .favorite-track:nth-child(3) { animation-delay: 0.3s; }
                .favorite-track:nth-child(4) { animation-delay: 0.4s; }
                .favorite-track:nth-child(5) { animation-delay: 0.5s; }

                .favorite-track:hover {
                    background: rgba(255, 255, 255, 0.1);
                    transform: translateX(10px);
                    border-color: #4ecdc4;
                    box-shadow: 0 5px 20px rgba(78, 205, 196, 0.2);
                }

                .track-number {
                    font-size: 1.2rem;
                    font-weight: bold;
                    color: #4ecdc4;
                    margin-right: 1rem;
                    min-width: 30px;
                }

                .track-info {
                    flex: 1;
                }

                .track-name {
                    font-size: 1rem;
                    font-weight: 600;
                    margin-bottom: 0.3rem;
                }

                .track-stats {
                    font-size: 0.8rem;
                    color: rgba(255, 255, 255, 0.6);
                }

                .favorite-albums-grid {
                    display: grid;
                    grid-template-columns: repeat(2, 1fr);
                    gap: 1.5rem;
                }

                .favorite-album {
                    position: relative;
                    padding: 1.5rem;
                    background: rgba(255, 255, 255, 0.05);
                    border-radius: 15px;
                    border: 1px solid rgba(255, 255, 255, 0.1);
                    cursor: pointer;
                    transition: all 0.3s ease;
                    opacity: 0;
                    transform: scale(0.9);
                    animation: scaleIn 0.6s ease-out forwards;
                }

                .favorite-album:nth-child(1) { animation-delay: 0.1s; }
                .favorite-album:nth-child(2) { animation-delay: 0.2s; }
                .favorite-album:nth-child(3) { animation-delay: 0.3s; }
                .favorite-album:nth-child(4) { animation-delay: 0.4s; }

                .favorite-album:hover {
                    background: rgba(255, 255, 255, 0.1);
                    transform: translateY(-5px) scale(1.02);
                    border-color: #ff6b6b;
                    box-shadow: 0 10px 30px rgba(255, 107, 107, 0.2);
                }

                .album-cover {
                    position: relative;
                    width: 80px;
                    height: 80px;
                    margin: 0 auto 1rem;
                    background: linear-gradient(45deg, #ff6b6b, #4ecdc4);
                    border-radius: 12px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    overflow: hidden;
                }

                .album-vinyl {
                    width: 60px;
                    height: 60px;
                    background: linear-gradient(45deg, #2c2c2c, #1a1a1a);
                    border-radius: 50%;
                    position: relative;
                    box-shadow: inset 0 0 20px rgba(0,0,0,0.5);
                }

                .album-vinyl::before {
                    content: '';
                    position: absolute;
                    top: 50%;
                    left: 50%;
                    width: 8px;
                    height: 8px;
                    background: #ff6b6b;
                    border-radius: 50%;
                    transform: translate(-50%, -50%);
                }

                .album-vinyl::after {
                    content: '';
                    position: absolute;
                    top: 50%;
                    left: 50%;
                    width: 30px;
                    height: 30px;
                    border: 1px solid rgba(255,255,255,0.1);
                    border-radius: 50%;
                    transform: translate(-50%, -50%);
                }

                .album-hover-overlay {
                    position: absolute;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(0, 0, 0, 0.7);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    opacity: 0;
                    transition: opacity 0.3s ease;
                }

                .favorite-album:hover .album-hover-overlay {
                    opacity: 1;
                }

                .album-play-icon {
                    color: white;
                    font-size: 1.5rem;
                }

                .album-info {
                    text-align: center;
                }

                .album-name {
                    font-size: 1rem;
                    font-weight: 600;
                    margin-bottom: 0.5rem;
                }

                .album-stats {
                    font-size: 0.8rem;
                    color: rgba(255, 255, 255, 0.6);
                }

                .album-tracks-popup {
                    position: fixed;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%) scale(0);
                    width: 400px;
                    max-height: 500px;
                    background: #1a1a2e;
                    border-radius: 20px;
                    border: 1px solid rgba(255, 255, 255, 0.2);
                    z-index: 1000;
                    opacity: 0;
                    transition: all 0.3s ease;
                    box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
                }

                .album-tracks-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    padding: 1.5rem;
                    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
                }

                .album-tracks-header h4 {
                    margin: 0;
                    color: #ff6b6b;
                }

                .close-popup {
                    background: none;
                    border: none;
                    color: rgba(255, 255, 255, 0.7);
                    font-size: 1.5rem;
                    cursor: pointer;
                    padding: 0;
                    width: 30px;
                    height: 30px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    border-radius: 50%;
                    transition: all 0.3s ease;
                }

                .close-popup:hover {
                    background: rgba(255, 255, 255, 0.1);
                    color: white;
                }

                .album-tracks-list {
                    padding: 1rem;
                    max-height: 300px;
                    overflow-y: auto;
                }

                .album-track {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    padding: 0.8rem;
                    border-radius: 8px;
                    transition: background 0.3s ease;
                }

                .album-track:hover {
                    background: rgba(255, 255, 255, 0.05);
                }

                .album-track-name {
                    font-weight: 500;
                }

                .album-track-plays {
                    color: #4ecdc4;
                    font-size: 0.9rem;
                }

                /* Animations */
                @keyframes slideInFromLeft {
                    from {
                        opacity: 0;
                        transform: translateX(-50px);
                    }
                    to {
                        opacity: 1;
                        transform: translateX(0);
                    }
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

                @keyframes slideInTrack {
                    to {
                        opacity: 1;
                        transform: translateX(0);
                    }
                }

                @keyframes scaleIn {
                    to {
                        opacity: 1;
                        transform: scale(1);
                    }
                }

                @keyframes pulse {
                    0% {
                        transform: scale(1);
                        opacity: 1;
                    }
                    50% {
                        transform: scale(1.1);
                        opacity: 0.7;
                    }
                    100% {
                        transform: scale(1);
                        opacity: 1;
                    }
                }

                @keyframes shimmer {
                    0% { background-position: 0% 50%; }
                    50% { background-position: 100% 50%; }
                    100% { background-position: 0% 50%; }
                }

                @keyframes waveAnimation {
                    0%, 100% { transform: scaleY(1); }
                    50% { transform: scaleY(1.2); }
                }

                @keyframes floatNotes {
                    0% { transform: translateX(-100px) translateY(0px); }
                    50% { transform: translateX(50vw) translateY(-20px); }
                    100% { transform: translateX(calc(100vw + 100px)) translateY(0px); }
                }

                /* Responsive design */
                @media (max-width: 768px) {
                    .artist-hero {
                        padding: 1rem;
                    }
                    
                    .artist-spotlight {
                        flex-direction: column;
                        text-align: center;
                    }
                    
                    .artist-avatar {
                        margin-right: 0;
                        margin-bottom: 2rem;
                    }
                    
                    .artist-name {
                        font-size: 2.5rem;
                    }
                    
                    .artist-content {
                        flex-direction: column;
                        gap: 2rem;
                    }
                    
                    .favorite-albums-grid {
                        grid-template-columns: 1fr;
                    }
                    
                    .album-tracks-popup {
                        width: 90vw;
                        max-width: 350px;
                    }
                }
            ";
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
                // Top Artist Interactive Functionality
                document.addEventListener('DOMContentLoaded', function() {
                    const artistName = document.querySelector('.artist-name');
                    const artistContent = document.querySelector('.artist-content');
                    const clickHint = document.querySelector('.click-hint');
                    const albums = document.querySelectorAll('.favorite-album');
                    
                    let contentVisible = false;
                    
                    // Artist name click to show/hide content
                    if (artistName && artistContent) {
                        artistName.addEventListener('click', function() {
                            contentVisible = !contentVisible;
                            
                            if (contentVisible) {
                                artistContent.classList.remove('hidden');
                                clickHint.classList.add('hidden');
                            } else {
                                artistContent.classList.add('hidden');
                                clickHint.classList.remove('hidden');
                            }
                        });
                    }
                    
                    // Album click functionality (no modal overlay)
                    albums.forEach(album => {
                        album.addEventListener('click', function() {
                            const popup = this.querySelector('.album-tracks-popup');
                            const albumName = this.dataset.album;
                            
                            // Close any other open popups
                            document.querySelectorAll('.album-tracks-popup.active').forEach(p => {
                                if (p !== popup) {
                                    p.classList.remove('active');
                                }
                            });
                            
                            // Toggle this popup
                            popup.classList.toggle('active');
                            
                            // Add close functionality
                            const closeBtn = popup.querySelector('.close-popup');
                            closeBtn.addEventListener('click', function(e) {
                                e.stopPropagation();
                                popup.classList.remove('active');
                            });
                        });
                    });
                    
                    // Close popup when clicking outside
                    document.addEventListener('click', function(e) {
                        if (!e.target.closest('.favorite-album')) {
                            document.querySelectorAll('.album-tracks-popup.active').forEach(popup => {
                                popup.classList.remove('active');
                            });
                        }
                    });
                    
                    // Avatar click animation
                    const avatarCircle = document.querySelector('.avatar-circle');
                    if (avatarCircle) {
                        avatarCircle.addEventListener('click', function() {
                            this.style.animation = 'none';
                            setTimeout(() => {
                                this.style.animation = '';
                                this.style.animation = 'pulse 0.6s ease-out';
                            }, 10);
                        });
                    }
                    
                    // Add sparkle effect to stats on hover
                    const statItems = document.querySelectorAll('.stat-item');
                    statItems.forEach(item => {
                        item.addEventListener('mouseenter', function() {
                            if (!this.querySelector('.sparkle')) {
                                const sparkle = document.createElement('div');
                                sparkle.className = 'sparkle';
                                sparkle.innerHTML = '*';
                                sparkle.style.position = 'absolute';
                                sparkle.style.top = '5px';
                                sparkle.style.right = '5px';
                                sparkle.style.fontSize = '1.2rem';
                                sparkle.style.color = '#4ecdc4';
                                sparkle.style.fontWeight = 'bold';
                                sparkle.style.animation = 'sparkleAnim 1s ease-out';
                                this.style.position = 'relative';
                                this.appendChild(sparkle);
                                
                                setTimeout(() => sparkle.remove(), 1000);
                            }
                        });
                    });
                });
                
                // Add sparkle animation
                const style = document.createElement('style');
                style.textContent = `
                    @keyframes sparkleAnim {
                        0% { opacity: 0; transform: scale(0) rotate(0deg); }
                        50% { opacity: 1; transform: scale(1) rotate(180deg); }
                        100% { opacity: 0; transform: scale(0) rotate(360deg); }
                    }
                `;
                document.head.appendChild(style);
            ";
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            if (!stats.TopArtists.Any()) return "No artist data available.";

            var topArtist = stats.TopArtists.First();
            var artistPlays = playHistory.Plays.Where(p => p.Artist == topArtist.Key).ToList();
            var totalMinutes = artistPlays.Sum(p => p.PlayDuration) / 60.0;
            var totalHours = totalMinutes / 60.0;
            
            return $"You spent {totalHours:F1} hours listening to {EscapeHtml(topArtist.Key)} - that's {(totalHours / 24):F1} full days of pure musical bliss!";
        }

        public override string GetInsightTitle(WrappedStatistics stats)
        {
            return "Your Musical Soulmate";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) && 
                   stats.TopArtists != null && 
                   stats.TopArtists.Any();
        }
    }
}
