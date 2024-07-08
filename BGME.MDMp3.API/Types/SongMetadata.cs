using P3R.CalendarAPI.Structs;

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
    /// The release date in a forma
    /// </summary>
    public P3Date Released;
}
