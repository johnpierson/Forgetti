using System;
using System.IO;
using System.Text;
using Forgetti.Revit;
using Xunit;

namespace Forgetti.Tests;

/// <summary>
/// The file logic, against files written the way Revit writes them and a few ways it does not.
/// Every test works on its own temporary file; nothing here goes near a real Revit.ini.
/// </summary>
public sealed class RecentFileListTests : IDisposable
{
    /// <summary>What a Revit.ini looks like, cut down: sections before, the list, a section after.</summary>
    private const string Sample =
        "[Revit.ini]\r\n" +
        "[Colors]\r\n" +
        "BackgroundColor=16777215\r\n" +
        "[Recent File List]\r\n" +
        "File1=C:\\Projects\\Tower.rvt\r\n" +
        "File2=C:\\Projects\\Hospital.rvt\r\n" +
        "File3=C:\\Families\\Door.rfa\r\n" +
        "[Recent Workset List]\r\n" +
        "Config1=All\r\n";

    private const string SampleCleared =
        "[Revit.ini]\r\n" +
        "[Colors]\r\n" +
        "BackgroundColor=16777215\r\n" +
        "[Recent File List]\r\n" +
        "[Recent Workset List]\r\n" +
        "Config1=All\r\n";

    private static readonly Encoding RevitEncoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);

    private readonly string _folder = Path.Combine(Path.GetTempPath(), "forgetti-tests-" + Guid.NewGuid().ToString("N"));

    public RecentFileListTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
    }

    [Fact]
    public void Count_counts_only_the_entries_in_the_recent_file_section()
    {
        string path = WriteIni(Sample, RevitEncoding);

        Assert.Equal(3, RecentFileList.Count(path));
    }

    [Fact]
    public void Clear_removes_the_entries_and_nothing_else()
    {
        string path = WriteIni(Sample, RevitEncoding);

        int removed = RecentFileList.Clear(path);

        Assert.Equal(3, removed);
        Assert.Equal(SampleCleared, File.ReadAllText(path, RevitEncoding));
        Assert.Equal(0, RecentFileList.Count(path));
    }

    [Fact]
    public void Clear_keeps_revits_utf16_byte_order_mark_and_crlf()
    {
        string path = WriteIni(Sample, RevitEncoding);

        RecentFileList.Clear(path);

        byte[] bytes = File.ReadAllBytes(path);
        Assert.Equal(new byte[] { 0xFF, 0xFE }, new[] { bytes[0], bytes[1] });

        string text = File.ReadAllText(path, RevitEncoding);
        Assert.Contains("\r\n", text);
        Assert.DoesNotContain("\n", text.Replace("\r\n", string.Empty));
    }

    [Fact]
    public void Clear_keeps_a_utf8_file_without_a_mark_as_utf8_without_a_mark_and_keeps_lf()
    {
        string path = WriteIni(Sample.Replace("\r\n", "\n"), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        RecentFileList.Clear(path);

        byte[] bytes = File.ReadAllBytes(path);
        Assert.NotEqual(new byte[] { 0xEF, 0xBB, 0xBF }, new[] { bytes[0], bytes[1], bytes[2] });
        Assert.Equal(SampleCleared.Replace("\r\n", "\n"), Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void Clear_takes_a_backup_first_and_restore_puts_it_back_byte_for_byte()
    {
        string path = WriteIni(Sample, RevitEncoding);
        byte[] original = File.ReadAllBytes(path);

        RecentFileList.Clear(path);

        Assert.True(File.Exists(RecentFileList.BackupPathFor(path)));
        Assert.Equal(original, File.ReadAllBytes(RecentFileList.BackupPathFor(path)));

        RecentFileList.Restore(path);

        Assert.Equal(original, File.ReadAllBytes(path));
        Assert.Equal(3, RecentFileList.Count(path));
    }

    [Fact]
    public void Clear_with_nothing_to_remove_writes_nothing()
    {
        string path = WriteIni(SampleCleared, RevitEncoding);
        DateTime stamp = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(path, stamp);

        int removed = RecentFileList.Clear(path);

        Assert.Equal(0, removed);
        Assert.Equal(stamp, File.GetLastWriteTimeUtc(path));
        Assert.False(File.Exists(RecentFileList.BackupPathFor(path)));
    }

    [Fact]
    public void Entries_in_other_sections_are_not_recent_files()
    {
        const string text =
            "[Directories]\r\n" +
            "File1=C:\\NotARecentFile.txt\r\n" +
            "[Recent File List]\r\n" +
            "File1=C:\\Projects\\Tower.rvt\r\n";
        string path = WriteIni(text, RevitEncoding);

        Assert.Equal(1, RecentFileList.Count(path));

        RecentFileList.Clear(path);

        Assert.Contains("File1=C:\\NotARecentFile.txt", File.ReadAllText(path, RevitEncoding));
        Assert.DoesNotContain("Tower.rvt", File.ReadAllText(path, RevitEncoding));
    }

    [Fact]
    public void The_section_header_is_matched_without_regard_to_case()
    {
        string path = WriteIni(Sample.Replace("[Recent File List]", "[RECENT FILE LIST]"), RevitEncoding);

        Assert.Equal(3, RecentFileList.Count(path));
    }

    [Fact]
    public void Restore_without_a_backup_says_so()
    {
        string path = WriteIni(Sample, RevitEncoding);

        Assert.Throws<FileNotFoundException>(() => RecentFileList.Restore(path));
    }

    private string WriteIni(string text, Encoding encoding)
    {
        string path = Path.Combine(_folder, Guid.NewGuid().ToString("N") + ".ini");
        File.WriteAllText(path, text, encoding);
        return path;
    }
}
