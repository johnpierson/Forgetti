using System.Windows.Media;

namespace Forgetti.Theming;

/// <summary>
/// The colours for one mode.
///
/// These are Interlude's neubrutalist palettes, value for value, so a Forgetti window sitting
/// next to an Interlude form reads as the same family. Every outline is the same black on
/// purpose: the style gets its structure from outline and offset, not from a hierarchy of greys.
/// </summary>
internal sealed class Palette
{
    /// <summary>Warm paper, pure black ink, hot pink.</summary>
    public static readonly Palette Paper = new()
    {
        Background = Hex("#FFF4E0"),
        Surface = Hex("#FFFFFF"),
        SurfaceAlt = Hex("#FFE55C"),
        Border = Hex("#000000"),
        Foreground = Hex("#000000"),
        ForegroundMuted = Hex("#4A4238"),
        ForegroundDisabled = Hex("#9A9186"),
        ControlBackground = Hex("#FFFFFF"),
        ControlBackgroundHover = Hex("#FFE55C"),
        ControlBackgroundDisabled = Hex("#E8E2D6"),
        Accent = Hex("#FF5C8A"),
        AccentHover = Hex("#FF87A8"),
        AccentForeground = Hex("#000000"),
        Error = Hex("#E5383B"),
        Warning = Hex("#FF8A00"),
        Success = Hex("#00B36B"),
        Shadow = Hex("#000000"),
    };

    /// <summary>
    /// The paper palette inverted rather than dimmed: bone-white outlines and shadows on near
    /// black, and an accent that survives being surrounded by them.
    /// </summary>
    public static readonly Palette Ink = new()
    {
        Background = Hex("#121214"),
        Surface = Hex("#1E1E22"),
        SurfaceAlt = Hex("#2C2C33"),
        Border = Hex("#F2F2F0"),
        Foreground = Hex("#F7F7F5"),
        ForegroundMuted = Hex("#AFAFB8"),
        ForegroundDisabled = Hex("#6A6A72"),
        ControlBackground = Hex("#1E1E22"),
        ControlBackgroundHover = Hex("#3A3A44"),
        ControlBackgroundDisabled = Hex("#26262A"),
        Accent = Hex("#C7F464"),
        AccentHover = Hex("#D9FF8C"),
        AccentForeground = Hex("#121214"),
        Error = Hex("#FF6B6B"),
        Warning = Hex("#FFC145"),
        Success = Hex("#5CE1A0"),
        Shadow = Hex("#F2F2F0"),
    };

    public Color Background { get; set; }
    public Color Surface { get; set; }
    public Color SurfaceAlt { get; set; }
    public Color Border { get; set; }
    public Color Foreground { get; set; }
    public Color ForegroundMuted { get; set; }
    public Color ForegroundDisabled { get; set; }
    public Color ControlBackground { get; set; }
    public Color ControlBackgroundHover { get; set; }
    public Color ControlBackgroundDisabled { get; set; }
    public Color Accent { get; set; }
    public Color AccentHover { get; set; }
    public Color AccentForeground { get; set; }
    public Color Error { get; set; }
    public Color Warning { get; set; }
    public Color Success { get; set; }
    public Color Shadow { get; set; }

    private static Color Hex(string value) => (Color)ColorConverter.ConvertFromString(value);
}
