using System;
using System.Collections.Generic;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide 8: Summary & Top 50 (Dark Espresso background + Warm Cream Summary Card).
    /// Left column displays the year's summary card and .M3U export button;
    /// Right column displays the searchable Top 50 tracks list.
    /// </summary>
    public class FinaleSlide : SlideComponentBase
    {
        public override string SlideId => "finale";
        public override string SlideTitle => "Summary";
        public override int SlideOrder => 8;
        public override string AccentTheme => "colophon";

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = GetYearPlays(playHistory, year);

            var topArtist = stats.TopArtists?.FirstOrDefault().Key ?? "Unknown";
            var topTrackRaw = stats.TopTracks?.FirstOrDefault().Key ?? "Unknown";
            var trackParts = topTrackRaw.Split(new[] { " - " }, 2, StringSplitOptions.None);
            var topTrackTitle = trackParts.Length > 1 ? trackParts[1] : topTrackRaw;
            var topTrackArtist = trackParts.Length > 0 ? trackParts[0] : topArtist;

            var topAlbumRaw = stats.TopAlbums?.FirstOrDefault().Key ?? "Unknown";
            var albumParts = topAlbumRaw.Split(new[] { " - " }, 2, StringSplitOptions.None);
            var topAlbumTitle = albumParts.Length > 0 ? albumParts[0] : topAlbumRaw;

            var topGenre = stats.TopGenres?.FirstOrDefault().Key ?? "Eclectic";

            var heroPlay = yearPlays.FirstOrDefault(p =>
                string.Equals(p.Title, topTrackTitle, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.Artist, topTrackArtist, StringComparison.OrdinalIgnoreCase))
                ?? yearPlays.FirstOrDefault();
            var colophonCoverUri = Artwork.GetTrackArtworkDataUri(heroPlay, 220);

            var top50 = GetTop50Tracks(yearPlays, stats);
            var top50RowsHtml = string.Join("\n", top50.Select((t, idx) => $@"
                <div class='master-tape-row' data-search='{EscapeHtml((t.Title + " " + t.Artist + " " + t.Album).ToLowerInvariant())}'>
                    <span class='tape-idx'>{(idx + 1):00}</span>
                    <div class='tape-main'>
                        <span class='tape-title'>{EscapeHtml(t.Title)}</span>
                        <span class='tape-sub'>{EscapeHtml(t.Artist)}</span>
                    </div>
                    <span class='tape-cnt'>{t.PlayCount:N0}</span>
                </div>"));

            var periodLabel = !string.IsNullOrEmpty(stats.PeriodLabel) ? stats.PeriodLabel : year.ToString();

            var content = $@"
                <header class='liner-header'>
                    <div>
                        <div class='liner-kicker'>SUMMARY</div>
                        <h1 class='liner-title'>{EscapeHtml(periodLabel)} at a Glance</h1>
                    </div>
                </header>

                <div class='colophon-layout'>
                    <!-- Left: Warm Cream Summary Card -->
                    <aside class='printed-colophon-card'>
                        <div class='colophon-hero-row'>
                            {RenderCoverArt(colophonCoverUri, topTrackTitle, topArtist, "sleeve-frame colophon-sleeve")}
                            <div class='colophon-hero-copy'>
                                <div class='colophon-year'>{EscapeHtml(periodLabel)}</div>
                                <div class='colophon-sub'>SUMMARY</div>
                            </div>
                        </div>

                        <div class='colophon-metrics'>
                            <div class='c-metric'>
                                <span>HOURS</span>
                                <strong>{stats.TotalHours:N1}</strong>
                            </div>
                            <div class='c-metric'>
                                <span>PLAYS</span>
                                <strong>{stats.TotalTracks:N0}</strong>
                            </div>
                            <div class='c-metric'>
                                <span>ARTISTS</span>
                                <strong>{stats.TotalUniqueArtists:N0}</strong>
                            </div>
                            <div class='c-metric'>
                                <span>ALBUMS</span>
                                <strong>{stats.TotalUniqueAlbums:N0}</strong>
                            </div>
                        </div>

                        <div class='colophon-honors'>
                            <div class='honor-line'>
                                <span class='honor-k'>TOP ARTIST</span>
                                <span class='honor-v'>{EscapeHtml(topArtist)}</span>
                            </div>
                            <div class='honor-line'>
                                <span class='honor-k'>TOP SONG</span>
                                <span class='honor-v'>{EscapeHtml(topTrackTitle)}</span>
                            </div>
                            <div class='honor-line'>
                                <span class='honor-k'>TOP ALBUM</span>
                                <span class='honor-v'>{EscapeHtml(topAlbumTitle)}</span>
                            </div>
                            <div class='honor-line'>
                                <span class='honor-k'>TOP GENRE</span>
                                <span class='honor-v'>{EscapeHtml(topGenre)}</span>
                            </div>
                        </div>

                        <div class='colophon-actions'>
                            <button type='button' class='colophon-export-btn' onclick='exportTop50Playlist({year})'>
                                ↓ EXPORT TOP 50 (.M3U)
                            </button>
                            <button type='button' class='colophon-replay-btn' onclick='showSlide(0, 0)'>
                                ↺ BACK TO START
                            </button>
                        </div>
                    </aside>

                    <!-- Right: Top 50 Tracks List -->
                    <div class='master-tape-panel'>
                        <div class='master-tape-header'>
                            <span class='mono-label'>TOP {top50.Count:00} TRACKS</span>
                            <input type='text' id='top50-filter-input' class='tape-search-input' placeholder='Search tracks...' aria-label='Filter Top 50 tracks'>
                        </div>

                        <div class='master-tape-scroll' id='top50-scroll-list'>
                            {top50RowsHtml}
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        private class TopTrackExportItem
        {
            public string Title { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public string FileUrl { get; set; }
            public int PlayCount { get; set; }
            public int DurationSeconds { get; set; }
            public double TotalSeconds { get; set; }
        }

        private List<TopTrackExportItem> GetTop50Tracks(List<TrackPlay> yearPlays, WrappedStatistics stats)
        {
            if (yearPlays != null && yearPlays.Any())
            {
                return yearPlays
                    .Where(p => !string.IsNullOrEmpty(p.Title))
                    .GroupBy(p => $"{(p.Artist ?? "Unknown").Trim()}|||{p.Title.Trim()}", StringComparer.OrdinalIgnoreCase)
                    .Select(g =>
                    {
                        var first = g.First();
                        return new TopTrackExportItem
                        {
                            Title = first.Title ?? "Unknown Track",
                            Artist = first.Artist ?? "Unknown Artist",
                            Album = first.Album ?? "",
                            FileUrl = g.Select(x => x.FileUrl).FirstOrDefault(u => !string.IsNullOrEmpty(u)) ?? "",
                            PlayCount = g.Count(),
                            DurationSeconds = first.Duration > 0 ? first.Duration : GetActualSeconds(first),
                            TotalSeconds = g.Sum(x => (double)GetActualSeconds(x))
                        };
                    })
                    .OrderByDescending(x => x.PlayCount)
                    .ThenByDescending(x => x.TotalSeconds)
                    .Take(50)
                    .ToList();
            }

            return (stats.TopTracks ?? new List<KeyValuePair<string, int>>())
                .Take(50)
                .Select(kv =>
                {
                    var parts = kv.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                    return new TopTrackExportItem
                    {
                        Artist = parts.Length > 0 ? parts[0] : "Unknown",
                        Title = parts.Length > 1 ? parts[1] : kv.Key,
                        Album = "",
                        FileUrl = "",
                        PlayCount = kv.Value,
                        DurationSeconds = 210,
                        TotalSeconds = kv.Value * 210.0
                    };
                })
                .ToList();
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        document.addEventListener('DOMContentLoaded', function() {
            const filterInput = document.getElementById('top50-filter-input');
            const rows = document.querySelectorAll('#top50-scroll-list .master-tape-row');
            if (filterInput) {
                filterInput.addEventListener('input', function() {
                    const q = (this.value || '').trim().toLowerCase();
                    rows.forEach(r => {
                        const hay = r.getAttribute('data-search') || '';
                        r.style.display = (!q || hay.indexOf(q) !== -1) ? 'grid' : 'none';
                    });
                });
            }
        });";
        }

        public override string GenerateCSS()
        {
            return @"
        .colophon-layout {
            display: grid;
            grid-template-columns: 370px 1fr;
            gap: 28px;
            align-items: stretch;
        }

        .printed-colophon-card {
            background: #E6E0D3;
            color: #181614;
            border: 1px solid #181614;
            padding: 24px 26px;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            gap: 18px;
            box-shadow: 0 20px 44px rgba(0, 0, 0, 0.45);
        }

        .colophon-hero-row {
            display: grid;
            grid-template-columns: 76px 1fr;
            gap: 16px;
            align-items: center;
            padding-bottom: 14px;
            border-bottom: 1px solid rgba(24, 22, 20, 0.2);
        }

        .colophon-sleeve {
            width: 76px;
            height: 76px;
            border-color: #181614;
        }

        .colophon-year {
            font-family: var(--font-serif);
            font-size: 3.4rem;
            line-height: 0.88;
            color: #181614;
        }

        .colophon-sub {
            font-family: var(--font-mono);
            font-size: 0.66rem;
            letter-spacing: 0.14em;
            color: #B3391E;
            font-weight: 600;
            margin-top: 4px;
        }

        .colophon-metrics {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 12px;
            padding-bottom: 14px;
            border-bottom: 1px solid rgba(24, 22, 20, 0.2);
            font-family: var(--font-mono);
            font-size: 0.72rem;
        }

        .c-metric {
            display: flex;
            flex-direction: column;
            gap: 2px;
        }

        .c-metric span {
            color: #6B6359;
            font-size: 0.64rem;
            letter-spacing: 0.1em;
        }

        .c-metric strong {
            font-family: var(--font-serif);
            font-size: 1.9rem;
            font-weight: 400;
            line-height: 1;
            color: #181614;
        }

        .colophon-honors {
            display: flex;
            flex-direction: column;
            gap: 8px;
            font-size: 0.86rem;
        }

        .honor-line {
            display: grid;
            grid-template-columns: 92px 1fr;
            align-items: baseline;
            gap: 8px;
        }

        .honor-k {
            font-family: var(--font-mono);
            font-size: 0.65rem;
            letter-spacing: 0.1em;
            color: #B3391E;
            font-weight: 600;
        }

        .honor-v {
            font-weight: 600;
            color: #181614;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .colophon-actions {
            display: flex;
            flex-direction: column;
            gap: 8px;
            padding-top: 4px;
        }

        .colophon-export-btn {
            width: 100%;
            padding: 11px 14px;
            background: #181614;
            color: #E6E0D3;
            border: 1px solid #181614;
            font-family: var(--font-mono);
            font-size: 0.72rem;
            font-weight: 600;
            letter-spacing: 0.12em;
            cursor: pointer;
            transition: background-color 0.18s ease;
        }

        .colophon-export-btn:hover {
            background: #B3391E;
            border-color: #B3391E;
        }

        .colophon-replay-btn {
            width: 100%;
            padding: 8px 14px;
            background: transparent;
            color: #181614;
            border: 1px solid rgba(24, 22, 20, 0.35);
            font-family: var(--font-mono);
            font-size: 0.68rem;
            font-weight: 600;
            letter-spacing: 0.12em;
            cursor: pointer;
        }

        .colophon-replay-btn:hover {
            border-color: #181614;
            background: rgba(24, 22, 20, 0.06);
        }

        /* Right Panel: Top 50 List */
        .master-tape-panel {
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            display: flex;
            flex-direction: column;
            height: 470px;
            min-width: 0;
        }

        .master-tape-header {
            padding: 12px 18px;
            border-bottom: 1px solid var(--rule-strong);
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 14px;
            flex-wrap: wrap;
        }

        .tape-search-input {
            background: var(--bg-paper);
            border: 1px solid var(--rule);
            color: var(--ink);
            font-family: var(--font-mono);
            font-size: 0.72rem;
            padding: 6px 12px;
            width: 220px;
            outline: none;
        }

        .tape-search-input:focus {
            border-color: var(--accent);
        }

        .master-tape-scroll {
            flex: 1;
            overflow-y: auto;
            padding: 4px 18px;
        }

        .master-tape-scroll::-webkit-scrollbar {
            width: 6px;
        }
        .master-tape-scroll::-webkit-scrollbar-thumb {
            background: var(--rule);
        }

        .master-tape-row {
            display: grid;
            grid-template-columns: 32px 1fr 48px;
            align-items: baseline;
            gap: 12px;
            padding: 9px 0;
            border-bottom: 1px solid var(--rule);
            font-size: 0.88rem;
        }

        .master-tape-row:last-child {
            border-bottom: none;
        }

        .tape-idx {
            font-family: var(--font-mono);
            font-size: 0.72rem;
            color: var(--accent);
            font-weight: 600;
        }

        .tape-main {
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .tape-title {
            font-weight: 600;
            color: var(--ink);
            margin-right: 8px;
        }

        .tape-sub {
            font-size: 0.8rem;
            color: var(--ink-secondary);
        }

        .tape-cnt {
            font-family: var(--font-mono);
            font-size: 0.78rem;
            font-weight: 600;
            color: var(--ink);
            text-align: right;
        }

        @media (max-width: 980px) {
            .colophon-layout {
                grid-template-columns: 1fr;
            }
        }
";
        }
    }
}