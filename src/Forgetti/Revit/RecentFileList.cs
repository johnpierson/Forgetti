using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Forgetti.Revit;

/// <summary>
/// Reads and clears the <c>[Recent File List]</c> section of a Revit.ini.
///
/// Revit writes this file as UTF-16 LE with a byte-order mark and CRLF line endings, and reads it
/// back the same way. Whatever encoding and newline the file arrives with is what it is written
/// back with. (The previous version of this tool silently rewrote the file as UTF-8, which Revit
/// tolerated but nobody intended.)
///
/// Only the <c>File1=</c>, <c>File2=</c>, ... entries go. The section header stays, every other
/// section is untouched, and a copy of the file is taken first so the change can be undone.
/// </summary>
internal static class RecentFileList
{
    private const string SectionHeader = "[Recent File List]";

    private const string BackupSuffix = ".forgetti-bak";

    private static readonly Regex EntryPattern = new(@"^File\d+\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex SectionPattern = new(@"^\[.*\]$", RegexOptions.CultureInvariant);

    /// <summary>Where <see cref="Clear"/> keeps the copy it takes before writing.</summary>
    public static string BackupPathFor(string iniPath) => iniPath + BackupSuffix;

    /// <summary>How many recent-file entries the file currently holds.</summary>
    public static int Count(string iniPath) => Strip(IniText.Read(iniPath).Lines, kept: null);

    /// <summary>
    /// Removes every recent-file entry and returns how many were removed. Takes a backup beside
    /// the file first, and writes nothing at all when there was nothing to remove.
    /// </summary>
    public static int Clear(string iniPath)
    {
        IniText ini = IniText.Read(iniPath);
        List<string> kept = new(ini.Lines.Count);

        int removed = Strip(ini.Lines, kept);
        if (removed == 0)
        {
            return 0;
        }

        File.Copy(iniPath, BackupPathFor(iniPath), overwrite: true);
        ini.Write(iniPath, kept);
        return removed;
    }

    /// <summary>Puts back the copy <see cref="Clear"/> took.</summary>
    public static void Restore(string iniPath)
    {
        string backup = BackupPathFor(iniPath);

        if (!File.Exists(backup))
        {
            throw new FileNotFoundException("There is no backup to restore.", backup);
        }

        File.Copy(backup, iniPath, overwrite: true);
    }

    /// <summary>
    /// The one place the section rule lives. Counting and clearing both go through it, so the
    /// number shown in the window is always the number that would be removed.
    /// </summary>
    private static int Strip(IReadOnlyList<string> lines, List<string>? kept)
    {
        int removed = 0;
        bool inSection = false;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (SectionPattern.IsMatch(trimmed))
            {
                inSection = trimmed.Equals(SectionHeader, StringComparison.OrdinalIgnoreCase);
            }
            else if (inSection && EntryPattern.IsMatch(trimmed))
            {
                removed++;
                continue;
            }

            kept?.Add(line);
        }

        return removed;
    }

    /// <summary>The file as lines, remembering the encoding and newline it used.</summary>
    private sealed class IniText
    {
        private IniText(IReadOnlyList<string> lines, Encoding encoding, string newline)
        {
            Lines = lines;
            Encoding = encoding;
            Newline = newline;
        }

        public IReadOnlyList<string> Lines { get; }

        public Encoding Encoding { get; }

        public string Newline { get; }

        public static IniText Read(string path)
        {
            string text;
            Encoding encoding;

            // A file with no byte-order mark is read as UTF-8 and written back without one. A file
            // with a mark keeps it: CurrentEncoding becomes the marked encoding, and its preamble
            // is what File.WriteAllText emits.
            using (StreamReader reader = new(path, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true))
            {
                text = reader.ReadToEnd();
                encoding = reader.CurrentEncoding;
            }

            string newline = text.Contains("\r\n") ? "\r\n" : "\n";
            string[] lines = text.Split(new[] { newline }, StringSplitOptions.None);

            return new IniText(lines, encoding, newline);
        }

        public void Write(string path, IReadOnlyList<string> lines)
        {
            File.WriteAllText(path, string.Join(Newline, lines), Encoding);
        }
    }
}
