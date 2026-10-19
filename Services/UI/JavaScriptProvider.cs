using System;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Provides JavaScript functionality for the MusicBee Wrapped web interface.
    /// Supports both slide-to-slide navigation and smooth intra-slide step transitions on Arrow keys.
    /// </summary>
    public class JavaScriptProvider
    {
        public string GetMainInterfaceJS()
        {
            return @"
        let currentSlide = 0;
        let currentStep = 0;
        let slides = [];
        let totalSlides = 0;

        document.addEventListener('DOMContentLoaded', function() {
            initializeApp();
        });

        function initializeApp() {
            const loading = document.getElementById('loading');
            if (loading) loading.style.display = 'none';

            slides = Array.from(document.querySelectorAll('.slide[data-slide-id]'));
            totalSlides = slides.length;

            buildSlideIndicator();
            initializeNavigation();
            initializeStepSwitchers();
            showSlide(0, 0);
        }

        function getSlideSteps(slideEl) {
            if (!slideEl) return [];
            return Array.from(slideEl.querySelectorAll('.slide-step'));
        }

        function initializeStepSwitchers() {
            slides.forEach((slideEl, sIdx) => {
                const pills = Array.from(slideEl.querySelectorAll('[data-goto-step]'));
                pills.forEach(pill => {
                    pill.addEventListener('click', () => {
                        const targetStep = parseInt(pill.getAttribute('data-goto-step') || '0', 10);
                        showSlide(sIdx, targetStep);
                    });
                });
            });
        }

        function buildSlideIndicator() {
            const indicator = document.getElementById('slide-indicator');
            if (!indicator) return;
            indicator.innerHTML = '';

            slides.forEach((slide, idx) => {
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'indicator-dot' + (idx === currentSlide ? ' active' : '');
                const title = slide.getAttribute('data-slide-title') || `Slide ${idx + 1}`;
                btn.title = `${idx + 1}. ${title}`;
                btn.setAttribute('aria-label', title);
                btn.addEventListener('click', () => showSlide(idx, 0));
                indicator.appendChild(btn);
            });
        }

        function initializeNavigation() {
            const prevBtn = document.getElementById('prev-btn');
            const nextBtn = document.getElementById('next-btn');

            if (prevBtn) prevBtn.addEventListener('click', () => previousSlide());
            if (nextBtn) nextBtn.addEventListener('click', () => nextSlide());

            document.addEventListener('keydown', (e) => {
                if (e.target && (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA')) return;
                if (e.key === 'ArrowRight') {
                    e.preventDefault();
                    nextSlide();
                } else if (e.key === 'ArrowLeft') {
                    e.preventDefault();
                    previousSlide();
                } else if (e.key === 'Home') {
                    e.preventDefault();
                    showSlide(0, 0);
                } else if (e.key === 'End') {
                    e.preventDefault();
                    showSlide(totalSlides - 1, 0);
                }
            });

            let startX = 0;
            document.addEventListener('touchstart', (e) => {
                if (e.touches && e.touches.length > 0) startX = e.touches[0].clientX;
            }, { passive: true });

            document.addEventListener('touchend', (e) => {
                if (!e.changedTouches || e.changedTouches.length === 0) return;
                const diff = startX - e.changedTouches[0].clientX;
                if (Math.abs(diff) > 50) {
                    if (diff > 0) nextSlide();
                    else previousSlide();
                }
            }, { passive: true });
        }

        function showSlide(index, stepIndex) {
            if (index < 0 || index >= totalSlides) return;

            currentSlide = index;
            slides.forEach((slide, i) => {
                if (i === index) {
                    slide.classList.add('active');
                    slide.style.display = 'block';
                } else {
                    slide.classList.remove('active');
                    slide.style.display = 'none';
                }
            });

            const activeSlide = slides[index];
            const steps = getSlideSteps(activeSlide);
            if (steps.length > 0) {
                if (typeof stepIndex !== 'number' || stepIndex < 0) stepIndex = 0;
                if (stepIndex >= steps.length) stepIndex = steps.length - 1;
                currentStep = stepIndex;

                steps.forEach((st, idx) => {
                    st.classList.toggle('active', idx === currentStep);
                });

                const pills = Array.from(activeSlide.querySelectorAll('[data-goto-step]'));
                pills.forEach(p => {
                    const pStep = parseInt(p.getAttribute('data-goto-step') || '0', 10);
                    p.classList.toggle('active', pStep === currentStep);
                });
            } else {
                currentStep = 0;
            }

            if (activeSlide) {
                const accent = activeSlide.getAttribute('data-accent') || 'parchment';
                document.body.setAttribute('data-theme', accent);

                const titleEl = document.getElementById('topbar-slide-title');
                if (titleEl) {
                    const stepTitle = steps[currentStep] && steps[currentStep].getAttribute('data-step-title');
                    titleEl.textContent = stepTitle || activeSlide.getAttribute('data-slide-title') || '';
                }
            }

            const counterEl = document.getElementById('topbar-slide-counter');
            if (counterEl) {
                const cur = String(index + 1).padStart(2, '0');
                const tot = String(totalSlides).padStart(2, '0');
                counterEl.textContent = `${cur} / ${tot}`;
            }

            const appContainer = document.getElementById('app');
            if (appContainer) appContainer.scrollTop = 0;

            updateNavigationButtons();
            updateSlideIndicator();

            const slideChangedEvent = new CustomEvent('slideChanged', {
                detail: { slideIndex: index, stepIndex: currentStep, slideElement: activeSlide }
            });
            document.dispatchEvent(slideChangedEvent);
        }

        function nextSlide() {
            const activeSlide = slides[currentSlide];
            const steps = getSlideSteps(activeSlide);
            if (steps.length > 0 && currentStep < steps.length - 1) {
                showSlide(currentSlide, currentStep + 1);
                return;
            }
            if (currentSlide < totalSlides - 1) {
                showSlide(currentSlide + 1, 0);
            }
        }

        function previousSlide() {
            const activeSlide = slides[currentSlide];
            const steps = getSlideSteps(activeSlide);
            if (steps.length > 0 && currentStep > 0) {
                showSlide(currentSlide, currentStep - 1);
                return;
            }
            if (currentSlide > 0) {
                const prevSlideEl = slides[currentSlide - 1];
                const prevSteps = getSlideSteps(prevSlideEl);
                const lastStep = prevSteps.length > 0 ? prevSteps.length - 1 : 0;
                showSlide(currentSlide - 1, lastStep);
            }
        }

        function updateNavigationButtons() {
            const prevBtn = document.getElementById('prev-btn');
            const nextBtn = document.getElementById('next-btn');
            const activeSlide = slides[currentSlide];
            const steps = getSlideSteps(activeSlide);
            const atFirstStep = currentStep <= 0;
            const atLastStep = steps.length === 0 || currentStep >= steps.length - 1;

            if (prevBtn) prevBtn.disabled = currentSlide <= 0 && atFirstStep;
            if (nextBtn) nextBtn.disabled = currentSlide >= totalSlides - 1 && atLastStep;
        }

        function updateSlideIndicator() {
            const dots = document.querySelectorAll('#slide-indicator .indicator-dot');
            dots.forEach((dot, idx) => {
                dot.classList.toggle('active', idx === currentSlide);
            });
        }

        function exportTop50Playlist(year) {
            const topTracks = window.top50Tracks || [];
            if (!topTracks || topTracks.length === 0) {
                alert('No track file paths found to export.');
                return;
            }
            let m3u = '#EXTM3U\n';
            topTracks.forEach(track => {
                const dur = track.duration || -1;
                const artist = track.artist || 'Unknown Artist';
                const title = track.title || 'Unknown Track';
                m3u += `#EXTINF:${dur},${artist} - ${title}\n${track.filePath}\n`;
            });
            const blob = new Blob([m3u], { type: 'audio/x-mpegurl;charset=utf-8' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `MusicBeeWrapped_Top50_${year}.m3u`;
            document.body.appendChild(a);
            a.click();
            setTimeout(() => {
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
            }, 150);
        }
        window.exportTop50Playlist = exportTop50Playlist;";
        }

        public string GetYearSelectorJS()
        {
            return @"
        document.addEventListener('DOMContentLoaded', function() {
            const cards = Array.from(document.querySelectorAll('.year-card'));
            cards.forEach((card) => {
                card.addEventListener('keydown', (e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        card.click();
                    }
                });
            });
        });";
        }
    }
}
