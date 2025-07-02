using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MusicBeeWrapped;

namespace MusicBeePlugin.Services
{
    /// <summary>
    /// Service responsible for managing play history data persistence
    /// </summary>
    public class PlayHistoryService
    {
        private readonly string _historyFilePath;
        private readonly Plugin _plugin;
        
        public PlayHistoryService(string historyFilePath, Plugin plugin)
        {
            _historyFilePath = historyFilePath ?? throw new ArgumentNullException(nameof(historyFilePath));
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }
        
        /// <summary>
        /// Loads play history from disk, creating new instance if file doesn't exist or is corrupted
        /// </summary>
        public MusicBeeWrapped.PlayHistory LoadPlayHistory()
        {
            if (File.Exists(_historyFilePath))
            {
                try
                {
                    using (FileStream fs = new FileStream(_historyFilePath, FileMode.Open))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        var history = (MusicBeeWrapped.PlayHistory)formatter.Deserialize(fs);
                        _plugin.ApiInterface.MB_Trace($"Loaded {history.Plays.Count} plays from history");
                        return history;
                    }
                }
                catch (Exception ex)
                {
                    _plugin.ApiInterface.MB_Trace($"Error loading play history: {ex.Message}");
                    return new MusicBeeWrapped.PlayHistory();
                }
            }
            else
            {
                _plugin.ApiInterface.MB_Trace("No existing play history found, starting fresh");
                return new MusicBeeWrapped.PlayHistory();
            }
        }
        
        /// <summary>
        /// Saves play history to disk
        /// </summary>
        public void SavePlayHistory(MusicBeeWrapped.PlayHistory playHistory)
        {
            try
            {
                using (FileStream fs = new FileStream(_historyFilePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, playHistory);
                }
                _plugin.ApiInterface.MB_Trace($"Saved {playHistory.Plays.Count} plays to history");
            }
            catch (Exception ex)
            {
                _plugin.ApiInterface.MB_Trace($"Error saving play history: {ex.Message}");
            }
        }
    }
}
