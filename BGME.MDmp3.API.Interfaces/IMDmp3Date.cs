using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGME.MDmp3.Interfaces
{
    internal interface IMDmp3Date
    {
        void SongRelease(int songID, int currentDate);
    }
}
