using System;
using System.Collections.Generic;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide 4: Top 10 Tracks (Classic Bone Cardstock).
    /// Clean two-column layout (01–05 and 06–10) with album cover thumbnails, title, artist, and play count.
    /// </summary>
    public class TopTracksSlide : SlideComponentBase
    {
        public override string SlideId => "top-tracks";
        public override string SlideTitle => "Top Tracks";
        public override int SlideOrder => 4;
        public override string AccentTheme => "bone";

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) &&
                   stats.TopTracks != null &&
                   stats.TopTracks.Count >= 2;
        }

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = GetYearPlays(playHistory, year);
            var topTracks = stats.TopTracks.Take(10).ToList();

            var enriched = topTracks.Select((entry, idx) =>
            {
                var parts = entry.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                var artist = parts.Length > 0 ? parts[0] : "Unknown Artist";
                var title = parts.Length > 1 ? parts[1] : entry.Key;
                var count = entry.Value;

                var samplePlay = yearPlays.FirstOrDefault(p =>
                    string.Equals(p.Title, title, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.Artist, artist, StringComparison.OrdinalIgnoreCase));

                string coverUri = Artwork.GetTrackArtworkDataUri(samplePlay, 150);

                return new
                {
                    Rank = idx + 1,
                    Title = title,
                    Artist = artist,
                    Count = count,
                    CoverUri = coverUri
                };
            }).ToList();

            var colLeft = enriched.Take(5).ToList();
            var colRight = enriched.Skip(5).Take(5).ToList();

            string RenderRows(IEnumerable<dynamic> rows)
            {
                return string.Join("\n", rows.Select(t => $@"
                    <div class='lp-track-row {(t.Rank == 1 ? "is-lead" : "")}'>
                        <span class='lp-track-num'>{t.Rank:00}</span>
                        {RenderCoverArt((string)t.CoverUri, (string)t.Title, (string)t.Artist, "sleeve-frame lp-thumb-sleeve")}
                        <div class='lp-track-meta'>
                            <div class='lp-track-title'>{EscapeHtml(t.Title)}</div>
                            <div class='lp-track-sub'>{EscapeHtml(t.Artist)}</div>
                        </div>
                        <div class='lp-spins'>{t.Count:N0}</div>
                    </div>"));
            }

            var content = $@"
                <header class='liner-header'>
                    <div>
                        <div class='liner-kicker'>MOST PLAYED</div>
                        <h1 class='liner-title'>Top 10 Tracks</h1>
                    </div>
                    <div class='liner-subtitle'>
                        PLAYS
                    </div>
                </header>

                <div class='gatefold-spread'>
                    <div class='gatefold-side'>
                        <div class='lp-track-list'>
                            {RenderRows(colLeft)}
                        </div>
                    </div>

                    <div class='gatefold-side'>
                        <div class='lp-track-list'>
                            {RenderRows(colRight)}
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GenerateCSS()
        {
            return @"
        .gatefold-spread {
            display: grid;
            grid-template-columns: 1fr 1fr;
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
        }

        .gatefold-side {
            padding: 12px 28px;
            display: flex;
            flex-direction: column;
            min-width: 0;
        }

        .gatefold-side:first-child {
            border-right: 1px solid var(--rule-strong);
        }

        .lp-track-list {
            display: flex;
            flex-direction: column;
        }

        .lp-track-row {
            display: grid;
            grid-template-columns: 28px 54px 1fr auto;
            align-items: center;
            gap: 16px;
            padding: 14px 0;
            border-bottom: 1px solid var(--rule);
        }

        .lp-track-row:last-child {
            border-bottom: none;
        }

        .lp-track-num {
            font-family: var(--font-mono);
            font-size: 0.8rem;
            font-weight: 600;
            color: var(--ink-muted);
        }

        .lp-track-row.is-lead .lp-track-num {
            color: var(--accent);
        }

        .lp-thumb-sleeve {
            width: 54px;
            height: 54px;
            box-shadow: 0 3px 8px rgba(0, 0, 0, 0.12);
        }

        .lp-thumb-sleeve .sleeve-fallback {
            font-size: 0.85rem;
        }

        .lp-thumb-sleeve .sleeve-fallback span {
            padding: 2px 5px;
        }

        .lp-track-meta {
            min-width: 0;
            display: flex;
            flex-direction: column;
            gap: 3px;
        }

        .lp-track-title {
            font-weight: 600;
            font-size: 1rem;
            color: var(--ink);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .lp-track-sub {
            font-size: 0.84rem;
            color: var(--ink-secondary);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .lp-spins {
            font-family: var(--font-serif);
            font-size: 1.85rem;
            line-height: 1;
            color: var(--ink);
            padding-left: 12px;
        }

        .lp-track-row.is-lead .lp-spins {
            color: var(--accent);
        }

        @media (max-width: 960px) {
            .gatefold-spread {
                grid-template-columns: 1fr;
            }
            .gatefold-side:first-child {
                border-right: none;
                border-bottom: 1px solid var(--rule-strong);
            }
        }
";
        }
    }
}
