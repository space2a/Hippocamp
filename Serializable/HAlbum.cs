using System;
using System.Collections.Generic;

[Serializable]
public class HAlbum
{
    public string Name;
    public string Artist;
    public List<HSong> Songs = new List<HSong>();
    public byte[] Cover;
}
