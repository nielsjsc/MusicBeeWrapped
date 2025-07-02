# MusicBeeWrapped

A MusicBee plugin that generates yearly music listening statistics and presents them through a web interface. Think Spotify Wrapped, but for your local music library with more detailed behavioral analysis.

## Overview

This plugin tracks your music listening habits in real-time and generates comprehensive yearly reports. The core differentiator is the behavioral analysis engine that detects listening patterns most streaming services miss: obsession periods where you're completely consumed by a particular artist or album, your album consumption patterns (do you listen to full albums or just skip around), and temporal listening behaviors.

The data is presented through a slide-based web interface that opens in your browser, similar to Spotify Wrapped but with deeper insights into your actual listening behavior rather than just play counts.

## Core Features

**Real-time Tracking**: Implements a 5-second minimum play rule to filter out accidental plays and skips. Uses MusicBee's event system for immediate capture without polling.

**Behavioral Analysis**: Goes beyond simple statistics to identify obsession periods (when you play an artist/album/track intensively over multiple days), classify your listening personality (Album Purist, Track Shuffler, Mood Curator), and analyze temporal patterns in your music consumption.

**Data Architecture**: Year-based data organization with XML persistence, automatic backups, and metadata caching. Cross-platform data directory management handles different OS conventions properly.

**Web Interface**: Single-file HTML generation with embedded CSS and JavaScript. Responsive design with canvas-based charts for data visualization. Multi-year support with year selection interface.

## Installation and Usage

Copy the compiled `mb_MusicBeeWrapped.dll` to your MusicBee `Plugins` folder and restart MusicBee. The plugin begins tracking immediately without configuration.

Access your yearly statistics through Tools → MusicBee Wrapped in the MusicBee menu. If multiple years of data exist, you'll first see a year selection interface. The generated report opens in your default browser and works offline.

Navigation supports keyboard arrows, mouse clicks on navigation dots, or the navigation buttons. The interface is fully responsive for mobile viewing.

## Data Storage

Listening data is stored in platform-appropriate directories:
- Windows: `%APPDATA%\MusicBee\Plugins\MusicBeeWrapped\`
- Other platforms: `~/.config/MusicBee/Plugins/MusicBeeWrapped/`

Data is organized by year with automatic metadata caching for quick year selection. The plugin handles data migration automatically when schema changes occur.

## Architecture

The codebase follows a service-oriented architecture with clear separation between tracking, data persistence, analytics calculation, and UI generation.

**TrackingService** handles real-time music event capture with play validation. It implements the 5-second rule and manages session state to avoid duplicate tracking.

**YearBasedDataService** manages data persistence using year-based file organization. This approach scales better than single-file storage and allows for efficient querying of historical data.

**WrappedStatistics** contains the analytics engine. Beyond basic statistics, it implements obsession detection algorithms, album session analysis, and temporal pattern recognition.

**WebUIService** generates the complete web interface as a single HTML file with embedded assets. This approach eliminates external dependencies and ensures the generated reports work offline indefinitely.

## Building

Requires .NET Framework 4.7.2 or higher. Build with Visual Studio or the .NET CLI:

```
git clone https://github.com/yourusername/MusicBeeWrapped.git
cd MusicBeeWrapped
dotnet build -c Release
```

The compiled plugin is output to `bin/Release/mb_MusicBeeWrapped.dll`.

## Key Implementation Details

**Obsession Detection**: Uses statistical analysis to identify periods where listening frequency exceeds normal patterns by a significant margin. The algorithm accounts for your baseline listening habits to avoid false positives.

**Album Behavior Classification**: Analyzes listening sessions to determine whether you consume music as complete albums, individual tracks, or curated playlists. The classification affects how statistics are presented.

**Temporal Analysis**: Correlates listening patterns with time-of-day, day-of-week, and seasonal data to identify behavioral trends most users aren't aware of.

**Data Integrity**: Implements automatic backup creation before schema migrations, with rollback capability if issues occur during upgrades.

The plugin maintains backwards compatibility with older data formats while migrating to newer schemas transparently.

## Project Structure

```
Models/                   # Data models and domain objects
Services/                 # Core business logic
  TrackingService.cs        # Real-time play detection
  WebUIService.cs           # UI generation and browser integration  
  YearBasedDataService.cs   # Data persistence and retrieval
  XmlDataService.cs         # Serialization utilities
Class1.cs                 # Plugin entry point and MusicBee integration
PlayHistory.cs            # Play data models and validation
WrappedStatistics.cs      # Analytics calculation engine
MusicBeeInterface.cs      # Complete MusicBee API bindings
```

This plugin demonstrates several patterns useful for MusicBee plugin development: proper event handling, cross-platform file management, and integration with external applications (web browsers) while maintaining plugin sandboxing.
