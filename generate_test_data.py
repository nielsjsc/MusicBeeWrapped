#!/usr/bin/env python3
"""
Test data generator for MusicBeeWrapped plugin
Generates a full year of realistic music listening data with 10,000+ plays
"""

import random
import xml.etree.ElementTree as ET
from datetime import datetime, timedelta
import os

# Configuration
TOTAL_PLAYS = 12000  # Generate 12,000 plays for a full year
NUM_ARTISTS = 100
NUM_ALBUMS = 75
YEAR = 2024  # Generate data for 2024

# Realistic music data
ARTISTS = [
    "The Beatles", "Taylor Swift", "Drake", "The Rolling Stones", "Led Zeppelin",
    "Pink Floyd", "Queen", "Michael Jackson", "Madonna", "Elvis Presley",
    "Bob Dylan", "The Beach Boys", "David Bowie", "Prince", "Stevie Wonder",
    "Radiohead", "Nirvana", "Pearl Jam", "Red Hot Chili Peppers", "U2",
    "Coldplay", "Imagine Dragons", "Ed Sheeran", "Adele", "Beyoncé",
    "Kanye West", "Jay-Z", "Eminem", "Kendrick Lamar", "The Weeknd",
    "Billie Eilish", "Ariana Grande", "Dua Lipa", "Post Malone", "Bruno Mars",
    "John Lennon", "Paul McCartney", "George Harrison", "Ringo Starr", "The Who",
    "AC/DC", "Metallica", "Black Sabbath", "Iron Maiden", "Judas Priest",
    "Deep Purple", "Cream", "The Doors", "Jimi Hendrix", "Janis Joplin",
    "Bob Marley", "Reggae Kings", "Jazz Masters", "Classical Ensemble", "Folk Heroes",
    "Indie Collective", "Electronic Dreams", "Hip Hop Legends", "R&B Sensations", "Country Stars",
    "Rock Anthems", "Pop Icons", "Alternative Wave", "Punk Revolution", "Grunge Movement",
    "New Wave Artists", "Synth Pop", "Dance Floor", "Chill Vibes", "Acoustic Sessions",
    "Live Performance", "Studio Sessions", "Remix Artists", "Cover Bands", "Tribute Acts",
    "Local Bands", "Emerging Artists", "Veteran Musicians", "Session Players", "Solo Acts",
    "Duos", "Trios", "Quartets", "Big Bands", "Orchestras",
    "Choirs", "A Cappella", "Gospel Groups", "Blues Artists", "Jazz Combos",
    "Folk Singers", "Singer-Songwriters", "Instrumentalists", "Composers", "Producers",
    "Sound Engineers", "Mix Artists", "Beat Makers", "Sample Artists", "Loop Masters",
    "Electronic Artists", "Ambient Creators", "Techno Artists", "House Musicians", "Trance DJs",
    "Dubstep Producers", "Drum & Bass", "Breakbeat Artists", "Jungle Music", "IDM Artists",
    "Experimental Music", "Avant-garde", "Noise Artists", "Drone Music", "Post-Rock"
]

ALBUMS = [
    "Abbey Road", "Sgt. Pepper's Lonely Hearts Club Band", "The Dark Side of the Moon", "Led Zeppelin IV",
    "Thriller", "Back in Black", "The Wall", "Rumours", "Hotel California", "Born to Run",
    "Purple Rain", "Like a Virgin", "Nevermind", "Ten", "OK Computer",
    "The Joshua Tree", "Appetite for Destruction", "London Calling", "Born in the U.S.A.", "Graceland",
    "Pet Sounds", "Revolver", "Blonde on Blonde", "Highway 61 Revisited", "Are You Experienced",
    "What's Going On", "Songs in the Key of Life", "Innervisions", "Talking Book", "Hotter Than July",
    "Blue", "Court and Spark", "Hejira", "Don Juan's Reckless Daughter", "Mingus",
    "Kind of Blue", "A Love Supreme", "Giant Steps", "Bitches Brew", "Miles Ahead",
    "The Velvet Underground & Nico", "White Light/White Heat", "Loaded", "Berlin", "Transformer",
    "Hunky Dory", "The Rise and Fall of Ziggy Stardust", "Aladdin Sane", "Diamond Dogs", "Young Americans",
    "Station to Station", "Low", "Heroes", "Lodger", "Scary Monsters",
    "The Man-Machine", "Computer World", "Trans-Europe Express", "Autobahn", "Radio-Activity",
    "Unknown Pleasures", "Closer", "Power, Corruption & Lies", "Low-Life", "Brotherhood",
    "Technique", "Republic", "Get Ready", "Waiting for the Sirens' Call", "Lost Sirens",
    "Doolittle", "Surfer Rosa", "Come On Pilgrim", "Bossanova", "Trompe le Monde",
    "Disintegration", "The Head on the Door", "Kiss Me Kiss Me Kiss Me", "Wish", "Wild Mood Swings",
    "Bloodflowers", "The Cure", "Boys Don't Cry", "Seventeen Seconds", "Faith"
]

