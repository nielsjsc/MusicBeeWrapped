# MusicBeeWrapped

A comprehensive MusicBee plugin that creates Spotify Wrapped-style yearly music statistics with a beautiful web-based interface.

## Features

### 📊 Comprehensive Analytics
- **Real-time tracking** with 5-second minimum play rule
- **Year-based organization** of listening data
- **Enhanced metrics**: listening patterns, skip rates, discovery metrics
- **Cross-platform compatibility** with automatic data directory management

### 🎯 Advanced Insights
- **Obsession period detection** - Find when you were intensely focused on specific artists/albums/tracks
- **Album listening behavior analysis** - Discover if you're an "Album Purist", "Track Shuffler", or "Mood Curator"
- **Temporal pattern analysis** - Your listening habits by time of day, day of week, and season
- **Discovery tracking** - New artists and genres explored throughout the year

### 🌐 Beautiful Web Interface
- **Slide-based presentation** similar to Spotify Wrapped
- **Interactive charts** showing daily listening activity
- **Responsive design** optimized for both desktop and mobile
- **Smooth animations** with counting effects and transitions
- **Multi-year support** with elegant year selection interface

## Installation

1. Download the latest release or build from source
2. Copy `mb_MusicBeeWrapped.dll` to your MusicBee `Plugins` folder
3. Restart MusicBee
4. The plugin will automatically start tracking your music listening

## Usage

### Viewing Your Wrapped
1. In MusicBee, go to **Tools** → **MusicBee Wrapped**
2. If you have multiple years of data, select the year you want to view
3. Your personalized Wrapped will open in your default browser

### Navigation
- Use arrow keys or the navigation buttons to move between slides
- Click the dots at the bottom to jump to specific slides
- The experience is fully responsive and works on mobile devices

## Data Storage

The plugin stores your listening data in:
- **Windows**: `%APPDATA%\MusicBee\Plugins\MusicBeeWrapped\`
- **Other platforms**: `~/.config/MusicBee/Plugins/MusicBeeWrapped/`

Data is organized by year with automatic backups and metadata caching for quick access.

## Technical Features

### Architecture
- **Service-based architecture** with clear separation of concerns
- **Real-time tracking** using MusicBee's event system
- **XML persistence** with automatic data migration
- **Memory-efficient** processing with lazy loading
- **Comprehensive error handling** with graceful degradation

### Advanced Analytics Engine
- **Obsession detection**: Identifies periods of intense listening to specific content
- **Behavioral fingerprinting**: Analyzes your unique listening patterns
- **Album session analysis**: Understands how you consume full albums vs individual tracks
- **Temporal pattern mining**: Discovers your listening habits across different time periods

### Web UI Technology
- **Single-file HTML generation** with embedded CSS and JavaScript
- **Canvas-based charting** for data visualization
- **Progressive enhancement** with fallbacks for older browsers
- **Cross-platform browser launching** with multiple fallback strategies

## Building from Source

### Prerequisites
- .NET Framework 4.7.2 or higher
- Visual Studio 2019+ or .NET CLI

### Build Instructions
```bash
git clone https://github.com/yourusername/MusicBeeWrapped.git
cd MusicBeeWrapped
dotnet build -c Release
```

The compiled plugin will be available in `bin/Release/mb_MusicBeeWrapped.dll`

## Development

### Project Structure
```
MusicBeeWrapped/
├── Models/                 # Data models
│   ├── AlbumListeningBehavior.cs
│   └── ObsessionPeriod.cs
├── Services/              # Core services
│   ├── TrackingService.cs       # Real-time play tracking
│   ├── WebUIService.cs          # Web interface generation
│   ├── YearBasedDataService.cs  # Data persistence
│   └── XmlDataService.cs        # XML utilities
├── Class1.cs              # Main plugin entry point
├── PlayHistory.cs         # Play data models
├── WrappedStatistics.cs   # Statistics calculation
└── MusicBeeInterface.cs   # MusicBee API bindings
```

### Key Components
- **TrackingService**: Handles real-time music tracking with play validation
- **WebUIService**: Generates the complete web-based user interface
- **YearBasedDataService**: Manages year-organized data storage and retrieval
- **WrappedStatistics**: Calculates all statistics and advanced analytics

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- **MusicBee** for providing an excellent plugin API
- **Spotify Wrapped** for inspiration on music analytics presentation
- The open-source community for various libraries and techniques used

## Screenshots

### Year Selection
Beautiful year selector with quick stats preview for each year of data.

### Welcome Slide
Animated introduction showing your total listening statistics.

### Top Artists
Your most-played artists with detailed play counts and smooth reveal animations.

### Obsession Analysis
Discover your musical obsessions - periods when you were completely consumed by specific artists, albums, or tracks.

### Album Behavior
Learn your listening personality - are you an Album Purist or a Track Shuffler?

### Daily Activity Chart
Interactive visualization of your daily listening patterns throughout the year.

---

**Made with ❤️ for music lovers who want to understand their listening habits**
