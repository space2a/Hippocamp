using System;
using System.Collections.Generic;

[Serializable]
public class HArtist
{
    public string Name;
    public List<HAlbum> hAlbums = new List<HAlbum>();
}