GENRES = [
    "Rock", "Pop", "Hip Hop", "R&B", "Country", "Jazz", "Blues", "Folk", "Electronic",
    "Classical", "Reggae", "Punk", "Metal", "Alternative", "Indie", "Dance", "Funk",
    "Soul", "Gospel", "World Music", "Ambient", "Experimental", "New Wave", "Grunge"
]

def generate_track_title(artist, album):
    """Generate realistic track titles"""
    track_prefixes = [
        "Love", "Heart", "Soul", "Dream", "Night", "Day", "Light", "Dark", "Fire", "Water",
        "Wind", "Earth", "Sky", "Rain", "Sun", "Moon", "Star", "Time", "Life", "Death",
        "Hope", "Fear", "Joy", "Pain", "Peace", "War", "Dance", "Song", "Music", "Sound",
        "Voice", "Whisper", "Scream", "Cry", "Laugh", "Smile", "Kiss", "Touch", "Feel", "See",
        "Hear", "Know", "Think", "Believe", "Wonder", "Question", "Answer", "Truth", "Lie", "Real"
    ]
    
    track_suffixes = [
        "Song", "Blues", "Anthem", "Ballad", "March", "Waltz", "Tango", "Samba", "Reggae", "Funk",
        "Rock", "Pop", "Jazz", "Folk", "Country", "Soul", "Gospel", "Hymn", "Prayer", "Praise",
        "Celebration", "Party", "Dance", "Groove", "Beat", "Rhythm", "Melody", "Harmony", "Symphony", "Sonata",
        "Concerto", "Overture", "Prelude", "Interlude", "Finale", "Reprise", "Remix", "Version", "Mix", "Edit"
    ]
    
    # Various title patterns
    patterns = [
        f"{random.choice(track_prefixes)} {random.choice(track_suffixes)}",
        f"{random.choice(track_prefixes)} of {random.choice(track_prefixes)}",
        f"The {random.choice(track_prefixes)}",
        f"{random.choice(track_prefixes)} Me",
        f"I {random.choice(['Love', 'Need', 'Want', 'Feel', 'See', 'Hear'])} {random.choice(track_prefixes)}",
        f"{random.choice(track_prefixes)} Tonight",
        f"{random.choice(track_prefixes)} Forever",
        f"Dancing {random.choice(track_prefixes)}",
        f"Walking {random.choice(track_prefixes)}",
        f"Running {random.choice(track_prefixes)}",
        f"{artist.split()[0] if ' ' in artist else artist}'s {random.choice(track_suffixes)}",
        f"{album.split()[0] if ' ' in album else album} {random.choice(track_suffixes)}"
    ]
    
    return random.choice(patterns)

def generate_realistic_play_duration(track_duration_ms):
    """Generate realistic play duration based on track length"""
    track_duration_seconds = track_duration_ms // 1000
    
    # 70% chance of playing most of the song (80-100%)
    if random.random() < 0.7:
        return random.randint(int(track_duration_seconds * 0.8), track_duration_seconds)
    # 20% chance of playing half to 80% 
    elif random.random() < 0.9:
        return random.randint(int(track_duration_seconds * 0.5), int(track_duration_seconds * 0.8))
    # 10% chance of short play (30 seconds to half)
    else:
        return random.randint(30, int(track_duration_seconds * 0.5))

def generate_listening_patterns():
    """Generate realistic listening patterns throughout the year"""
    # Define listening patterns for different times
    patterns = {
        "morning_commute": (7, 9, ["Pop", "Rock", "Electronic"], 1.2),  # hour_start, hour_end, preferred_genres, frequency_multiplier
        "work_hours": (9, 17, ["Jazz", "Classical", "Ambient", "Instrumental"], 0.8),
        "evening_relax": (18, 22, ["Folk", "Indie", "Alternative", "Chill"], 1.0),
        "weekend_party": (20, 24, ["Dance", "Hip Hop", "Pop", "Electronic"], 1.5),
        "late_night": (22, 2, ["Ambient", "Jazz", "Blues", "Soul"], 0.7),
        "workout": (6, 8, ["Electronic", "Hip Hop", "Rock", "Metal"], 1.3),
    }
    return patterns

