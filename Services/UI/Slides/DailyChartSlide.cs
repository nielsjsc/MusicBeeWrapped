using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide 7: Timeline & Monthly Breakdown (Warm Newsprint stock).
    /// Uses 2 intra-slide steps on the Right Arrow key:
    /// Step 1: Spacious 365-Day Timeline Chart.
    /// Step 2: 12-Month Breakdown with each month's #1 artist and album cover.
    /// </summary>
    public class DailyChartSlide : SlideComponentBase
    {
        public override string SlideId => "daily-chart";
        public override string SlideTitle => "Timeline";
        public override int SlideOrder => 7;
        public override bool RequiresChartRendering => true;
        public override string AccentTheme => "newsprint";

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) &&
                   stats.DailyListeningMinutes != null &&
                   stats.DailyListeningMinutes.Any();
        }

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = GetYearPlays(playHistory, year);
            var activeDays = stats.DailyListeningMinutes.Count(kv => kv.Value > 0);

            var monthCards = new List<string>();
            double maxMonthHours = 1.0;
            var monthlyData = new List<(int Month, string Name, int Plays, double Hours, string TopArtist, string CoverUri)>();

            for (int m = 1; m <= 12; m++)
            {
                var mPlays = yearPlays.Where(p => p.PlayedAt.Month == m).ToList();
                int count = mPlays.Count;
                double hours = Math.Round(mPlays.Sum(p => GetActualSeconds(p)) / 3600.0, 1);
                if (hours > maxMonthHours) maxMonthHours = hours;

                var topArtistGroup = mPlays
                    .Where(p => !string.IsNullOrEmpty(p.Artist))
                    .GroupBy(p => p.Artist)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                string topArtist = topArtistGroup != null ? topArtistGroup.Key : "—";
                var samplePlay = topArtistGroup?.FirstOrDefault();
                string coverUri = samplePlay != null ? Artwork.GetTrackArtworkDataUri(samplePlay, 140) : null;

                string mName = new DateTime(year, m, 1).ToString("MMM", CultureInfo.InvariantCulture).ToUpperInvariant();
                monthlyData.Add((m, mName, count, hours, topArtist, coverUri));
            }

            var peakMonth = monthlyData.OrderByDescending(x => x.Hours).First();

            foreach (var m in monthlyData)
            {
                bool isPeak = m.Month == peakMonth.Month && m.Hours > 0;
                bool isEmpty = m.Plays == 0;
                int barPct = m.Hours > 0 ? Math.Max(6, (int)Math.Round(m.Hours * 100.0 / maxMonthHours)) : 0;

                string artHtml = !isEmpty
                    ? RenderCoverArt(m.CoverUri, m.TopArtist, m.Name, "sleeve-frame almanac-mini-sleeve")
                    : "<div class='almanac-empty-box'>—</div>";

                monthCards.Add($@"
                    <div class='almanac-cell {(isPeak ? "is-peak" : "")} {(isEmpty ? "is-empty" : "")}'>
                        <div class='almanac-cell-top'>
                            <span class='almanac-mname'>{m.Name}</span>
                            <span class='almanac-hrs'>{m.Hours:F1} hrs</span>
                        </div>
                        <div class='almanac-cell-body'>
                            {artHtml}
                            <div class='almanac-meta'>
                                <div class='almanac-artist' title='{EscapeHtml(m.TopArtist)}'>{EscapeHtml(m.TopArtist)}</div>
                                <div class='almanac-plays'>{m.Plays:N0} plays</div>
                            </div>
                        </div>
                        <div class='almanac-rail'><div class='almanac-ink' style='width:{barPct}%'></div></div>
                    </div>");
            }

            var content = $@"
                <header class='liner-header'>
                    <div>
                        <div class='liner-kicker'>ACROSS THE YEAR</div>
                        <h1 class='liner-title'>{year} Timeline</h1>
                    </div>
                    <div class='step-tabs'>
                        <button type='button' class='step-tab-btn active' data-goto-step='0'>01 · Daily Chart</button>
                        <button type='button' class='step-tab-btn' data-goto-step='1'>02 · By Month</button>
                    </div>
                </header>

                <!-- Step 1: Spacious Daily Ink Canvas -->
                <div class='slide-step active' data-step-title='Timeline'>
                    <div class='chronicle-sheet'>
                        <div class='chronicle-toolbar'>
                            <div class='chronicle-readout' id='daily-hover-readout'>
                                <strong>{stats.TotalHours:N1} hours</strong> across <strong>{activeDays} active days</strong> · Longest streak: <strong>{stats.LongestListeningStreak} days</strong>
                            </div>
                            <div class='chronicle-controls'>
                                <button type='button' class='ink-mode-btn active' data-chart-mode='minutes'>MINUTES</button>
                                <button type='button' class='ink-mode-btn' data-chart-mode='plays'>PLAYS</button>
                                <button type='button' class='ink-mode-btn' data-chart-mode='rolling'>7-DAY AVG</button>
                            </div>
                        </div>
                        <div class='chronicle-canvas-wrap'>
                            <canvas id='daily-chart-canvas'></canvas>
                        </div>
                    </div>
                </div>

                <!-- Step 2: 12-Month Grid -->
                <div class='slide-step' data-step-title='By Month'>
                    <div class='almanac-grid'>
                        {string.Join("\n", monthCards)}
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        (function() {
            let chartMode = 'minutes';
            let pointsCache = [];

            function buildSeries(year, mode) {
                const data = window.WRAPPED_DATA || {};
                const dailyMins = data.dailyMinutes || {};
                const dailyPlays = data.dailyPlays || {};
                const dailyDetails = data.dailyDetails || {};

                const start = new Date(year, 0, 1);
                const end = new Date(year, 11, 31);
                const raw = [];

                for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
                    const yyyy = d.getFullYear();
                    const mm = String(d.getMonth() + 1).padStart(2, '0');
                    const dd = String(d.getDate()).padStart(2, '0');
                    const key = `${yyyy}-${mm}-${dd}`;
                    const mins = Number(dailyMins[key] || 0);
                    const plays = Number(dailyPlays[key] || 0);
                    const detail = dailyDetails[key] || {};
                    raw.push({
                        key,
                        dateObj: new Date(d),
                        mins,
                        plays,
                        topArtist: detail.topArtist || '',
                        topTrack: detail.topTrack || ''
                    });
                }

                raw.forEach((pt, idx) => {
                    if (mode === 'plays') {
                        pt.value = pt.plays;
                    } else if (mode === 'rolling') {
                        let sum = 0, cnt = 0;
                        for (let k = Math.max(0, idx - 6); k <= idx; k++) {
                            sum += raw[k].mins;
                            cnt++;
                        }
                        pt.value = Math.round((sum / Math.max(1, cnt)) * 10) / 10;
                    } else {
                        pt.value = pt.mins;
                    }
                });

                return raw;
            }

            function renderDailyChronicle() {
                const canvas = document.getElementById('daily-chart-canvas');
                if (!canvas) return;
                const parent = canvas.parentElement;
                const rect = parent.getBoundingClientRect();
                if (rect.width <= 10) return;

                const dpr = window.devicePixelRatio || 1;
                const width = rect.width;
                const height = rect.height || 270;
                canvas.width = width * dpr;
                canvas.height = height * dpr;
                canvas.style.width = width + 'px';
                canvas.style.height = height + 'px';

                const ctx = canvas.getContext('2d');
                ctx.scale(dpr, dpr);
                ctx.clearRect(0, 0, width, height);

                const year = (window.WRAPPED_DATA && window.WRAPPED_DATA.year) || new Date().getFullYear();
                const series = buildSeries(year, chartMode);
                const maxVal = Math.max(10, ...series.map(s => s.value));

                const padL = 42, padR = 16, padT = 18, padB = 26;
                const plotW = width - padL - padR;
                const plotH = height - padT - padB;

                ctx.strokeStyle = 'rgba(24, 22, 20, 0.12)';
                ctx.lineWidth = 1;
                ctx.fillStyle = '#726B61';
                ctx.font = '500 10px ""JetBrains Mono"", monospace';
                ctx.textAlign = 'right';

                for (let i = 0; i <= 3; i++) {
                    const y = padT + (plotH * i / 3);
                    const val = Math.round(maxVal * (1 - i / 3));
                    ctx.beginPath();
                    ctx.moveTo(padL, y);
                    ctx.lineTo(width - padR, y);
                    ctx.stroke();
                    ctx.fillText(String(val), padL - 8, y + 3);
                }

                const months = ['JAN','FEB','MAR','APR','MAY','JUN','JUL','AUG','SEP','OCT','NOV','DEC'];
                ctx.textAlign = 'left';
                months.forEach((mLbl, mIdx) => {
                    const dayOfYear = Math.floor((new Date(year, mIdx, 1) - new Date(year, 0, 1)) / 86400000);
                    const x = padL + (dayOfYear / Math.max(1, series.length - 1)) * plotW;
                    ctx.fillStyle = '#726B61';
                    ctx.fillText(mLbl, x + 2, height - 6);
                });

                pointsCache = series.map((pt, idx) => {
                    const x = padL + (idx / Math.max(1, series.length - 1)) * plotW;
                    const y = padT + plotH - (pt.value / maxVal) * plotH;
                    return { ...pt, x, y, plotBottom: padT + plotH };
                });

                ctx.beginPath();
                ctx.moveTo(pointsCache[0].x, padT + plotH);
                pointsCache.forEach(p => ctx.lineTo(p.x, p.y));
                ctx.lineTo(pointsCache[pointsCache.length - 1].x, padT + plotH);
                ctx.closePath();
                ctx.fillStyle = 'rgba(179, 57, 30, 0.14)';
                ctx.fill();

                ctx.beginPath();
                pointsCache.forEach((p, i) => {
                    if (i === 0) ctx.moveTo(p.x, p.y);
                    else ctx.lineTo(p.x, p.y);
                });
                ctx.strokeStyle = '#181614';
                ctx.lineWidth = 1.5;
                ctx.stroke();

                const peakPt = pointsCache.reduce((best, cur) => cur.value > best.value ? cur : best, pointsCache[0]);
                if (peakPt && peakPt.value > 0) {
                    ctx.fillStyle = '#B3391E';
                    ctx.beginPath();
                    ctx.arc(peakPt.x, peakPt.y, 4.5, 0, Math.PI * 2);
                    ctx.fill();
                }
            }

            document.addEventListener('DOMContentLoaded', function() {
                const btns = document.querySelectorAll('.ink-mode-btn[data-chart-mode]');
                btns.forEach(btn => {
                    btn.addEventListener('click', () => {
                        chartMode = btn.getAttribute('data-chart-mode') || 'minutes';
                        btns.forEach(b => b.classList.toggle('active', b === btn));
                        renderDailyChronicle();
                    });
                });

                const canvas = document.getElementById('daily-chart-canvas');
                const readout = document.getElementById('daily-hover-readout');
                if (canvas && readout) {
                    canvas.addEventListener('mousemove', (e) => {
                        if (!pointsCache.length) return;
                        const r = canvas.getBoundingClientRect();
                        const mx = e.clientX - r.left;
                        let nearest = pointsCache[0];
                        let bestDist = Math.abs(mx - nearest.x);
                        for (let i = 1; i < pointsCache.length; i++) {
                            const d = Math.abs(mx - pointsCache[i].x);
                            if (d < bestDist) {
                                bestDist = d;
                                nearest = pointsCache[i];
                            }
                        }
                        const dateStr = nearest.dateObj.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
                        const artistInfo = nearest.topArtist && nearest.topArtist !== 'Unknown'
                            ? ` · <strong>${nearest.topArtist}</strong>`
                            : '';
                        readout.innerHTML = `<span class='mono-label'>${dateStr.toUpperCase()}</span> — <strong>${nearest.mins} mins</strong> (${nearest.plays} plays)${artistInfo}`;
                    });
                }

                document.addEventListener('slideChanged', (e) => {
                    if (e.detail && e.detail.slideElement && e.detail.slideElement.getAttribute('data-slide-id') === 'daily-chart') {
                        setTimeout(renderDailyChronicle, 40);
                    }
                });

                window.addEventListener('resize', () => {
                    const active = document.querySelector('.slide.active');
                    if (active && active.getAttribute('data-slide-id') === 'daily-chart') {
                        renderDailyChronicle();
                    }
                });
            });
        })();";
        }

        public override string GenerateCSS()
        {
            return @"
        .chronicle-sheet {
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            padding: 22px 26px;
            display: flex;
            flex-direction: column;
            gap: 16px;
            margin-top: 8px;
        }

        .chronicle-toolbar {
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 12px;
            padding-bottom: 12px;
            border-bottom: 1px solid var(--rule);
        }

        .chronicle-readout {
            font-size: 0.92rem;
            color: var(--ink-secondary);
        }

        .chronicle-readout strong {
            color: var(--ink);
            font-weight: 600;
        }

        .chronicle-controls {
            display: flex;
            gap: 6px;
        }

        .ink-mode-btn {
            background: transparent;
            border: 1px solid var(--rule-strong);
            color: var(--ink-secondary);
            font-family: var(--font-mono);
            font-size: 0.68rem;
            font-weight: 600;
            letter-spacing: 0.1em;
            padding: 5px 12px;
            cursor: pointer;
        }

        .ink-mode-btn.active {
            background: var(--ink);
            color: var(--bg-card);
        }

        .chronicle-canvas-wrap {
            width: 100%;
            height: 270px;
            position: relative;
        }

        /* Step 2: 12-Month Grid */
        .almanac-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            border-top: 1px solid var(--rule-strong);
            border-left: 1px solid var(--rule-strong);
            background: var(--bg-card);
            margin-top: 8px;
        }

        .almanac-cell {
            padding: 16px 18px;
            border-right: 1px solid var(--rule-strong);
            border-bottom: 1px solid var(--rule-strong);
            display: flex;
            flex-direction: column;
            gap: 12px;
            min-width: 0;
        }

        .almanac-cell.is-peak {
            background: rgba(179, 57, 30, 0.08);
        }

        .almanac-cell.is-empty {
            opacity: 0.45;
        }

        .almanac-cell-top {
            display: flex;
            justify-content: space-between;
            align-items: baseline;
            font-family: var(--font-mono);
            font-size: 0.74rem;
        }

        .almanac-mname {
            font-weight: 600;
            color: var(--ink);
            letter-spacing: 0.1em;
        }

        .almanac-cell.is-peak .almanac-mname {
            color: var(--accent);
        }

        .almanac-hrs {
            color: var(--ink-secondary);
            font-weight: 500;
        }

        .almanac-cell-body {
            display: grid;
            grid-template-columns: 44px 1fr;
            gap: 12px;
            align-items: center;
        }

        .almanac-mini-sleeve {
            width: 44px;
            height: 44px;
            box-shadow: none;
        }

        .almanac-mini-sleeve .sleeve-fallback {
            font-size: 0.75rem;
        }

        .almanac-mini-sleeve .sleeve-fallback span {
            padding: 1px 4px;
        }

        .almanac-empty-box {
            width: 44px;
            height: 44px;
            border: 1px dashed var(--rule);
            display: flex;
            align-items: center;
            justify-content: center;
            color: var(--ink-muted);
            font-family: var(--font-mono);
            font-size: 0.75rem;
        }

        .almanac-meta {
            min-width: 0;
        }

        .almanac-artist {
            font-size: 0.9rem;
            font-weight: 600;
            color: var(--ink);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .almanac-plays {
            font-family: var(--font-mono);
            font-size: 0.68rem;
            color: var(--ink-muted);
            margin-top: 2px;
        }

        .almanac-rail {
            height: 3px;
            background: var(--bg-subtle);
            width: 100%;
        }

        .almanac-ink {
            height: 100%;
            background: var(--ink-secondary);
        }

        .almanac-cell.is-peak .almanac-ink {
            background: var(--accent);
        }

        @media (max-width: 1024px) {
            .almanac-grid {
                grid-template-columns: repeat(3, 1fr);
            }
        }
";
        }
    }
}