using System;
using System.Collections.Generic;

namespace MusicBeeWrapped.Models
{
    /// <summary>
    /// Analyzes how a user listens to albums - whether they're a purist who plays full albums
    /// or a track shuffler who skips around
    /// </summary>
    public class AlbumListeningBehavior
    {
        public double FullAlbumPercentage { get; set; }
        public double SequentialListeningPercentage { get; set; }
        public double AverageTracksPerAlbumSession { get; set; }
        public int TotalAlbumSessions { get; set; }
        public string ListenerType { get; set; } // "Album Purist", "Track Shuffler", "Mood Curator", "Balanced Listener"
        public List<AlbumSession> NotableAlbumSessions { get; set; } = new List<AlbumSession>();
        public string PersonalityInsight { get; set; }
        
        /// <summary>
        /// Determines the listener personality based on their album behavior
        /// </summary>
        public void DetermineListenerPersonality()
        {
            if (FullAlbumPercentage >= 70)
            {
                ListenerType = "Album Purist";
                PersonalityInsight = $"You're an album purist in a singles world - {FullAlbumPercentage:F1}% of your listening is full albums. You understand that music is meant to be experienced as the artist intended.";
            }
            else if (FullAlbumPercentage >= 40)
            {
                ListenerType = "Balanced Listener";
                PersonalityInsight = $"You strike the perfect balance - {FullAlbumPercentage:F1}% album listening shows you appreciate artistic vision while still enjoying the freedom to explore individual tracks.";
            }
            else if (AverageTracksPerAlbumSession < 2)
            {
                ListenerType = "Track Shuffler";
                PersonalityInsight = $"You're a track shuffler who treats music like a buffet - sampling the best bites rather than sitting down for the full meal. Only {FullAlbumPercentage:F1}% full album listening.";
            }
            else
            {
                ListenerType = "Mood Curator";
                PersonalityInsight = $"You're a mood curator - you pick and choose tracks to craft the perfect emotional journey, averaging {AverageTracksPerAlbumSession:F1} tracks per album session.";
            }
        }
    }
    
    public class AlbumSession
    {
        public string Album { get; set; }
        public string Artist { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TracksPlayed { get; set; }
        public int TotalTracksInAlbum { get; set; }
        public bool IsSequential { get; set; }
        public bool IsComplete { get; set; }
        public double CompletionPercentage => TotalTracksInAlbum > 0 ? (double)TracksPlayed / TotalTracksInAlbum * 100 : 0;
    }
}
