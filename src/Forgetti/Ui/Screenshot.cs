using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Forgetti.Ui;

/// <summary>Renders a window's content to a PNG, for documentation and for checking the look without eyes on a screen.</summary>
internal static class Screenshot
{
    /// <summary>Rendered at twice the logical size, so 2px outlines and hard shadows survive the README.</summary>
    private const double Scale = 2d;

    public static void Save(Window window, string path)
    {
        if (window.Content is not FrameworkElement content)
        {
            throw new InvalidOperationException("The window has no content to render.");
        }

        int width = (int)Math.Ceiling(content.ActualWidth * Scale);
        int height = (int)Math.Ceiling(content.ActualHeight * Scale);

        RenderTargetBitmap bitmap = new(width, height, 96d * Scale, 96d * Scale, PixelFormats.Pbgra32);
        bitmap.Render(content);

        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream stream = File.Create(path);
        encoder.Save(stream);
    }
}
