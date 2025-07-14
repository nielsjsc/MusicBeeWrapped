# MusicBeeWrapped

A MusicBee plugin that generates yearly music listening statistics and presents them through a web interface. Think Spotify Wrapped, but for your local music library with more detailed behavioral analysis.

## Overview

This plugin tracks your music listening habits in real-time, and stores them in an XML file. It then generates comprehensive yearly reports when you prompt.

The data is presented through a slide-based web interface that opens in your browser, similar to Spotify Wrapped but with deeper insights into your actual listening behavior rather than just play counts.

## Core Features

**Real-time Tracking**: Implements a 5-second minimum play rule to filter out accidental plays and skips. Uses MusicBee's event system for immediate capture without polling.

**Data Architecture**: Year-based data organization with XML persistence, automatic backups, and metadata caching. Cross-platform data directory management handles different OS conventions properly.

**Web Interface**: Single-file HTML generation with embedded CSS and JavaScript. Responsive design with canvas-based charts for data visualization. Multi-year support with year selection interface.

## Installation and Usage

Copy the compiled `mb_MusicBeeWrapped.dll` to your MusicBee `Plugins` folder and restart MusicBee. The plugin begins tracking immediately without configuration.

Access your yearly statistics through Tools → MusicBee Wrapped in the MusicBee menu. If multiple years of data exist, you'll first see a year selection interface. The generated report opens in your default browser and works offline.


## Data Storage

Listening data is stored in platform-appropriate directories:
- Windows: `%APPDATA%\MusicBee\Plugins\MusicBeeWrapped\`
- Other platforms: `~/.config/MusicBee/Plugins/MusicBeeWrapped/`

Data is organized by year with automatic metadata caching for quick year selection. The plugin handles data migration automatically when schema changes occur.


## Building

Requires .NET Framework 4.7.2 or higher. Build with Visual Studio or the .NET CLI:

```
git clone https://github.com/yourusername/MusicBeeWrapped.git
cd MusicBeeWrapped
dotnet build -c Release
```

The compiled plugin is output to `bin/Release/mb_MusicBeeWrapped.dll`.




## Project Structure


```
Models/                   # Supporting data models (ObsessionPeriod, AlbumListeningBehavior, etc.)
Services/                 # Core business logic and data access
  TrackingService.cs        # Real-time play detection and session management
  YearBasedDataService.cs   # Year-based data persistence and retrieval
  XmlDataService.cs         # XML serialization utilities
  DataPathService.cs        # Platform-aware data directory management
  PlaylistExportService.cs  # Playlist export logic
  PlayHistoryService.cs     # Play history utilities
  UI/                      # Web interface generation and UI helpers
    WebUIService.cs           # Main web UI generator
    CssStyleProvider.cs        # CSS generation
    JavaScriptProvider.cs      # JS generation and export logic
    HtmlTemplateBuilder.cs     # HTML template builder
    DataSerializer.cs          # Data serialization for UI
    BrowserLauncher.cs         # Opens browser for report
    YearSelectorService.cs     # Multi-year selection UI
    SessionManager.cs          # UI session state
    Slides/                    # Individual slide components (FinaleSlide, TopSongSlide, etc.)
Class1.cs                 # Plugin entry point and MusicBee integration
PlayHistory.cs            # Play data models and validation
WrappedStatistics.cs      # Analytics calculation engine
YearMetadata.cs           # Year summary metadata
MusicBeeInterface.cs      # MusicBee API bindings
Properties/               # Assembly info and project metadata
bin/, obj/                # Build output and intermediate files
```

