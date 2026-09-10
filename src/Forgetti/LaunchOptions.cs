using System;

namespace Forgetti;

/// <summary>
/// Command-line switches. All of them exist for development and screenshots; a user
/// double-clicking the .exe never passes any.
/// </summary>
internal sealed class LaunchOptions
{
    /// <summary>Start in the dark palette instead of the light one.</summary>
    public bool Dark { get; private set; }

    /// <summary>Render the window to this PNG and exit, instead of waiting for the user.</summary>
    public string? ScreenshotPath { get; private set; }

    /// <summary>Pretend Revit is running, so the warning banner can be seen without launching Revit.</summary>
    public bool PretendRevitRunning { get; private set; }

    /// <summary>Show made-up rows in every state instead of this machine's Revit versions. Touches no files.</summary>
    public bool Sample { get; private set; }

    public static LaunchOptions Parse(string[] args)
    {
        LaunchOptions options = new();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg.Equals("--dark", StringComparison.OrdinalIgnoreCase))
            {
                options.Dark = true;
            }
            else if (arg.Equals("--revit-running", StringComparison.OrdinalIgnoreCase))
            {
                options.PretendRevitRunning = true;
            }
            else if (arg.Equals("--sample", StringComparison.OrdinalIgnoreCase))
            {
                options.Sample = true;
            }
            else if (arg.Equals("--screenshot", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                options.ScreenshotPath = args[++i];
            }
        }

        return options;
    }
}
