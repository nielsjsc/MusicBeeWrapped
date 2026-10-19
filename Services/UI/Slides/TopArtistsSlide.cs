using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide 5: Top Artists (Warm Greige stock).
    /// Clean interactive layout: Top 5 artists on the left, selected artist's stats and top tracks on the right.
    /// </summary>
    public class TopArtistsSlide : SlideComponentBase
    {
        public override string SlideId => "top-artists";
        public override string SlideTitle => "Top Artists";
        public override int SlideOrder => 5;
        public override string AccentTheme => "greige";

        public override bool CanRender(WrappedStatistics stats, PlayHistory playHistory)
        {
            return base.CanRender(stats, playHistory) &&
                   stats.TopArtists != null &&
                   stats.TopArtists.Any();
        }

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = GetYearPlays(playHistory, year);
            var topArtists = stats.TopArtists.Take(5).ToList();

            var profiles = topArtists.Select((entry, idx) =>
            {
                var artistName = entry.Key;
                var playCount = entry.Value;

                var artistPlays = yearPlays
                    .Where(p => string.Equals(p.Artist, artistName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                double totalSeconds = artistPlays.Any()
                    ? artistPlays.Sum(p => GetActualSeconds(p))
                    : playCount * 210.0;
                double hours = totalSeconds / 3600.0;

                int uniqueTracks = artistPlays
                    .Where(p => !string.IsNullOrEmpty(p.Title))
                    .Select(p => p.Title.ToLowerInvariant())
                    .Distinct()
                    .Count();
                if (uniqueTracks == 0) uniqueTracks = 1;

                var topAlbumGroup = artistPlays
                    .Where(p => !string.IsNullOrWhiteSpace(p.Album))
                    .GroupBy(p => p.Album)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                string primaryAlbum = topAlbumGroup != null ? topAlbumGroup.Key : "";
                var samplePlayForArt = (topAlbumGroup != null ? topAlbumGroup.FirstOrDefault() : null) ?? artistPlays.FirstOrDefault();
                string coverUri = Artwork.GetTrackArtworkDataUri(samplePlayForArt, 300);

                var topArtistTracks = artistPlays
                    .Where(p => !string.IsNullOrEmpty(p.Title))
                    .GroupBy(p => p.Title)
                    .Select(g => new
                    {
                        Title = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(t => t.Count)
                    .Take(5)
                    .ToList();

                return new
                {
                    Rank = idx + 1,
                    Name = artistName,
                    Plays = playCount,
                    Hours = hours,
                    UniqueTracks = uniqueTracks,
                    PrimaryAlbum = primaryAlbum,
                    CoverUri = coverUri,
                    TopTracks = topArtistTracks
                };
            }).ToList();

            var rosterTabsHtml = string.Join("\n", profiles.Select((a, idx) => $@"
                <button type='button' class='roster-tab {(idx == 0 ? "active" : "")}' data-artist-tab='{idx}'>
                    <span class='roster-rank'>{a.Rank:00}</span>
                    {RenderCoverArt(a.CoverUri, a.Name, a.PrimaryAlbum, "sleeve-frame roster-mini-sleeve")}
                    <div class='roster-name'>{EscapeHtml(a.Name)}</div>
                    <div class='roster-count'>{a.Plays:N0}</div>
                </button>"));

            var dossiersHtml = string.Join("\n", profiles.Select((a, idx) =>
            {
                var trackRows = a.TopTracks.Any()
                    ? string.Join("\n", a.TopTracks.Select((t, tIdx) => $@"
                        <div class='dossier-cut-row'>
                            <span class='cut-idx'>0{tIdx + 1}</span>
                            <span class='cut-title'>{EscapeHtml(t.Title)}</span>
                            <span class='cut-plays'>{t.Count:N0}</span>
                        </div>"))
                    : "";

                return $@"
                <div class='artist-dossier-sheet {(idx == 0 ? "active" : "")}' data-artist-panel='{idx}'>
                    <div class='dossier-top'>
                        {RenderCoverArt(a.CoverUri, a.Name, a.PrimaryAlbum, "sleeve-frame dossier-hero-sleeve")}
                        <div class='dossier-headline'>
                            <div class='liner-kicker'>#{a.Rank:00} ARTIST</div>
                            <h2 class='dossier-artist-name'>{EscapeHtml(a.Name)}</h2>
                        </div>
                    </div>

                    <div class='dossier-ledger'>
                        <div class='dossier-cell'>
                            <span class='mono-label'>PLAYS</span>
                            <div class='dossier-val'>{a.Plays:N0}</div>
                        </div>
                        <div class='dossier-cell'>
                            <span class='mono-label'>HOURS</span>
                            <div class='dossier-val'>{a.Hours:F1}</div>
                        </div>
                        <div class='dossier-cell'>
                            <span class='mono-label'>SONGS</span>
                            <div class='dossier-val'>{a.UniqueTracks}</div>
                        </div>
                    </div>

                    <div class='dossier-cuts-section'>
                        <div class='dossier-cuts-header'>
                            <span class='mono-label'>MOST PLAYED SONGS</span>
                            <span class='mono-label'>PLAYS</span>
                        </div>
                        <div class='dossier-cuts-list'>
                            {trackRows}
                        </div>
                    </div>
                </div>";
            }));

            var content = $@"
                <header class='liner-header'>
                    <div>
                        <div class='liner-kicker'>MOST PLAYED</div>
                        <h1 class='liner-title'>Top Artists</h1>
                    </div>
                </header>

                <div class='presskit-layout'>
                    <div class='roster-col'>
                        <div class='roster-list'>
                            {rosterTabsHtml}
                        </div>
                    </div>

                    <div class='dossier-col'>
                        {dossiersHtml}
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        public override string GenerateJavaScript(WrappedStatistics stats, int year)
        {
            return @"
        document.addEventListener('DOMContentLoaded', function() {
            const tabs = document.querySelectorAll('.roster-tab[data-artist-tab]');
            const panels = document.querySelectorAll('.artist-dossier-sheet[data-artist-panel]');
            tabs.forEach(tab => {
                tab.addEventListener('click', () => {
                    const target = tab.getAttribute('data-artist-tab');
                    tabs.forEach(t => t.classList.toggle('active', t === tab));
                    panels.forEach(p => p.classList.toggle('active', p.getAttribute('data-artist-panel') === target));
                });
            });
        });";
        }

        public override string GenerateCSS()
        {
            return @"
        .presskit-layout {
            display: grid;
            grid-template-columns: 350px 1fr;
            gap: 28px;
            align-items: stretch;
        }

        .roster-col {
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            display: flex;
            flex-direction: column;
        }

        .roster-list {
            display: flex;
            flex-direction: column;
            flex: 1;
        }

        .roster-tab {
            display: grid;
            grid-template-columns: 26px 48px 1fr auto;
            align-items: center;
            gap: 14px;
            padding: 16px 18px;
            background: transparent;
            border: none;
            border-bottom: 1px solid var(--rule);
            text-align: left;
            color: var(--ink);
            font-family: inherit;
            cursor: pointer;
            transition: background-color 0.15s ease;
            flex: 1;
        }

        .roster-tab:last-child {
            border-bottom: none;
        }

        .roster-tab:hover {
            background: var(--bg-subtle);
        }

        .roster-tab.active {
            background: var(--ink);
            color: var(--bg-card);
        }

        .roster-rank {
            font-family: var(--font-mono);
            font-size: 0.76rem;
            font-weight: 600;
            color: var(--ink-muted);
        }

        .roster-tab.active .roster-rank {
            color: #E89984;
        }

        .roster-mini-sleeve {
            width: 48px;
            height: 48px;
            box-shadow: none;
        }

        .roster-mini-sleeve .sleeve-fallback {
            font-size: 0.78rem;
        }

        .roster-name {
            font-weight: 600;
            font-size: 0.98rem;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .roster-count {
            font-family: var(--font-serif);
            font-size: 1.65rem;
            line-height: 1;
        }

        /* Right Dossier Panel */
        .artist-dossier-sheet {
            display: none;
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            padding: 26px 32px;
            height: 100%;
            flex-direction: column;
            justify-content: space-between;
            gap: 22px;
        }

        .artist-dossier-sheet.active {
            display: flex;
        }

        .dossier-top {
            display: grid;
            grid-template-columns: 116px 1fr;
            gap: 24px;
            align-items: center;
        }

        .dossier-hero-sleeve {
            width: 116px;
            height: 116px;
        }

        .dossier-artist-name {
            font-family: var(--font-serif);
            font-size: clamp(2.5rem, 3.8vw, 3.5rem);
            font-weight: 400;
            line-height: 0.96;
            color: var(--ink);
            margin-top: 4px;
        }

        .dossier-ledger {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            border-top: 1px solid var(--rule-strong);
            border-bottom: 1px solid var(--rule-strong);
            background: var(--bg-paper);
        }

        .dossier-cell {
            padding: 14px 18px;
            border-right: 1px solid var(--rule);
        }

        .dossier-cell:last-child {
            border-right: none;
        }

        .dossier-val {
            font-family: var(--font-serif);
            font-size: 2.2rem;
            line-height: 1;
            color: var(--ink);
            margin-top: 4px;
        }

        .dossier-cuts-header {
            display: flex;
            justify-content: space-between;
            padding-bottom: 8px;
            border-bottom: 1px solid var(--rule-strong);
            margin-bottom: 4px;
        }

        .dossier-cuts-list {
            display: flex;
            flex-direction: column;
        }

        .dossier-cut-row {
            display: grid;
            grid-template-columns: 28px 1fr auto;
            align-items: baseline;
            gap: 12px;
            padding: 9px 0;
            border-bottom: 1px solid var(--rule);
            font-size: 0.9rem;
        }

        .dossier-cut-row:last-child {
            border-bottom: none;
        }

        .cut-idx {
            font-family: var(--font-mono);
            font-size: 0.72rem;
            color: var(--accent);
            font-weight: 600;
        }

        .cut-title {
            font-weight: 500;
            color: var(--ink);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .cut-plays {
            font-family: var(--font-mono);
            font-size: 0.8rem;
            font-weight: 600;
            color: var(--ink);
            text-align: right;
        }

        @media (max-width: 960px) {
            .presskit-layout {
                grid-template-columns: 1fr;
            }
        }
";
        }
    }
}
