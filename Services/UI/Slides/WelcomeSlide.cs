using System;
using System.Collections.Generic;
using System.Linq;
using MusicBeeWrapped;

namespace MusicBeeWrapped.Services.UI.Slides
{
    /// <summary>
    /// Slide 1: Overview (Warm Parchment stock).
    /// Clean asymmetric layout with the year's 4 core figures on the left and a 2x2 album cover mosaic on the right.
    /// </summary>
    public class WelcomeSlide : SlideComponentBase
    {
        public override string SlideId => "welcome";
        public override string SlideTitle => "Overview";
        public override int SlideOrder => 1;
        public override string AccentTheme => "parchment";

        public override string GenerateHTML(WrappedStatistics stats, PlayHistory playHistory, int year)
        {
            var yearPlays = GetYearPlays(playHistory, year).OrderBy(p => p.PlayedAt).ToList();
            var firstPlay = yearPlays.FirstOrDefault();
            var lastPlay = yearPlays.LastOrDefault();

            var firstTrackText = firstPlay != null
                ? $"{EscapeHtml(firstPlay.Title)} — {EscapeHtml(firstPlay.Artist)}"
                : "—";
            var firstDateText = firstPlay != null ? firstPlay.PlayedAt.ToString("MMM d") : "";

            var lastTrackText = lastPlay != null
                ? $"{EscapeHtml(lastPlay.Title)} — {EscapeHtml(lastPlay.Artist)}"
                : "—";
            var lastDateText = lastPlay != null ? lastPlay.PlayedAt.ToString("MMM d") : "";

            var mosaicItems = BuildCoverMosaic(stats, yearPlays);
            var mosaicHtml = string.Join("\n", mosaicItems.Select(item =>
                $"<div class='jacket-quad-item' title='{EscapeHtml(item.Title)} — {EscapeHtml(item.Artist)}'>" +
                $"{RenderCoverArt(item.DataUri, item.Title, item.Artist, "sleeve-frame jacket-quad-sleeve")}" +
                $"</div>"));

            var periodLabel = !string.IsNullOrEmpty(stats.PeriodLabel) ? stats.PeriodLabel : year.ToString();
            var periodSubtitle = !string.IsNullOrEmpty(stats.PeriodSubtitle) ? stats.PeriodSubtitle : "Your Year in Music";

            var content = $@"
                <div class='welcome-jacket'>
                    <div class='welcome-editorial-col'>
                        <div class='welcome-headline-block'>
                            <div class='liner-kicker'>OVERVIEW</div>
                            <div class='welcome-year-numeral'>{EscapeHtml(periodLabel)}</div>
                            <h1 class='welcome-serif-title'>{EscapeHtml(periodSubtitle)}</h1>
                        </div>

                        <div class='welcome-ledger-grid'>
                            <div class='welcome-ledger-cell'>
                                <span class='mono-label'>HOURS</span>
                                <div class='welcome-stat-num'>{stats.TotalHours:N0}</div>
                            </div>
                            <div class='welcome-ledger-cell'>
                                <span class='mono-label'>PLAYS</span>
                                <div class='welcome-stat-num'>{stats.TotalTracks:N0}</div>
                            </div>
                            <div class='welcome-ledger-cell'>
                                <span class='mono-label'>ARTISTS</span>
                                <div class='welcome-stat-num'>{stats.TotalUniqueArtists:N0}</div>
                            </div>
                            <div class='welcome-ledger-cell'>
                                <span class='mono-label'>ALBUMS</span>
                                <div class='welcome-stat-num'>{stats.TotalUniqueAlbums:N0}</div>
                            </div>
                        </div>

                        <div class='welcome-bookends'>
                            <div class='bookend-row'>
                                <span class='bookend-stamp'>FIRST PLAY</span>
                                <span class='bookend-track'>{firstTrackText}</span>
                                <span class='bookend-date'>{firstDateText}</span>
                            </div>
                            <div class='bookend-row'>
                                <span class='bookend-stamp'>LAST PLAY</span>
                                <span class='bookend-track'>{lastTrackText}</span>
                                <span class='bookend-date'>{lastDateText}</span>
                            </div>
                        </div>
                    </div>

                    <div class='welcome-art-col'>
                        <div class='jacket-quad-grid'>
                            {mosaicHtml}
                        </div>
                    </div>
                </div>";

            return WrapInSlideContainer(content);
        }

