using System;

namespace MusicBeeWrapped.Models
{
    /// <summary>
    /// Represents a period when a user was intensely focused on a particular artist or album
    /// </summary>
    public class ObsessionPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Track { get; set; }
        public int PlayCount { get; set; }
        public double IntensityScore { get; set; } // Average plays per day during peak
        public int DurationInDays => (EndDate - StartDate).Days + 1;
        public string ObsessionType { get; set; } // "Artist", "Album", "Track"
        public string Description { get; set; }
        
        /// <summary>
        /// Generates a human-readable description of the obsession
        /// </summary>
        public string GenerateDescription()
        {
            var durationText = DurationInDays == 1 ? "1 day" : $"{DurationInDays} days";
            var intensityText = IntensityScore > 10 ? "completely consumed by" : 
                               IntensityScore > 5 ? "obsessed with" : "really into";
            
            switch (ObsessionType)
            {
                case "Artist":
                    return $"You were {intensityText} {Artist} for {durationText} ({PlayCount} plays)";
                case "Album":
                    return $"You were {intensityText} \"{Album}\" by {Artist} for {durationText} ({PlayCount} plays)";
                case "Track":
                    return $"You were {intensityText} \"{Track}\" by {Artist} for {durationText} ({PlayCount} plays)";
                default:
                    return $"You had a {durationText} music obsession ({PlayCount} plays)";
            }
        }
    }
}
