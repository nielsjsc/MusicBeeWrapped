using System;
using System.Collections.Generic;
using System.Linq;
using MusicBeeWrapped;
using MusicBeeWrapped.Models;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide 6: Top Albums & Genres (Muted Stone-Olive stock).
    /// Uses 2 intra-slide steps on the Right Arrow key:
    /// Step 1: Top 5 Albums showcase with large album covers.
    /// Step 2: Top Genres & Peak Rotation Streaks.
    /// </summary>
    public class ObsessionPeriodsSlide : SlideComponentBase
    {
        public override string SlideId => "deep-dives";
        public override string SlideTitle => "Top Albums";
        public override int SlideOrder => 6;
        public override string AccentTheme => "olive";

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) &&
                   ((stats.TopAlbums != null && stats.TopAlbums.Any()) ||
                    (stats.TopGenres != null && stats.TopGenres.Any()));
        }

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = GetYearPlays(playHistory, year);

            // Step 1: Top 5 Albums with Cover Art
            var topAlbums = (stats.TopAlbums ?? new List<KeyValuePair<string, int>>()).Take(5).ToList();
            var shelfCardsHtml = string.Join("\n", topAlbums.Select((entry, idx) =>
            {
                var parts = entry.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                var albumName = parts.Length > 0 ? parts[0] : entry.Key;
                var artistName = parts.Length > 1 ? parts[1] : "Various Artists";
                int count = entry.Value;

                var albumPlays = yearPlays
                    .Where(p => string.Equals(p.Album, albumName, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(p.Artist, artistName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                double hours = (albumPlays.Any()
                    ? albumPlays.Sum(p => GetActualSeconds(p))
                    : count * 210.0) / 3600.0;

                string coverUri = Artwork.GetAlbumArtworkDataUri(yearPlays, artistName, albumName, 360);

                return $@"
                    <div class='shelf-lp-card'>
                        <div class='shelf-lp-top'>
                            <span class='mono-label'>0{idx + 1}</span>
                            <span class='shelf-lp-hrs'>{hours:F1} HRS</span>
                        </div>
                        {RenderCoverArt(coverUri, albumName, artistName, "sleeve-frame shelf-lp-sleeve")}
                        <div class='shelf-lp-meta'>
                            <div class='shelf-lp-title' title='{EscapeHtml(albumName)}'>{EscapeHtml(albumName)}</div>
                            <div class='shelf-lp-artist' title='{EscapeHtml(artistName)}'>{EscapeHtml(artistName)}</div>
                        </div>
                    </div>";
            }));

            // Step 2: Top 5 Genres + Peak Obsession Periods
            var topGenres = (stats.TopGenres ?? new List<KeyValuePair<string, double>>())
                .Where(g => !string.Equals(g.Key, "Unknown", StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .ToList();
            if (!topGenres.Any() && stats.TopGenres != null)
            {
                topGenres = stats.TopGenres.Take(5).ToList();
            }
            double maxGenrePct = topGenres.Any() ? Math.Max(1.0, topGenres.Max(g => g.Value)) : 1.0;

            var genreRowsHtml = string.Join("\n", topGenres.Select((g, i) =>
            {
                int pct = Math.Max(1, (int)Math.Round(g.Value));
                int barW = Math.Max(6, (int)Math.Round(g.Value * 100.0 / maxGenrePct));
                return $@"
                    <div class='genre-bar-row'>
                        <span class='genre-idx'>0{i + 1}</span>
                        <span class='genre-name'>{EscapeHtml(g.Key)}</span>
                        <div class='genre-rail'><div class='genre-ink' style='width:{barW}%'></div></div>
                        <span class='genre-pct'>{pct}%</span>
                    </div>";
            }));

            var obsessions = (stats.ObsessionPeriods ?? new List<ObsessionPeriod>())
                .OrderByDescending(o => o.PlayCount)
                .Take(3)
                .ToList();

            string erasHtml;
            if (obsessions.Any())
            {
                erasHtml = string.Join("\n", obsessions.Select((o, idx) =>
                {
                    string subject = string.Equals(o.ObsessionType, "Track", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(o.Track)
                        ? $"{EscapeHtml(o.Track)} — {EscapeHtml(o.Artist)}"
                        : (!string.IsNullOrEmpty(o.Album) && string.Equals(o.ObsessionType, "Album", StringComparison.OrdinalIgnoreCase)
                            ? $"{EscapeHtml(o.Album)} — {EscapeHtml(o.Artist)}"
                            : EscapeHtml(o.Artist));
                    string dateRange = $"{o.StartDate:MMM d} – {o.EndDate:MMM d}";
                    return $@"
                        <div class='era-index-card'>
                            <div class='era-card-top'>
                                <span class='mono-label'>0{idx + 1} · {dateRange}</span>
                                <span class='era-dates'>{o.PlayCount:N0} PLAYS</span>
                            </div>
                            <div class='era-subject'>{subject}</div>
                        </div>";
                }));
            }
            else
            {
                erasHtml = @"
                    <div class='era-index-card'>
                        <div class='era-subject'>Steady rotation across the year</div>
                    </div>";
            }

            var content = $@"
                <header class='liner-header'>
                    <div>
                        <div class='liner-kicker'>ALBUMS &amp; TASTE</div>
                        <h1 class='liner-title'>Albums &amp; Genres</h1>
                    </div>
                    <div class='step-tabs'>
                        <button type='button' class='step-tab-btn active' data-goto-step='0'>01 · Top Albums</button>
                        <button type='button' class='step-tab-btn' data-goto-step='1'>02 · Genres &amp; Streaks</button>
                    </div>
                </header>

                <!-- Step 1: 5-Across Top Albums -->
                <div class='slide-step active' data-step-title='Top Albums'>
                    <div class='record-shelf-grid'>
                        {shelfCardsHtml}
                    </div>
                </div>

                <!-- Step 2: Top Genres & Heavy Rotation Weeks -->
                <div class='slide-step' data-step-title='Genres & Streaks'>
                    <div class='taste-step-grid'>
                        <div class='taste-card'>
                            <div class='taste-card-header'>
                                <span class='mono-label'>TOP GENRES</span>
                            </div>
                            <div class='genre-bars-list'>
                                {genreRowsHtml}
                            </div>
                        </div>

                        <div class='taste-card'>
                            <div class='taste-card-header'>
                                <span class='mono-label'>PEAK ROTATION WEEKS</span>
                            </div>
                            <div class='eras-stack'>
                                {erasHtml}
                            </div>
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GenerateCSS()
        {
            return @"
        /* Step 1: 5-Across Top Albums */
        .record-shelf-grid {
            display: grid;
            grid-template-columns: repeat(5, 1fr);
            gap: 20px;
            margin-top: 12px;
        }

        .shelf-lp-card {
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            padding: 16px;
            display: flex;
            flex-direction: column;
            gap: 12px;
            min-width: 0;
        }

        .shelf-lp-top {
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .shelf-lp-hrs {
            font-family: var(--font-mono);
            font-size: 0.7rem;
            font-weight: 600;
            color: var(--accent);
        }

        .shelf-lp-sleeve {
            width: 100%;
        }

        .shelf-lp-meta {
            display: flex;
            flex-direction: column;
            gap: 3px;
            min-width: 0;
        }

        .shelf-lp-title {
            font-family: var(--font-serif);
            font-size: 1.35rem;
            line-height: 1.06;
            color: var(--ink);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .shelf-lp-artist {
            font-size: 0.84rem;
            color: var(--ink-secondary);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        /* Step 2: Genres & Heavy Rotation Weeks */
        .taste-step-grid {
            display: grid;
            grid-template-columns: 1.1fr 0.9fr;
            gap: 24px;
            margin-top: 12px;
        }

        .taste-card {
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            padding: 24px 28px;
            display: flex;
            flex-direction: column;
            gap: 18px;
        }

        .taste-card-header {
            padding-bottom: 10px;
            border-bottom: 1px solid var(--rule-strong);
        }

        .genre-bars-list {
            display: flex;
            flex-direction: column;
            gap: 16px;
        }

        .genre-bar-row {
            display: grid;
            grid-template-columns: 26px 140px 1fr 44px;
            align-items: center;
            gap: 12px;
        }

        .genre-idx {
            font-family: var(--font-mono);
            font-size: 0.74rem;
            color: var(--accent);
            font-weight: 600;
        }

        .genre-name {
            font-weight: 600;
            font-size: 0.95rem;
            color: var(--ink);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .genre-rail {
            height: 8px;
            background: var(--bg-subtle);
            border: 1px solid var(--rule);
        }

        .genre-ink {
            height: 100%;
            background: var(--ink);
        }

        .genre-pct {
            font-family: var(--font-mono);
            font-size: 0.78rem;
            font-weight: 600;
            color: var(--ink);
            text-align: right;
        }

        .eras-stack {
            display: flex;
            flex-direction: column;
            gap: 12px;
        }

        .era-index-card {
            background: var(--bg-paper);
            border: 1px solid var(--rule);
            padding: 14px 18px;
            display: flex;
            flex-direction: column;
            gap: 6px;
        }

        .era-card-top {
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .era-dates {
            font-family: var(--font-mono);
            font-size: 0.7rem;
            color: var(--accent);
            font-weight: 600;
        }

        .era-subject {
            font-family: var(--font-serif);
            font-size: 1.45rem;
            line-height: 1.08;
            color: var(--ink);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        @media (max-width: 1024px) {
            .record-shelf-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            .taste-step-grid {
                grid-template-columns: 1fr;
            }
        }
";
        }
    }
}
