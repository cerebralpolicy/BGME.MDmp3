namespace BGME.BattleThemes.Interfaces;

public interface IBattleThemesApi
{
    /// <summary>
    /// Add a battle theme music script file.
    /// </summary>
    /// <param name="modId">Mod ID whose songs to use for theme.</param>
    /// <param name="filePath">Theme music script file path.</param>
    void AddFile(string modId, string filePath);

    /// <summary>
    /// Add a folder containing battle theme music script(s).
    /// </summary>
    /// <param name="modId">Mod ID whose songs to use for theme(s).</param>
    /// <param name="folderPath">Folder path.</param>
    void AddFolder(string modId, string folderPath);
}
