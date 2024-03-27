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

}
