using System;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Welcome slide component - The opening slide of the wrapped experience
    /// Sets the tone and provides an overview of what's to come
    /// </summary>
    public class WelcomeSlide : SlideComponentBase
    {
        public override string SlideId => "welcome";
        public override string SlideTitle => "Welcome";
        public override int SlideOrder => 0;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var content = $@"
                <div class='welcome-container'>
                    <div class='background-elements'>
                        <div class='waveform-bg'></div>
                        <div class='particle-system'></div>
                        <div class='gradient-orb gradient-orb-1'></div>
                        <div class='gradient-orb gradient-orb-2'></div>
                        <div class='gradient-orb gradient-orb-3'></div>
                    </div>
                    
                    <div class='content-grid'>
                        <div class='hero-section'>
                            <div class='year-display'>
                                <span class='year-text'>{year}</span>
                                <div class='year-underline'></div>
                            </div>
                            
                            <div class='title-stack'>
                                <h1 class='main-title'>
                                    <span class='title-word word-1'>Your</span>
                                    <span class='title-word word-2'>Music</span>
                                    <span class='title-word word-3'>Story</span>
                                </h1>
                                <div class='title-subtitle'>A deep dive into your personal soundtrack</div>
                            </div>
                            
                            <div class='stats-preview'>
                                <div class='stat-item stat-1'>
                                    <div class='stat-number'>{FormatNumber(stats.TotalTracks)}</div>
                                    <div class='stat-label'>Tracks Played</div>
                                </div>
                                <div class='stat-item stat-2'>
                                    <div class='stat-number'>{stats.TotalHours:F0}h</div>
                                    <div class='stat-label'>Hours Listened</div>
                                </div>
                                <div class='stat-item stat-3'>
                                    <div class='stat-number'>{stats.TotalUniqueArtists}</div>
                                    <div class='stat-label'>Unique Artists</div>
                                </div>
                            </div>
                        </div>
                        
                        <div class='navigation-hint'>
                            <div class='hint-line'></div>
                            <span class='hint-text'>Navigate with arrow keys or click</span>
                            <div class='hint-pulse'></div>
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            return GetWelcomeInsight(stats, year);
        }

        private string GetWelcomeInsight(WrappedStatistics stats, int year)
        {
            var trackText = stats.TotalTracks == 1 ? "track" : "tracks";
            var hourText = Math.Abs(stats.TotalHours - 1) < 0.1 ? "hour" : "hours";
            
            return $"This year you listened to <strong>{FormatNumber(stats.TotalTracks)} {trackText}</strong> " +
                   $"across <strong>{stats.TotalHours:F1} {hourText}</strong> of music. " +
                   $"Let's explore what made your {year} special.";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) && stats.TotalTracks > 0;
        }

        public override string GenerateCSS()
        {
            return @"
        /* Welcome Slide - Professional Design System */
        .welcome-container {
            position: relative;
            width: 100%;
            height: 100vh;
            overflow: hidden;
            background: linear-gradient(135deg, #0c0c0c 0%, #1a1a2e 25%, #16213e  50%, #0f3460 75%, #533483 100%);
        }

        /* Background Elements */
        .background-elements {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 1;
        }

        .waveform-bg {
            position: absolute;
            bottom: 0;
            left: 0;
            width: 100%;
            height: 20vh;
            background: repeating-linear-gradient(
                90deg,
                transparent 0,
                rgba(255, 255, 255, 0.03) 0.05vw,
                rgba(255, 255, 255, 0.03) 0.1vw,
                transparent 0.15vw,
                transparent 0.5vw
            );
            animation: waveform-pulse 3s ease-in-out infinite;
        }

        .particle-system {
            position: absolute;
            width: 100%;
            height: 100%;
            background-image: 
                radial-gradient(circle at 20% 30%, rgba(255, 255, 255, 0.1) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 80% 20%, rgba(83, 52, 131, 0.3) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 40% 70%, rgba(15, 52, 96, 0.2) 0.1vw, transparent 0.1vw),
                radial-gradient(circle at 90% 80%, rgba(255, 255, 255, 0.05) 0.1vw, transparent 0.1vw);
            background-size: 10vw 10vh, 15vw 15vh, 8vw 8vh, 12vw 12vh;
            animation: particle-float 20s linear infinite;
        }

        .gradient-orb {
            position: absolute;
            border-radius: 50%;
            filter: blur(4vw);
            opacity: 0.4;
            animation: orb-float 8s ease-in-out infinite;
        }

        .gradient-orb-1 {
            width: 30vw;
            height: 30vw;
            background: radial-gradient(circle, #533483, transparent);
            top: 10%;
            right: 10%;
            animation-delay: 0s;
        }

        .gradient-orb-2 {
            width: 20vw;
            height: 20vw;
            background: radial-gradient(circle, #0f3460, transparent);
            bottom: 20%;
            left: 15%;
            animation-delay: 3s;
        }

        .gradient-orb-3 {
            width: 15vw;
            height: 15vw;
            background: radial-gradient(circle, #16213e, transparent);
            top: 50%;
            left: 50%;
            animation-delay: 6s;
        }

        /* Content Grid */
        .content-grid {
            position: relative;
            z-index: 2;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            height: 100vh;
            padding: clamp(2rem, 4vw, 4rem);
            text-align: center;
        }

        /* Hero Section */
        .hero-section {
            max-width: 80vw;
            margin-bottom: clamp(3rem, 6vh, 6rem);
        }

        .year-display {
            margin-bottom: clamp(2rem, 4vh, 4rem);
            animation: year-entrance 1s ease-out;
        }

        .year-text {
            font-size: clamp(1.5rem, 4vw, 2rem);
            font-weight: 300;
            color: rgba(255, 255, 255, 0.7);
            letter-spacing: 0.8vw;
            text-transform: uppercase;
            position: relative;
        }

        .year-underline {
            width: 0;
            height: 0.2vh;
            background: linear-gradient(90deg, transparent, #533483, transparent);
            margin: clamp(1rem, 2vh, 2rem) auto 0;
            animation: underline-grow 1.5s ease-out 0.5s forwards;
        }

        /* Title Stack */
        .title-stack {
            margin-bottom: clamp(3rem, 6vh, 6rem);
        }

        .main-title {
            font-size: clamp(3rem, 8vw, 6rem);
            font-weight: 700;
            line-height: 1.1;
            margin-bottom: clamp(1.5rem, 3vh, 3rem);
            background: linear-gradient(135deg, #ffffff 0%, #b8b8b8 50%, #533483 100%);
            background-clip: text;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-size: 200% 200%;
            animation: title-shimmer 3s ease-in-out infinite;
        }

        .title-word {
            display: inline-block;
            animation: word-entrance 0.8s ease-out forwards;
            opacity: 0;
            transform: translateY(3vh);
        }

        .word-1 { animation-delay: 0.2s; }
        .word-2 { animation-delay: 0.4s; }
        .word-3 { animation-delay: 0.6s; }

        .title-subtitle {
            font-size: clamp(1rem, 2.5vw, 1.2rem);
            font-weight: 300;
            color: rgba(255, 255, 255, 0.6);
            letter-spacing: 0.2vw;
            animation: subtitle-fade-in 1s ease-out 1s forwards;
            opacity: 0;
        }

        /* Stats Preview */
        .stats-preview {
            display: flex;
            justify-content: center;
            gap: clamp(2rem, 6vw, 6rem);
            flex-wrap: wrap;
        }

        .stat-item {
            opacity: 0;
            transform: translateY(2vh);
            animation: stat-entrance 0.6s ease-out forwards;
        }

        .stat-1 { animation-delay: 1.2s; }
        .stat-2 { animation-delay: 1.4s; }
        .stat-3 { animation-delay: 1.6s; }

        .stat-number {
            font-size: clamp(2rem, 5vw, 2.5rem);
            font-weight: 600;
            color: #533483;
            margin-bottom: clamp(0.5rem, 1vh, 1rem);
            text-shadow: 0 0 2vw rgba(83, 52, 131, 0.5);
        }

        .stat-label {
            font-size: clamp(0.8rem, 2vw, 0.9rem);
            color: rgba(255, 255, 255, 0.5);
            text-transform: uppercase;
            letter-spacing: 0.1vw;
        }

        /* Navigation Hint */
        .navigation-hint {
            display: flex;
            align-items: center;
            gap: clamp(1rem, 3vw, 2rem);
            opacity: 0;
            animation: hint-fade-in 1s ease-out 2s forwards;
        }

        .hint-line {
            width: clamp(30px, 5vw, 60px);
            height: 0.1vh;
            background: rgba(255, 255, 255, 0.3);
        }

        .hint-text {
            font-size: clamp(0.8rem, 2vw, 0.9rem);
            color: rgba(255, 255, 255, 0.5);
            letter-spacing: 0.1vw;
        }

        .hint-pulse {
            width: clamp(6px, 1vw, 12px);
            height: clamp(6px, 1vw, 12px);
            border-radius: 50%;
            background: #533483;
            animation: pulse-glow 2s ease-in-out infinite;
        }

        /* Animations */
        @keyframes waveform-pulse {
            0%, 100% { opacity: 0.3; transform: scaleY(1); }
            50% { opacity: 0.6; transform: scaleY(1.2); }
        }

        @keyframes particle-float {
            0% { transform: translateX(0) translateY(0); }
            25% { transform: translateX(1vw) translateY(-0.5vh); }
            50% { transform: translateX(0) translateY(-1vh); }
            75% { transform: translateX(-1vw) translateY(-0.5vh); }
            100% { transform: translateX(0) translateY(0); }
        }

        @keyframes orb-float {
            0%, 100% { transform: translateY(0) scale(1); }
            50% { transform: translateY(-2vh) scale(1.1); }
        }

        @keyframes year-entrance {
            from { opacity: 0; transform: translateY(-2vh); }
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes underline-grow {
            from { width: 0; }
            to { width: clamp(60px, 8vw, 120px); }
        }

        @keyframes title-shimmer {
            0% { background-position: 0% 50%; }
            50% { background-position: 100% 50%; }
            100% { background-position: 0% 50%; }
        }

        @keyframes word-entrance {
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes subtitle-fade-in {
            to { opacity: 1; }
        }

        @keyframes stat-entrance {
            to { opacity: 1; transform: translateY(0); }
        }

        @keyframes hint-fade-in {
            to { opacity: 1; }
        }

        @keyframes pulse-glow {
            0%, 100% { box-shadow: 0 0 0.5vw rgba(83, 52, 131, 0.5); }
            50% { box-shadow: 0 0 2vw rgba(83, 52, 131, 1), 0 0 3vw rgba(83, 52, 131, 0.5); }
        }

        /* Responsive Design */
        @media (max-width: 48rem) {
            .content-grid { padding: clamp(1.5rem, 3vw, 3rem); }
            .stats-preview { 
                gap: clamp(1.5rem, 4vw, 3rem); 
                flex-direction: column;
                align-items: center;
            }
            .stat-item { margin-bottom: 1rem; }
            .year-text { font-size: clamp(1.2rem, 4vw, 1.6rem); letter-spacing: 0.5vw; }
            .stat-number { font-size: clamp(1.8rem, 5vw, 2.2rem); }
            .navigation-hint { flex-direction: column; gap: clamp(0.8rem, 2vw, 1.2rem); }
        }";
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        // Professional Welcome Slide Enhancements
        function initializeWelcomeSlide() {
            const welcomeSlide = document.querySelector('[data-slide-id=""welcome""]');
            if (!welcomeSlide) return;

            // Enhanced particle system
            createDynamicParticles();
            
            // Interactive gradient orbs
            initializeOrbInteractions();
            
            // Advanced number counter animation
            animateStatNumbers();
            
            // Audio-reactive waveform (simulated)
            simulateAudioWaveform();
        }

        function createDynamicParticles() {
            const particleSystem = document.querySelector('.particle-system');
            if (!particleSystem) return;

            // Create floating geometric particles
            for (let i = 0; i < 15; i++) {
                const particle = document.createElement('div');
                particle.className = 'dynamic-particle';
                particle.style.cssText = `
                    position: absolute;
                    width: ${Math.random() * 4 + 2}px;
                    height: ${Math.random() * 4 + 2}px;
                    background: rgba(255, 255, 255, ${Math.random() * 0.3 + 0.1});
                    border-radius: 50%;
                    left: ${Math.random() * 100}%;
                    top: ${Math.random() * 100}%;
                    animation: particle-drift ${Math.random() * 20 + 10}s linear infinite;
                    animation-delay: ${Math.random() * 10}s;
                `;
                particleSystem.appendChild(particle);
            }

            // Add CSS for particle drift
            const style = document.createElement('style');
            style.textContent = `
                @keyframes particle-drift {
                    0% { transform: translateY(100vh) translateX(0); opacity: 0; }
                    10% { opacity: 1; }
                    90% { opacity: 1; }
                    100% { transform: translateY(-10vh) translateX(${Math.random() * 20 - 10}vw); opacity: 0; }
                }
            `;
            document.head.appendChild(style);
        }

        function initializeOrbInteractions() {
            const orbs = document.querySelectorAll('.gradient-orb');
            
            document.addEventListener('mousemove', (e) => {
                const mouseX = e.clientX / window.innerWidth;
                const mouseY = e.clientY / window.innerHeight;
                
                orbs.forEach((orb, index) => {
                    const speed = (index + 1) * 0.02;
                    const xOffset = (mouseX - 0.5) * speed * 100;
                    const yOffset = (mouseY - 0.5) * speed * 100;
                    
                    orb.style.transform = `translate(${xOffset}px, ${yOffset}px)`;
                });
            });
        }

        function animateStatNumbers() {
            const statNumbers = document.querySelectorAll('.stat-number');
            
            statNumbers.forEach((element, index) => {
                setTimeout(() => {
                    const finalValue = element.textContent;
                    const numericValue = parseInt(finalValue.replace(/[^\d]/g, ''));
                    
                    if (!isNaN(numericValue)) {
                        animateCounter(element, 0, numericValue, 1500, finalValue);
                    }
                }, 1200 + (index * 200));
            });
        }

        function animateCounter(element, start, end, duration, finalText) {
            const startTime = performance.now();
            
            function updateCounter(currentTime) {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / duration, 1);
                const easeOut = 1 - Math.pow(1 - progress, 3);
                
                const current = Math.floor(start + (end - start) * easeOut);
                
                if (finalText.includes('h')) {
                    element.textContent = current + 'h';
                } else if (end > 1000) {
                    element.textContent = (current / 1000).toFixed(1) + 'k';
                } else {
                    element.textContent = current.toLocaleString();
                }
                
                if (progress < 1) {
                    requestAnimationFrame(updateCounter);
                } else {
                    element.textContent = finalText;
                }
            }
            
            requestAnimationFrame(updateCounter);
        }

        function simulateAudioWaveform() {
            const waveform = document.querySelector('.waveform-bg');
            if (!waveform) return;

            let intensity = 0;
            
            function updateWaveform() {
                intensity = Math.sin(Date.now() * 0.003) * 0.5 + 0.5;
                const scale = 0.8 + intensity * 0.4;
                const opacity = 0.3 + intensity * 0.3;
                
                waveform.style.transform = `scaleY(${scale})`;
                waveform.style.opacity = opacity;
                
                requestAnimationFrame(updateWaveform);
            }
            
            updateWaveform();
        }

        // Initialize when the welcome slide becomes active
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(initializeWelcomeSlide, 100);
        });

        // Use event listener approach instead of function override to avoid conflicts
        document.addEventListener('slideChanged', function(event) {
            setTimeout(() => {
                const activeSlide = document.querySelector('.slide.active');
                if (activeSlide && activeSlide.querySelector('[data-slide-id=""welcome""]')) {
                    initializeWelcomeSlide();
                }
            }, 100);
        });";
        }
    }
}
