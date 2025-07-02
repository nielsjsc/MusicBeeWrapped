using System;
using System.Linq;
using System.Threading;
using MusicBeePlugin.Services;
using MusicBeeWrapped.Services;

namespace MusicBeePlugin
{
    /// <summary>
    /// MusicBee Wrapped Plugin - Entry point for MusicBee plugin integration
    /// Handles plugin lifecycle and delegates specific responsibilities to service classes
    /// </summary>
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();
        
        // Service layer
        private TrackingService trackingService;
        private WebUIService webUIService;
        private YearBasedDataService yearBasedDataService;
        
        // Timer for periodic checks (5-second rule enforcement)
        private Timer periodicCheckTimer;
        
        /// <summary>
        /// Provides access to the MusicBee API for service classes
        /// </summary>
        public MusicBeeApiInterface ApiInterface => mbApiInterface;
        
        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            
            mbApiInterface.MB_Trace("MusicBee Wrapped initializing...");
            
            // Configure plugin information
            ConfigurePluginInfo();
            
            // Initialize services
            InitializeServices();
            
            // Register UI menu items
            RegisterMenuItems();
            
            return about;
        }
        
        private void ConfigurePluginInfo()
        {
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "MusicBee Wrapped";
            about.Description = "Provides Spotify Wrapped-like yearly statistics and visualizations for your MusicBee library";
            about.Author = "Your Name";
            about.TargetApplication = "";
            about.Type = PluginType.General;
            about.VersionMajor = 1;
            about.VersionMinor = 0;
            about.Revision = 1;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = ReceiveNotificationFlags.PlayerEvents | 
                                         ReceiveNotificationFlags.TagEvents;
            about.ConfigurationPanelHeight = 0;
        }
        
        private void InitializeServices()
        {
            // Initialize data directory
            string dataDirectory = DataPathService.InitializeDataDirectory();
            
            // Create new year-based data service
            yearBasedDataService = new YearBasedDataService(dataDirectory);
            
            // Create service instances using the new XML-based system
            trackingService = new TrackingService(this, yearBasedDataService);
            webUIService = new WebUIService(yearBasedDataService);
            
            // Initialize tracking service
            trackingService.Initialize();
            
            // Cleanup old web UI sessions
            webUIService.CleanupOldSessions();
            
            // Start periodic check timer (runs every second to check 5-second rule)
            periodicCheckTimer = new Timer(PeriodicCheckCallback, null, 1000, 1000); // 1 second intervals
            mbApiInterface.MB_Trace("Periodic check timer started for 5-second rule enforcement");
        }
        
        private void RegisterMenuItems()
        {
            // Add menu item to Tools menu for easy access
            mbApiInterface.MB_AddMenuItem("mnuTools/MusicBee Wrapped - Current Year", "", OnShowWrappedCurrentYear);
            mbApiInterface.MB_AddMenuItem("mnuTools/MusicBee Wrapped - All Years", "", OnShowWrappedAllYears);
            
            // Register commands for hotkey support
            mbApiInterface.MB_RegisterCommand("MusicBee Wrapped - Current Year", OnShowWrappedCurrentYear);
            mbApiInterface.MB_RegisterCommand("MusicBee Wrapped - All Years", OnShowWrappedAllYears);
        }

        public bool GetPanelInfo(int panel, out string name, out string description, out IntPtr icon)
        {
            // We no longer provide a panel UI - only the standalone WPF window
            name = null;
            description = null;
            icon = IntPtr.Zero;
            return false;
        }

        public object GetPanel(int panel)
        {
            // We don't provide a panel UI - plugin uses menu integration only
            return null;
        }

        private void OnShowWrappedCurrentYear(object sender, EventArgs e)
        {
            ShowWrappedForYear(DateTime.Now.Year);
        }

        private void OnShowWrappedAllYears(object sender, EventArgs e)
        {
            ShowWrappedYearSelector();
        }

        private void OnShowWrapped(object sender, EventArgs e)
        {
            // Default to current year for backward compatibility
            ShowWrappedForYear(DateTime.Now.Year);
        }

        private void ShowWrappedForYear(int year)
        {
            mbApiInterface.MB_Trace($"MusicBee Wrapped - Generating web UI for year {year}...");
            try
            {
                // Get play history for the specified year
                var playHistory = yearBasedDataService.GetYearData(year);
                
                // Create statistics for the specified year
                var stats = new MusicBeeWrapped.WrappedStatistics(playHistory, year);
                mbApiInterface.MB_Trace($"Generated statistics for {year}: {playHistory.Plays.Count} total plays");
                
                // Generate and launch web UI
                bool success = webUIService.GenerateWrappedUI(stats, playHistory, year);
                
                if (success)
                {
                    mbApiInterface.MB_Trace($"MusicBee Wrapped web UI launched successfully for year {year}");
                }
                else
                {
                    mbApiInterface.MB_Trace("Failed to launch web UI - check browser settings");
                    // Fallback: Log key statistics to trace
                    mbApiInterface.MB_Trace($"Total Hours: {stats.TotalHours}");
                    mbApiInterface.MB_Trace($"Top Artist: {(stats.TopArtists.Any() ? stats.TopArtists.First().Key : "None")}");
                }
            }
            catch (Exception ex)
            {
                mbApiInterface.MB_Trace($"Error showing MusicBee Wrapped for year {year}: {ex.Message}");
                mbApiInterface.MB_Trace($"Stack trace: {ex.StackTrace}");
            }
        }

        private void ShowWrappedYearSelector()
        {
            mbApiInterface.MB_Trace("MusicBee Wrapped - Generating year selector UI...");
            try
            {
                // Get available years
                var availableYears = yearBasedDataService.GetAvailableYears();
                var metadata = yearBasedDataService.GetMetadata();
                
                // Generate year selector UI
                bool success = webUIService.GenerateYearSelectorUI(availableYears, metadata);
                
                if (success)
                {
                    mbApiInterface.MB_Trace("MusicBee Wrapped year selector launched successfully");
                }
                else
                {
                    mbApiInterface.MB_Trace("Failed to launch year selector - check browser settings");
                    // Fallback: Show current year
                    ShowWrappedForYear(DateTime.Now.Year);
                }
            }
            catch (Exception ex)
            {
                mbApiInterface.MB_Trace($"Error showing year selector: {ex.Message}");
                // Fallback: Show current year
                ShowWrappedForYear(DateTime.Now.Year);
            }
        }

        public bool Configure(IntPtr panelHandle)
        {
            // When the user clicks Configure in the plugin list, show the Wrapped window
            mbApiInterface.MB_Trace("Configure triggered - opening MusicBee Wrapped");
            try
            {
                OnShowWrapped(null, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                mbApiInterface.MB_Trace($"Error in Configure: {ex.Message}");
                return false;
            }
        }

        public void SaveSettings()
        {
            // Delegate to tracking service
            trackingService?.Shutdown();
        }

        public void Close(PluginCloseReason reason)
        {
            // Stop the periodic timer
            periodicCheckTimer?.Dispose();
            periodicCheckTimer = null;
            
            // Ensure all data is saved before closing
            trackingService?.Shutdown();
            yearBasedDataService?.Dispose();
            
            mbApiInterface.MB_Trace("MusicBee Wrapped plugin closed");
        }

        public void Uninstall()
        {
            // Clean up when plugin is uninstalled
            mbApiInterface.MB_Trace("MusicBee Wrapped plugin uninstalled");
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            // Delegate to tracking service
            switch (type)
            {
                case NotificationType.TrackChanged:
                    trackingService?.OnTrackChanged();
                    break;
                    
                case NotificationType.PlayStateChanged:
                    var playState = mbApiInterface.Player_GetPlayState();
                    trackingService?.OnPlayStateChanged(playState);
                    break;
            }
        }

        /// <summary>
        /// Callback for periodic timer to enforce 5-second rule
        /// </summary>
        private void PeriodicCheckCallback(object state)
        {
            try
            {
                trackingService?.PeriodicCheck();
            }
            catch (Exception ex)
            {
                mbApiInterface.MB_Trace($"Error in periodic check: {ex.Message}");
            }
        }
    }
}
