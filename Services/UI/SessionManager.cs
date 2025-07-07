using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Manages temporary session files and cleanup for MusicBee Wrapped web interface
    /// Handles secure temp directory creation, automatic cleanup, and cross-platform compatibility
    /// </summary>
    public class SessionManager : IDisposable
    {
        private readonly string _tempBasePath;
        private string _currentSessionPath;
        private Timer _cleanupTimer;
        private readonly object _lockObject = new object();
        private bool _disposed = false;

        /// <summary>
        /// Gets the current session directory path
        /// </summary>
        public string CurrentSessionPath 
        { 
            get 
            { 
                lock (_lockObject)
                {
                    return _currentSessionPath;
                }
            } 
        }

        /// <summary>
        /// Gets the base temporary directory path
        /// </summary>
        public string TempBasePath => _tempBasePath;

        public SessionManager()
        {
            _tempBasePath = Path.Combine(Path.GetTempPath(), "MusicBeeWrapped");
            EnsureTempDirectoryExists();
            
            // Initialize cleanup timer for periodic old session removal
            _cleanupTimer = new Timer(CleanupOldSessionsCallback, null, 
                TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(30));
        }

        /// <summary>
        /// Creates a new session directory with unique identifier
        /// Returns the path to the created session directory
        /// </summary>
        /// <returns>Path to the new session directory</returns>
        public string CreateSession()
        {
            lock (_lockObject)
            {
                ThrowIfDisposed();
                
                var sessionId = $"session_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
                _currentSessionPath = Path.Combine(_tempBasePath, sessionId);
                
                try
                {
                    Directory.CreateDirectory(_currentSessionPath);
                    
                    // Create a session info file for metadata
                    var sessionInfo = Path.Combine(_currentSessionPath, ".session_info");
                    File.WriteAllText(sessionInfo, $"Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\nSession ID: {sessionId}");
                    
                    return _currentSessionPath;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to create session directory: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Writes content to a file within the current session directory
        /// </summary>
        /// <param name="fileName">Name of the file to create</param>
        /// <param name="content">Content to write to the file</param>
        /// <returns>Full path to the created file</returns>
        public string WriteSessionFile(string fileName, string content)
        {
            lock (_lockObject)
            {
                ThrowIfDisposed();
                
                if (string.IsNullOrEmpty(_currentSessionPath))
                {
                    throw new InvalidOperationException("No active session. Call CreateSession() first.");
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
                }

                // Sanitize filename to prevent directory traversal
                var sanitizedFileName = SanitizeFileName(fileName);
                var filePath = Path.Combine(_currentSessionPath, sanitizedFileName);

                try
                {
                    File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
                    return filePath;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to write session file '{fileName}': {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Writes binary content to a file within the current session directory
        /// </summary>
        /// <param name="fileName">Name of the file to create</param>
        /// <param name="content">Binary content to write</param>
        /// <returns>Full path to the created file</returns>
        public string WriteSessionFile(string fileName, byte[] content)
        {
            lock (_lockObject)
            {
                ThrowIfDisposed();
                
                if (string.IsNullOrEmpty(_currentSessionPath))
                {
                    throw new InvalidOperationException("No active session. Call CreateSession() first.");
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
                }

                if (content == null)
                {
                    throw new ArgumentNullException(nameof(content));
                }

                var sanitizedFileName = SanitizeFileName(fileName);
                var filePath = Path.Combine(_currentSessionPath, sanitizedFileName);

                try
                {
                    File.WriteAllBytes(filePath, content);
                    return filePath;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to write session file '{fileName}': {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Schedules cleanup of the current session after the specified delay
        /// </summary>
        /// <param name="delay">Time to wait before cleanup</param>
        public void ScheduleSessionCleanup(TimeSpan delay)
        {
            lock (_lockObject)
            {
                ThrowIfDisposed();
                
                if (string.IsNullOrEmpty(_currentSessionPath))
                {
                    return; // No session to clean up
                }

                var sessionPath = _currentSessionPath;
                Task.Delay(delay).ContinueWith(_ => CleanupSession(sessionPath));
            }
        }

        /// <summary>
        /// Immediately cleans up the current session
        /// </summary>
        public void CleanupCurrentSession()
        {
            lock (_lockObject)
            {
                if (!string.IsNullOrEmpty(_currentSessionPath))
                {
                    CleanupSession(_currentSessionPath);
                    _currentSessionPath = null;
                }
            }
        }

        /// <summary>
        /// Cleans up all sessions older than the specified age
        /// </summary>
        /// <param name="maxAge">Maximum age of sessions to keep</param>
        public void CleanupOldSessions(TimeSpan? maxAge = null)
        {
            var cutoffAge = maxAge ?? TimeSpan.FromHours(2);
            var cutoffTime = DateTime.Now - cutoffAge;

            try
            {
                if (!Directory.Exists(_tempBasePath))
                {
                    return;
                }

                var sessionDirs = Directory.GetDirectories(_tempBasePath, "session_*");
                
                foreach (var sessionDir in sessionDirs)
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(sessionDir);
                        if (dirInfo.CreationTime < cutoffTime)
                        {
                            CleanupSession(sessionDir);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log but don't throw - continue with other sessions
                        System.Diagnostics.Debug.WriteLine($"Failed to cleanup session {sessionDir}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to enumerate session directories: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets information about the current session
        /// </summary>
        /// <returns>Session information or null if no active session</returns>
        public SessionInfo GetCurrentSessionInfo()
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentSessionPath) || !Directory.Exists(_currentSessionPath))
                {
                    return null;
                }

                try
                {
                    var dirInfo = new DirectoryInfo(_currentSessionPath);
                    var files = dirInfo.GetFiles();
                    
                    return new SessionInfo
                    {
                        SessionPath = _currentSessionPath,
                        SessionId = Path.GetFileName(_currentSessionPath),
                        CreatedAt = dirInfo.CreationTime,
                        FileCount = files.Length,
                        TotalSize = files.Sum(f => f.Length)
                    };
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private void EnsureTempDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_tempBasePath))
                {
                    Directory.CreateDirectory(_tempBasePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create temp directory '{_tempBasePath}': {ex.Message}", ex);
            }
        }

        private void CleanupSession(string sessionPath)
        {
            if (string.IsNullOrEmpty(sessionPath) || !Directory.Exists(sessionPath))
            {
                return;
            }

            try
            {
                // First, make all files writable (in case they're read-only)
                var files = Directory.GetFiles(sessionPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                    }
                    catch
                    {
                        // Ignore individual file attribute errors
                    }
                }

                Directory.Delete(sessionPath, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to cleanup session '{sessionPath}': {ex.Message}");
            }
        }

        private void CleanupOldSessionsCallback(object state)
        {
            try
            {
                CleanupOldSessions();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cleanup timer error: {ex.Message}");
            }
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty");
            }

            // Remove path separators and invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            
            // Ensure filename isn't empty after sanitization
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "file";
            }

            // Limit length to prevent filesystem issues
            if (sanitized.Length > 200)
            {
                sanitized = sanitized.Substring(0, 200);
            }

            return sanitized;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SessionManager));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                lock (_lockObject)
                {
                    _cleanupTimer?.Dispose();
                    _cleanupTimer = null;
                    
                    CleanupCurrentSession();
                    _disposed = true;
                }
            }
        }

        ~SessionManager()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Information about a session
    /// </summary>
    public class SessionInfo
    {
        public string SessionPath { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FileCount { get; set; }
        public long TotalSize { get; set; }
    }
}
