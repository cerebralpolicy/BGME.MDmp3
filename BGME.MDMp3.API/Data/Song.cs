using P3R.CalendarAPI.Structs;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BGME.MDmp3.Data;

internal record ModSong(string ModId, string Name, int BgmId, string FilePath, string BuildFilePath, SongMetadata Metadata);

internal record SongMetadata
{
    /// <summary>
    /// The raw filename.
    /// </summary>
    public string FileName;
    /// <summary>
    /// The title of the track.
    /// </summary>
    public string? Title;
    /// <summary>
    /// The artist/band who made the track.
    /// </summary>
    public string? Artist;
    /// <summary>
    /// The album the track is from.
    /// </summary>
    public string? Album;
    /// <summary>
    /// The genres tagged in the track's metadata.
    /// </summary>
    public string[]? Genres;
    /// <summary>
    /// The release date in a P3R friendly format.
    /// </summary>
    public P3Date? Released;

    public SongMetadata(string Name)
    {
        DateTimeStyles styles = DateTimeStyles;
        this.FileName = Name;

    }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
}