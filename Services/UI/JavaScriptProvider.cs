using System;
using System.Text;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Provides JavaScript functionality for the MusicBee Wrapped web interface
    /// Handles slide navigation, animations, chart rendering, and user interactions
    /// </summary>
    public class JavaScriptProvider
    {
        /// <summary>
        /// Gets the main JavaScript for slide navigation and core functionality
        /// Includes slide management, keyboard navigation, and animation controls
        /// </summary>
        /// <returns>Complete JavaScript code as string</returns>
        public string GetMainInterfaceJS()
        {
            return @"
        // Global state management
        let currentSlide = 0;
        let slides = [];
        let totalSlides = 0;
        let isAnimating = false;

        // Initialize the application when DOM is ready
        document.addEventListener('DOMContentLoaded', function() {
            initializeApp();
        });

        function initializeApp() {
            // Hide loading screen after a brief delay
            setTimeout(() => {
                document.getElementById('loading').style.display = 'none';
                generateSlides();
                initializeNavigation();
                showSlide(0);
            }, 2000);
        }

        function generateSlides() {
            const app = document.getElementById('app');
            const data = window.WRAPPED_DATA;
            
            // Clear existing content except loading
            const loading = document.getElementById('loading');
            app.innerHTML = '';
            app.appendChild(loading);

            // Generate slide content
            slides = [
                createWelcomeSlide(data),
                createOverviewSlide(data),
                createTopArtistsSlide(data),
                createTopTracksSlide(data),
                createTopAlbumsSlide(data),
                createListeningPatternsSlide(data),
                createDiscoverySlide(data),
                createFinaleSlide(data)
            ];

            // Add slides to DOM
            slides.forEach((slide, index) => {
                slide.classList.add('slide');
                slide.style.display = 'none';
                app.appendChild(slide);
            });

            totalSlides = slides.length;
            updateSlideIndicator();
        }

        function createWelcomeSlide(data) {
            const slide = document.createElement('div');
            slide.innerHTML = `
                <h1 class='gradient-text fade-in'>Your ${data.year} Music Wrapped</h1>
                <h3 class='slide-up' style='animation-delay: 0.5s;'>Ready to dive into your musical journey?</h3>
                <div class='insight-box slide-up' style='animation-delay: 1s;'>
                    <p class='insight-text'>This year you listened to <strong>${data.totalTracks} tracks</strong> 
                    across <strong>${data.totalHours} hours</strong> of music. Let's explore what made your year special.</p>
                </div>
            `;
            return slide;
        }

        function createOverviewSlide(data) {
            const slide = document.createElement('div');
            slide.innerHTML = `
                <h2 class='gradient-text'>Your Year in Numbers</h2>
                <div class='stats-grid'>
                    <div class='stat-card'>
                        <div class='stat-number'>${data.totalTracks}</div>
                        <div class='stat-label'>Tracks Played</div>
                    </div>
                    <div class='stat-card'>
                        <div class='stat-number'>${data.totalHours}</div>
                        <div class='stat-label'>Hours Listened</div>
                    </div>
                    <div class='stat-card'>
                        <div class='stat-number'>${data.totalArtists}</div>
                        <div class='stat-label'>Unique Artists</div>
                    </div>
                    <div class='stat-card'>
                        <div class='stat-number'>${data.totalAlbums}</div>
                        <div class='stat-label'>Unique Albums</div>
                    </div>
                </div>
                <div class='insight-box'>
                    <h3>Quick Stats</h3>
                    <p class='insight-text'>Your most active hour was <strong>${data.mostActiveHour}:00</strong>, 
                    and you had a listening streak of <strong>${data.longestStreak} days</strong>!</p>
                </div>
            `;
            return slide;
        }

        function createTopArtistsSlide(data) {
            const slide = document.createElement('div');
            const artistsList = data.topArtists.map((artist, index) => 
                `<li><span class='rank'>#${index + 1}</span><span class='name'>${artist.name}</span><span class='count'>${artist.count} plays</span></li>`
            ).join('');

            slide.innerHTML = `
                <h2 class='gradient-text'>Your Top Artists</h2>
                <ul class='top-list'>
                    ${artistsList}
                </ul>
                <div class='insight-box'>
                    <h3>Artist Spotlight</h3>
                    <p class='insight-text'>You played <strong>${data.topArtists[0].name}</strong> 
                    ${data.topArtists[0].count} times this year. That's dedication!</p>
                </div>
            `;
            return slide;
        }

        function createTopTracksSlide(data) {
            const slide = document.createElement('div');
            const tracksList = data.topTracks.map((track, index) => 
                `<li><span class='rank'>#${index + 1}</span><span class='name'>${track.title} <small>by ${track.artist}</small></span><span class='count'>${track.count} plays</span></li>`
            ).join('');

            slide.innerHTML = `
                <h2 class='gradient-text'>Your Top Tracks</h2>
                <ul class='top-list'>
                    ${tracksList}
                </ul>
                <div class='insight-box'>
                    <h3>Track on Repeat</h3>
                    <p class='insight-text'><strong>'${data.topTracks[0].title}'</strong> by ${data.topTracks[0].artist} 
                    was your most played track with ${data.topTracks[0].count} plays!</p>
                </div>
            `;
            return slide;
        }

        function createTopAlbumsSlide(data) {
            const slide = document.createElement('div');
            const albumsList = data.topAlbums.map((album, index) => {
                const topTracks = album.topTracks.map(track => track.title).join(', ');
                return `<li><span class='rank'>#${index + 1}</span><span class='name'>${album.album} <small>by ${album.artist}</small><br><small style='opacity: 0.6;'>Top tracks: ${topTracks}</small></span><span class='count'>${album.count} plays</span></li>`;
            }).join('');

            slide.innerHTML = `
                <h2 class='gradient-text'>Your Top Albums</h2>
                <ul class='top-list'>
                    ${albumsList}
                </ul>
                <div class='insight-box'>
                    <h3>Album Deep Dive</h3>
                    <p class='insight-text'>You really connected with <strong>'${data.topAlbums[0].album}'</strong> 
                    by ${data.topAlbums[0].artist}, playing it ${data.topAlbums[0].count} times!</p>
                </div>
            `;
            return slide;
        }

        function createListeningPatternsSlide(data) {
            const slide = document.createElement('div');
            slide.innerHTML = `
                <h2 class='gradient-text'>Your Listening Patterns</h2>
                <div class='chart-container'>
                    <canvas id='listening-chart' width='800' height='400'></canvas>
                </div>
                <div class='insight-box'>
                    <h3>Pattern Analysis</h3>
                    <p class='insight-text'>Your peak listening time is around <strong>${data.mostActiveHour}:00</strong>. 
                    Weekend vs weekday ratio: <strong>${data.weekendRatio}</strong></p>
                </div>
            `;
            
            // Add chart rendering after slide is added to DOM
            setTimeout(() => renderListeningChart(data), 100);
            return slide;
        }

        function createDiscoverySlide(data) {
            const slide = document.createElement('div');
            const firstPlay = data.firstPlay;
            const lastPlay = data.lastPlay;
            
            slide.innerHTML = `
                <h2 class='gradient-text'>Your Musical Journey</h2>
                <div class='stats-grid'>
                    <div class='stat-card'>
                        <div class='stat-number'>${data.longestStreak}</div>
                        <div class='stat-label'>Longest Listening Streak</div>
                    </div>
                    <div class='stat-card'>
                        <div class='stat-number'>${firstPlay ? firstPlay.month : 'N/A'}</div>
                        <div class='stat-label'>First Song Month</div>
                    </div>
                </div>
                <div class='insight-box'>
                    <h3>Year Bookends</h3>
                    <p class='insight-text'>
                        ${firstPlay ? `Your year started with <strong>'${firstPlay.title}'</strong> by ${firstPlay.artist} on ${firstPlay.month} ${firstPlay.day}.` : 'No first play data available.'}
                        <br><br>
                        ${lastPlay ? `And it's ending with <strong>'${lastPlay.title}'</strong> by ${lastPlay.artist}.` : 'No last play data available.'}
                    </p>
                </div>
            `;
            return slide;
        }

        function createFinaleSlide(data) {
            const slide = document.createElement('div');
            slide.innerHTML = `
                <h1 class='gradient-text'>That's Your ${data.year} Wrapped!</h1>
                <div class='stats-grid'>
                    <div class='stat-card'>
                        <div class='stat-number'>${data.totalHours}</div>
                        <div class='stat-label'>Total Hours</div>
                    </div>
                    <div class='stat-card'>
                        <div class='stat-number'>${data.totalTracks}</div>
                        <div class='stat-label'>Total Tracks</div>
                    </div>
                </div>
                <div class='insight-box'>
                    <h3>Thank You for the Music</h3>
                    <p class='insight-text'>Another year of incredible music discoveries and memorable moments. 
                    Here's to an even better ${data.year + 1}!</p>
                </div>
                <div style='margin-top: 2rem; opacity: 0.7;'>
                    <p>Generated by MusicBee Wrapped</p>
                </div>
            `;
            return slide;
        }

        function renderListeningChart(data) {
            const canvas = document.getElementById('listening-chart');
            if (!canvas) return;
            
            const ctx = canvas.getContext('2d');
            const width = canvas.width;
            const height = canvas.height;
            
            // Clear canvas
            ctx.clearRect(0, 0, width, height);
            
            // Simple bar chart for monthly data
            const monthlyData = data.monthlyHours;
            const months = Object.keys(monthlyData);
            const values = Object.values(monthlyData);
            const maxValue = Math.max(...values);
            
            const barWidth = width / months.length * 0.8;
            const barSpacing = width / months.length * 0.2;
            
            ctx.fillStyle = '#4ecdc4';
            ctx.strokeStyle = '#96ceb4';
            ctx.lineWidth = 2;
            
            months.forEach((month, index) => {
                const value = values[index];
                const barHeight = (value / maxValue) * (height - 60);
                const x = index * (barWidth + barSpacing) + barSpacing / 2;
                const y = height - barHeight - 30;
                
                // Draw bar
                ctx.fillRect(x, y, barWidth, barHeight);
                ctx.strokeRect(x, y, barWidth, barHeight);
                
                // Draw value label
                ctx.fillStyle = '#ffffff';
                ctx.font = '12px Segoe UI';
                ctx.textAlign = 'center';
                ctx.fillText(value.toFixed(1) + 'h', x + barWidth / 2, y - 5);
                
                // Draw month label
                ctx.fillText(month.substring(5), x + barWidth / 2, height - 5);
                
                ctx.fillStyle = '#4ecdc4';
            });
        }

        function initializeNavigation() {
            const prevBtn = document.getElementById('prev-btn');
            const nextBtn = document.getElementById('next-btn');

            prevBtn.addEventListener('click', () => previousSlide());
            nextBtn.addEventListener('click', () => nextSlide());

            // Keyboard navigation
            document.addEventListener('keydown', (e) => {
                if (e.key === 'ArrowLeft') previousSlide();
                if (e.key === 'ArrowRight') nextSlide();
            });

            // Touch/swipe support
            let startX = 0;
            let endX = 0;

            document.addEventListener('touchstart', (e) => {
                startX = e.touches[0].clientX;
            });

            document.addEventListener('touchend', (e) => {
                endX = e.changedTouches[0].clientX;
                const diff = startX - endX;
                
                if (Math.abs(diff) > 50) { // Minimum swipe distance
                    if (diff > 0) nextSlide();
                    else previousSlide();
                }
            });
        }

        // Enhanced keyboard navigation with smooth transitions
        function initializeKeyboardNavigation() {
            let keyPressTimeout;
            
            document.addEventListener('keydown', function(e) {
                // Debounce rapid key presses to prevent transition conflicts
                clearTimeout(keyPressTimeout);
                keyPressTimeout = setTimeout(() => {
                    handleKeyNavigation(e);
                }, 100);
            });
        }

        function handleKeyNavigation(e) {
            if (isAnimating) return; // Prevent navigation during transitions
            
            switch(e.key) {
                case 'ArrowRight':
                case ' ': // Spacebar
                    e.preventDefault();
                    nextSlide();
                    break;
                case 'ArrowLeft':
                    e.preventDefault();
                    previousSlide();
                    break;
                case 'Home':
                    e.preventDefault();
                    showSlide(0);
                    break;
                case 'End':
                    e.preventDefault();
                    showSlide(totalSlides - 1);
                    break;
                case 'Escape':
                    // Could add fullscreen toggle or help overlay
                    break;
            }
        }

        // Check for reduced motion preference
        function shouldUseReducedMotion() {
            return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        }

        // Enhanced transition function that respects accessibility
        function performAccessibleTransition(previousSlide, nextSlide, isForward, onComplete) {
            if (shouldUseReducedMotion()) {
                // Use simple fade for users who prefer reduced motion
                performSimpleTransition(previousSlide, nextSlide, onComplete);
            } else {
                // Use full cinematic transition for others
                performCinematicTransition(previousSlide, nextSlide, isForward, onComplete);
            }
        }

        function performSimpleTransition(previousSlide, nextSlide, onComplete) {
            if (previousSlide) {
                previousSlide.style.transition = 'opacity 0.3s ease';
                previousSlide.style.opacity = '0';
                
                setTimeout(() => {
                    previousSlide.style.display = 'none';
                    previousSlide.style.opacity = '';
                    previousSlide.style.transition = '';
                }, 300);
            }
            
            nextSlide.style.display = 'flex';
            nextSlide.style.opacity = '0';
            nextSlide.style.transition = 'opacity 0.3s ease';
            
            requestAnimationFrame(() => {
                nextSlide.style.opacity = '1';
                nextSlide.classList.add('active');
            });
            
            setTimeout(() => {
                nextSlide.style.transition = '';
                onComplete();
            }, 300);
        }

        function showSlide(index) {
            if (isAnimating || index < 0 || index >= totalSlides) return;
            
            isAnimating = true;
            
            // Handle initial load case (no transition needed)
            if (currentSlide === index) {
                const slideElement = slides[index];
                if (slideElement) {
                    slideElement.style.display = 'flex';
                    slideElement.classList.add('active');
                }
                
                updateNavigationButtons();
                updateSlideIndicator();
                isAnimating = false;
                
                // Emit slideChanged event
                const slideChangedEvent = new CustomEvent('slideChanged', { 
                    detail: { 
                        slideIndex: index, 
                        slideElement: slideElement 
                    } 
                });
                document.dispatchEvent(slideChangedEvent);
                return;
            }
            
            const previousSlide = slides[currentSlide];
            const nextSlide = slides[index];
            
            // Simple but smooth transition
            performSmoothTransition(previousSlide, nextSlide, () => {
                currentSlide = index;
                updateNavigationButtons();
                updateSlideIndicator();
                isAnimating = false;
                
                // Special handling for daily chart slide
                const slideElement = slides[index];
                if (slideElement && slideElement.querySelector('#dailyChart')) {
                    setTimeout(() => {
                        if (typeof drawDailyChart === 'function') {
                            drawDailyChart();
                        }
                    }, 500);
                }
                
                // Emit slideChanged event for other slides to listen to
                const slideChangedEvent = new CustomEvent('slideChanged', { 
                    detail: { 
                        slideIndex: index, 
                        slideElement: slideElement 
                    } 
                });
                document.dispatchEvent(slideChangedEvent);
            });
        }

        function performSmoothTransition(previousSlide, nextSlide, onComplete) {
            if (!previousSlide || !nextSlide) {
                // Fallback for initial load
                if (nextSlide) {
                    nextSlide.style.display = 'flex';
                    nextSlide.classList.add('active');
                }
                onComplete();
                return;
            }

            // Simple but elegant fade transition
            nextSlide.style.display = 'flex';
            nextSlide.style.opacity = '0';
            nextSlide.style.transform = 'scale(0.95)';
            nextSlide.style.transition = 'all 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
            
            previousSlide.style.transition = 'all 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
            previousSlide.style.opacity = '0';
            previousSlide.style.transform = 'scale(1.05)';
            
            // Animate in the new slide
            requestAnimationFrame(() => {
                nextSlide.style.opacity = '1';
                nextSlide.style.transform = 'scale(1)';
                nextSlide.classList.add('active');
            });
            
            // Clean up after transition
            setTimeout(() => {
                if (previousSlide) {
                    previousSlide.style.display = 'none';
                    previousSlide.classList.remove('active');
                    previousSlide.style.opacity = '';
                    previousSlide.style.transform = '';
                    previousSlide.style.transition = '';
                }
                
                if (nextSlide) {
                    nextSlide.style.opacity = '';
                    nextSlide.style.transform = '';
                    nextSlide.style.transition = '';
                }
                
                onComplete();
            }, 500);
        }

        function performCinematicTransition(previousSlide, nextSlide, isForward, onComplete) {
            if (!previousSlide || !nextSlide) {
                // Fallback for initial load
                nextSlide.style.display = 'flex';
                setTimeout(() => {
                    nextSlide.classList.add('active');
                    onComplete();
                }, 50);
                return;
            }

            // Prepare slides for transition
            nextSlide.style.display = 'flex';
            nextSlide.classList.remove('active');
            previousSlide.classList.remove('active');
            
            // Create transition overlay for seamless effect
            const transitionOverlay = document.createElement('div');
            transitionOverlay.className = 'transition-overlay';
            transitionOverlay.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: linear-gradient(135deg, 
                    rgba(10, 10, 10, 0.95) 0%, 
                    rgba(26, 26, 46, 0.98) 50%, 
                    rgba(45, 27, 105, 0.95) 100%);
                z-index: 1000;
                opacity: 0;
                backdrop-filter: blur(0px);
                transition: all 0.8s cubic-bezier(0.4, 0, 0.2, 1);
                pointer-events: none;
            `;
            document.body.appendChild(transitionOverlay);

            // Create particle burst effect
            createTransitionParticles(transitionOverlay);
            
            // Phase 1: Fade out previous slide with overlay
            requestAnimationFrame(() => {
                transitionOverlay.style.opacity = '1';
                transitionOverlay.style.backdropFilter = 'blur(20px)';
                
                previousSlide.style.transform = isForward ? 
                    'translateX(-30px) scale(0.95)' : 
                    'translateX(30px) scale(0.95)';
                previousSlide.style.opacity = '0';
                previousSlide.style.filter = 'blur(10px)';
                previousSlide.style.transition = 'all 0.8s cubic-bezier(0.4, 0, 0.2, 1)';
            });

            // Phase 2: Prepare next slide
            setTimeout(() => {
                previousSlide.style.display = 'none';
                previousSlide.style.transform = '';
                previousSlide.style.opacity = '';
                previousSlide.style.filter = '';
                previousSlide.style.transition = '';
                
                nextSlide.style.transform = isForward ? 
                    'translateX(30px) scale(1.05)' : 
                    'translateX(-30px) scale(1.05)';
                nextSlide.style.opacity = '0';
                nextSlide.style.filter = 'blur(5px)';
                nextSlide.style.transition = 'all 0.8s cubic-bezier(0.4, 0, 0.2, 1)';
            }, 200);

            // Phase 3: Fade in next slide
            setTimeout(() => {
                transitionOverlay.style.opacity = '0';
                transitionOverlay.style.backdropFilter = 'blur(0px)';
                
                nextSlide.style.transform = 'translateX(0) scale(1)';
                nextSlide.style.opacity = '1';
                nextSlide.style.filter = 'blur(0px)';
                
                requestAnimationFrame(() => {
                    nextSlide.classList.add('active');
                });
            }, 400);

            // Phase 4: Cleanup
            setTimeout(() => {
                if (document.body.contains(transitionOverlay)) {
                    document.body.removeChild(transitionOverlay);
                }
                
                nextSlide.style.transform = '';
                nextSlide.style.opacity = '';
                nextSlide.style.filter = '';
                nextSlide.style.transition = '';
                
                onComplete();
            }, 1200);
        }

        function createTransitionParticles(container) {
            for (let i = 0; i < 20; i++) {
                const particle = document.createElement('div');
                particle.style.cssText = `
                    position: absolute;
                    width: ${Math.random() * 4 + 2}px;
                    height: ${Math.random() * 4 + 2}px;
                    background: rgba(255, 255, 255, ${Math.random() * 0.6 + 0.2});
                    border-radius: 50%;
                    left: ${Math.random() * 100}%;
                    top: ${Math.random() * 100}%;
                    animation: transition-particle-float 1.2s ease-out forwards;
                    pointer-events: none;
                `;
                container.appendChild(particle);
            }

            // Add particle animation CSS
            if (!document.getElementById('transition-particle-styles')) {
                const style = document.createElement('style');
                style.id = 'transition-particle-styles';
                style.textContent = `
                    @keyframes transition-particle-float {
                        0% { 
                            transform: translateY(0) scale(0); 
                            opacity: 0; 
                        }
                        20% { 
                            opacity: 1; 
                            transform: translateY(-20px) scale(1); 
                        }
                        100% { 
                            transform: translateY(-100px) scale(0.5); 
                            opacity: 0; 
                        }
                    }
                `;
                document.head.appendChild(style);
            }
        }

        function nextSlide() {
            if (currentSlide < totalSlides - 1) {
                showSlide(currentSlide + 1);
            }
        }

        function previousSlide() {
            if (currentSlide > 0) {
                showSlide(currentSlide - 1);
            }
        }

        function updateNavigationButtons() {
            const prevBtn = document.getElementById('prev-btn');
            const nextBtn = document.getElementById('next-btn');
            
            prevBtn.disabled = currentSlide === 0;
            nextBtn.disabled = currentSlide === totalSlides - 1;
        }

        function updateSlideIndicator() {
            const indicator = document.getElementById('slide-indicator');
            indicator.innerHTML = '';
            
            for (let i = 0; i < totalSlides; i++) {
                const dot = document.createElement('div');
                dot.className = `indicator-dot ${i === currentSlide ? 'active' : ''}`;
                dot.addEventListener('click', () => showSlide(i));
                indicator.appendChild(dot);
            }
        }

        // Utility functions
        function formatNumber(num) {
            if (num >= 1000) {
                return (num / 1000).toFixed(1) + 'k';
            }
            return num.toString();
        }

        function formatDuration(seconds) {
            const hours = Math.floor(seconds / 3600);
            const minutes = Math.floor((seconds % 3600) / 60);
            
            if (hours > 0) {
                return `${hours}h ${minutes}m`;
            }
            return `${minutes}m`;
        }

        // Export functions for external use
        window.MusicBeeWrapped = {
            showSlide,
            nextSlide,
            previousSlide,
            formatNumber,
            formatDuration
        };";
        }

        /// <summary>
        /// Gets JavaScript for the year selector interface
        /// Handles year card interactions and navigation to specific years
        /// </summary>
        /// <returns>Year selector JavaScript code as string</returns>
        public string GetYearSelectorJS()
        {
            return @"
        document.addEventListener('DOMContentLoaded', function() {
            initializeYearSelector();
        });

        function initializeYearSelector() {
            // Add click handlers to year cards
            const yearCards = document.querySelectorAll('.year-card');
            
            yearCards.forEach(card => {
                card.addEventListener('click', function(e) {
                    e.preventDefault();
                    const year = this.dataset.year;
                    
                    if (year) {
                        // Add loading state
                        this.style.opacity = '0.6';
                        this.style.pointerEvents = 'none';
                        
                        // Navigate to wrapped for this year
                        window.location.href = `wrapped_${year}.html`;
                    }
                });

                // Add hover effects
                card.addEventListener('mouseenter', function() {
                    this.style.transform = 'translateY(-10px) scale(1.02)';
                });

                card.addEventListener('mouseleave', function() {
                    this.style.transform = 'translateY(0) scale(1)';
                });
            });

            // Add keyboard navigation
            document.addEventListener('keydown', function(e) {
                const cards = Array.from(yearCards);
                const currentFocus = document.activeElement;
                const currentIndex = cards.indexOf(currentFocus);

                switch(e.key) {
                    case 'ArrowRight':
                    case 'ArrowDown':
                        e.preventDefault();
                        const nextIndex = (currentIndex + 1) % cards.length;
                        cards[nextIndex].focus();
                        break;
                    case 'ArrowLeft':
                    case 'ArrowUp':
                        e.preventDefault();
                        const prevIndex = currentIndex > 0 ? currentIndex - 1 : cards.length - 1;
                        cards[prevIndex].focus();
                        break;
                    case 'Enter':
                    case ' ':
                        e.preventDefault();
                        if (currentFocus && currentFocus.classList.contains('year-card')) {
                            currentFocus.click();
                        }
                        break;
                }
            });

            // Add animation to cards on load
            yearCards.forEach((card, index) => {
                card.style.opacity = '0';
                card.style.transform = 'translateY(30px)';
                
                setTimeout(() => {
                    card.style.transition = 'all 0.6s ease';
                    card.style.opacity = '1';
                    card.style.transform = 'translateY(0)';
                }, index * 100);
            });
        }

        // Utility function for year stats formatting
        function formatYearStats(stats) {
            return {
                tracks: formatNumber(stats.tracks),
                hours: stats.hours.toFixed(1),
                artists: formatNumber(stats.artists),
                albums: formatNumber(stats.albums)
            };
        }

        function formatNumber(num) {
            if (num >= 1000) {
                return (num / 1000).toFixed(1) + 'k';
            }
            return num.toString();
        }";
        }

        /// <summary>
        /// Gets shared JavaScript utilities used across interfaces
        /// Common functions for formatting, animations, and interactions
        /// </summary>
        /// <returns>Shared JavaScript utilities as string</returns>
        public string GetSharedUtilities()
        {
            return @"
        // Shared utility functions
        window.MusicBeeUtils = {
            formatNumber: function(num) {
                if (num >= 1000000) {
                    return (num / 1000000).toFixed(1) + 'M';
                }
                if (num >= 1000) {
                    return (num / 1000).toFixed(1) + 'k';
                }
                return num.toString();
            },

            formatDuration: function(seconds) {
                const hours = Math.floor(seconds / 3600);
                const minutes = Math.floor((seconds % 3600) / 60);
                
                if (hours > 0) {
                    return `${hours}h ${minutes}m`;
                }
                return `${minutes}m`;
            },

            formatDate: function(dateString) {
                const date = new Date(dateString);
                return date.toLocaleDateString('en-US', { 
                    year: 'numeric', 
                    month: 'long', 
                    day: 'numeric' 
                });
            },

            animateValue: function(element, start, end, duration) {
                const range = end - start;
                const startTime = performance.now();
                
                function update(currentTime) {
                    const elapsed = currentTime - startTime;
                    const progress = Math.min(elapsed / duration, 1);
                    
                    const currentValue = start + (range * this.easeOutCubic(progress));
                    element.textContent = Math.floor(currentValue);
                    
                    if (progress < 1) {
                        requestAnimationFrame(update);
                    }
                }
                
                requestAnimationFrame(update);
            },

            easeOutCubic: function(t) {
                return 1 - Math.pow(1 - t, 3);
            },

            debounce: function(func, wait) {
                let timeout;
                return function executedFunction(...args) {
                    const later = () => {
                        clearTimeout(timeout);
                        func(...args);
                    };
                    clearTimeout(timeout);
                    timeout = setTimeout(later, wait);
                };
            },

            throttle: function(func, limit) {
                let inThrottle;
                return function() {
                    const args = arguments;
                    const context = this;
                    if (!inThrottle) {
                        func.apply(context, args);
                        inThrottle = true;
                        setTimeout(() => inThrottle = false, limit);
                    }
                };
            }
        };

        // Global animation helpers
        function fadeIn(element, duration = 300) {
            element.style.opacity = '0';
            element.style.display = 'block';
            
            const start = performance.now();
            
            function animate(currentTime) {
                const elapsed = currentTime - start;
                const progress = Math.min(elapsed / duration, 1);
                
                element.style.opacity = progress;
                
                if (progress < 1) {
                    requestAnimationFrame(animate);
                }
            }
            
            requestAnimationFrame(animate);
        }

        function slideUp(element, duration = 300) {
            element.style.transform = 'translateY(30px)';
            element.style.opacity = '0';
            element.style.display = 'block';
            
            const start = performance.now();
            
            function animate(currentTime) {
                const elapsed = currentTime - start;
                const progress = Math.min(elapsed / duration, 1);
                const eased = 1 - Math.pow(1 - progress, 3);
                
                element.style.transform = `translateY(${30 * (1 - eased)}px)`;
                element.style.opacity = progress;
                
                if (progress < 1) {
                    requestAnimationFrame(animate);
                }
            }
            
            requestAnimationFrame(animate);
        }";
        }

        /// <summary>
        /// Combines all JavaScript components into a single script
        /// Used when a complete JavaScript bundle is needed
        /// </summary>
        /// <param name="includeYearSelector">Whether to include year selector scripts</param>
        /// <returns>Complete combined JavaScript code</returns>
        public string GetCombinedJS(bool includeYearSelector = false)
        {
            var js = new StringBuilder();
            
            js.AppendLine(GetSharedUtilities());
            js.AppendLine(GetMainInterfaceJS());
            
            if (includeYearSelector)
            {
                js.AppendLine(GetYearSelectorJS());
            }
            
            return js.ToString();
        }
    }
}
