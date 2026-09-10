using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Forgetti.Revit;

/// <summary>One Revit version found on this machine, identified by its Revit.ini.</summary>
internal sealed class RevitInstall
{
    private const string FolderPrefix = "Autodesk Revit ";

    public RevitInstall(string name, string iniPath)
    {
        Name = name;
        IniPath = iniPath;
    }

    /// <summary>"Revit 2026".</summary>
    public string Name { get; }

    public string IniPath { get; }

    /// <summary>
    /// Finds every Revit version with a settings file under
    /// <c>%AppData%\Autodesk\Revit\Autodesk Revit 20xx\Revit.ini</c>, newest first.
    ///
    /// The folder is what identifies a version, not the registry: a version that was uninstalled
    /// but left its settings behind still has a recent-file list worth clearing, and one that is
    /// installed but never launched has no list and no folder.
    /// </summary>
    public static IReadOnlyList<RevitInstall> Discover()
    {
        string root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Autodesk",
            "Revit");

        if (!Directory.Exists(root))
        {
            return Array.Empty<RevitInstall>();
        }

        return Directory.GetDirectories(root)
            .Select(directory => new DirectoryInfo(directory))
            .Where(directory => directory.Name.StartsWith(FolderPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(directory => new
            {
                Version = directory.Name.Substring(FolderPrefix.Length).Trim(),
                Ini = Path.Combine(directory.FullName, "Revit.ini"),
            })
            .Where(candidate => File.Exists(candidate.Ini))
            .OrderByDescending(candidate => candidate.Version, StringComparer.OrdinalIgnoreCase)
            .Select(candidate => new RevitInstall("Revit " + candidate.Version, candidate.Ini))
            .ToList();
    }
}
