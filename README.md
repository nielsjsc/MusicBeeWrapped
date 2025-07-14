# MusicBeeWrapped

A MusicBee plugin that creates personalized yearly music reviews - like Spotify Wrapped, but for your local music library with complete privacy and deeper behavioral insights.

## Why MusicBeeWrapped?

Unlike external services, MusicBeeWrapped keeps all your listening data completely local while providing richer analysis than simple play counts. The main advantages are privacy and data control - everything stays on your machine rather than being sent to external servers. Plus, it's integrated directly into MusicBee, so you don't need a separate account or to visit external websites for your yearly insights.

**Key Benefits:**
- **Complete Privacy**: All data stays on your computer
- **Deeper Analysis**: Tracks listening behavior, not just play counts
- **MusicBee Integration**: Access reports directly from the Tools menu
- **Offline Operation**: Works without internet connectivity
- **Multi-Year Support**: Compare statistics across different years

## Installation

1. Download the latest `mb_MusicBeeWrapped.dll` from the [releases page](https://github.com/yourusername/MusicBeeWrapped/releases)
2. Copy the file to your MusicBee `Plugins` folder:
   - **Windows**: Usually `C:\Program Files (x86)\MusicBee\Plugins\`
   - **Other platforms**: `[MusicBee installation]/Plugins/`
3. Restart MusicBee

The plugin begins tracking your listening habits immediately - no configuration needed.

## Usage

Once installed, MusicBeeWrapped silently tracks your music listening in the background. It uses a 5-second minimum play rule to filter out accidental plays and skips.

**To view your yearly report:**
1. Go to **Tools → MusicBee Wrapped** in the MusicBee menu
2. If you have multiple years of data, select which year to review
3. Your personalized report opens in your default browser

The report includes detailed statistics about your listening habits, favorite songs, discovery patterns, and behavioral insights - all presented in an engaging, slide-based format.

## Data Storage

Your listening data is stored locally in platform-appropriate directories:
- **Windows**: `%APPDATA%\MusicBee\Plugins\MusicBeeWrapped\`
- **Other platforms**: `~/.config/MusicBee/Plugins/MusicBeeWrapped/`

Data is organized by year with automatic backups and handles schema changes automatically.

## Building from Source

Requires .NET Framework 4.7.2 or higher:

```bash
git clone https://github.com/yourusername/MusicBeeWrapped.git
cd MusicBeeWrapped
dotnet build -c Release
```

The compiled plugin outputs to `bin/Release/mb_MusicBeeWrapped.dll`.
