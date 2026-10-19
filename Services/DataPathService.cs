using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MusicBeePlugin.Services
{
    /// <summary>
    /// Service for managing year-based data paths and organization.
    /// Supports both standard installations and portable MusicBee installations via Setting_GetPersistentStoragePath().
    /// </summary>
    public static class DataPathService
    {
        /// <summary>
        /// Creates and returns the plugin data directory path.
        /// Uses MusicBee's persistent storage path when available so portable installs stay self-contained,
        /// and automatically migrates existing data from %APPDATA% if upgrading from v1.0/v2.0.
        /// </summary>
        public static string InitializeDataDirectory(string persistentStoragePath = null)
        {
            string defaultAppDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MusicBee",
                "Plugins",
                "MusicBeeWrapped");

            string targetFolder;
            if (!string.IsNullOrWhiteSpace(persistentStoragePath))
            {
                targetFolder = Path.Combine(persistentStoragePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), "Plugins", "MusicBeeWrapped");
            }
            else
            {
                targetFolder = defaultAppDataFolder;
            }

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // If running a portable install whose targetFolder differs from %APPDATA% and has no data yet,
            // migrate any existing history from %APPDATA%\MusicBee\Plugins\MusicBeeWrapped automatically.
            try
            {
                if (!string.Equals(Path.GetFullPath(targetFolder), Path.GetFullPath(defaultAppDataFolder), StringComparison.OrdinalIgnoreCase) &&
                    Directory.Exists(defaultAppDataFolder) &&
                    GetAvailableYears(targetFolder).Count == 0 &&
                    GetAvailableYears(defaultAppDataFolder).Count > 0)
                {
                    CopyDirectoryRecursive(defaultAppDataFolder, targetFolder);
                }
            }
            catch
            {
                // Ignore migration errors and continue using targetFolder
            }

            return targetFolder;
        }

        private static void CopyDirectoryRecursive(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                if (!File.Exists(destFile))
                {
                    File.Copy(file, destFile, false);
                }
            }
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectoryRecursive(dir, destSubDir);
            }
        }

        /// <summary>
        /// Gets the data directory for a specific year
        /// </summary>
        public static string GetYearDataPath(string baseDataPath, int year)
        {
            var yearPath = Path.Combine(baseDataPath, year.ToString());
            if (!Directory.Exists(yearPath))
            {
                Directory.CreateDirectory(yearPath);
            }
            return yearPath;
        }

        /// <summary>
        /// Gets the play history file path for a specific year
        /// </summary>
        public static string GetYearHistoryFilePath(string baseDataPath, int year)
        {
            return Path.Combine(GetYearDataPath(baseDataPath, year), "play_history.xml");
        }

        /// <summary>
        /// Gets the backup file path for a specific year
        /// </summary>
        public static string GetYearBackupFilePath(string baseDataPath, int year)
        {
            return Path.Combine(GetYearDataPath(baseDataPath, year), "play_history_backup.xml");
        }

        /// <summary>
        /// Gets all years that have data available
        /// </summary>
        public static List<int> GetAvailableYears(string baseDataPath)
        {
            var years = new List<int>();

            if (!Directory.Exists(baseDataPath))
                return years;

            foreach (var directory in Directory.GetDirectories(baseDataPath))
            {
                var dirName = Path.GetFileName(directory);
                if (int.TryParse(dirName, out int year) && year >= 2000 && year <= DateTime.Now.Year + 1)
                {
                    years.Add(year);
                }
            }

            return years.OrderByDescending(y => y).ToList();
        }

        /// <summary>
        /// Gets the legacy data file path (for migration)
        /// </summary>
        public static string GetLegacyHistoryFilePath(string baseDataPath)
        {
            return Path.Combine(baseDataPath, "play_history.dat");
        }

        /// <summary>
        /// Checks if legacy data exists (needs migration)
        /// </summary>
        public static bool HasLegacyData(string baseDataPath)
        {
            return File.Exists(GetLegacyHistoryFilePath(baseDataPath));
        }

        /// <summary>
        /// Gets metadata file path for storing year summaries (legacy - for backwards compatibility)
        /// </summary>
        public static string GetMetadataFilePath(string baseDataPath)
        {
            return Path.Combine(baseDataPath, "year_metadata.xml");
        }

        /// <summary>
        /// Gets metadata file path for a specific year
        /// </summary>
        public static string GetYearMetadataFilePath(string baseDataPath, int year)
        {
            return Path.Combine(GetYearDataPath(baseDataPath, year), "year_metadata.xml");
        }

        /// <summary>
        /// Gets the full path for the play history data file (legacy method for backwards compatibility)
        /// </summary>
        public static string GetHistoryFilePath(string dataDirectory)
        {
            return Path.Combine(dataDirectory, "play_history.dat");
        }
    }
}
