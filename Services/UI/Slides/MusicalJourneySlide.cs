using System;
using System.Collections.Generic;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Musical Journey slide component - Shows timeline of first play to last play with peak month
    /// Displays the user's musical evolution throughout the year
    /// </summary>
    public class MusicalJourneySlide : SlideComponentBase
    {
        public override string SlideId => "musical-journey";
        public override string SlideTitle => "Your Musical Journey";
        public override int SlideOrder => 6;

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = playHistory.GetPlaysByYear(year).OrderBy(p => p.PlayedAt).ToList();
            var firstPlay = yearPlays.FirstOrDefault();
            var lastPlay = yearPlays.LastOrDefault();
            
            var monthlyData = stats.MonthlyListeningHours ?? new Dictionary<string, double>();
            var peakMonth = monthlyData.Any() ? monthlyData.OrderByDescending(x => x.Value).First().Key : "January";
            var quietMonth = monthlyData.Any() ? monthlyData.OrderBy(x => x.Value).First().Key : "January";
            
            var content = $@"
                <div class='journey-slide-container'>
                    <div class='journey-header'>
                        <h2 class='journey-title'>🚀 Your Musical Journey Through {year}</h2>
                        <p class='journey-subtitle'>From your first song to your latest discovery</p>
                    </div>
                    
                    <div class='journey-timeline-horizontal'>
                        {(firstPlay != null ? $@"
                            <div class='timeline-milestone start-milestone'>
                                <div class='milestone-marker'>
                                    <div class='milestone-icon'>🎬</div>
                                </div>
                                <div class='milestone-card'>
                                    <div class='milestone-date'>{firstPlay.PlayedAt:MMMM} {firstPlay.PlayedAt.Day}</div>
                                    <h4>Journey Began</h4>
                                    <div class='milestone-song'>
                                        <div class='song-name'>{EscapeHtml(firstPlay.Title)}</div>
                                        <div class='song-artist'>by {EscapeHtml(firstPlay.Artist)}</div>
                                    </div>
                                </div>
                            </div>
                        " : "")}
                        
                        <div class='timeline-connector'>
                            <div class='connector-line'></div>
                        </div>
                        
                        <div class='timeline-milestone peak-milestone'>
                            <div class='milestone-marker peak-marker'>
                                <div class='milestone-icon'>🔥</div>
                            </div>
                            <div class='milestone-card peak-card'>
                                <div class='milestone-date'>{FormatMonth(peakMonth)} {year}</div>
                                <h4>Peak Period</h4>
                                <div class='peak-info'>
                                    <div class='peak-value'>{Math.Round(monthlyData.ContainsKey(peakMonth) ? monthlyData[peakMonth] : 0)} hours</div>
                                    <div class='peak-desc'>Most active month</div>
                                </div>
                            </div>
                        </div>
                        
                        <div class='timeline-connector'>
                            <div class='connector-line'></div>
                        </div>
                        
                        {(lastPlay != null ? $@"
                            <div class='timeline-milestone end-milestone'>
                                <div class='milestone-marker'>
                                    <div class='milestone-icon'>🌟</div>
                                </div>
                                <div class='milestone-card'>
                                    <div class='milestone-date'>{lastPlay.PlayedAt:MMMM} {lastPlay.PlayedAt.Day}</div>
                                    <h4>Latest Discovery</h4>
                                    <div class='milestone-song'>
                                        <div class='song-name'>{EscapeHtml(lastPlay.Title)}</div>
                                        <div class='song-artist'>by {EscapeHtml(lastPlay.Artist)}</div>
                                    </div>
                                </div>
                            </div>
                        " : "")}
                    </div>
                    
                    <div class='journey-insights'>
                        <div class='insight-card'>
                            <span class='insight-icon'>🎯</span>
                            <span class='insight-message'>Your musical taste evolved beautifully from {FormatMonth(peakMonth)} peaks to {FormatMonth(quietMonth)} quiet moments</span>
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        private string FormatMonth(string monthStr)
        {
            var monthNames = new Dictionary<string, string>
            {
                {"01", "January"}, {"02", "February"}, {"03", "March"}, {"04", "April"},
                {"05", "May"}, {"06", "June"}, {"07", "July"}, {"08", "August"},
                {"09", "September"}, {"10", "October"}, {"11", "November"}, {"12", "December"}
            };
            
            if (monthStr != null && monthStr.Contains("-"))
            {
                var monthNum = monthStr.Split('-')[1];
                return monthNames.ContainsKey(monthNum) ? monthNames[monthNum] : monthStr;
            }
            return monthStr ?? "January";
        }

        public override string GetInsightText(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = playHistory.GetPlaysByYear(year).OrderBy(p => p.PlayedAt).ToList();
            if (!yearPlays.Any())
                return "Your musical journey is waiting to begin!";

            var firstPlay = yearPlays.First();
            var lastPlay = yearPlays.Last();
            var daysDiff = (lastPlay.PlayedAt - firstPlay.PlayedAt).Days;
            
            return $"Your musical journey spanned {daysDiff} days, from {firstPlay.PlayedAt:MMMM} to {lastPlay.PlayedAt:MMMM}.";
        }

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return playHistory.Plays.Any();
        }
    }
}