def create_play_history_xml():
    """Create the XML structure for play history"""
    root = ET.Element("PlayHistory")
    root.set("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")
    root.set("xmlns:xsd", "http://www.w3.org/2001/XMLSchema")
    
    plays_element = ET.SubElement(root, "Plays")
    
    # Generate plays throughout the year
    start_date = datetime(YEAR, 1, 1)
    end_date = datetime(YEAR, 12, 31, 23, 59, 59)
    
    listening_patterns = generate_listening_patterns()
    
    # Create a pool of tracks to choose from
    track_pool = []
    for i in range(500):  # Create 500 unique tracks
        artist = random.choice(ARTISTS)
        album = random.choice(ALBUMS)
        title = generate_track_title(artist, album)
        genre = random.choice(GENRES)
        year = random.randint(1960, 2024)
        duration = random.randint(120000, 360000)  # 2-6 minutes in milliseconds
        
        track_pool.append({
            'artist': artist,
            'album': album,
            'title': title,
            'genre': genre,
            'year': year,
            'duration': duration
        })
    
    # Generate plays
    current_plays = 0
    current_date = start_date
    
    print(f"Starting generation from {start_date} to {end_date}")
    
    while current_plays < TOTAL_PLAYS and current_date <= end_date:
        # Determine listening pattern for current time
        hour = current_date.hour
        pattern_name = "general"
        pattern_info = None
        
        for pattern, (start_hour, end_hour, genres, freq_mult) in listening_patterns.items():
            if start_hour <= end_hour:
                if start_hour <= hour < end_hour:
                    pattern_name = pattern
                    pattern_info = (start_hour, end_hour, genres, freq_mult)
                    break
            else:  # Crosses midnight
                if hour >= start_hour or hour < end_hour:
                    pattern_name = pattern
                    pattern_info = (start_hour, end_hour, genres, freq_mult)
                    break
        
        # Decide if we should generate a play at this time
        # Increase base probability to ensure we generate enough plays
        base_probability = 0.8  # Increased from 0.3 to 0.8
        if pattern_info:
            base_probability *= pattern_info[3]  # Apply frequency multiplier
        
        # Adjust for day of week (more listening on weekends)
        if current_date.weekday() >= 5:  # Weekend
            base_probability *= 1.2
        
        # Ensure we don't exceed 100% probability
        base_probability = min(base_probability, 1.0)
        
        if random.random() < base_probability:
            # Choose a track, preferring genres from current pattern
            if pattern_info and pattern_info[2]:
                # Filter tracks by preferred genres
                preferred_tracks = [t for t in track_pool if t['genre'] in pattern_info[2]]
                if preferred_tracks:
                    track = random.choice(preferred_tracks)
                else:
                    track = random.choice(track_pool)
            else:
                track = random.choice(track_pool)
            
            # Generate play data
            play_duration = generate_realistic_play_duration(track['duration'])
            
            # Create file path (realistic structure)
            file_path = f"P:\\Music\\{track['artist']}\\{track['album']}\\{random.randint(1, 15):02d}. {track['title']}.mp3"
            
            # Create track play element
            track_play = ET.SubElement(plays_element, "TrackPlay")
            
            # Add all the child elements
            file_url_elem = ET.SubElement(track_play, "FileUrl")
            file_url_elem.text = file_path
            
            title_elem = ET.SubElement(track_play, "Title")
            title_elem.text = track['title']
            
            artist_elem = ET.SubElement(track_play, "Artist")
            artist_elem.text = track['artist']
            
            album_elem = ET.SubElement(track_play, "Album")
            album_elem.text = track['album']
            
            album_artist_elem = ET.SubElement(track_play, "AlbumArtist")
            album_artist_elem.text = track['artist']
            
            genre_elem = ET.SubElement(track_play, "Genre")
            genre_elem.text = track['genre']
            
            year_elem = ET.SubElement(track_play, "Year")
            year_elem.text = str(track['year'])
            
            duration_elem = ET.SubElement(track_play, "Duration")
            duration_elem.text = str(track['duration'])
            
            played_at_elem = ET.SubElement(track_play, "PlayedAt")
            played_at_elem.text = current_date.strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "-07:00"
            
            play_duration_elem = ET.SubElement(track_play, "PlayDuration")
            play_duration_elem.text = str(play_duration)
            
            playlist_elem = ET.SubElement(track_play, "PlaylistName")
            playlist_elem.text = random.choice(["Library", "Favorites", "Recently Added", "Top Rated", "Dance Mix", "Chill Playlist", "Workout Mix"])
            
            listening_mode_elem = ET.SubElement(track_play, "ListeningMode")
            listening_mode_elem.text = random.choice(["Sequential", "Shuffle", "Repeat All", "Repeat One"])
            
            # Debug: print first few tracks
            if current_plays <= 3:
                print(f"DEBUG: Created track {current_plays}: {track['artist']} - {track['title']}")
                print(f"       Play duration: {play_duration}s, Date: {current_date}")
            
            current_plays += 1
            
            if current_plays % 1000 == 0:
                print(f"Generated {current_plays} plays... (Date: {current_date.strftime('%Y-%m-%d')})")
        
        # Advance time (reduce time steps to generate more plays)
        advance_minutes = random.randint(5, 60)  # Reduced from 1-120 to 5-60 minutes
        current_date += timedelta(minutes=advance_minutes)
        
        # Safety check - if we're running out of time, increase probability
        days_remaining = (end_date - current_date).days
        if days_remaining < 30 and current_plays < TOTAL_PLAYS * 0.8:
            # Force more plays in the remaining time
            base_probability = 0.95
    
    # Add LastUpdated element
    ET.SubElement(root, "LastUpdated").text = datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "-07:00"
    
    return root

