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
            var dailyPlays = stats.DailyPlayCounts ?? new Dictionary<string, int>();
            var peakDay = dailyPlays.Any() ? dailyPlays.Values.Max() : 0;
            var activeDays = dailyPlays.Count;
            var weekendRatio = stats.WeekendVsWeekdayRatio;

            var content = $@"
                <h2>📅 Your Daily Listening Journey</h2>
                <p style='margin-bottom: 2rem; opacity: 0.8;'>Track plays each day throughout {year}</p>
                <div class='chart-container'>
                    <canvas id='dailyChart' width='1200' height='500'></canvas>
                </div>
                <div class='chart-stats'>
                    <div class='chart-stat'>
                        <div class='stat-number'>{peakDay}</div>
                        <div class='stat-label'>Peak Day (Plays)</div>
                    </div>
                    <div class='chart-stat'>
                        <div class='stat-number'>{weekendRatio:F1}x</div>
                        <div class='stat-label'>Weekend vs Weekday</div>
                    </div>
                    <div class='chart-stat'>
                        <div class='stat-number'>{activeDays}</div>
                        <div class='stat-label'>Active Days</div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        function drawDailyChart() {
            const canvas = document.getElementById('dailyChart');
            if (!canvas) return;
            
            const ctx = canvas.getContext('2d');
            const data = window.WRAPPED_DATA;
            const dailyPlays = data.dailyPlays || {};
            
            // Set canvas size with high DPI support
            const rect = canvas.getBoundingClientRect();
            const dpr = window.devicePixelRatio || 1;
            canvas.width = rect.width * dpr;
            canvas.height = rect.height * dpr;
            ctx.scale(dpr, dpr);
            
            const width = rect.width;
            const height = rect.height;
            const padding = { top: 40, right: 40, bottom: 80, left: 80 };
            
            // Create full year of dates for the chart
            const year = data.year;
            const startDate = new Date(year, 0, 1);
            const endDate = new Date(year, 11, 31);
            const allDates = [];
            
            // Generate all dates in the year
            for (let d = new Date(startDate); d <= endDate; d.setDate(d.getDate() + 1)) {
                allDates.push(new Date(d));
            }
            
            // Create data array with play counts for each day
            const chartData = allDates.map(date => {
                const dateStr = date.toISOString().split('T')[0];
                return {
                    date: date,
                    dateStr: dateStr,
                    plays: dailyPlays[dateStr] || 0,
                    month: date.getMonth(),
                    dayOfYear: Math.floor((date - startDate) / (24 * 60 * 60 * 1000))
                };
            });
            
            const maxPlays = Math.max(...chartData.map(d => d.plays), 1);
            const chartWidth = width - padding.left - padding.right;
            const chartHeight = height - padding.top - padding.bottom;
            const barWidth = Math.max(1, chartWidth / chartData.length);
            
            // Clear canvas
            ctx.clearRect(0, 0, width, height);
            
            // Set up styling
            ctx.fillStyle = '#ffffff';
            ctx.strokeStyle = '#ffffff';
            ctx.font = '12px sans-serif';
            
            // Draw background grid lines
            ctx.strokeStyle = 'rgba(255, 255, 255, 0.1)';
            ctx.lineWidth = 1;
            
            // Horizontal grid lines
            const gridLines = 5;
            for (let i = 0; i <= gridLines; i++) {
                const y = padding.top + (chartHeight * i / gridLines);
                ctx.beginPath();
                ctx.moveTo(padding.left, y);
                ctx.lineTo(width - padding.right, y);
                ctx.stroke();
            }
            
            // Draw month separators and labels
            ctx.strokeStyle = 'rgba(255, 255, 255, 0.3)';
            ctx.lineWidth = 1;
            ctx.fillStyle = 'rgba(255, 255, 255, 0.8)';
            ctx.textAlign = 'center';
            
            const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 
                               'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
            let lastMonth = -1;
            
            chartData.forEach((dataPoint, index) => {
                const x = padding.left + (index * barWidth);
                
                if (dataPoint.month !== lastMonth) {
                    ctx.beginPath();
                    ctx.moveTo(x, padding.top);
                    ctx.lineTo(x, height - padding.bottom);
                    ctx.stroke();
                    
                    const monthLabelX = x + (barWidth * 15);
                    ctx.fillText(monthNames[dataPoint.month], monthLabelX, height - padding.bottom + 25);
                    
                    lastMonth = dataPoint.month;
                }
            });
            
            // Draw bars
            chartData.forEach((dataPoint, index) => {
                const x = padding.left + (index * barWidth);
                const barHeight = (dataPoint.plays / maxPlays) * chartHeight;
                const y = height - padding.bottom - barHeight;
                
                if (dataPoint.plays > 0) {
                    const gradient = ctx.createLinearGradient(0, y, 0, y + barHeight);
                    gradient.addColorStop(0, '#4ecdc4');
                    gradient.addColorStop(1, 'rgba(78, 205, 196, 0.6)');
                    
                    ctx.fillStyle = gradient;
                    ctx.fillRect(x, y, Math.max(1, barWidth - 0.5), barHeight);
                    
                    if (dataPoint.plays > maxPlays * 0.7) {
                        ctx.shadowColor = '#4ecdc4';
                        ctx.shadowBlur = 3;
                        ctx.fillRect(x, y, Math.max(1, barWidth - 0.5), barHeight);
                        ctx.shadowBlur = 0;
                    }
                }
            });
            
            // Draw Y-axis labels
            ctx.fillStyle = 'rgba(255, 255, 255, 0.8)';
            ctx.textAlign = 'right';
            ctx.textBaseline = 'middle';
            
            for (let i = 0; i <= gridLines; i++) {
                const y = padding.top + (chartHeight * i / gridLines);
                const value = Math.round(maxPlays * (gridLines - i) / gridLines);
                ctx.fillText(value.toString(), padding.left - 15, y);
            }
            
            // Draw axis labels
            ctx.fillStyle = 'rgba(255, 255, 255, 0.9)';
            ctx.font = 'bold 14px sans-serif';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            
            // Y-axis label
            ctx.save();
            ctx.translate(20, height / 2);
            ctx.rotate(-Math.PI / 2);
            ctx.fillText('Track Plays', 0, 0);
            ctx.restore();
            
            // X-axis label
            ctx.fillText('Days in ' + year, width / 2, height - 20);
            
            // Draw chart title
            ctx.font = 'bold 16px sans-serif';
            ctx.fillStyle = '#ffffff';
            ctx.fillText('Daily Listening Activity', width / 2, 25);
        }";
        }

        public override string GenerateCSS()
        {
            return @"
        /* Chart Styles for Daily Chart Slide */
        .chart-container {
            width: 100%;
            max-width: 1200px;
            height: 500px;
            margin: 2rem auto;
            position: relative;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 15px;
            padding: 1rem;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
        }

        .chart-container canvas {
            width: 100%;
            height: 100%;
            border-radius: 10px;
        }

        .chart-stats {
            display: flex;
            gap: 3rem;
            justify-content: center;
            margin-top: 2rem;
        }

        .chart-stat {
            text-align: center;
        }

        .chart-stat .stat-number {
            font-size: 2.5rem;
            font-weight: bold;
            color: #4ecdc4;
            margin-bottom: 0.5rem;
        }

        .chart-stat .stat-label {
            font-size: 1rem;
            opacity: 0.8;
            text-transform: uppercase;
            letter-spacing: 1px;
        }";
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
