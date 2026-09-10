using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Forgetti.Theming;

/// <summary>
/// Writes a palette into a resource dictionary as brushes, effects and transforms.
///
/// Every style in Themes/Forgetti.xaml reads these keys as dynamic resources, so switching
/// between light and dark is a matter of writing the other palette: the whole window repaints
/// without a single control being rebuilt.
/// </summary>
internal static class Theme
{
    /// <summary>Outline thickness. Two pixels is the heavy line the style is built on.</summary>
    public const double BorderWidth = 2d;

    /// <summary>How far the solid, unblurred shadow sits down and to the right of each control.</summary>
    public const double ShadowOffset = 4d;

    public const string Background = "Forgetti.Background";
    public const string Surface = "Forgetti.Surface";
    public const string SurfaceAlt = "Forgetti.SurfaceAlt";
    public const string Border = "Forgetti.Border";
    public const string Foreground = "Forgetti.Foreground";
    public const string ForegroundMuted = "Forgetti.ForegroundMuted";
    public const string ForegroundDisabled = "Forgetti.ForegroundDisabled";
    public const string ControlBackground = "Forgetti.ControlBackground";
    public const string ControlBackgroundHover = "Forgetti.ControlBackgroundHover";
    public const string ControlBackgroundDisabled = "Forgetti.ControlBackgroundDisabled";
    public const string Accent = "Forgetti.Accent";
    public const string AccentHover = "Forgetti.AccentHover";
    public const string AccentForeground = "Forgetti.AccentForeground";
    public const string Error = "Forgetti.Error";
    public const string Warning = "Forgetti.Warning";
    public const string Success = "Forgetti.Success";
    public const string ControlShadow = "Forgetti.ControlShadow";
    public const string PressTransform = "Forgetti.PressTransform";
    public const string FontFamily = "Forgetti.FontFamily";
    public const string BorderThickness = "Forgetti.BorderThickness";

    /// <summary>Whether the dark palette is currently applied.</summary>
    public static bool IsDark { get; private set; }

    public static void Apply(ResourceDictionary resources, bool dark)
    {
        Palette palette = dark ? Palette.Ink : Palette.Paper;
        IsDark = dark;

        resources[Background] = Brush(palette.Background);
        resources[Surface] = Brush(palette.Surface);
        resources[SurfaceAlt] = Brush(palette.SurfaceAlt);
        resources[Border] = Brush(palette.Border);
        resources[Foreground] = Brush(palette.Foreground);
        resources[ForegroundMuted] = Brush(palette.ForegroundMuted);
        resources[ForegroundDisabled] = Brush(palette.ForegroundDisabled);
        resources[ControlBackground] = Brush(palette.ControlBackground);
        resources[ControlBackgroundHover] = Brush(palette.ControlBackgroundHover);
        resources[ControlBackgroundDisabled] = Brush(palette.ControlBackgroundDisabled);
        resources[Accent] = Brush(palette.Accent);
        resources[AccentHover] = Brush(palette.AccentHover);
        resources[AccentForeground] = Brush(palette.AccentForeground);
        resources[Error] = Brush(palette.Error);
        resources[Warning] = Brush(palette.Warning);
        resources[Success] = Brush(palette.Success);

        resources[ControlShadow] = HardShadow(palette.Shadow);
        resources[BorderThickness] = new Thickness(BorderWidth);

        TranslateTransform press = new(ShadowOffset, ShadowOffset);
        press.Freeze();
        resources[PressTransform] = press;

        resources[FontFamily] = ResolveFontFamily();
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// A drop shadow with no blur is the cheap case: no blur pass, and one frozen instance shared
    /// by every control. Direction 315 is down and to the right (WPF measures anticlockwise from
    /// east), and depth is measured along that direction, hence the diagonal.
    /// </summary>
    private static DropShadowEffect HardShadow(Color color)
    {
        DropShadowEffect effect = new()
        {
            Color = color,
            BlurRadius = 0d,
            ShadowDepth = ShadowOffset * Math.Sqrt(2d),
            Direction = 315d,
            Opacity = 1d,
        };

        effect.Freeze();
        return effect;
    }

    private static readonly Uri FontBaseUri = new("pack://application:,,,/");

    /// <summary>
    /// The embedded font first, then fallbacks: if the resource ever fails to resolve, WPF walks
    /// the list rather than dropping to a serif default that would make the window look broken.
    /// </summary>
    private const string EmbeddedFont = "./Fonts/#Space Grotesk, Segoe UI Variable Text, Segoe UI, Tahoma";

    private static FontFamily ResolveFontFamily()
    {
        try
        {
            return new FontFamily(FontBaseUri, EmbeddedFont);
        }
        catch (Exception ex) when (ex is ArgumentException or UriFormatException)
        {
            return new FontFamily("Segoe UI Variable Text, Segoe UI, Tahoma");
        }
    }
}
