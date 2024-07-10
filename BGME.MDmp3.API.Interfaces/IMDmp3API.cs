namespace BGME.MDmp3.Interfaces
{
    public interface IMDmp3API
    {
        // BORROWING FROM BATTLETHEMES
        void AddPath(string modID, string path, string type);
        void RemovePath(string path);
    }
}
