using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Handles cross-platform browser launching with fallback mechanisms
    /// Provides secure URL validation and proper error handling
    /// </summary>
    public class BrowserLauncher
    {
        private readonly TimeSpan _launchTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Launches the specified URL in the default browser
        /// </summary>
        /// <param name="url">URL or file path to launch</param>
        /// <returns>True if launch was successful, false otherwise</returns>
        public bool LaunchUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }

            try
            {
                // Validate and prepare the URL
                var processedUrl = ProcessUrl(url);
                
                // Attempt to launch using platform-specific methods
                return LaunchUrlInternal(processedUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to launch URL '{url}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Launches a local HTML file in the default browser
        /// </summary>
        /// <param name="htmlFilePath">Path to the HTML file</param>
        /// <returns>True if launch was successful, false otherwise</returns>
        public bool LaunchHtmlFile(string htmlFilePath)
        {
            if (string.IsNullOrWhiteSpace(htmlFilePath))
            {
                throw new ArgumentException("HTML file path cannot be null or empty", nameof(htmlFilePath));
            }

            if (!File.Exists(htmlFilePath))
            {
                throw new FileNotFoundException($"HTML file not found: {htmlFilePath}");
            }

            try
            {
                // Convert to file:// URL for consistent handling
                var fileUri = new Uri(htmlFilePath).AbsoluteUri;
                return LaunchUrl(fileUri);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to launch HTML file '{htmlFilePath}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously launches the specified URL in the default browser
        /// </summary>
        /// <param name="url">URL to launch</param>
        /// <returns>Task that completes when launch attempt is finished</returns>
        public async Task<bool> LaunchUrlAsync(string url)
        {
            return await Task.Run(() => LaunchUrl(url));
        }

        /// <summary>
        /// Attempts to launch URL with a specific browser
        /// </summary>
        /// <param name="url">URL to launch</param>
        /// <param name="browserPath">Path to specific browser executable</param>
        /// <returns>True if launch was successful, false otherwise</returns>
        public bool LaunchWithBrowser(string url, string browserPath)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }

            if (string.IsNullOrWhiteSpace(browserPath))
            {
                throw new ArgumentException("Browser path cannot be null or empty", nameof(browserPath));
            }

            if (!File.Exists(browserPath))
            {
                throw new FileNotFoundException($"Browser executable not found: {browserPath}");
            }

            try
            {
                var processedUrl = ProcessUrl(url);
                
                using (var process = new Process())
                {
                    process.StartInfo.FileName = browserPath;
                    process.StartInfo.Arguments = $"\"{processedUrl}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    
                    return process.Start();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to launch URL '{url}' with browser '{browserPath}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets common browser paths for the current platform
        /// </summary>
        /// <returns>Array of potential browser executable paths</returns>
        public string[] GetCommonBrowserPaths()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsBrowserPaths();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetMacBrowserPaths();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxBrowserPaths();
            }
            else
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Validates if a URL is safe to launch
        /// </summary>
        /// <param name="url">URL to validate</param>
        /// <returns>True if URL appears safe, false otherwise</returns>
        public bool IsUrlSafe(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            try
            {
                var uri = new Uri(url);
                
                // Allow file:// and http(s):// schemes
                return uri.Scheme == Uri.UriSchemeFile || 
                       uri.Scheme == Uri.UriSchemeHttp || 
                       uri.Scheme == Uri.UriSchemeHttps;
            }
            catch
            {
                return false;
            }
        }

        private bool LaunchUrlInternal(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return LaunchUrlWindows(url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return LaunchUrlMac(url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LaunchUrlLinux(url);
            }
            else
            {
                // Fallback for unknown platforms
                return LaunchUrlGeneric(url);
            }
        }

        private bool LaunchUrlWindows(string url)
        {
            try
            {
                // Method 1: Use Process.Start with UseShellExecute
                using (var process = new Process())
                {
                    process.StartInfo.FileName = url;
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.CreateNoWindow = true;
                    return process.Start();
                }
            }
            catch
            {
                // Method 2: Use cmd /c start
                try
                {
                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = "cmd";
                        process.StartInfo.Arguments = $"/c start \"\" \"{url}\"";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        return process.Start();
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private bool LaunchUrlMac(string url)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "open";
                    process.StartInfo.Arguments = $"\"{url}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    return process.Start();
                }
            }
            catch
            {
                return false;
            }
        }

        private bool LaunchUrlLinux(string url)
        {
            // Try multiple Linux browser launchers
            string[] launchers = { "xdg-open", "gnome-open", "kde-open", "firefox", "chromium-browser", "google-chrome" };
            
            foreach (var launcher in launchers)
            {
                try
                {
                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = launcher;
                        process.StartInfo.Arguments = $"\"{url}\"";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardError = true;
                        
                        if (process.Start())
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    // Continue to next launcher
                }
            }
            
            return false;
        }

        private bool LaunchUrlGeneric(string url)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = url;
                    process.StartInfo.UseShellExecute = true;
                    return process.Start();
                }
            }
            catch
            {
                return false;
            }
        }

        private string ProcessUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be null or empty");
            }

            // If it's a local file path, convert to file:// URL
            if (File.Exists(url))
            {
                return new Uri(url).AbsoluteUri;
            }

            // If it's already a valid URI, return as-is
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                if (!IsUrlSafe(url))
                {
                    throw new ArgumentException($"URL scheme not allowed: {uri.Scheme}");
                }
                return url;
            }

            throw new ArgumentException($"Invalid URL format: {url}");
        }

        private string[] GetWindowsBrowserPaths()
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            return new[]
            {
                Path.Combine(programFiles, "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(programFilesX86, "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(localAppData, "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(programFiles, "Mozilla Firefox", "firefox.exe"),
                Path.Combine(programFilesX86, "Mozilla Firefox", "firefox.exe"),
                Path.Combine(programFiles, "Microsoft", "Edge", "Application", "msedge.exe"),
                "msedge.exe", // Edge via PATH
                "chrome.exe", // Chrome via PATH
                "firefox.exe" // Firefox via PATH
            };
        }

        private string[] GetMacBrowserPaths()
        {
            return new[]
            {
                "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                "/Applications/Firefox.app/Contents/MacOS/firefox",
                "/Applications/Safari.app/Contents/MacOS/Safari",
                "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge"
            };
        }

        private string[] GetLinuxBrowserPaths()
        {
            return new[]
            {
                "/usr/bin/google-chrome",
                "/usr/bin/chromium-browser",
                "/usr/bin/firefox",
                "/usr/bin/firefox-esr",
                "/snap/bin/chromium",
                "/snap/bin/firefox",
                "google-chrome", // Via PATH
                "chromium-browser",
                "firefox"
            };
        }
    }
}
