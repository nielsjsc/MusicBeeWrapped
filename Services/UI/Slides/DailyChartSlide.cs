using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Daily Chart slide component - Shows daily listening activity throughout the year
    /// Displays a chart of daily play counts and listening statistics
    /// </summary>
    public class DailyChartSlide : SlideComponentBase
    {
        public override string SlideId => "daily-chart";
        public override string SlideTitle => "Your Daily Listening Journey";
        public override int SlideOrder => 7;
        public override bool RequiresChartRendering => true;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var content = $@"
                <div class='cosmic-chart-bg'>
                    <div class='starfield'></div>
                    <div class='nebula-purple'></div>
                    <div class='nebula-blue'></div>
                    <div class='orbit-rings'></div>
                    <div class='cosmic-planet primary'></div>
                    <div class='cosmic-planet secondary'></div>
                    <div class='cosmic-comet'></div>
                    <div class='cosmic-asteroids'></div>
                    <div class='shooting-stars'></div>
                </div>
                <div class='cosmic-chart-nav'>
                    <button id='chart-back-btn' class='zoom-out-btn'>
                        <span class='zoom-icon'>⟨</span>
                        <span class='zoom-label'>Back</span>
                    </button>
                    <h2 id='chart-nav-title'>Monthly Listening Journey</h2>
                </div>
                <div class='chart-container'>
                    <div class='chart-units' id='chart-units'>Plays</div>
                    <canvas id='dailyChart' width='1200' height='500'></canvas>
                    <div class='chart-interaction-hint' id='chart-hint'>
                        <div class='hint-glow'></div>
                        <span class='hint-text'>Click to explore deeper</span>
                    </div>
                </div>";
            return WrapInSlideContainer(content);
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        let chartView = 'month';
        let chartState = { selectedMonth: null, selectedDay: null };
        let animationFrame = null;
        let chartTransition = { progress: 0, animating: false };
        
        const canvas = document.getElementById('dailyChart');
        const ctx = canvas.getContext('2d');
        const navTitle = document.getElementById('chart-nav-title');
        const backBtn = document.getElementById('chart-back-btn');
        const chartUnits = document.getElementById('chart-units');
        const chartHint = document.getElementById('chart-hint');
        const data = window.WRAPPED_DATA;
        const dailyPlays = data.dailyPlays || {};
        const playHistory = data.playHistory || [];
        

        // High DPI canvas setup
        function setupHighDPICanvas() {
        const container = canvas.parentElement;
        const containerRect = container.getBoundingClientRect();
        const dpr = window.devicePixelRatio || 1;
        
        // Set actual canvas size
        canvas.width = containerRect.width * dpr;
        canvas.height = containerRect.height * dpr;
        
        // Set display size
        canvas.style.width = containerRect.width + 'px';
        canvas.style.height = containerRect.height + 'px';
        
        // Scale the context for high DPI
        ctx.scale(dpr, dpr);
        
        // Store logical dimensions for drawing calculations
        canvas.logicalWidth = containerRect.width;
        canvas.logicalHeight = containerRect.height;
    }

        function getMonthlyTotals() {
            const months = Array(12).fill(0);
            Object.keys(dailyPlays).forEach(dateStr => {
                const d = new Date(dateStr);
                months[d.getMonth()] += dailyPlays[dateStr];
            });
            return months;
        }

        function getDailyTotals(monthIdx) {
            const year = data.year;
            const days = [];
            const start = new Date(year, monthIdx, 1);
            const end = new Date(year, monthIdx + 1, 0);
            for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
                const dateStr = d.toISOString().split('T')[0];
                days.push({ date: new Date(d), plays: dailyPlays[dateStr] || 0 });
            }
            return days;
        }

        function getHourlyTotals(dateObj) {
            // Remove hourly zoom functionality: always return null or empty
            return null;
        }

        function easeInOutCubic(t) {
            return t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
        }

        function animateTransition(callback) {
            if (chartTransition.animating) return;
            
            chartTransition.animating = true;
            chartTransition.progress = 0;
            
            function animate() {
                chartTransition.progress += 0.08;
                if (chartTransition.progress >= 1) {
                    chartTransition.progress = 1;
                    chartTransition.animating = false;
                }
                
                callback();
                
                if (chartTransition.animating) {
                    requestAnimationFrame(animate);
                }
            }
            
            requestAnimationFrame(animate);
        }

        function updateUI() {
            if (chartView === 'month') {
                navTitle.textContent = 'Monthly Listening Journey';
                backBtn.style.display = 'none';
                chartHint.style.display = 'flex';
                chartUnits.textContent = 'Total Plays';
            } else if (chartView === 'day') {
                const monthNames = ['January', 'February', 'March', 'April', 'May', 'June', 
                                 'July', 'August', 'September', 'October', 'November', 'December'];
                navTitle.textContent = monthNames[chartState.selectedMonth] + ' Daily Activity';
                backBtn.style.display = 'flex';
                chartHint.style.display = 'none'; // Remove hint for further zoom
                chartUnits.textContent = 'Plays per Day';
            }
        }

        function drawChart() {
            if (!canvas) return;
            const width = canvas.logicalWidth || canvas.offsetWidth;
            const height = canvas.logicalHeight || canvas.offsetHeight;
            ctx.clearRect(0, 0, width, height);
            ctx.save();
            
            // Subtle cosmic background on canvas
            const gradient = ctx.createRadialGradient(width/2, height/2, 0, width/2, height/2, width/2);
            gradient.addColorStop(0, 'rgba(79, 70, 229, 0.05)');
            gradient.addColorStop(1, 'rgba(15, 16, 33, 0.1)');
            ctx.fillStyle = gradient;
            ctx.fillRect(0, 0, width, height);
            
            // Chart logic with smooth transitions
            const easedProgress = easeInOutCubic(chartTransition.progress);
            
            if (chartView === 'month') {
                const months = getMonthlyTotals();
                drawSmoothCurve(ctx, months, width, height, 'month', easedProgress);
                drawMonthLabels(ctx, width, height, easedProgress);
            } else if (chartView === 'day') {
                const days = getDailyTotals(chartState.selectedMonth);
                drawSmoothCurve(ctx, days.map(d => d.plays), width, height, 'day', easedProgress);
                drawDayLabels(ctx, days, width, height, easedProgress);
            }
            
            ctx.restore();
        }

        function drawSmoothCurve(ctx, dataArr, width, height, mode, progress = 1) {
            const padding = 80;
            const chartW = width - 2 * padding;
            const chartH = height - 2 * padding;
            const maxVal = Math.max(...dataArr, 1);
            
            // Y-axis scale labels
            ctx.save();
            ctx.font = '12px -apple-system, BlinkMacSystemFont, sans-serif';
            ctx.fillStyle = 'rgba(255, 255, 255, 0.7)';
            ctx.textAlign = 'right';
            ctx.textBaseline = 'middle';
            
            for (let i = 0; i <= 5; i++) {
                const value = Math.round((maxVal * i) / 5);
                const y = padding + chartH - (chartH * i / 5);
                ctx.fillText(value.toString(), padding - 15, y);
                
                // Grid lines
                ctx.save();
                ctx.strokeStyle = 'rgba(255, 255, 255, 0.1)';
                ctx.lineWidth = 1;
                ctx.beginPath();
                ctx.moveTo(padding, y);
                ctx.lineTo(padding + chartW, y);
                ctx.stroke();
                ctx.restore();
            }
            ctx.restore();
            
            // Main curve
            ctx.save();
            
            // Gradient stroke
            const gradient = ctx.createLinearGradient(0, padding, 0, padding + chartH);
            gradient.addColorStop(0, '#8B5CF6');
            gradient.addColorStop(0.5, '#A855F7');
            gradient.addColorStop(1, '#EC4899');
            
            ctx.strokeStyle = gradient;
            ctx.lineWidth = 3;
            ctx.lineCap = 'round';
            ctx.lineJoin = 'round';
            ctx.shadowColor = '#A855F7';
            ctx.shadowBlur = 15;
            
            ctx.beginPath();
            
            let points = [];
            for (let i = 0; i < dataArr.length; i++) {
                const x = padding + (chartW * i / Math.max(dataArr.length - 1, 1));
                const y = padding + chartH - (chartH * dataArr[i] / maxVal);
                points.push({x, y, value: dataArr[i]});
            }
            
            if (points.length > 1) {
                ctx.moveTo(points[0].x, points[0].y);
                
                for (let i = 1; i < points.length; i++) {
                    const prevPoint = points[i - 1];
                    const currentPoint = points[i];
                    
                    const cpx1 = prevPoint.x + (currentPoint.x - prevPoint.x) * 0.3;
                    const cpy1 = prevPoint.y;
                    const cpx2 = currentPoint.x - (currentPoint.x - prevPoint.x) * 0.3;
                    const cpy2 = currentPoint.y;
                    
                    ctx.bezierCurveTo(cpx1, cpy1, cpx2, cpy2, currentPoint.x, currentPoint.y);
                }
            }
            
            ctx.stroke();
            
            // Glow points
            ctx.shadowBlur = 8;
            points.forEach(point => {
                if (point.value > 0) {
                    ctx.beginPath();
                    ctx.arc(point.x, point.y, 4, 0, 2 * Math.PI);
                    ctx.fillStyle = '#FFFFFF';
                    ctx.fill();
                }
            });
            
            ctx.restore();
        }

        const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        
        function drawMonthLabels(ctx, width, height, progress = 1) {
            const padding = 80;
            const chartW = width - 2 * padding;
            ctx.save();
            ctx.font = '600 14px -apple-system, BlinkMacSystemFont, sans-serif';
            ctx.fillStyle = `rgba(255, 255, 255, ${0.8 * progress})`;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'top';
            
            for (let i = 0; i < 12; i++) {
                const x = padding + (chartW * i / 11);
                ctx.fillText(monthNames[i], x, height - 30);
            }
            ctx.restore();
        }
        
        function drawDayLabels(ctx, days, width, height, progress = 1) {
            const padding = 80;
            const chartW = width - 2 * padding;
            ctx.save();
            ctx.font = '600 12px -apple-system, BlinkMacSystemFont, sans-serif';
            ctx.fillStyle = `rgba(255, 255, 255, ${0.8 * progress})`;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'top';
            
            const step = Math.max(1, Math.ceil(days.length / 8));
            for (let i = 0; i < days.length; i += step) {
                const x = padding + (chartW * i / Math.max(days.length - 1, 1));
                ctx.fillText(days[i].date.getDate().toString(), x, height - 30);
            }
            ctx.restore();
        }
        
        function drawHourLabels(ctx, width, height, progress = 1) {
            const padding = 80;
            const chartW = width - 2 * padding;
            ctx.save();
            ctx.font = '600 12px -apple-system, BlinkMacSystemFont, sans-serif';
            ctx.fillStyle = `rgba(255, 255, 255, ${0.8 * progress})`;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'top';
            
            for (let i = 0; i < 24; i += 3) {
                const x = padding + (chartW * i / 23);
                const label = i === 0 ? '12 AM' : i === 12 ? '12 PM' : i < 12 ? i + ' AM' : (i - 12) + ' PM';
                ctx.fillText(label, x, height - 30);
            }
            ctx.restore();
        }

        canvas.addEventListener('click', function(e) {
            if (chartTransition.animating) return;
            const rect = canvas.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const padding = 80;
            const chartW = rect.width - 2 * padding;
            if (chartView === 'month') {
                const monthIdx = Math.round((x - padding) / (chartW / 11));
                if (monthIdx >= 0 && monthIdx < 12) {
                    chartView = 'day';
                    chartState.selectedMonth = monthIdx;
                    updateUI();
                    animateTransition(drawChart);
                }
            }
            // Remove day-to-hour zoom
        });

        backBtn.addEventListener('click', function() {
            if (chartTransition.animating) return;
            
            if (chartView === 'hour') {
                chartView = 'day';
            } else if (chartView === 'day') {
                chartView = 'month';
            }
            updateUI();
            animateTransition(drawChart);
        });

        window.addEventListener('resize', function() {
            setupHighDPICanvas();
            // Small delay to ensure container has resized
            setTimeout(() => {
                drawChart();
            }, 100);
        });

        // Initialize
        setupHighDPICanvas();
        updateUI();
        animateTransition(drawChart);
        ";
        }

        public override string GenerateCSS()
        {
            return @"
        .cosmic-chart-bg {
            position: absolute;
            width: 100%;
            height: 100%;
            z-index: 0;
            pointer-events: none;
            overflow: hidden;
        }

        .starfield {
            position: absolute;
            width: 100%;
            height: 100%;
            background: 
                radial-gradient(2px 2px at 20% 30%, #ffffff, transparent),
                radial-gradient(2px 2px at 40% 70%, rgba(255,255,255,0.8), transparent),
                radial-gradient(1px 1px at 90% 40%, rgba(255,255,255,0.6), transparent),
                radial-gradient(1px 1px at 60% 10%, rgba(255,255,255,0.4), transparent),
                radial-gradient(2px 2px at 10% 80%, rgba(255,255,255,0.7), transparent),
                radial-gradient(ellipse at center, #0f0f23 0%, #000000 100%);
            background-size: 550px 550px, 350px 350px, 250px 250px, 150px 150px, 400px 400px, 100% 100%;
            animation: starfield 120s linear infinite;
        }

        @keyframes starfield {
            0% { transform: translateY(0) rotate(0deg); }
            100% { transform: translateY(-100px) rotate(360deg); }
        }

        .nebula-purple {
            position: absolute;
            width: 100%;
            height: 100%;
            background: radial-gradient(ellipse at 30% 20%, rgba(139, 92, 246, 0.4) 0%, transparent 60%);
            animation: nebulaDrift 60s ease-in-out infinite;
        }

        .nebula-blue {
            position: absolute;
            width: 100%;
            height: 100%;
            background: radial-gradient(ellipse at 70% 80%, rgba(79, 70, 229, 0.3) 0%, transparent 70%);
            animation: nebulaDrift 80s ease-in-out infinite reverse;
        }

        @keyframes nebulaDrift {
            0%, 100% { transform: translateX(0) scale(1); }
            50% { transform: translateX(20px) scale(1.1); }
        }

        .orbit-rings {
            position: absolute;
            width: 100%;
            height: 100%;
            background: 
                repeating-radial-gradient(circle at 80% 30%, rgba(168, 85, 247, 0.1) 0px, transparent 60px),
                repeating-radial-gradient(circle at 20% 70%, rgba(79, 70, 229, 0.08) 0px, transparent 80px);
            animation: orbitRotation 100s linear infinite;
        }

        @keyframes orbitRotation {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        .cosmic-planet {
            position: absolute;
            border-radius: 50%;
            box-shadow: 0 0 40px rgba(168, 85, 247, 0.6);
        }

        .cosmic-planet.primary {
            left: 8%;
            top: 15%;
            width: 100px;
            height: 100px;
            background: radial-gradient(circle at 30% 30%, #A855F7 0%, #4C1D95 80%);
            animation: planetFloat 20s ease-in-out infinite;
        }

        .cosmic-planet.secondary {
            right: 12%;
            bottom: 20%;
            width: 60px;
            height: 60px;
            background: radial-gradient(circle at 40% 40%, #EC4899 0%, #7C2D92 80%);
            animation: planetFloat 15s ease-in-out infinite reverse;
        }

        @keyframes planetFloat {
            0%, 100% { transform: translateY(0) rotate(0deg); }
            50% { transform: translateY(-10px) rotate(180deg); }
        }

        .cosmic-comet {
            position: absolute;
            right: 20%;
            top: 10%;
            width: 3px;
            height: 3px;
            background: #ffffff;
            border-radius: 50%;
            box-shadow: 
                0 0 10px #ffffff,
                -20px 0 8px rgba(255, 255, 255, 0.6),
                -40px 0 6px rgba(255, 255, 255, 0.4),
                -60px 0 4px rgba(255, 255, 255, 0.2);
            animation: cometTrail 8s ease-in-out infinite;
        }

        @keyframes cometTrail {
            0% { transform: translateX(100px) translateY(-20px) rotate(45deg); opacity: 0; }
            10% { opacity: 1; }
            90% { opacity: 1; }
            100% { transform: translateX(-100px) translateY(20px) rotate(45deg); opacity: 0; }
        }

        .cosmic-asteroids {
            position: absolute;
            left: 60%;
            top: 60%;
            width: 2px;
            height: 2px;
            background: rgba(255, 255, 255, 0.8);
            border-radius: 50%;
            box-shadow: 
                10px 15px 0 1px rgba(255, 255, 255, 0.6),
                -15px 10px 0 0px rgba(255, 255, 255, 0.4),
                25px -10px 0 1px rgba(255, 255, 255, 0.7),
                -10px -15px 0 0px rgba(255, 255, 255, 0.5);
            animation: asteroidDrift 25s linear infinite;
        }

        @keyframes asteroidDrift {
            0% { transform: translateX(-20px) rotate(0deg); }
            100% { transform: translateX(20px) rotate(360deg); }
        }

        .shooting-stars {
            position: absolute;
            width: 100%;
            height: 100%;
            background: 
                radial-gradient(1px 1px at 10% 20%, rgba(255,255,255,0.8), transparent),
                radial-gradient(1px 1px at 80% 50%, rgba(255,255,255,0.6), transparent),
                radial-gradient(1px 1px at 50% 90%, rgba(255,255,255,0.4), transparent);
            background-size: 100% 100%;
            animation: shootingStars 3s ease-in-out infinite;
        }

        @keyframes shootingStars {
            0%, 90% { opacity: 0; transform: translateX(-100px); }
            10%, 80% { opacity: 1; transform: translateX(0); }
            100% { opacity: 0; transform: translateX(100px); }
        }

        .cosmic-chart-nav {
            position: relative;
            z-index: 2;
            display: flex;
            align-items: center;
            gap: 1.5rem;
            margin-bottom: 2rem;
        }

        .zoom-out-btn {
            display: none;
            align-items: center;
            gap: 0.5rem;
            background: linear-gradient(135deg, rgba(139, 92, 246, 0.2) 0%, rgba(79, 70, 229, 0.2) 100%);
            border: 1px solid rgba(168, 85, 247, 0.3);
            border-radius: 12px;
            padding: 0.75rem 1.5rem;
            color: #ffffff;
            font-size: 0.95rem;
            font-weight: 600;
            font-family: -apple-system, BlinkMacSystemFont, sans-serif;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            backdrop-filter: blur(8px);
        }

        .zoom-out-btn:hover {
            background: linear-gradient(135deg, rgba(168, 85, 247, 0.3) 0%, rgba(79, 70, 229, 0.3) 100%);
            border-color: rgba(168, 85, 247, 0.5);
            transform: translateY(-1px);
            box-shadow: 0 8px 25px rgba(168, 85, 247, 0.2);
        }

        .zoom-icon {
            font-size: 1.2rem;
            font-weight: bold;
        }

        #chart-nav-title {
            font-size: 1.5rem;
            font-weight: 700;
            background: linear-gradient(135deg, #A855F7 0%, #EC4899 50%, #8B5CF6 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            font-family: -apple-system, BlinkMacSystemFont, sans-serif;
            text-shadow: 0 0 30px rgba(168, 85, 247, 0.6);
            letter-spacing: -0.025em;
            position: relative;
            filter: drop-shadow(0 0 10px rgba(168, 85, 247, 0.4));
        }

        .chart-container {
            position: relative;
            width: 100%;
            max-width: 1200px;
            height: 500px;
            margin: 0 auto;
            background: linear-gradient(135deg, rgba(15, 16, 33, 0.4) 0%, rgba(30, 27, 75, 0.2) 100%);
            border-radius: 20px;
            padding: 2rem;
            box-shadow: 
                0 20px 40px rgba(0, 0, 0, 0.3),
                inset 0 1px 0 rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            z-index: 1;
        }

        .chart-units {
            position: absolute;
            top: 1rem;
            left: 1rem;
            color: rgba(255, 255, 255, 0.7);
            font-size: 0.9rem;
            font-weight: 500;
            font-family: -apple-system, BlinkMacSystemFont, sans-serif;
            z-index: 2;
        }

        .chart-container canvas {
            width: 100%;
            height: 100%;
            border-radius: 15px;
            cursor: pointer;
            transition: transform 0.2s ease;
        }

        .chart-container canvas:hover {
            transform: scale(1.005);
        }

        .chart-interaction-hint {
            position: absolute;
            top: 1rem;
            right: 1rem;
            display: flex;
            align-items: center;
            gap: 0.5rem;
            z-index: 10;
            pointer-events: none;
            animation: hintPulse 2s ease-in-out infinite;
        }

        .hint-glow {
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: radial-gradient(circle, #A855F7 0%, transparent 70%);
            box-shadow: 
                0 0 10px #A855F7,
                0 0 20px #A855F7,
                0 0 30px #A855F7;
        }

        .hint-text {
            color: rgba(255, 255, 255, 0.8);
            font-size: 0.9rem;
            font-weight: 500;
            font-family: -apple-system, BlinkMacSystemFont, sans-serif;
        }

        @keyframes hintPulse {
            0%, 100% { opacity: 0.6; transform: scale(1); }
            50% { opacity: 1; transform: scale(1.05); }
        }

        /* Responsive design */
        @media (max-width: 768px) {
            .chart-container {
                height: 400px;
                padding: 1rem;
            }
            
            #chart-nav-title {
                font-size: 1.2rem;
            }
            
            .zoom-out-btn {
                padding: 0.5rem 1rem;
                font-size: 0.85rem;
            }
        }
        ";
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var dailyPlays = stats.DailyPlayCounts ?? new Dictionary<string, int>();
            if (!dailyPlays.Any())
                return "Start listening daily to see your listening patterns!";

            var activeDays = dailyPlays.Count;
            var averagePlaysPerDay = dailyPlays.Values.Average();
            
            return $"You were active on {activeDays} days in {year}, averaging {averagePlaysPerDay:F1} plays per active day.";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            var dailyPlays = stats.DailyPlayCounts ?? new Dictionary<string, int>();
            return dailyPlays.Any() && stats.TotalTracks > 0;
        }
    }
}