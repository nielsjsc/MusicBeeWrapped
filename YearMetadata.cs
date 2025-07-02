using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MusicBeeWrapped
{
    /// <summary>
    /// Metadata for a single year's data
    /// </summary>
    [XmlRoot("YearMetadata")]
    public class YearMetadata
    {
        public int Year { get; set; }
        public int TotalPlays { get; set; }
        public int TotalMinutes { get; set; }
        public DateTime FirstPlay { get; set; }
        public DateTime LastPlay { get; set; }
        public string TopArtist { get; set; }
        public string TopTrack { get; set; }
        public string TopGenre { get; set; }
        public DateTime LastUpdated { get; set; }
        
        public YearMetadata()
        {
            LastUpdated = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Collection of metadata for all years
    /// </summary>
    [XmlRoot("YearMetadataCollection")]
    public class YearMetadataCollection
    {
        [XmlArray("Years")]
        [XmlArrayItem("Year")]
        public List<YearMetadata> YearsList { get; set; } = new List<YearMetadata>();
        
        [XmlIgnore]
        public Dictionary<int, YearMetadata> Years 
        { 
            get 
            {
                var dict = new Dictionary<int, YearMetadata>();
                foreach (var year in YearsList)
                {
                    dict[year.Year] = year;
                }
                return dict;
            }
        }
        
        [XmlElement("LastUpdated")]
        public DateTime LastUpdated { get; set; }
        
        public YearMetadataCollection()
        {
            YearsList = new List<YearMetadata>();
            LastUpdated = DateTime.Now;
        }
        
        public void UpdateYearMetadata(int year, YearMetadata metadata)
        {
            // Remove existing entry for this year
            YearsList.RemoveAll(y => y.Year == year);
            // Add the new metadata
            YearsList.Add(metadata);
            LastUpdated = DateTime.Now;
        }
        
        public YearMetadata GetYearMetadata(int year)
        {
            return YearsList.FirstOrDefault(y => y.Year == year);
        }
        
        public List<int> GetAvailableYears()
        {
            var years = YearsList.Select(y => y.Year).ToList();
            years.Sort((a, b) => b.CompareTo(a)); // Descending order
            return years;
        }
    }
}