def create_year_metadata_xml(play_history_root):
    """Create the year metadata XML based on the play history"""
    plays = play_history_root.find('Plays')
    
    if len(plays) == 0:
        return None
    
    # Calculate statistics
    artists = {}
    tracks = {}
    genres = {}
    total_duration = 0
    first_play = None
    last_play = None
    
    for play in plays:
        # Get play data
        artist = play.find('Artist').text
        title = play.find('Title').text
        genre = play.find('Genre').text
        play_duration = int(play.find('PlayDuration').text)
        played_at_str = play.find('PlayedAt').text
        
        # Parse the date
        # Remove timezone info for parsing
        date_part = played_at_str.split('-07:00')[0] if '-07:00' in played_at_str else played_at_str.split('+')[0]
        played_at = datetime.fromisoformat(date_part)
        
        # Track first and last plays
        if first_play is None or played_at < first_play:
            first_play = played_at
        if last_play is None or played_at > last_play:
            last_play = played_at
        
        # Count artists
        if artist in artists:
            artists[artist] += 1
        else:
            artists[artist] = 1
        
        # Count tracks
        track_key = f"{artist} - {title}"
        if track_key in tracks:
            tracks[track_key] += 1
        else:
            tracks[track_key] = 1
        
        # Count genres
        if genre:
            if genre in genres:
                genres[genre] += 1
            else:
                genres[genre] = 1
        
        # Add to total duration
        total_duration += play_duration
    
    # Find top items
    top_artist = max(artists.items(), key=lambda x: x[1])[0]
    top_track = max(tracks.items(), key=lambda x: x[1])[0]
    top_genre = max(genres.items(), key=lambda x: x[1])[0] if genres else ""
    
    # Create metadata XML
    metadata_root = ET.Element("YearMetadata")
    metadata_root.set("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")
    metadata_root.set("xmlns:xsd", "http://www.w3.org/2001/XMLSchema")
    
    # Add elements
    ET.SubElement(metadata_root, "Year").text = str(YEAR)
    ET.SubElement(metadata_root, "TotalPlays").text = str(len(plays))
    ET.SubElement(metadata_root, "TotalMinutes").text = str(total_duration // 60)
    ET.SubElement(metadata_root, "FirstPlay").text = first_play.strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "-07:00"
    ET.SubElement(metadata_root, "LastPlay").text = last_play.strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "-07:00"
    ET.SubElement(metadata_root, "TopArtist").text = top_artist
    ET.SubElement(metadata_root, "TopTrack").text = top_track
    ET.SubElement(metadata_root, "TopGenre").text = top_genre
    ET.SubElement(metadata_root, "LastUpdated").text = datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "-07:00"
    
    return metadata_root

def main():
    print(f"Generating test data for MusicBeeWrapped...")
    print(f"Target: {TOTAL_PLAYS} plays from {NUM_ARTISTS} artists across {NUM_ALBUMS} albums")
    print(f"Year: {YEAR}")
    print()
    
    # Create the play history XML
    root = create_play_history_xml()
    
    # Create the directory structure
    data_dir = os.path.join(os.path.expanduser("~"), "AppData", "Roaming", "MusicBee", "Plugins", "MusicBeeWrapped", str(YEAR))
    os.makedirs(data_dir, exist_ok=True)
    
    # Write the play history XML file
    output_file = os.path.join(data_dir, "play_history.xml")
    
    # Write XML directly using ElementTree
    tree = ET.ElementTree(root)
    
    # Try to use ET.indent if available (Python 3.9+), otherwise write without pretty printing
    try:
        ET.indent(tree, space="  ", level=0)  # Pretty print with indentation
    except AttributeError:
        print("DEBUG: ET.indent not available, writing without pretty printing")
    
    tree.write(output_file, encoding='utf-8', xml_declaration=True)
    
    # Create and write the year metadata XML file
    metadata_root = create_year_metadata_xml(root)
    if metadata_root is not None:
        metadata_file = os.path.join(data_dir, "year_metadata.xml")
        metadata_tree = ET.ElementTree(metadata_root)
        
        # Try to indent metadata too
        try:
            ET.indent(metadata_tree, space="  ", level=0)
        except AttributeError:
            pass
        
        metadata_tree.write(metadata_file, encoding='utf-8', xml_declaration=True)
        print(f"📁 Year metadata saved to: {metadata_file}")
    
    # Also create backup files (as the plugin does)
    backup_file = os.path.join(data_dir, "play_history_backup.xml")
    tree.write(backup_file, encoding='utf-8', xml_declaration=True)
    print(f"📁 Backup file saved to: {backup_file}")
    
    # Verify the file was written correctly
    print(f"DEBUG: Verifying written XML file...")
    try:
        verify_tree = ET.parse(output_file)
        verify_root = verify_tree.getroot()
        verify_plays = verify_root.find('Plays')
        if verify_plays is not None and len(verify_plays) > 0:
            first_play = verify_plays[0]
            title_elem = first_play.find('Title')
            artist_elem = first_play.find('Artist')
            if title_elem is not None and artist_elem is not None:
                print(f"DEBUG: Verification SUCCESS - First play: {artist_elem.text} - {title_elem.text}")
            else:
                print("DEBUG: ERROR - First play has empty Title/Artist elements!")
                print(f"DEBUG: First play XML: {ET.tostring(first_play, encoding='unicode')}")
        else:
            print("DEBUG: ERROR - No plays found in the written file!")
    except Exception as e:
        print(f"DEBUG: Error during verification: {e}")
    
    print(f"✅ Generated test data successfully!")
    print(f"📁 Play history saved to: {output_file}")
    print(f"📊 Total plays generated: {len(root.find('Plays'))}")
    
    # Show some statistics
    plays = root.find('Plays')
    artists = set()
    albums = set()
    genres = set()
    total_duration = 0
    
    for play in plays:
        artists.add(play.find('Artist').text)
        albums.add(play.find('Album').text)
        genres.add(play.find('Genre').text)
        total_duration += int(play.find('PlayDuration').text)
    
    print(f"📈 Statistics:")
    print(f"   - Unique artists: {len(artists)}")
    print(f"   - Unique albums: {len(albums)}")
    print(f"   - Unique genres: {len(genres)}")
    print(f"   - Total listening time: {total_duration // 3600} hours, {(total_duration % 3600) // 60} minutes")
    print(f"   - Average play duration: {total_duration / len(plays):.1f} seconds")
    
    # Show metadata statistics if generated
    if metadata_root is not None:
        print(f"📊 Year Metadata:")
        print(f"   - Top Artist: {metadata_root.find('TopArtist').text}")
        print(f"   - Top Track: {metadata_root.find('TopTrack').text}")
        print(f"   - Top Genre: {metadata_root.find('TopGenre').text}")
        print(f"   - Total Minutes: {metadata_root.find('TotalMinutes').text}")
    
    print("\n🎵 Ready to test your MusicBeeWrapped plugin with a full year of data!")

if __name__ == "__main__":
    main()
