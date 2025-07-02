using System;
using System.IO;
using System.Xml.Serialization;

namespace MusicBeePlugin.Services
{
    /// <summary>
    /// Helper service for XML serialization operations
    /// </summary>
    public static class XmlDataService
    {
        /// <summary>
        /// Saves an object to XML file
        /// </summary>
        public static void SaveToXml<T>(T obj, string filePath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"XmlDataService: Starting save to {filePath}");
                
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    System.Diagnostics.Debug.WriteLine($"XmlDataService: Creating directory {directory}");
                    Directory.CreateDirectory(directory);
                }

                var serializer = new XmlSerializer(typeof(T));
                System.Diagnostics.Debug.WriteLine($"XmlDataService: Created serializer for type {typeof(T).Name}");
                
                using (var writer = new FileStream(filePath, FileMode.Create))
                {
                    System.Diagnostics.Debug.WriteLine($"XmlDataService: Opened file stream for {filePath}");
                    serializer.Serialize(writer, obj);
                    System.Diagnostics.Debug.WriteLine($"XmlDataService: Successfully serialized object to {filePath}");
                }
                
                // Verify file was created
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    System.Diagnostics.Debug.WriteLine($"XmlDataService: File created successfully, size: {fileInfo.Length} bytes");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"XmlDataService: WARNING - File was not created: {filePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"XmlDataService: Error saving XML to {filePath}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"XmlDataService: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Loads an object from XML file
        /// </summary>
        public static T LoadFromXml<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var serializer = new XmlSerializer(typeof(T));
                using (var reader = new FileStream(filePath, FileMode.Open))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading XML from {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tries to load an object from XML file with error handling
        /// </summary>
        public static bool TryLoadFromXml<T>(string filePath, out T result) where T : class
        {
            result = null;
            try
            {
                result = LoadFromXml<T>(filePath);
                return result != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
