using System;
using System.Collections.Generic;

[Serializable]
public class UserProfiles
{
    public List<UserProfile> Profiles = new List<UserProfile>();
    public int loadedProfileId = 0;
    public int IDG = 0; //will increase when a new user is created.

    public SavedDiscovereds SavedDiscovereds = new SavedDiscovereds();
}


[Serializable]
public class UserProfile
{
    public int id = 0;
    public string Username;
    public byte[] Avatar;
    public List<SavedPath> savedPaths = new List<SavedPath>();
    public double PlayerVolume = 1;
}

[Serializable]
public class SavedPath
{
    public string Path;
    public List<string> ignoredSongs = new List<string>();
    public int SongsDiscovered = -1;
}

[Serializable]
public class HPlaylist
{
    public int id = -1;
    public string Name;
    public byte[] Cover;
    public List<string> hSongs = new List<string>();
}

[Serializable]
public class Likes
{
    public List<string> hSongs = new List<string>();
}

[Serializable]
public class SavedDiscovereds
{
    public int ownerId = -1;
    public long lastSize = 0;
    public List<HAlbum> discoveredAlbums = null;
    public List<HArtist> discoveredArtists = null;
    public List<HAlbum> mixsCreated = null;
}

[Serializable]
public class UserExperience
{
    public int UserId = -1;
    public int PlaylistId = 0;
    public List<string> lastPlayedArtists = new List<string>();
    public List<SongPlay> songPlays = new List<SongPlay>();
    public List<HPlaylist> playlists = new List<HPlaylist>();
    public Likes Likes = new Likes();
}

[Serializable]
public class SongPlay
{
    public string Path;
    public int playAmount = 0;
}