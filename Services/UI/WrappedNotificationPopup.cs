using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using MusicBeePlugin.Services;

namespace MusicBeeWrapped.Services.UI
{
    [Serializable]
    [XmlRoot("WrappedNotificationState")]
    public class WrappedNotificationState
    {
        public bool AutoNotifyEnabled { get; set; } = true;
        public DateTime LastWeeklyNotifiedDate { get; set; } = DateTime.MinValue;
        public DateTime LastMonthlyNotifiedDate { get; set; } = DateTime.MinValue;
        public int LastAnnualNotifiedYear { get; set; } = 0;
    }

    public enum WrappedReportType
    {
        Weekly,
        Monthly,
        Annual
    }

    /// <summary>
    /// Non-blocking, warm editorial popup card inside MusicBee that notifies the user
    /// when a Weekly, Monthly, or Annual Wrapped report is ready to view.
    /// </summary>
    public class WrappedNotificationPopup : Form
    {
        private readonly Action _onOpenReport;
        private readonly Action<bool> _onToggleAutoNotify;

        public WrappedNotificationPopup(
            string kickerText,
            string headlineText,
            string summaryText,
            string buttonText,
            bool autoNotifyEnabled,
            Action onOpenReport,
            Action<bool> onToggleAutoNotify)
        {
            _onOpenReport = onOpenReport;
            _onToggleAutoNotify = onToggleAutoNotify;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Size = new Size(370, 210);
            BackColor = Color.FromArgb(226, 219, 208); // Warm Parchment (#E2DBD0)
            Padding = new Padding(22, 20, 22, 16);

            // Position in bottom-right of primary screen working area
            var workArea = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(workArea.Right - Width - 24, workArea.Bottom - Height - 24);

            // Paint border and terracotta top bar
            Paint += OnPaintCard;

            // Close '×' button
            var closeBtn = new Label
            {
                Text = "×",
                Font = new Font("Segoe UI", 12f, FontStyle.Regular),
                ForeColor = Color.FromArgb(115, 108, 98),
                AutoSize = true,
                Cursor = Cursors.Hand,
                Location = new Point(Width - 32, 12),
                BackColor = Color.Transparent
            };
            closeBtn.Click += (s, e) => Close();
            Controls.Add(closeBtn);

            // Kicker
            var kickerLabel = new Label
            {
                Text = kickerText.ToUpperInvariant(),
                Font = new Font("Consolas", 8.2f, FontStyle.Bold),
                ForeColor = Color.FromArgb(179, 57, 30), // Terracotta (#B3391E)
                AutoSize = true,
                Location = new Point(22, 18),
                BackColor = Color.Transparent
            };
            Controls.Add(kickerLabel);

            // Headline
            var headlineLabel = new Label
            {
                Text = headlineText,
                Font = new Font("Georgia", 15f, FontStyle.Regular),
                ForeColor = Color.FromArgb(24, 22, 20), // Charcoal ink (#181614)
                AutoSize = false,
                Size = new Size(Width - 44, 30),
                Location = new Point(20, 38),
                BackColor = Color.Transparent
            };
            Controls.Add(headlineLabel);

            // Summary stats line
            var summaryLabel = new Label
            {
                Text = summaryText,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(74, 69, 62),
                AutoSize = false,
                Size = new Size(Width - 44, 40),
                Location = new Point(22, 72),
                BackColor = Color.Transparent
            };
            Controls.Add(summaryLabel);

            // Primary CTA Button
            var openBtn = new Button
            {
                Text = buttonText,
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                ForeColor = Color.FromArgb(244, 240, 232),
                BackColor = Color.FromArgb(24, 22, 20),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(176, 36),
                Location = new Point(22, 118),
                Cursor = Cursors.Hand
            };
            openBtn.FlatAppearance.BorderSize = 0;
            openBtn.Click += (s, e) =>
            {
                Close();
                _onOpenReport?.Invoke();
            };
            Controls.Add(openBtn);

            // Secondary Dismiss Button
            var laterBtn = new Button
            {
                Text = "Later",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(74, 69, 62),
                BackColor = Color.FromArgb(234, 228, 218),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(88, 36),
                Location = new Point(208, 118),
                Cursor = Cursors.Hand
            };
            laterBtn.FlatAppearance.BorderColor = Color.FromArgb(180, 173, 162);
            laterBtn.FlatAppearance.BorderSize = 1;
            laterBtn.Click += (s, e) => Close();
            Controls.Add(laterBtn);

            // Auto-notify toggle checkbox
            var autoNotifyCheck = new CheckBox
            {
                Text = "Notify me when new reports are ready",
                Checked = autoNotifyEnabled,
                Font = new Font("Segoe UI", 8.2f, FontStyle.Regular),
                ForeColor = Color.FromArgb(115, 108, 98),
                AutoSize = true,
                Location = new Point(22, 168),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            autoNotifyCheck.CheckedChanged += (s, e) =>
            {
                _onToggleAutoNotify?.Invoke(autoNotifyCheck.Checked);
            };
            Controls.Add(autoNotifyCheck);
        }

        protected override bool ShowWithoutActivation => true;

        private void OnPaintCard(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            // Top terracotta rule
            using (var accentBrush = new SolidBrush(Color.FromArgb(179, 57, 30)))
            {
                g.FillRectangle(accentBrush, 0, 0, Width, 4);
            }

            // Outer charcoal frame
            using (var borderPen = new Pen(Color.FromArgb(24, 22, 20), 1))
            {
                g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
            }
        }

        /// <summary>
        /// Checks if a new Annual, Monthly, or Weekly report is ready and hasn't been notified yet.
        /// If ready, displays the non-blocking notification card on the MusicBee UI thread.
        /// </summary>
        public static void CheckAndShowIfReady(
            IntPtr mbWindowHandle,
            YearBasedDataService dataService,
            Action<WrappedReportType, int> openReportCallback)
        {
            try
            {
                if (dataService == null || string.IsNullOrEmpty(dataService.BaseDataPath))
                    return;

                string stateFile = Path.Combine(dataService.BaseDataPath, "notification_state.xml");
                var state = LoadState(stateFile);

                if (!state.AutoNotifyEnabled)
                    return;

                var now = DateTime.Now;

                // 1. Check Annual Wrapped (Dec 20-31 for current year, or Jan 1-15 for previous year)
                int targetAnnualYear = 0;
                if (now.Month == 12 && now.Day >= 20)
                    targetAnnualYear = now.Year;
                else if (now.Month == 1 && now.Day <= 15)
                    targetAnnualYear = now.Year - 1;

                if (targetAnnualYear > 0 && state.LastAnnualNotifiedYear < targetAnnualYear)
                {
                    var yearHistory = dataService.GetYearData(targetAnnualYear);
                    if (yearHistory != null && yearHistory.Plays.Count >= 10)
                    {
                        var stats = new WrappedStatistics(yearHistory, targetAnnualYear);
                        state.LastAnnualNotifiedYear = targetAnnualYear;
                        SaveState(stateFile, state);

                        ShowPopupOnUiThread(
                            mbWindowHandle,
                            $"MUSICBEE WRAPPED · {targetAnnualYear}",
                            $"Your {targetAnnualYear} Wrapped is ready",
                            $"{stats.TotalTracks:N0} tracks · {stats.TotalHours:N1} hours tracked in {targetAnnualYear}.",
                            $"Open {targetAnnualYear} Wrapped →",
                            state.AutoNotifyEnabled,
                            () => openReportCallback?.Invoke(WrappedReportType.Annual, targetAnnualYear),
                            enabled => UpdateAutoNotify(stateFile, enabled));
                        return;
                    }
                }

                // 2. Check Monthly Recap (at most once every 30 days when at least 15 tracks played in last 30 days)
                if ((now.Date - state.LastMonthlyNotifiedDate.Date).TotalDays >= 30)
                {
                    var monthlyStart = now.Date.AddDays(-29);
                    var monthlyHistory = dataService.GetPlaysInDateRange(monthlyStart, now);
                    if (monthlyHistory != null && monthlyHistory.Plays.Count >= 15)
                    {
                        var stats = new WrappedStatistics(monthlyHistory, monthlyStart, now, "This Month", "");
                        state.LastMonthlyNotifiedDate = now.Date;
                        state.LastWeeklyNotifiedDate = now.Date; // avoid back-to-back weekly popup on same day
                        SaveState(stateFile, state);

                        ShowPopupOnUiThread(
                            mbWindowHandle,
                            "MUSICBEE WRAPPED · MONTHLY",
                            "Your Monthly Recap is ready",
                            $"{stats.TotalTracks:N0} tracks · {stats.TotalHours:N1} hours over the last 30 days.",
                            "Open Monthly Wrapped →",
                            state.AutoNotifyEnabled,
                            () => openReportCallback?.Invoke(WrappedReportType.Monthly, now.Year),
                            enabled => UpdateAutoNotify(stateFile, enabled));
                        return;
                    }
                }

                // 3. Check Weekly Recap (at most once every 7 days when at least 5 tracks played in last 7 days)
                if ((now.Date - state.LastWeeklyNotifiedDate.Date).TotalDays >= 7)
                {
                    var weeklyStart = now.Date.AddDays(-6);
                    var weeklyHistory = dataService.GetPlaysInDateRange(weeklyStart, now);
                    if (weeklyHistory != null && weeklyHistory.Plays.Count >= 5)
                    {
                        var stats = new WrappedStatistics(weeklyHistory, weeklyStart, now, "This Week", "");
                        state.LastWeeklyNotifiedDate = now.Date;
                        SaveState(stateFile, state);

                        ShowPopupOnUiThread(
                            mbWindowHandle,
                            "MUSICBEE WRAPPED · WEEKLY",
                            "Your Weekly Recap is ready",
                            $"{stats.TotalTracks:N0} tracks · {stats.TotalHours:N1} hours over the last 7 days.",
                            "Open Weekly Wrapped →",
                            state.AutoNotifyEnabled,
                            () => openReportCallback?.Invoke(WrappedReportType.Weekly, now.Year),
                            enabled => UpdateAutoNotify(stateFile, enabled));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WrappedNotificationPopup check failed: {ex.Message}");
            }
        }

        private static void ShowPopupOnUiThread(
            IntPtr mbWindowHandle,
            string kicker,
            string headline,
            string summary,
            string buttonText,
            bool autoNotify,
            Action onOpen,
            Action<bool> onToggleAutoNotify)
        {
            try
            {
                Control hostControl = mbWindowHandle != IntPtr.Zero ? Control.FromHandle(mbWindowHandle) : null;
                Action showAction = () =>
                {
                    try
                    {
                        var popup = new WrappedNotificationPopup(
                            kicker,
                            headline,
                            summary,
                            buttonText,
                            autoNotify,
                            onOpen,
                            onToggleAutoNotify);

                        if (hostControl != null)
                        {
                            // Position near bottom-right of MusicBee window if visible on screen
                            var bounds = hostControl.Bounds;
                            if (bounds.Width > 400 && bounds.Height > 300)
                            {
                                popup.Location = new Point(
                                    bounds.Right - popup.Width - 28,
                                    bounds.Bottom - popup.Height - 36);
                            }
                            popup.Show(hostControl);
                        }
                        else
                        {
                            popup.Show();
                        }
                    }
                    catch
                    {
                        // Silently fail if window handle is closing
                    }
                };

                if (hostControl != null && hostControl.InvokeRequired)
                {
                    hostControl.BeginInvoke(showAction);
                }
                else
                {
                    showAction();
                }
            }
            catch
            {
                // Silently ignore UI invocation errors
            }
        }

        private static void UpdateAutoNotify(string stateFile, bool enabled)
        {
            var state = LoadState(stateFile);
            state.AutoNotifyEnabled = enabled;
            SaveState(stateFile, state);
        }

        private static WrappedNotificationState LoadState(string stateFile)
        {
            try
            {
                if (File.Exists(stateFile))
                {
                    var serializer = new XmlSerializer(typeof(WrappedNotificationState));
                    using (var reader = new StreamReader(stateFile))
                    {
                        return (WrappedNotificationState)serializer.Deserialize(reader) ?? new WrappedNotificationState();
                    }
                }
            }
            catch
            {
            }
            return new WrappedNotificationState();
        }

        private static void SaveState(string stateFile, WrappedNotificationState state)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(WrappedNotificationState));
                using (var writer = new StreamWriter(stateFile, false))
                {
                    serializer.Serialize(writer, state);
                }
            }
            catch
            {
            }
        }
    }
}
