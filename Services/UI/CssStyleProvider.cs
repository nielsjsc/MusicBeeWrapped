using System;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Provides the Vinyl Liner Notes (Warm Archival Editorial) stylesheet for MusicBee Wrapped.
    /// Features distinct paper/sleeve stock palettes per slide, Instrument Serif + Inter + JetBrains Mono typography,
    /// hairline ledger rules, and tactile album sleeve frames with silent monogram fallback.
    /// </summary>
    public class CssStyleProvider
    {
        public string GetMainInterfaceCSS()
        {
            return @"
        :root {
            --bg-paper: #E2DBD0;
            --bg-card: #EAE4DA;
            --bg-subtle: rgba(24, 22, 20, 0.045);
            --ink: #181614;
            --ink-secondary: #4A453E;
            --ink-muted: #767067;
            --rule: rgba(24, 22, 20, 0.16);
            --rule-strong: #181614;
            --accent: #B3391E;
            --accent-soft: rgba(179, 57, 30, 0.12);
            --topbar-bg: rgba(226, 219, 208, 0.92);

            --font-serif: 'Instrument Serif', Georgia, 'Times New Roman', serif;
            --font-sans: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
            --font-mono: 'JetBrains Mono', 'Consolas', monospace;
        }

        /* Per-Slide Paper & Sleeve Stock Variations */
        body[data-theme='parchment'] {
            --bg-paper: #E2DBD0;
            --bg-card: #EAE4DA;
            --bg-subtle: rgba(24, 22, 20, 0.045);
            --ink: #181614;
            --ink-secondary: #4A453E;
            --ink-muted: #736C62;
            --rule: rgba(24, 22, 20, 0.16);
            --rule-strong: #181614;
            --accent: #B3391E;
            --accent-soft: rgba(179, 57, 30, 0.12);
            --topbar-bg: rgba(226, 219, 208, 0.92);
        }

        body[data-theme='sand'] {
            --bg-paper: #DAD2C4;
            --bg-card: #E4DDD0;
            --bg-subtle: rgba(24, 22, 20, 0.05);
            --ink: #191614;
            --ink-secondary: #48423B;
            --ink-muted: #6E675E;
            --rule: rgba(24, 22, 20, 0.17);
            --rule-strong: #191614;
            --accent: #9E331B;
            --accent-soft: rgba(158, 51, 27, 0.12);
            --topbar-bg: rgba(218, 210, 196, 0.92);
        }

        body[data-theme='espresso'] {
            --bg-paper: #171513;
            --bg-card: #211E1B;
            --bg-subtle: rgba(236, 230, 218, 0.05);
            --ink: #ECE6DA;
            --ink-secondary: #B8B0A2;
            --ink-muted: #857E73;
            --rule: rgba(236, 230, 218, 0.15);
            --rule-strong: #ECE6DA;
            --accent: #D95338;
            --accent-soft: rgba(217, 83, 56, 0.16);
            --topbar-bg: rgba(23, 21, 19, 0.92);
        }

        body[data-theme='bone'] {
            --bg-paper: #E4DFD4;
            --bg-card: #ECE8DF;
            --bg-subtle: rgba(23, 21, 19, 0.04);
            --ink: #171513;
            --ink-secondary: #47433D;
            --ink-muted: #706B63;
            --rule: rgba(23, 21, 19, 0.15);
            --rule-strong: #171513;
            --accent: #1E4D3B;
            --accent-soft: rgba(30, 77, 59, 0.11);
            --topbar-bg: rgba(228, 223, 212, 0.92);
        }

        body[data-theme='greige'] {
            --bg-paper: #D7D1C5;
            --bg-card: #E2DDD2;
            --bg-subtle: rgba(23, 21, 19, 0.05);
            --ink: #171513;
            --ink-secondary: #46413B;
            --ink-muted: #6C665E;
            --rule: rgba(23, 21, 19, 0.17);
            --rule-strong: #171513;
            --accent: #8C311E;
            --accent-soft: rgba(140, 49, 30, 0.12);
            --topbar-bg: rgba(215, 209, 197, 0.92);
        }

        body[data-theme='olive'] {
            --bg-paper: #D6D4CA;
            --bg-card: #E1DFD6;
            --bg-subtle: rgba(23, 22, 20, 0.045);
            --ink: #171614;
            --ink-secondary: #45443E;
            --ink-muted: #6C6A61;
            --rule: rgba(23, 22, 20, 0.16);
            --rule-strong: #171614;
            --accent: #A64225;
            --accent-soft: rgba(166, 66, 37, 0.12);
            --topbar-bg: rgba(214, 212, 202, 0.92);
        }

        body[data-theme='newsprint'] {
            --bg-paper: #DFD9CE;
            --bg-card: #E8E3D9;
            --bg-subtle: rgba(24, 22, 20, 0.045);
            --ink: #181614;
            --ink-secondary: #49443D;
            --ink-muted: #726B61;
            --rule: rgba(24, 22, 20, 0.16);
            --rule-strong: #181614;
            --accent: #B3391E;
            --accent-soft: rgba(179, 57, 30, 0.12);
            --topbar-bg: rgba(223, 217, 206, 0.92);
        }

        body[data-theme='colophon'] {
            --bg-paper: #191715;
            --bg-card: #23201D;
            --bg-subtle: rgba(236, 230, 218, 0.05);
            --ink: #EAE4D8;
            --ink-secondary: #B5ADA0;
            --ink-muted: #827B70;
            --rule: rgba(234, 228, 216, 0.15);
            --rule-strong: #EAE4D8;
            --accent: #D95338;
            --accent-soft: rgba(217, 83, 56, 0.15);
            --topbar-bg: rgba(25, 23, 21, 0.92);
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        html, body {
            width: 100%;
            height: 100%;
            background-color: var(--bg-paper);
            color: var(--ink);
            font-family: var(--font-sans);
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
            overflow: hidden;
            transition: background-color 0.45s cubic-bezier(0.22, 1, 0.36, 1), color 0.35s ease;
        }

        /* Subtle tactile paper texture */
        body::before {
            content: '';
            position: fixed;
            inset: 0;
            pointer-events: none;
            z-index: 1;
            opacity: 0.035;
            background-image: radial-gradient(rgba(0, 0, 0, 0.75) 1px, transparent 0);
            background-size: 20px 20px;
        }

        /* Archival Header Bar */
        .app-topbar {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            height: 52px;
            padding: 0 36px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            z-index: 50;
            background: var(--topbar-bg);
            backdrop-filter: blur(8px);
            border-bottom: 1px solid var(--rule);
            transition: background-color 0.45s ease, border-color 0.35s ease;
        }

        .brand-group {
            display: flex;
            align-items: center;
            gap: 12px;
            font-family: var(--font-mono);
            font-size: 0.72rem;
            letter-spacing: 0.14em;
            text-transform: uppercase;
        }

        .brand-catno {
            font-weight: 600;
            color: var(--accent);
        }

        .brand-divider {
            color: var(--ink-muted);
        }

        .brand-name {
            color: var(--ink-secondary);
            font-weight: 500;
        }

        .topbar-right {
            display: flex;
            align-items: center;
            gap: 18px;
            font-family: var(--font-mono);
            font-size: 0.72rem;
            letter-spacing: 0.14em;
            text-transform: uppercase;
        }

        .active-slide-label {
            color: var(--ink-muted);
        }

        .slide-counter-badge {
            font-weight: 600;
            color: var(--ink);
            padding-left: 14px;
            border-left: 1px solid var(--rule);
        }

        /* Main Viewport */
        #app {
            position: relative;
            z-index: 10;
            width: 100%;
            height: 100vh;
            padding: 66px 36px 76px;
            overflow-y: auto;
            overflow-x: hidden;
            display: flex;
            justify-content: center;
            align-items: flex-start;
        }

        #app::-webkit-scrollbar {
            width: 6px;
        }
        #app::-webkit-scrollbar-track {
            background: transparent;
        }
        #app::-webkit-scrollbar-thumb {
            background: var(--rule);
        }

        .slide {
            display: none;
            width: 100%;
            max-width: 1260px;
            margin: auto;
            animation: slideFadeIn 0.38s cubic-bezier(0.2, 0.9, 0.3, 1);
        }

        .slide.active {
            display: block;
        }

        @keyframes slideFadeIn {
            from {
                opacity: 0;
                transform: translateY(8px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .slide-stage {
            width: 100%;
            display: flex;
            flex-direction: column;
            gap: 24px;
        }

        /* Intra-slide Step Transitions (Arrow keys step through views inside a slide) */
        .slide-step {
            display: none;
            width: 100%;
            animation: stepTransitionIn 0.34s cubic-bezier(0.22, 1, 0.36, 1);
        }

        .slide-step.active {
            display: block;
        }

        @keyframes stepTransitionIn {
            from {
                opacity: 0;
                transform: translateY(10px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .step-tabs {
            display: inline-flex;
            align-items: center;
            border: 1px solid var(--rule-strong);
            background: var(--bg-card);
        }

        .step-tab-btn {
            background: transparent;
            border: none;
            border-right: 1px solid var(--rule);
            padding: 6px 14px;
            font-family: var(--font-mono);
            font-size: 0.68rem;
            font-weight: 500;
            letter-spacing: 0.1em;
            text-transform: uppercase;
            color: var(--ink-muted);
            cursor: pointer;
            transition: background-color 0.18s ease, color 0.18s ease;
        }

        .step-tab-btn:last-child {
            border-right: none;
        }

        .step-tab-btn:hover {
            color: var(--ink);
        }

        .step-tab-btn.active {
            background: var(--ink);
            color: var(--bg-paper);
            font-weight: 600;
        }

        /* Shared Archival Typography & Section Headers */
        .liner-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            padding-bottom: 14px;
            border-bottom: 1px solid var(--rule-strong);
            gap: 16px;
            flex-wrap: wrap;
        }

        .liner-kicker {
            font-family: var(--font-mono);
            font-size: 0.72rem;
            font-weight: 500;
            letter-spacing: 0.14em;
            text-transform: uppercase;
            color: var(--accent);
            margin-bottom: 4px;
        }

        .liner-title {
            font-family: var(--font-serif);
            font-size: clamp(2.1rem, 3.4vw, 3.1rem);
            font-weight: 400;
            line-height: 1.02;
            letter-spacing: -0.015em;
            color: var(--ink);
        }

        .liner-subtitle {
            font-family: var(--font-mono);
            font-size: 0.72rem;
            letter-spacing: 0.08em;
            text-transform: uppercase;
            color: var(--ink-muted);
            text-align: right;
        }

        .mono-label {
            font-family: var(--font-mono);
            font-size: 0.68rem;
            font-weight: 500;
            letter-spacing: 0.13em;
            text-transform: uppercase;
            color: var(--ink-muted);
        }

        .serif-val {
            font-family: var(--font-serif);
            font-weight: 400;
            line-height: 0.96;
            color: var(--ink);
        }

        /* Tactile Album Cover Sleeve Frame with Silent Monogram Fallback */
        .sleeve-frame {
            position: relative;
            aspect-ratio: 1 / 1;
            background: #23201D;
            border: 1px solid var(--rule);
            overflow: hidden;
            flex-shrink: 0;
            box-shadow: 0 10px 26px rgba(0, 0, 0, 0.14);
        }

        /* Subtle vinyl jacket crease sheen */
        .sleeve-frame::after {
            content: '';
            position: absolute;
            inset: 0;
            pointer-events: none;
            z-index: 3;
            background: linear-gradient(135deg, rgba(255,255,255,0.10) 0%, rgba(255,255,255,0) 42%, rgba(0,0,0,0.18) 100%);
            border: 1px solid rgba(255,255,255,0.06);
        }

        .sleeve-fallback {
            position: absolute;
            inset: 0;
            z-index: 1;
            display: flex;
            align-items: center;
            justify-content: center;
            background:
                radial-gradient(circle at center, rgba(255,255,255,0.06) 0, rgba(255,255,255,0.06) 14%, transparent 15%),
                repeating-radial-gradient(circle at center, rgba(255,255,255,0.025) 0px, rgba(255,255,255,0.025) 2px, transparent 2px, transparent 6px),
                linear-gradient(145deg, #272320, #161412);
            color: #D8D0C3;
            font-family: var(--font-serif);
            font-style: italic;
            font-size: 1.35em;
            letter-spacing: 0.03em;
            user-select: none;
        }

        .sleeve-fallback span {
            padding: 6px 10px;
            border: 1px solid rgba(216, 208, 195, 0.22);
            background: rgba(22, 20, 18, 0.72);
            line-height: 1;
        }

        .sleeve-frame img {
            position: absolute;
            inset: 0;
            width: 100%;
            height: 100%;
            object-fit: cover;
            z-index: 2;
            display: block;
        }

        /* Editorial Footnote Strip */
        .liner-footnote {
            border-top: 1px solid var(--rule);
            padding-top: 12px;
            display: flex;
            align-items: baseline;
            justify-content: space-between;
            gap: 20px;
            font-size: 0.88rem;
            color: var(--ink-secondary);
        }

        .liner-footnote strong {
            color: var(--ink);
            font-weight: 600;
        }

        /* Bottom Navigation Controls */
        #nav-controls {
            position: fixed;
            bottom: 18px;
            left: 50%;
            transform: translateX(-50%);
            z-index: 60;
            display: flex;
            align-items: center;
            gap: 16px;
            padding: 8px 18px;
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
            transition: background-color 0.45s ease, border-color 0.35s ease;
        }

        .nav-btn {
            background: transparent;
            border: none;
            color: var(--ink);
            font-family: var(--font-mono);
            font-size: 0.72rem;
            font-weight: 600;
            letter-spacing: 0.12em;
            padding: 4px 8px;
            cursor: pointer;
            transition: color 0.18s ease, opacity 0.18s ease;
        }

        .nav-btn:hover:not(:disabled) {
            color: var(--accent);
        }

        .nav-btn:disabled {
            opacity: 0.3;
            cursor: not-allowed;
        }

        #slide-indicator {
            display: flex;
            align-items: center;
            gap: 7px;
            padding: 0 10px;
            border-left: 1px solid var(--rule);
            border-right: 1px solid var(--rule);
        }

        .indicator-dot {
            width: 8px;
            height: 8px;
            border-radius: 0;
            border: 1px solid var(--ink-muted);
            background: transparent;
            cursor: pointer;
            padding: 0;
            transition: all 0.2s ease;
        }

        .indicator-dot:hover {
            border-color: var(--ink);
            background: var(--rule);
        }

        .indicator-dot.active {
            width: 22px;
            background: var(--accent);
            border-color: var(--accent);
        }

        @media (max-width: 960px) {
            #app {
                padding: 62px 18px 80px;
            }
            .app-topbar {
                padding: 0 18px;
            }
        }
";
        }

        public string GetYearSelectorCSS()
        {
            return @"
        :root {
            --bg-paper: #E0D9CD;
            --bg-card: #EAE4D9;
            --ink: #181614;
            --ink-secondary: #4A453E;
            --ink-muted: #736C62;
            --rule: rgba(24, 22, 20, 0.18);
            --rule-strong: #181614;
            --accent: #B3391E;
            --font-serif: 'Instrument Serif', Georgia, serif;
            --font-sans: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
            --font-mono: 'JetBrains Mono', monospace;
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            min-height: 100vh;
            background: var(--bg-paper);
            color: var(--ink);
            font-family: var(--font-sans);
            padding: 64px 28px;
            display: flex;
            justify-content: center;
            align-items: flex-start;
        }

        .year-selector-container {
            width: 100%;
            max-width: 1080px;
        }

        .year-selector-header {
            border-bottom: 2px solid var(--rule-strong);
            padding-bottom: 22px;
            margin-bottom: 36px;
        }

        .archive-kicker {
            font-family: var(--font-mono);
            font-size: 0.75rem;
            letter-spacing: 0.16em;
            color: var(--accent);
            margin-bottom: 8px;
        }

        .year-selector-header h1 {
            font-family: var(--font-serif);
            font-size: clamp(3rem, 5vw, 4.5rem);
            font-weight: 400;
            line-height: 0.95;
            margin-bottom: 8px;
        }

        .year-selector-subtitle {
            color: var(--ink-secondary);
            font-size: 1rem;
        }

        .years-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(310px, 1fr));
            gap: 24px;
        }

        .year-card {
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            text-decoration: none;
            color: inherit;
            background: var(--bg-card);
            border: 1px solid var(--rule-strong);
            padding: 28px;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

        .year-card:hover {
            transform: translateY(-4px);
            box-shadow: 6px 6px 0 var(--rule-strong);
        }

        .year-title {
            font-family: var(--font-serif);
            font-size: 3.6rem;
            line-height: 0.95;
            color: var(--ink);
            border-bottom: 1px solid var(--rule);
            padding-bottom: 14px;
            margin-bottom: 18px;
        }

        .year-stats {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 16px;
            margin-bottom: 20px;
            padding-bottom: 18px;
            border-bottom: 1px solid var(--rule);
        }

        .year-stat-number {
            font-family: var(--font-serif);
            font-size: 2rem;
            line-height: 1;
            color: var(--accent);
        }

        .year-stat-label {
            font-family: var(--font-mono);
            font-size: 0.68rem;
            text-transform: uppercase;
            letter-spacing: 0.12em;
            color: var(--ink-muted);
            margin-top: 4px;
        }

        .year-highlights {
            display: flex;
            flex-direction: column;
            gap: 6px;
            font-size: 0.86rem;
            color: var(--ink-secondary);
        }

        .year-highlight strong {
            color: var(--ink);
            font-weight: 600;
        }
";
        }
    }
}