        private class MosaicItem
        {
            public string Title { get; set; }
            public string Artist { get; set; }
            public string DataUri { get; set; }
        }

        private List<MosaicItem> BuildCoverMosaic(WrappedStatistics stats, List<TrackPlay> yearPlays)
        {
            var result = new List<MosaicItem>();
            var seenAlbums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var trackKv in stats.TopTracks ?? new List<KeyValuePair<string, int>>())
            {
                if (result.Count >= 4) break;
                var parts = trackKv.Key.Split(new[] { " - " }, 2, StringSplitOptions.None);
                var artist = parts.Length > 0 ? parts[0] : "";
                var title = parts.Length > 1 ? parts[1] : trackKv.Key;

                var play = yearPlays.FirstOrDefault(p =>
                    string.Equals(p.Title, title, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.Artist, artist, StringComparison.OrdinalIgnoreCase));

                string albumKey = play != null && !string.IsNullOrWhiteSpace(play.Album)
                    ? $"{play.Artist}|||{play.Album}"
                    : $"{artist}|||{title}";

                if (seenAlbums.Contains(albumKey)) continue;
                seenAlbums.Add(albumKey);

                string uri = play != null ? Artwork.GetTrackArtworkDataUri(play, 360) : null;
                result.Add(new MosaicItem
                {
                    Title = play != null && !string.IsNullOrWhiteSpace(play.Album) ? play.Album : title,
                    Artist = artist,
                    DataUri = uri
                });
            }

            while (result.Count < 4)
            {
                result.Add(new MosaicItem
                {
                    Title = $"Vol. {result.Count + 1}",
                    Artist = "MusicBee",
                    DataUri = null
                });
            }

            return result;
        }

        public override string GenerateCSS()
        {
            return @"
        .welcome-jacket {
            display: grid;
            grid-template-columns: 1.05fr 0.95fr;
            gap: 48px;
            align-items: center;
            padding: 12px 0;
        }

        .welcome-editorial-col {
            display: flex;
            flex-direction: column;
            gap: 28px;
        }

        .welcome-headline-block {
            padding-bottom: 16px;
            border-bottom: 1px solid var(--rule-strong);
        }

        .welcome-year-numeral {
            font-family: var(--font-serif);
            font-size: clamp(5rem, 8.5vw, 7.2rem);
            line-height: 0.86;
            letter-spacing: -0.03em;
            color: var(--accent);
            margin: 4px 0 6px;
        }

        .welcome-serif-title {
            font-family: var(--font-serif);
            font-size: clamp(2rem, 3vw, 2.7rem);
            font-weight: 400;
            line-height: 1.04;
            color: var(--ink);
        }

        .welcome-ledger-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            border-top: 1px solid var(--rule-strong);
            border-left: 1px solid var(--rule-strong);
            background: var(--bg-card);
        }

        .welcome-ledger-cell {
            padding: 22px 24px;
            border-right: 1px solid var(--rule-strong);
            border-bottom: 1px solid var(--rule-strong);
            display: flex;
            flex-direction: column;
            gap: 6px;
        }

        .welcome-stat-num {
            font-family: var(--font-serif);
            font-size: 3.1rem;
            line-height: 0.94;
            color: var(--ink);
        }

        .welcome-bookends {
            display: flex;
            flex-direction: column;
            border-top: 1px solid var(--rule);
        }

        .bookend-row {
            display: grid;
            grid-template-columns: 90px 1fr auto;
            align-items: baseline;
            gap: 12px;
            padding: 10px 0;
            border-bottom: 1px solid var(--rule);
            font-size: 0.86rem;
        }

        .bookend-stamp {
            font-family: var(--font-mono);
            font-size: 0.66rem;
            font-weight: 600;
            letter-spacing: 0.12em;
            color: var(--ink-muted);
        }

        .bookend-track {
            font-weight: 500;
            color: var(--ink);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .bookend-date {
            font-family: var(--font-mono);
            font-size: 0.74rem;
            color: var(--ink-muted);
            white-space: nowrap;
        }

        .welcome-art-col {
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .jacket-quad-grid {
            width: 100%;
            max-width: 470px;
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 16px;
            padding: 18px;
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
        }

        .jacket-quad-sleeve {
            width: 100%;
        }

        @media (max-width: 980px) {
            .welcome-jacket {
                grid-template-columns: 1fr;
            }
        }
";
        }
    }
}
