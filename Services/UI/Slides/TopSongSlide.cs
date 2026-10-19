using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide 3: Top Song (Deep Espresso dark contrast stock).
    /// Minimal centerfold layout showcasing the #1 track with its album cover,
    /// serif headline, 3 core stats, and 12-month stem chart.
    /// </summary>
    public class TopSongSlide : SlideComponentBase
    {
        public override string SlideId => "top-song";
        public override string SlideTitle => "Top Song";
        public override int SlideOrder => 3;
        public override string AccentTheme => "espresso";

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) &&
                   stats.TopTracks != null &&
                   stats.TopTracks.Any();
        }

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var topEntry = stats.TopTracks.First();
            var parts = topEntry.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
            var artist = parts.Length > 0 ? parts[0] : "Unknown Artist";
            var title = parts.Length > 1 ? parts[1] : topEntry.Key;
            var playCount = topEntry.Value;

            var yearPlays = GetYearPlays(playHistory, year);
            var matchingPlays = yearPlays
                .Where(p => string.Equals(p.Title, title, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(p.Artist, artist, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.PlayedAt)
                .ToList();

            var firstMatch = matchingPlays.FirstOrDefault();
            var album = firstMatch != null && !string.IsNullOrWhiteSpace(firstMatch.Album)
                ? firstMatch.Album
                : "";

            var coverDataUri = Artwork.GetTrackArtworkDataUri(firstMatch, 460);

            double totalSeconds = matchingPlays.Any()
                ? matchingPlays.Sum(p => GetActualSeconds(p))
                : playCount * 210.0;
            double totalMinutes = Math.Round(totalSeconds / 60.0);

            string firstSpinDate = firstMatch != null
                ? firstMatch.PlayedAt.ToString("MMM d", CultureInfo.InvariantCulture)
                : "—";

            // Monthly distribution (Jan - Dec)
            var monthlyCounts = new int[12];
            foreach (var p in matchingPlays)
            {
                monthlyCounts[p.PlayedAt.Month - 1]++;
            }
            int maxMonthCount = Math.Max(1, monthlyCounts.Max());
            int peakMonthIdx = Array.IndexOf(monthlyCounts, monthlyCounts.Max());

            string[] monthAbbrevs = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
            var stemColsHtml = string.Join("", Enumerable.Range(0, 12).Select(m =>
            {
                int c = monthlyCounts[m];
                int pct = c > 0 ? Math.Max(8, (int)Math.Round(c * 100.0 / maxMonthCount)) : 0;
                bool isPeak = m == peakMonthIdx && c > 0;
                return $@"
                    <div class='stem-col {(isPeak ? "is-peak" : "")}' title='{monthAbbrevs[m]}: {c} plays'>
                        <span class='stem-val'>{(c > 0 ? c.ToString() : "·")}</span>
                        <div class='stem-track'>
                            {(c > 0 ? $"<div class='stem-line' style='height:{pct}%'></div>" : "")}
                        </div>
                        <span class='stem-lbl'>{monthAbbrevs[m]}</span>
                    </div>";
            }));

            var albumPart = !string.IsNullOrEmpty(album)
                ? $"<span class='credit-sep'>—</span><em>{EscapeHtml(album)}</em>"
                : "";

            var content = $@"
                <div class='single-centerfold'>
                    <div class='single-sleeve-col'>
                        {RenderCoverArt(coverDataUri, title, artist, "sleeve-frame single-hero-sleeve")}
                    </div>

                    <div class='single-details-col'>
                        <div class='single-title-group'>
                            <div class='liner-kicker'>#01 MOST PLAYED SONG</div>
                            <h1 class='single-song-title'>{EscapeHtml(title)}</h1>
                            <div class='single-song-credits'>
                                <strong>{EscapeHtml(artist)}</strong>{albumPart}
                            </div>
                        </div>

                        <div class='single-ledger'>
                            <div class='single-ledger-cell'>
                                <span class='mono-label'>PLAYS</span>
                                <div class='single-ledger-val'>{playCount:N0}</div>
                            </div>
                            <div class='single-ledger-cell'>
                                <span class='mono-label'>MINUTES</span>
                                <div class='single-ledger-val'>{totalMinutes:N0}</div>
                            </div>
                            <div class='single-ledger-cell'>
                                <span class='mono-label'>FIRST PLAYED</span>
                                <div class='single-ledger-val'>{firstSpinDate}</div>
                            </div>
                        </div>

                        <div class='single-stem-section'>
                            <div class='stem-header'>
                                <span class='mono-label'>MONTHLY PLAYS</span>
                            </div>
                            <div class='stem-grid'>
                                {stemColsHtml}
                            </div>
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GenerateCSS()
        {
            return @"
        .single-centerfold {
            display: grid;
            grid-template-columns: 360px 1fr;
            gap: 48px;
            align-items: center;
            padding: 20px 0;
        }

        .single-hero-sleeve {
            width: 360px;
            height: 360px;
            border: 1px solid rgba(236, 230, 218, 0.28);
            box-shadow: 0 24px 50px rgba(0, 0, 0, 0.55);
        }

        .single-details-col {
            display: flex;
            flex-direction: column;
            gap: 26px;
            min-width: 0;
        }

        .single-song-title {
            font-family: var(--font-serif);
            font-size: clamp(3rem, 5vw, 4.6rem);
            font-weight: 400;
            line-height: 0.96;
            letter-spacing: -0.02em;
            color: var(--ink);
            margin: 6px 0 10px;
        }

        .single-song-credits {
            font-size: 1.15rem;
            color: var(--ink-secondary);
        }

        .single-song-credits strong {
            color: var(--ink);
            font-weight: 500;
        }

        .single-song-credits .credit-sep {
            margin: 0 8px;
            color: var(--ink-muted);
        }

        .single-song-credits em {
            font-family: var(--font-serif);
            font-size: 1.25rem;
            font-style: italic;
            color: var(--ink-secondary);
        }

        .single-ledger {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            border-top: 1px solid var(--rule-strong);
            border-bottom: 1px solid var(--rule-strong);
            background: var(--bg-card);
        }

        .single-ledger-cell {
            padding: 18px 20px;
            border-right: 1px solid var(--rule);
            display: flex;
            flex-direction: column;
            gap: 4px;
        }

        .single-ledger-cell:last-child {
            border-right: none;
        }

        .single-ledger-val {
            font-family: var(--font-serif);
            font-size: 2.7rem;
            line-height: 0.96;
            color: var(--ink);
            margin-top: 2px;
        }

        /* 12-Month Stem Chart */
        .single-stem-section {
            background: var(--bg-card);
            border: 1px solid var(--rule);
            padding: 16px 20px;
        }

        .stem-header {
            padding-bottom: 10px;
            border-bottom: 1px solid var(--rule);
            margin-bottom: 12px;
        }

        .stem-grid {
            display: grid;
            grid-template-columns: repeat(12, 1fr);
            gap: 8px;
            height: 112px;
            align-items: end;
        }

        .stem-col {
            display: flex;
            flex-direction: column;
            align-items: center;
            height: 100%;
        }

        .stem-val {
            font-family: var(--font-mono);
            font-size: 0.66rem;
            color: var(--ink-muted);
            margin-bottom: 4px;
        }

        .stem-track {
            flex: 1;
            width: 100%;
            display: flex;
            align-items: flex-end;
            justify-content: center;
            border-bottom: 1px solid var(--rule);
            padding-bottom: 2px;
        }

        .stem-line {
            width: 2px;
            background: var(--ink-secondary);
            position: relative;
        }

        .stem-line::before {
            content: '';
            position: absolute;
            top: -3px;
            left: -3px;
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: var(--ink);
        }

        .stem-col.is-peak .stem-val,
        .stem-col.is-peak .stem-lbl {
            color: var(--accent);
            font-weight: 600;
        }

        .stem-col.is-peak .stem-line,
        .stem-col.is-peak .stem-line::before {
            background: var(--accent);
        }

        .stem-lbl {
            font-family: var(--font-mono);
            font-size: 0.64rem;
            color: var(--ink-muted);
            margin-top: 6px;
        }

        @media (max-width: 960px) {
            .single-centerfold {
                grid-template-columns: 1fr;
            }
            .single-hero-sleeve {
                width: 240px;
                height: 240px;
            }
        }
";
        }
    }
}
