using System;
using System.IO;
using System.Reflection;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Provides CSS stylesheets for the MusicBee Wrapped web interface
    /// Manages both main interface and year selector styling with proper organization
    /// </summary>
    public class CssStyleProvider
    {
        /// <summary>
        /// Gets the complete CSS for the main wrapped interface
        /// Includes responsive design, animations, and slide styling
        /// </summary>
        /// <returns>Complete CSS stylesheet as string</returns>
        public string GetMainInterfaceCSS()
        {
            return @"
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0f0f23 0%, #1a1a2e 50%, #16213e 100%);
            color: white;
            overflow: hidden;
            height: 100vh;
        }

        #app {
            position: relative;
            width: 100%;
            height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .slide {
            position: absolute;
            width: 100%;
            height: 100%;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            opacity: 0;
            transform: translateX(0);
            transition: none; /* Transitions handled by JavaScript for cinematic effect */
            padding: 2rem;
            text-align: center;
            will-change: transform, opacity, filter;
            backface-visibility: hidden;
            perspective: 1000px;
        }

        .slide.active {
            opacity: 1;
            transform: translateX(0);
        }

        .slide.prev {
            transform: translateX(-100%);
        }

        /* Cinematic transition support */
        .slide.transitioning {
            transition: all 0.8s cubic-bezier(0.4, 0, 0.2, 1);
        }

        .transition-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 1000;
            pointer-events: none;
        }

        .loading-content h1 {
            font-size: 3rem;
            margin-bottom: 2rem;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1, #96ceb4);
            background-size: 300% 300%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            animation: gradientShift 3s ease-in-out infinite;
        }

        .loading-spinner {
            width: 60px;
            height: 60px;
            border: 3px solid rgba(255, 255, 255, 0.1);
            border-top: 3px solid #4ecdc4;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto;
        }

        #nav-controls {
            position: fixed;
            bottom: 2rem;
            left: 50%;
            transform: translateX(-50%);
            display: flex;
            align-items: center;
            gap: 1rem;
            z-index: 1000;
        }

        .nav-btn {
            background: rgba(255, 255, 255, 0.1);
            border: 2px solid rgba(255, 255, 255, 0.2);
            color: white;
            width: 50px;
            height: 50px;
            border-radius: 50%;
            cursor: pointer;
            font-size: 1.2rem;
            transition: all 0.3s ease;
            backdrop-filter: blur(10px);
        }

        .nav-btn:hover {
            background: rgba(255, 255, 255, 0.2);
            border-color: rgba(255, 255, 255, 0.4);
            transform: scale(1.1);
        }

        .nav-btn:disabled {
            opacity: 0.3;
            cursor: not-allowed;
            transform: scale(1);
        }

        #slide-indicator {
            display: flex;
            gap: 0.5rem;
        }

        .indicator-dot {
            width: 12px;
            height: 12px;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.3);
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .indicator-dot.active {
            background: #4ecdc4;
            transform: scale(1.2);
        }

        .indicator-dot:hover {
            background: rgba(255, 255, 255, 0.5);
        }

        .slide h1 {
            font-size: 3.5rem;
            margin-bottom: 1rem;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4);
            background-size: 200% 200%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            animation: gradientShift 2s ease-in-out infinite;
        }

        .slide h2 {
            font-size: 2.5rem;
            margin-bottom: 2rem;
            color: #4ecdc4;
        }

        .slide h3 {
            font-size: 1.8rem;
            margin-bottom: 1rem;
            color: #96ceb4;
        }

        /* Enhanced Statistics Styling */
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 3rem;
            margin-top: 3rem;
            max-width: 1000px;
            width: 100%;
        }

        .stat-card {
            background: rgba(255, 255, 255, 0.05);
            border-radius: 20px;
            padding: 2rem;
            border: 1px solid rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
            transition: all 0.3s ease;
            transform: translateY(20px);
            opacity: 0;
        }

        .stat-card.animate {
            transform: translateY(0);
            opacity: 1;
        }

        .stat-card:hover {
            transform: translateY(-5px);
            border-color: rgba(78, 205, 196, 0.3);
            box-shadow: 0 10px 30px rgba(78, 205, 196, 0.1);
        }

        .stat-number {
            font-size: 3.5rem;
            font-weight: 700;
            color: #4ecdc4;
            margin: 0.5rem 0;
            counter-reset: stat-counter 0;
        }

        .stat-number.animate {
            animation: countUp 2s ease-out;
        }

        .stat-label {
            font-size: 1.1rem;
            color: rgba(255, 255, 255, 0.8);
            text-transform: uppercase;
            letter-spacing: 2px;
            font-weight: 500;
        }

        /* Progress Bars */
        .progress-bar {
            width: 100%;
            height: 8px;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 4px;
            overflow: hidden;
            margin: 1rem 0;
        }

        .progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #4ecdc4, #45b7d1);
            border-radius: 4px;
            width: 0%;
            transition: width 2s ease-out;
        }

        .progress-fill.animate {
            width: var(--progress-width);
        }
        }

        /* Top Lists Styling */
        .top-list {
            max-width: 700px;
            width: 100%;
            margin-top: 3rem;
        }

        .top-item {
            display: flex;
            align-items: center;
            padding: 1.5rem;
            margin: 1rem 0;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 15px;
            border-left: 4px solid #4ecdc4;
            transition: all 0.3s ease;
            opacity: 1;
            transform: translateX(0);
        }

        .top-item.animate {
            transform: translateX(0);
            opacity: 1;
        }

        .top-item:hover {
            background: rgba(255, 255, 255, 0.1);
            transform: translateX(10px);
        }

        .top-rank {
            font-size: 2rem;
            font-weight: bold;
            color: #4ecdc4;
            width: 4rem;
            text-align: center;
        }

        .top-content {
            flex: 1;
            text-align: left;
            margin-left: 1.5rem;
        }

        .top-name {
            font-size: 1.4rem;
            font-weight: 600;
            margin-bottom: 0.3rem;
            color: #ffffff;
        }

        .top-subtitle {
            font-size: 1rem;
            color: rgba(255, 255, 255, 0.6);
        }

        .top-count {
            font-size: 1.2rem;
            color: rgba(255, 255, 255, 0.7);
            font-weight: 500;
        }

        .chart-container {
            width: 100%;
            max-width: 800px;
            height: 400px;
            margin: 2rem 0;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 15px;
            padding: 1rem;
            backdrop-filter: blur(10px);
        }

        .insight-box {
            background: linear-gradient(135deg, rgba(255, 107, 107, 0.2), rgba(78, 205, 196, 0.2));
            border: 1px solid rgba(255, 255, 255, 0.3);
            border-radius: 15px;
            padding: 2rem;
            margin: 2rem 0;
            max-width: 700px;
            backdrop-filter: blur(10px);
        }

        .insight-box h3 {
            color: #ff6b6b;
            margin-bottom: 1rem;
        }

        .insight-text {
            font-size: 1.2rem;
            line-height: 1.6;
            color: rgba(255, 255, 255, 0.9);
        }

        @keyframes gradientShift {
            0%, 100% { background-position: 0% 50%; }
            50% { background-position: 100% 50%; }
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        @keyframes countUp {
            from { transform: scale(0.8); opacity: 0; }
            to { transform: scale(1); opacity: 1; }
        }

        .fade-in-up {
            animation: fadeInUp 0.6s ease-out forwards;
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .slide h1 {
                font-size: 2.5rem;
            }

            .slide h2 {
                font-size: 2rem;
            }

            .nav-btn {
                width: 40px;
                height: 40px;
                font-size: 1rem;
            }

            .stats-grid {
                grid-template-columns: 1fr;
                gap: 1rem;
            }

            .stat-card {
                padding: 1.5rem;
            }

            .chart-container {
                height: 300px;
            }

            .slide {
                padding: 1rem;
            }
        }

        @media (max-width: 480px) {
            .slide h1 {
                font-size: 2rem;
            }

            .loading-content h1 {
                font-size: 2rem;
            }

            .stat-number {
                font-size: 2rem;
            }

            .top-list li {
                padding: 0.8rem 1rem;
                font-size: 0.9rem;
            }

            .insight-box {
                padding: 1.5rem;
                margin: 1rem 0;
            }
        }

        /* Night Sky Top Tracks - Elegant & Serene */
        .night-sky-container {
            position: relative;
            width: 100%;
            height: 100vh;
            overflow: hidden;
        }

        .night-sky {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: linear-gradient(180deg, 
                #0f0f23 0%, 
                #1a1a2e 40%, 
                #16213e 70%, 
                #0f1419 100%);
        }

        /* Twinkling stars */
        .star {
            position: absolute;
            width: 2px;
            height: 2px;
            background: white;
            border-radius: 50%;
            animation: twinkle 3s ease-in-out infinite;
        }

        .star-1 { top: 15%; left: 20%; animation-delay: 0s; }
        .star-2 { top: 25%; left: 80%; animation-delay: 0.5s; }
        .star-3 { top: 35%; left: 60%; animation-delay: 1s; }
        .star-4 { top: 45%; left: 30%; animation-delay: 1.5s; }
        .star-5 { top: 20%; left: 70%; animation-delay: 2s; }
        .star-6 { top: 40%; left: 85%; animation-delay: 2.5s; }
        .star-7 { top: 30%; left: 15%; animation-delay: 3s; }
        .star-8 { top: 50%; left: 50%; animation-delay: 3.5s; }
        .star-9 { top: 18%; left: 45%; animation-delay: 4s; }
        .star-10 { top: 42%; left: 75%; animation-delay: 4.5s; }

        @keyframes twinkle {
            0%, 100% { opacity: 0.3; transform: scale(1); }
            50% { opacity: 1; transform: scale(1.2); }
        }

        /* Shooting comets */
        .comet {
            position: absolute;
            width: 2px;
            height: 2px;
            background: linear-gradient(45deg, white, transparent);
            border-radius: 50%;
            box-shadow: 0 0 6px rgba(255, 255, 255, 0.8);
        }

        .comet-1 {
            top: 20%;
            left: -10%;
            animation: shootingComet 12s linear infinite;
        }

        .comet-2 {
            top: 40%;
            left: -15%;
            animation: shootingComet 15s linear infinite;
            animation-delay: 8s;
        }

        @keyframes shootingComet {
            0% { 
                transform: translateX(0) translateY(0);
                opacity: 0;
            }
            10% { opacity: 1; }
            90% { opacity: 1; }
            100% { 
                transform: translateX(120vw) translateY(-30vh);
                opacity: 0;
            }
        }

        /* Mountain silhouette */
        .mountains {
            position: absolute;
            bottom: 0;
            left: 0;
            width: 100%;
            height: 25vh;
            background: linear-gradient(to top, #000 0%, #1a1a2e 100%);
            clip-path: polygon(
                0% 100%, 
                0% 60%, 
                15% 50%, 
                25% 55%, 
                35% 45%, 
                45% 40%, 
                55% 50%, 
                65% 35%, 
                75% 45%, 
                85% 30%, 
                95% 40%, 
                100% 35%, 
                100% 100%
            );
        }

        /* Content overlay */
        .content-overlay {
            position: relative;
            z-index: 10;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            height: 100vh;
            padding: clamp(2rem, 4vw, 4rem);
        }

        .slide-header {
            text-align: center;
            margin-bottom: clamp(2rem, 4vw, 4rem);
            animation: fadeInDown 1s ease-out;
        }

        .slide-header h2 {
            font-size: clamp(2.5rem, 5vw, 4rem);
            font-weight: 700;
            margin-bottom: clamp(0.5rem, 1vw, 1rem);
            color: white;
            text-shadow: 0 0 20px rgba(255, 255, 255, 0.3);
        }

        .slide-header p {
            font-size: clamp(1.1rem, 2vw, 1.6rem);
            color: rgba(255, 255, 255, 0.8);
            font-weight: 300;
        }

        /* Tracks list */
        .tracks-list {
            width: 100%;
            max-width: clamp(500px, 60vw, 800px);
            margin: 0 auto;
        }

        .track-item {
            display: flex;
            align-items: center;
            padding: clamp(1rem, 2vw, 1.8rem);
            margin-bottom: clamp(0.8rem, 1.5vw, 1.5rem);
            background: rgba(255, 255, 255, 0.08);
            backdrop-filter: blur(10px);
            border-radius: clamp(12px, 2vw, 20px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            transition: all 0.3s ease;
            animation: fadeInUp 0.8s ease-out forwards;
            opacity: 0;
            transform: translateY(20px);
            position: relative;
            cursor: pointer;
        }

        .track-item:hover {
            background: rgba(255, 255, 255, 0.12);
            border-color: rgba(78, 205, 196, 0.4);
            transform: translateY(-2px);
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
        }

        .track-item:hover .play-count-tooltip {
            opacity: 1;
            transform: translateY(-5px);
        }

        @keyframes fadeInUp {
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        @keyframes fadeInDown {
            from {
                opacity: 0;
                transform: translateY(-20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .track-rank {
            font-size: clamp(1.2rem, 2.5vw, 2rem);
            font-weight: bold;
            color: #4ecdc4;
            width: clamp(2.5rem, 4vw, 3.5rem);
            text-align: center;
            margin-right: clamp(1rem, 2vw, 1.5rem);
        }

        .track-info {
            flex: 1;
            min-width: 0;
        }

        .track-title {
            font-size: clamp(1.1rem, 2vw, 1.5rem);
            font-weight: 600;
            color: white;
            margin-bottom: clamp(0.2rem, 0.5vw, 0.4rem);
            text-shadow: 0 0 10px rgba(255, 255, 255, 0.2);
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .track-artist {
            font-size: clamp(0.9rem, 1.6vw, 1.2rem);
            color: rgba(255, 255, 255, 0.7);
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .play-count-tooltip {
            position: absolute;
            top: -40px;
            right: 20px;
            background: rgba(0, 0, 0, 0.9);
            color: #4ecdc4;
            padding: 0.5rem 1rem;
            border-radius: 8px;
            font-size: 0.9rem;
            font-weight: 500;
            opacity: 0;
            pointer-events: none;
            transition: all 0.3s ease;
            white-space: nowrap;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        }

        .play-count-tooltip::after {
            content: '';
            position: absolute;
            top: 100%;
            right: 20px;
            width: 0;
            height: 0;
            border-left: 5px solid transparent;
            border-right: 5px solid transparent;
            border-top: 5px solid rgba(0, 0, 0, 0.9);
        }

        /* Responsive design */
        @media (max-width: 768px) {
            .content-overlay {
                padding: clamp(1rem, 3vw, 2rem);
            }
            
            .tracks-list {
                max-width: 95vw;
            }
            
            .track-item {
                padding: clamp(0.8rem, 1.5vw, 1.2rem);
                margin-bottom: clamp(0.6rem, 1vw, 1rem);
            }
        }

        @media (max-width: 480px) {
            .track-rank {
                width: clamp(2rem, 3vw, 2.5rem);
                margin-right: clamp(0.8rem, 1.5vw, 1rem);
            }
            
            .play-count-tooltip {
                right: 10px;
                top: -35px;
            }
        }
";
        }

        /// <summary>
        /// Gets the CSS for the year selector interface
        /// Focused on grid layout and card-based year selection
        /// </summary>
        /// <returns>Year selector CSS stylesheet as string</returns>
        public string GetYearSelectorCSS()
        {
            return @"
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #0f0f23 0%, #1a1a2e 50%, #16213e 100%);
            color: white;
            min-height: 100vh;
            padding: 2rem;
        }

        .year-selector-container {
            max-width: 1200px;
            margin: 0 auto;
            text-align: center;
        }

        .year-selector-header {
            margin-bottom: 3rem;
        }

        .year-selector-header h1 {
            font-size: 3.5rem;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1, #96ceb4);
            background-size: 300% 300%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            animation: gradientShift 3s ease-in-out infinite;
            margin-bottom: 1rem;
        }

        .year-selector-subtitle {
            font-size: 1.2rem;
            opacity: 0.8;
            margin-bottom: 2rem;
        }

        .years-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 2rem;
            margin-bottom: 3rem;
        }

        .year-card {
            background: rgba(255, 255, 255, 0.1);
            border: 2px solid rgba(255, 255, 255, 0.2);
            border-radius: 20px;
            padding: 2rem;
            cursor: pointer;
            transition: all 0.3s ease;
            backdrop-filter: blur(10px);
            text-decoration: none;
            color: inherit;
        }

        .year-card:hover {
            transform: translateY(-10px);
            border-color: #4ecdc4;
            background: rgba(78, 205, 196, 0.2);
            box-shadow: 0 10px 30px rgba(78, 205, 196, 0.3);
        }

        .year-title {
            font-size: 2.5rem;
            font-weight: bold;
            color: #4ecdc4;
            margin-bottom: 1rem;
        }

        .year-stats {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1rem;
            margin-bottom: 1rem;
        }

        .year-stat {
            text-align: center;
        }

        .year-stat-number {
            font-size: 1.5rem;
            font-weight: bold;
            color: #96ceb4;
        }

        .year-stat-label {
            font-size: 0.9rem;
            opacity: 0.7;
            margin-top: 0.25rem;
        }

        .year-highlights {
            margin-top: 1.5rem;
            padding-top: 1.5rem;
            border-top: 1px solid rgba(255, 255, 255, 0.2);
        }

        .year-highlight {
            font-size: 0.9rem;
            margin: 0.5rem 0;
            opacity: 0.8;
        }

        .year-highlight strong {
            color: #ff6b6b;
        }

        @keyframes gradientShift {
            0%, 100% { background-position: 0% 50%; }
            50% { background-position: 100% 50%; }
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .years-grid {
                grid-template-columns: 1fr;
                gap: 1.5rem;
            }

            .year-selector-header h1 {
                font-size: 2.5rem;
            }

            .year-card {
                padding: 1.5rem;
            }

            .year-title {
                font-size: 2rem;
            }

            body {
                padding: 1rem;
            }
        }

        @media (max-width: 480px) {
            .year-selector-header h1 {
                font-size: 2rem;
            }

            .year-stats {
                grid-template-columns: 1fr;
                gap: 0.5rem;
            }

            .year-stat-number {
                font-size: 1.2rem;
            }
        }";
        }

        /// <summary>
        /// Gets shared CSS utilities and base styles
        /// Common styles that can be reused across different interfaces
        /// </summary>
        /// <returns>Shared CSS utilities as string</returns>
        public string GetSharedUtilities()
        {
            return @"
        /* Shared Color Variables and Utilities */
        :root {
            --primary-gradient: linear-gradient(135deg, #0f0f23 0%, #1a1a2e 50%, #16213e 100%);
            --accent-color: #4ecdc4;
            --secondary-color: #96ceb4;
            --warning-color: #ff6b6b;
            --text-primary: #ffffff;
            --text-secondary: rgba(255, 255, 255, 0.8);
            --glass-bg: rgba(255, 255, 255, 0.1);
            --glass-border: rgba(255, 255, 255, 0.2);
        }

        /* Utility Classes */
        .glass-card {
            background: var(--glass-bg);
            border: 1px solid var(--glass-border);
            border-radius: 15px;
            backdrop-filter: blur(10px);
        }

        .gradient-text {
            background: linear-gradient(45deg, var(--warning-color), var(--accent-color));
            background-size: 200% 200%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            animation: gradientShift 2s ease-in-out infinite;
        }

        .fade-in {
            animation: fadeIn 0.6s ease-out forwards;
        }

        .slide-up {
            animation: slideUp 0.6s ease-out forwards;
        }

        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }

        @keyframes slideUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        /* Song Soulmate Slide Styles */
        .soulmate-container {
            text-align: center;
            position: relative;
            overflow: hidden;
        }

        .soulmate-title {
            font-size: 3rem;
            font-weight: 700;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1);
            background-size: 300% 300%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            animation: gradientShift 3s ease-in-out infinite;
            margin-bottom: 1rem;
        }

        .soulmate-subtitle {
            font-size: 1.3rem;
            color: rgba(255, 255, 255, 0.8);
            margin-bottom: 3rem;
            font-style: italic;
        }

        .soulmate-card {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 4rem;
            max-width: 1000px;
            margin: 0 auto 3rem;
            padding: 2rem;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 20px;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.2);
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
        }

        .soulmate-vinyl {
            flex-shrink: 0;
        }

        .vinyl-disc {
            width: 200px;
            height: 200px;
            background: radial-gradient(circle, #1a1a1a 20%, #333 21%, #1a1a1a 22%, #333 40%, #1a1a1a 41%);
            border-radius: 50%;
            position: relative;
            animation: vinylSpin 8s linear infinite;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
        }

        .vinyl-center {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 60px;
            height: 60px;
            background: linear-gradient(45deg, #4ecdc4, #45b7d1);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5rem;
            color: white;
            font-weight: bold;
        }

        .vinyl-groove {
            position: absolute;
            border-radius: 50%;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .vinyl-groove:nth-child(2) {
            top: 30px; left: 30px; right: 30px; bottom: 30px;
        }

        .vinyl-groove:nth-child(3) {
            top: 50px; left: 50px; right: 50px; bottom: 50px;
        }

        .vinyl-groove:nth-child(4) {
            top: 70px; left: 70px; right: 70px; bottom: 70px;
        }

        .soulmate-info {
            flex: 1;
            text-align: left;
        }

        .soulmate-track {
            font-size: 2.5rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 0.5rem;
            line-height: 1.2;
        }

        .soulmate-artist {
            font-size: 1.5rem;
            color: rgba(255, 255, 255, 0.8);
            margin-bottom: 2rem;
            font-style: italic;
        }

        .soulmate-stat-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 2rem;
        }

        .soulmate-stat {
            text-align: center;
            padding: 1rem;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 10px;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .soulmate-stat .stat-number {
            font-size: 2rem;
            font-weight: 700;
            color: #ff6b6b;
            display: block;
            margin-bottom: 0.5rem;
        }

        .soulmate-stat .stat-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .soulmate-message {
            max-width: 600px;
            margin: 0 auto;
            font-size: 1.2rem;
            line-height: 1.6;
            color: rgba(255, 255, 255, 0.9);
        }

        .soulmate-message p {
            margin-bottom: 1rem;
        }

        .soulmate-empty {
            padding: 4rem 2rem;
            text-align: center;
        }

        .empty-vinyl {
            font-size: 8rem;
            color: rgba(255, 255, 255, 0.3);
            margin-bottom: 2rem;
            animation: pulse 2s ease-in-out infinite;
        }

        .floating-hearts {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            overflow: hidden;
        }

        .floating-hearts::before,
        .floating-hearts::after {
            content: '💕';
            position: absolute;
            font-size: 2rem;
            animation: floatHeart 6s infinite linear;
            opacity: 0.6;
        }

        .floating-hearts::before {
            left: 10%;
            animation-delay: 0s;
        }

        .floating-hearts::after {
            right: 10%;
            animation-delay: 3s;
        }

        @keyframes vinylSpin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        @keyframes floatHeart {
            0% {
                transform: translateY(100vh) rotate(0deg);
                opacity: 0;
            }
            10% {
                opacity: 0.6;
            }
            90% {
                opacity: 0.6;
            }
            100% {
                transform: translateY(-100px) rotate(360deg);
                opacity: 0;
            }
        }

        @keyframes pulse {
            0%, 100% { 
                transform: scale(1); 
                opacity: 1; 
            }
            50% { 
                transform: scale(1.05); 
                opacity: 0.8; 
            }
        }

        /* Musical Journey Timeline Styles */
        .journey-slide-container {
            width: 100%;
            max-width: 1200px;
            margin: 0 auto;
            padding: 1rem;
            height: 100%;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }

        .journey-header {
            text-align: center;
            margin-bottom: 3rem;
        }

        .journey-title {
            font-size: 2.5rem;
            font-weight: 700;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1, #96ceb4);
            background-size: 300% 300%;
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            animation: gradientShift 4s ease-in-out infinite;
            margin-bottom: 0.5rem;
        }

        .journey-subtitle {
            font-size: 1.1rem;
            color: rgba(255, 255, 255, 0.8);
            font-style: italic;
            margin: 0;
        }

        .journey-timeline-horizontal {
            display: flex;
            align-items: center;
            justify-content: space-between;
            width: 100%;
            margin: 2rem 0;
            position: relative;
            padding: 2rem 0;
        }

        .timeline-milestone {
            display: flex;
            flex-direction: column;
            align-items: center;
            position: relative;
            flex: 0 0 auto;
            z-index: 2;
        }

        .milestone-marker {
            width: 80px;
            height: 80px;
            border-radius: 50%;
            background: linear-gradient(135deg, rgba(255, 255, 255, 0.15), rgba(255, 255, 255, 0.05));
            border: 3px solid rgba(78, 205, 196, 0.4);
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 1rem;
            transition: all 0.4s ease;
            backdrop-filter: blur(10px);
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
        }

        .milestone-marker:hover {
            transform: translateY(-5px);
            border-color: rgba(78, 205, 196, 0.8);
            box-shadow: 0 15px 40px rgba(0, 0, 0, 0.4);
        }

        .peak-marker {
            border-color: rgba(255, 107, 107, 0.4);
            background: linear-gradient(135deg, rgba(255, 107, 107, 0.15), rgba(255, 107, 107, 0.05));
        }

        .peak-marker:hover {
            border-color: rgba(255, 107, 107, 0.8);
        }

        .milestone-icon {
            font-size: 2rem;
            filter: drop-shadow(0 0 10px rgba(255, 255, 255, 0.3));
        }

        .milestone-card {
            background: rgba(255, 255, 255, 0.08);
            border-radius: 12px;
            padding: 1.2rem;
            min-width: 180px;
            max-width: 220px;
            border: 1px solid rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(15px);
            text-align: center;
            transition: all 0.3s ease;
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.2);
        }

        .milestone-card:hover {
            background: rgba(255, 255, 255, 0.12);
            transform: translateY(-3px);
            box-shadow: 0 12px 35px rgba(0, 0, 0, 0.3);
        }

        .peak-card {
            border-color: rgba(255, 107, 107, 0.2);
            background: rgba(255, 107, 107, 0.08);
        }

        .peak-card:hover {
            background: rgba(255, 107, 107, 0.12);
        }

        .milestone-date {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 0.5rem;
        }

        .milestone-card h4 {
            font-size: 1.1rem;
            color: #4ecdc4;
            font-weight: 600;
            margin-bottom: 0.8rem;
        }

        .peak-card h4 {
            color: #ff6b6b;
        }

        .milestone-song {
            margin-top: 0.5rem;
        }

        .song-name {
            font-size: 1rem;
            font-weight: 600;
            color: white;
            margin-bottom: 0.3rem;
            line-height: 1.2;
        }

        .song-artist {
            font-size: 0.85rem;
            color: rgba(255, 255, 255, 0.7);
            font-style: italic;
        }

        .timeline-connector {
            flex: 1;
            height: 2px;
            position: relative;
            margin: 0 1rem;
            background: linear-gradient(90deg, rgba(78, 205, 196, 0.3), rgba(255, 107, 107, 0.3));
            border-radius: 1px;
        }

        .connector-line {
            width: 100%;
            height: 100%;
            background: inherit;
            position: relative;
            overflow: hidden;
        }

        .connector-line::before {
            content: '';
            position: absolute;
            top: 0;
            left: -100%;
            width: 100%;
            height: 100%;
            background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.4), transparent);
            animation: shimmer 3s infinite;
        }

        @keyframes shimmer {
            0% { left: -100%; }
            100% { left: 100%; }
        }

        /* Artist Showcase Styles */
        .artist-showcase {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 3rem;
            max-width: 800px;
            margin: 0 auto;
        }

        .artist-card {
            text-align: center;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 20px;
            padding: 3rem;
            border: 1px solid rgba(255, 255, 255, 0.2);
            backdrop-filter: blur(10px);
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
        }

        .artist-icon {
            font-size: 4rem;
            margin-bottom: 1rem;
        }

        .artist-name {
            font-size: 2.5rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 1rem;
            line-height: 1.2;
        }

        .artist-plays {
            font-size: 1.5rem;
            color: rgba(255, 255, 255, 0.8);
        }

        .artist-stats {
            display: flex;
            gap: 3rem;
            justify-content: center;
        }

        .stat-item {
            text-align: center;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 15px;
            padding: 2rem;
            border: 1px solid rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
        }

        .stat-value {
            font-size: 2.5rem;
            font-weight: 700;
            color: #ff6b6b;
            margin-bottom: 0.5rem;
        }

        .stat-item .stat-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        /* Chart Styles */
        .chart-container {
            width: 100%;
            max-width: 1200px;
            height: 500px;
            margin: 2rem auto;
            position: relative;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 15px;
            padding: 1rem;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
        }

        .chart-container canvas {
            width: 100%;
            height: 100%;
            border-radius: 10px;
        }

        .chart-stats {
            display: flex;
            gap: 3rem;
            justify-content: center;
            margin-top: 2rem;
        }

        .chart-stat {
            text-align: center;
        }

        .chart-stat .stat-number {
            font-size: 2.5rem;
            font-weight: 700;
            color: #4ecdc4;
            margin-bottom: 0.5rem;
        }

        .chart-stat .stat-label {
            font-size: 0.9rem;
            color: rgba(255, 255, 255, 0.7);
            text-transform: uppercase;
            letter-spacing: 1px;
        }";
        }

        /// <summary>
        /// Combines all CSS components into a single stylesheet
        /// Used when a complete CSS bundle is needed
        /// </summary>
        /// <param name="includeYearSelector">Whether to include year selector styles</param>
        /// <returns>Complete combined CSS stylesheet</returns>
        public string GetCombinedCSS(bool includeYearSelector = false)
        {
            var css = new System.Text.StringBuilder();
            
            css.AppendLine(GetSharedUtilities());
            css.AppendLine(GetMainInterfaceCSS());
            
            if (includeYearSelector)
            {
                css.AppendLine(GetYearSelectorCSS());
            }
            
            return css.ToString();
        }
    }
}
