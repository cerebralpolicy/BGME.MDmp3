using P3R.CalendarAPI.Structs;
using System.Xml.Linq;

namespace BGME.MDmp3.Types;

public struct SongMetadata
{
    /// <summary>
    /// The raw filename.
    /// </summary>
    public string FileName;
    /// <summary>
    /// The title of the track.
    /// </summary>
    public string Title;
    /// <summary>
    /// The artist/band who made the track.
    /// </summary>
    public string Artist;
    /// <summary>
    /// The album the track is from.
    /// </summary>
    public string Album;
    /// <summary>
    /// The genres tagged in the track's metadata.
    /// </summary>
    public string[] Genres;
    /// <summary>
    /// The release date in a P3R friendly format.
    /// </summary>
    public P3Date Released;

    private string Symbols(string field)
    {
        var newS = field.Replace("&apos;", "'");
        newS = newS.Replace("&amp;", "&");
        return newS;
    }
    #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
    public SongMetadata(string _FileName, string MDMP3_path)
    {
        char fieldSep = '|';
        FileName = _FileName;
        var XMLpath = Path.Combine(MDMP3_path, "MDmp3", "metadata","songDB.xml");
        var XML = XDocument.Load(XMLpath);
        var SongList = XML.Root!.Descendants("songs");
        //Song Query
        var Query = from s in SongList
                    where (string)s.Attribute("filename") == _FileName
                    select s.Element("title")!.Value     + fieldSep + 
                           s.Element("artist")!.Value    + fieldSep + 
                           s.Element("album")!.Value     + fieldSep +
                           s.Element("genres")!.Value    + fieldSep +
                           s.Element("released")!.Value;

        var thisSongData = Query.FirstOrDefault()!.Split(fieldSep);
        Title = Symbols(thisSongData[0]);
        Artist = Symbols(thisSongData[1]);
        Album = Symbols(thisSongData[2]);
        Genres = thisSongData[3].Split('/');
        var releaseDateFields = thisSongData[4].Split('-');
        var year = int.Parse(releaseDateFields[0]);
        var month = int.Parse(releaseDateFields[1]);
        var day = int.Parse(releaseDateFields[2]);
        Released = new(year, month, day);
    }
    #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
}
