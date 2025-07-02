using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MusicBeeWrapped
{
    [XmlRoot("TrackPlay")]
    public class TrackPlay
    {
        public string FileUrl { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public string Genre { get; set; }
        public string Year { get; set; }
        public int Duration { get; set; } // Track duration in seconds
        public DateTime PlayedAt { get; set; }
        public int PlayDuration { get; set; } // Actual seconds listened
        public double CompletionPercentage => Duration > 0 ? (double)PlayDuration / Duration * 100 : 0;
        
        // New enhanced tracking fields
        public string PlaylistName { get; set; } // Which playlist track came from
        public string ListeningMode { get; set; } // "Shuffle", "Sequential", "Repeat", "Unknown"
        public bool IsWeekend => PlayedAt.DayOfWeek == DayOfWeek.Saturday || PlayedAt.DayOfWeek == DayOfWeek.Sunday;
        public string DateString => PlayedAt.ToString("yyyy-MM-dd"); // For daily charts
        
        // Enhanced tracking fields
        public DayOfWeek DayOfWeek => PlayedAt.DayOfWeek;
        public int HourOfDay => PlayedAt.Hour;
        public string MonthYear => PlayedAt.ToString("yyyy-MM");
        public string Season => GetSeason(PlayedAt);
        public bool IsSkipped => CompletionPercentage < 50; // Less than 50% played
        public bool IsFullPlay => CompletionPercentage >= 80; // 80%+ played
          private string GetSeason(DateTime date)
        {
            int month = date.Month;
            if (month == 12 || month == 1 || month == 2)
                return "Winter";
            else if (month >= 3 && month <= 5)
                return "Spring";
            else if (month >= 6 && month <= 8)
                return "Summer";
            else if (month >= 9 && month <= 11)
                return "Fall";
            else
                return "Unknown";
        }
    }

    [XmlRoot("PlayHistory")]
    public class PlayHistory
    {
        [XmlArray("Plays")]
        [XmlArrayItem("TrackPlay")]
        public List<TrackPlay> Plays { get; set; } = new List<TrackPlay>();
        
        [XmlElement("LastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        public void AddPlay(TrackPlay play)
        {
            Plays.Add(play);
            LastUpdated = DateTime.Now;
        }
        
        public IEnumerable<TrackPlay> GetPlaysByYear(int year)
        {
            return Plays.Where(p => p.PlayedAt.Year == year);
        }
        
        public IEnumerable<TrackPlay> GetPlaysByMonth(int year, int month)
        {
            return Plays.Where(p => p.PlayedAt.Year == year && p.PlayedAt.Month == month);
        }
        
        public IEnumerable<TrackPlay> GetPlaysBySeason(int year, string season)
        {
            return Plays.Where(p => p.PlayedAt.Year == year && p.Season == season);
        }
        
        public IEnumerable<TrackPlay> GetFullPlaysOnly()
        {
            return Plays.Where(p => p.IsFullPlay);
        }
        
        // Get listening streaks
        public List<DateTime> GetListeningDays(int year)
        {
            return Plays.Where(p => p.PlayedAt.Year == year)
                       .Select(p => p.PlayedAt.Date)
                       .Distinct()
                       .OrderBy(d => d)
                       .ToList();
        }
    }
}