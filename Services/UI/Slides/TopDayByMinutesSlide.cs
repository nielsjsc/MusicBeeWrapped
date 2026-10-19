using System;
using System.Collections.Generic;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide 2: Peak Day & Listening Rhythm (Warm Sand stock).
    /// Uses 2 intra-slide steps so pressing the Right Arrow transitions cleanly from
    /// Step 1 (Biggest Listening Day) to Step 2 (Time of Day & Day of Week).
    /// </summary>
    public class TopDayByMinutesSlide : SlideComponentBase
    {
        public override string SlideId => "listening-clock";
        public override string SlideTitle => "Peak Day";
        public override int SlideOrder => 2;
        public override string AccentTheme => "sand";

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) &&
                   stats.DailyListeningMinutes != null &&
                   stats.DailyListeningMinutes.Any();
        }

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = GetYearPlays(playHistory, year);

            // 1. Peak Day details + cover art
            var topDayEntry = stats.DailyListeningMinutes.OrderByDescending(kv => kv.Value).FirstOrDefault();
            DateTime.TryParse(topDayEntry.Key, out var topDate);
            var topDayMinutes = Math.Round(topDayEntry.Value);
            var topDayHours = topDayEntry.Value / 60.0;

            var topDayPlays = yearPlays.Where(p => p.PlayedAt.Date == topDate.Date).ToList();
            var topDayTrackCount = topDayPlays.Count;
            var topDayArtist = topDayPlays
                .Where(p => !string.IsNullOrEmpty(p.Artist))
                .GroupBy(p => p.Artist)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "Various Artists";

            var topDaySongGroup = topDayPlays
                .Where(p => !string.IsNullOrEmpty(p.Title))
                .GroupBy(p => $"{p.Title}|||{p.Artist}")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            var topDaySongPlay = topDaySongGroup?.FirstOrDefault() ?? topDayPlays.FirstOrDefault();
            var topDaySongTitle = topDaySongPlay != null ? EscapeHtml(topDaySongPlay.Title) : "—";
            var topDayCoverUri = Artwork.GetTrackArtworkDataUri(topDaySongPlay, 380);

            // 2. 24-Hour Distribution
            var hourlyCounts = new int[24];
            for (int h = 0; h < 24; h++)
            {
                int c;
                if (stats.ListeningByHour != null && stats.ListeningByHour.TryGetValue(h, out c))
                    hourlyCounts[h] = c;
            }
            int maxHourCount = Math.Max(1, hourlyCounts.Max());
            int peakHour = Array.IndexOf(hourlyCounts, hourlyCounts.Max());

            var hourBarsHtml = string.Join("", Enumerable.Range(0, 24).Select(h =>
            {
                int count = hourlyCounts[h];
                int pct = count > 0 ? Math.Max(6, (int)Math.Round(count * 100.0 / maxHourCount)) : 2;
                bool isPeak = h == peakHour && count > 0;
                string label = h % 3 == 0 ? $"{h:00}" : "";
                return $@"
                    <div class='vu-col {(isPeak ? "is-peak" : "")}' title='{FormatHourLabel(h)}: {count:N0} plays'>
                        <div class='vu-track'>
                            <div class='vu-fill' style='height:{pct}%'></div>
                        </div>
                        <span class='vu-lbl'>{label}</span>
                    </div>";
            }));

            // 3. Day-of-Week Distribution (Mon -> Sun)
            var orderedDays = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
            };
            int maxDowPlays = Math.Max(1, orderedDays.Max(d =>
            {
                int v;
                return stats.ListeningByDay != null && stats.ListeningByDay.TryGetValue(d, out v) ? v : 0;
            }));
            var peakDow = orderedDays.OrderByDescending(d =>
            {
                int v;
                return stats.ListeningByDay != null && stats.ListeningByDay.TryGetValue(d, out v) ? v : 0;
            }).First();

            var dowRowsHtml = string.Join("", orderedDays.Select(d =>
            {
                int v;
                int c = stats.ListeningByDay != null && stats.ListeningByDay.TryGetValue(d, out v) ? v : 0;
                int pct = c > 0 ? Math.Max(4, (int)Math.Round(c * 100.0 / maxDowPlays)) : 2;
                bool isPeak = d == peakDow && c > 0;
                return $@"
                    <div class='cadence-row {(isPeak ? "is-peak" : "")}'>
                        <span class='cadence-day'>{d.ToString().Substring(0, 3).ToUpperInvariant()}</span>
                        <div class='cadence-bar-track'>
                            <div class='cadence-bar-fill' style='width:{pct}%'></div>
                        </div>
                        <span class='cadence-val'>{c:N0}</span>
                    </div>";
            }));

            var content = $@"
                <header class='liner-header'>
                    <div>
                        <div class='liner-kicker'>LISTENING HABITS</div>
                        <h1 class='liner-title'>When You Listened</h1>
                    </div>
                    <div class='step-tabs'>
                        <button type='button' class='step-tab-btn active' data-goto-step='0'>01 · Peak Day</button>
                        <button type='button' class='step-tab-btn' data-goto-step='1'>02 · By Hour &amp; Day</button>
                    </div>
                </header>

                <!-- Step 1: Biggest Single Day Spotlight -->
                <div class='slide-step active' data-step-title='Peak Day'>
                    <div class='peak-day-spotlight'>
                        {RenderCoverArt(topDayCoverUri, topDaySongPlay?.Title ?? topDayArtist, topDayArtist, "sleeve-frame peak-spotlight-sleeve")}
                        <div class='peak-spotlight-body'>
                            <div class='mono-label'>{topDate:dddd, yyyy} · BIGGEST DAY</div>
                            <div class='peak-spotlight-date'>{topDate:MMMM d}</div>

                            <div class='peak-spotlight-stats'>
                                <div class='peak-stat-box'>
                                    <span class='mono-label'>MINUTES</span>
                                    <div class='peak-stat-val'>{topDayMinutes:N0}</div>
                                </div>
                                <div class='peak-stat-box'>
                                    <span class='mono-label'>HOURS</span>
                                    <div class='peak-stat-val'>{topDayHours:F1}</div>
                                </div>
                                <div class='peak-stat-box'>
                                    <span class='mono-label'>PLAYS</span>
                                    <div class='peak-stat-val'>{topDayTrackCount}</div>
                                </div>
                            </div>

                            <div class='peak-spotlight-top'>
                                <div><span class='mono-label'>TOP ARTIST</span> <strong>{EscapeHtml(topDayArtist)}</strong></div>
                                <div><span class='mono-label'>TOP TRACK</span> <span>{topDaySongTitle}</span></div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Step 2: 24-Hour Clock & Day of Week -->
                <div class='slide-step' data-step-title='Listening Clock'>
                    <div class='clock-step-grid'>
                        <div class='vu-meter-sheet'>
                            <div class='sheet-stamp-row'>
                                <span class='mono-label'>BY HOUR (00:00 — 23:00)</span>
                                <span class='sheet-cat'>PEAK: {FormatHourLabel(peakHour)}</span>
                            </div>
                            <div class='vu-chart-box'>
                                {hourBarsHtml}
                            </div>
                        </div>

                        <div class='cadence-sheet'>
                            <div class='sheet-stamp-row'>
                                <span class='mono-label'>BY DAY OF WEEK</span>
                                <span class='sheet-cat'>PEAK: {peakDow.ToString().ToUpperInvariant()}</span>
                            </div>
                            <div class='cadence-table'>
                                {dowRowsHtml}
                            </div>
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        private string FormatHourLabel(int hour)
        {
            if (hour == 0) return "12 AM";
            if (hour < 12) return $"{hour} AM";
            if (hour == 12) return "12 PM";
            return $"{hour - 12} PM";
        }

        public override string GenerateCSS()
        {
            return @"
        .peak-day-spotlight {
            display: grid;
            grid-template-columns: 280px 1fr;
            gap: 44px;
            align-items: center;
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            padding: 36px 40px;
            margin-top: 8px;
        }

        .peak-spotlight-sleeve {
            width: 280px;
            height: 280px;
        }

        .peak-spotlight-body {
            display: flex;
            flex-direction: column;
            gap: 20px;
        }

        .peak-spotlight-date {
            font-family: var(--font-serif);
            font-size: clamp(3.4rem, 5.5vw, 4.8rem);
            line-height: 0.9;
            color: var(--ink);
            margin-top: -8px;
        }

        .peak-spotlight-stats {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            border-top: 1px solid var(--rule-strong);
            border-bottom: 1px solid var(--rule-strong);
            background: var(--bg-paper);
        }

        .peak-stat-box {
            padding: 16px 20px;
            border-right: 1px solid var(--rule);
        }

        .peak-stat-box:last-child {
            border-right: none;
        }

        .peak-stat-val {
            font-family: var(--font-serif);
            font-size: 2.6rem;
            line-height: 1;
            color: var(--ink);
            margin-top: 4px;
        }

        .peak-spotlight-top {
            display: flex;
            gap: 32px;
            flex-wrap: wrap;
            font-size: 0.95rem;
            color: var(--ink-secondary);
        }

        .peak-spotlight-top > div {
            display: flex;
            align-items: baseline;
            gap: 10px;
        }

        .peak-spotlight-top strong {
            color: var(--ink);
            font-weight: 600;
        }

        /* Step 2: Clock & Weekly Grid */
        .clock-step-grid {
            display: grid;
            grid-template-columns: 1.3fr 0.9fr;
            gap: 24px;
            margin-top: 8px;
        }

        .cadence-sheet,
        .vu-meter-sheet {
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            padding: 24px 26px;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
        }

        .sheet-stamp-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-bottom: 12px;
            border-bottom: 1px solid var(--rule);
            margin-bottom: 18px;
        }

        .sheet-cat {
            font-family: var(--font-mono);
            font-size: 0.68rem;
            font-weight: 600;
            letter-spacing: 0.1em;
            color: var(--accent);
        }

        .cadence-table {
            display: flex;
            flex-direction: column;
            gap: 14px;
            justify-content: center;
            flex: 1;
        }

        .cadence-row {
            display: grid;
            grid-template-columns: 42px 1fr 48px;
            align-items: center;
            gap: 12px;
            font-family: var(--font-mono);
            font-size: 0.76rem;
        }

        .cadence-day {
            color: var(--ink-secondary);
            font-weight: 500;
        }

        .cadence-bar-track {
            height: 8px;
            background: var(--bg-subtle);
            border: 1px solid var(--rule);
        }

        .cadence-bar-fill {
            height: 100%;
            background: var(--ink-secondary);
        }

        .cadence-row.is-peak .cadence-day,
        .cadence-row.is-peak .cadence-val {
            color: var(--accent);
            font-weight: 600;
        }

        .cadence-row.is-peak .cadence-bar-fill {
            background: var(--accent);
        }

        .cadence-val {
            text-align: right;
            color: var(--ink);
        }

        .vu-chart-box {
            display: grid;
            grid-template-columns: repeat(24, 1fr);
            gap: 6px;
            height: 240px;
            padding: 16px 8px 6px;
            background:
                repeating-linear-gradient(
                    to bottom,
                    transparent 0,
                    transparent 45px,
                    rgba(24, 22, 20, 0.06) 45px,
                    rgba(24, 22, 20, 0.06) 46px
                );
            border-bottom: 1px solid var(--rule-strong);
        }

        .vu-col {
            display: flex;
            flex-direction: column;
            align-items: center;
            height: 100%;
        }

        .vu-track {
            flex: 1;
            width: 100%;
            display: flex;
            align-items: flex-end;
            justify-content: center;
        }

        .vu-fill {
            width: 100%;
            max-width: 14px;
            background: var(--ink);
            opacity: 0.78;
        }

        .vu-col.is-peak .vu-fill {
            background: var(--accent);
            opacity: 1;
        }

        .vu-lbl {
            height: 16px;
            margin-top: 6px;
            font-family: var(--font-mono);
            font-size: 0.63rem;
            color: var(--ink-muted);
        }

        @media (max-width: 960px) {
            .peak-day-spotlight,
            .clock-step-grid {
                grid-template-columns: 1fr;
            }
        }
";
        }
    }
}
