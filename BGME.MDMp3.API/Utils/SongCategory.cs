using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGME.MDmp3.Utils;

internal class SongCategory
{
   public static string GetCatDir (string modDir, string songCat)
    {
        var catDir = Path.Join(modDir, "MDmp3", "music", songCat);
        return catDir;
    }
    public static string GetCatDef(string catDir)
    {
        string catDef = new DirectoryInfo(catDir).Name;
        return catDef;
    }
}
internal class PlaylistCategory
{
    public static string GetCatDir(string modDir, string playlistCat)
    {
        var catDir = Path.Join(modDir, "MDmp3", "playlist", playlistCat);
        return catDir;
    }
    public static string GetCatDef(string catDir)
    {
        string catDef = new DirectoryInfo(catDir).Name;
        return catDef;
    }
}
