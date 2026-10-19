using System;
using System.Linq;
using System.Threading;
using MusicBeePlugin.Services;
using MusicBeeWrapped.Services.UI;

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
        private ModularWebUIService modularWebUIService;
        private YearBasedDataService yearBasedDataService;
        
        // Timer for periodic checks (5-second rule enforcement)
        private Timer periodicCheckTimer;
        private Timer startupNotificationTimer;
        private bool notificationChecked;
        
        /// <summary>
        /// Provides access to the MusicBee API for service classes
        /// </summary>
        public MusicBeeApiInterface ApiInterface => mbApiInterface;
        
        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            
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
            about.Description = "Provides Weekly, Monthly, and Annual listening reports and visualizations for your MusicBee library";
            about.Author = "Nielsjsc";
            about.TargetApplication = "";
            about.Type = PluginType.General;
            about.VersionMajor = 2;
            about.VersionMinor = 1;
            about.Revision = 0;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = ReceiveNotificationFlags.PlayerEvents | 
                                         ReceiveNotificationFlags.TagEvents;
            about.ConfigurationPanelHeight = 0;
        }
        
        private void InitializeServices()
        {
            // Use MusicBee's persistent storage path so portable installs stay self-contained
            string persistentStoragePath = null;
            try
            {
                if (mbApiInterface.Setting_GetPersistentStoragePath != null)
                {
                    persistentStoragePath = mbApiInterface.Setting_GetPersistentStoragePath();
                }
            }
            catch
            {
            }

            string dataDirectory = DataPathService.InitializeDataDirectory(persistentStoragePath);
            
            // Create year-based data service
            yearBasedDataService = new YearBasedDataService(dataDirectory);
            
            // Create service instances
            trackingService = new TrackingService(this, yearBasedDataService);
            modularWebUIService = new ModularWebUIService(yearBasedDataService);
            
            // Initialize tracking service
            trackingService.Initialize();
            
            // Cleanup old web UI sessions
            modularWebUIService.CleanupOldSessions();
            
            // Start periodic check timer (runs every second to check 5-second rule)
            periodicCheckTimer = new Timer(PeriodicCheckCallback, null, 1000, 1000);

            // Schedule non-blocking startup notification check after MusicBee UI is ready
            startupNotificationTimer = new Timer(CheckStartupNotificationCallback, null, 4500, Timeout.Infinite);
        }
        
        private void RegisterMenuItems()
        {
            // Add menu items to Tools menu for Weekly, Monthly, Current Year, and All Reports
            mbApiInterface.MB_AddMenuItem("mnuTools/MusicBee Wrapped - Weekly (Last 7 Days)", "", OnShowWrappedWeekly);
            mbApiInterface.MB_AddMenuItem("mnuTools/MusicBee Wrapped - Monthly (Last 30 Days)", "", OnShowWrappedMonthly);
            mbApiInterface.MB_AddMenuItem("mnuTools/MusicBee Wrapped - Current Year", "", OnShowWrappedCurrentYear);
            mbApiInterface.MB_AddMenuItem("mnuTools/MusicBee Wrapped - All Reports && Years", "", OnShowWrappedAllYears);
            
            // Register commands for hotkey support
            mbApiInterface.MB_RegisterCommand("MusicBee Wrapped - Weekly (Last 7 Days)", OnShowWrappedWeekly);
            mbApiInterface.MB_RegisterCommand("MusicBee Wrapped - Monthly (Last 30 Days)", OnShowWrappedMonthly);
            mbApiInterface.MB_RegisterCommand("MusicBee Wrapped - Current Year", OnShowWrappedCurrentYear);
            mbApiInterface.MB_RegisterCommand("MusicBee Wrapped - All Years", OnShowWrappedAllYears);
        }

        public bool GetPanelInfo(int panel, out string name, out string description, out IntPtr icon)
        {
            name = null;
            description = null;
            icon = IntPtr.Zero;
            return false;
        }

        public object GetPanel(int panel)
        {
            return null;
        }

        private void OnShowWrappedWeekly(object sender, EventArgs e)
        {
            ShowWrappedForDateRange(DateTime.Now.Date.AddDays(-6), DateTime.Now, "This Week");
        }

        private void OnShowWrappedMonthly(object sender, EventArgs e)
        {
            ShowWrappedForDateRange(DateTime.Now.Date.AddDays(-29), DateTime.Now, "This Month");
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
            ShowWrappedYearSelector();
        }

        private void ShowWrappedForDateRange(DateTime startDate, DateTime endDate, string periodLabel)
        {
            try
            {
                var playHistory = yearBasedDataService.GetPlaysInDateRange(startDate, endDate);
                var subtitle = $"{startDate:MMM d} – {endDate:MMM d, yyyy}";
                var stats = new MusicBeeWrapped.WrappedStatistics(playHistory, startDate, endDate, periodLabel, subtitle);
                modularWebUIService.GenerateWrappedUI(stats, playHistory, endDate.Year);
            }
            catch (Exception ex)
            {
                mbApiInterface.MB_Trace($"[MusicBee Wrapped Error] Failed to generate {periodLabel} report: {ex.Message}");
            }
        }

        private void ShowWrappedForYear(int year)
        {
            try
            {
                var playHistory = yearBasedDataService.GetYearData(year);
                var stats = new MusicBeeWrapped.WrappedStatistics(playHistory, year);
                modularWebUIService.GenerateWrappedUI(stats, playHistory, year);
            }
            catch (Exception ex)
            {
                mbApiInterface.MB_Trace($"[MusicBee Wrapped Error] Failed to show Wrapped for {year}: {ex.Message}");
            }
        }

        private void ShowWrappedYearSelector()
        {
            try
            {
                var availableYears = yearBasedDataService.GetAvailableYears();
                var metadata = yearBasedDataService.GetMetadata();
                bool success = modularWebUIService.GenerateYearSelectorUI(availableYears, metadata);
                if (!success)
                {
                    ShowWrappedForYear(DateTime.Now.Year);
                }
            }
            catch (Exception ex)
            {
                mbApiInterface.MB_Trace($"[MusicBee Wrapped Error] Failed to show report selector: {ex.Message}");
                ShowWrappedForYear(DateTime.Now.Year);
            }
        }

        private void CheckStartupNotificationCallback(object state)
        {
            if (notificationChecked) return;
            notificationChecked = true;

            try
            {
                IntPtr hwnd = IntPtr.Zero;
                try
                {
                    if (mbApiInterface.MB_GetWindowHandle != null)
                    {
                        hwnd = mbApiInterface.MB_GetWindowHandle();
                    }
                }
                catch
                {
                }

                WrappedNotificationPopup.CheckAndShowIfReady(
                    hwnd,
                    yearBasedDataService,
                    (reportType, targetYear) =>
                    {
                        switch (reportType)
                        {
                            case WrappedReportType.Weekly:
                                ShowWrappedForDateRange(DateTime.Now.Date.AddDays(-6), DateTime.Now, "This Week");
                                break;
                            case WrappedReportType.Monthly:
                                ShowWrappedForDateRange(DateTime.Now.Date.AddDays(-29), DateTime.Now, "This Month");
                                break;
                            case WrappedReportType.Annual:
                            default:
                                ShowWrappedForYear(targetYear);
                                break;
                        }
                    });
            }
            catch
            {
            }
        }

        public bool Configure(IntPtr panelHandle)
        {
            try
            {
                OnShowWrapped(null, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                mbApiInterface.MB_Trace($"[MusicBee Wrapped Error] Configure failed: {ex.Message}");
                return false;
            }
        }

        public void SaveSettings()
        {
            trackingService?.Shutdown();
        }

        public void Close(PluginCloseReason reason)
        {
            periodicCheckTimer?.Dispose();
            periodicCheckTimer = null;

            startupNotificationTimer?.Dispose();
            startupNotificationTimer = null;
            
            trackingService?.Shutdown();
            yearBasedDataService?.Dispose();
        }

        public void Uninstall()
        {
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.PluginStartup:
                case NotificationType.MusicBeeStarted:
                    CheckStartupNotificationCallback(null);
                    break;

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
            catch
            {
            }
        }
    }
}
