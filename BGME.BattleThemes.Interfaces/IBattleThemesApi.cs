namespace BGME.BattleThemes.Interfaces;

public interface IBattleThemesApi
{
    /// <summary>
    /// Add a battle theme music script file.
    /// </summary>
    /// <param name="modId">Mod ID whose songs to use for theme.</param>
    /// <param name="filePath">Theme file path.</param>
    void AddFile(string modId, string filePath);

    /// <summary>
    /// Add a folder containing battle theme music script(s).
    /// </summary>
    /// <param name="modId">Mod ID whose songs to use for theme(s).</param>
    /// <param name="folderPath">Folder path.</param>
    void AddFolder(string modId, string folderPath);

    /// <summary>
    /// Remove a previously added theme music script file.
    /// </summary>
    /// <param name="filePath">Theme file path.</param>
    void RemoveFile(string filePath);

    /// <summary>
    /// Remove a previously added theme folder.
    /// </summary>
    /// <param name="folderPath">Theme folder path.</param>
    void RemoveFolder(string folderPath);
}
