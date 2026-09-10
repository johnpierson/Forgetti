using System.Diagnostics;

namespace Forgetti.Revit;

internal static class RevitProcess
{
    /// <summary>
    /// Whether any Revit is open. Only the main process counts: the RevitAccelerator and
    /// RevitWorker helpers run without a Revit window and never touch Revit.ini.
    /// </summary>
    public static bool IsRunning()
    {
        Process[] processes = Process.GetProcessesByName("Revit");

        try
        {
            return processes.Length > 0;
        }
        finally
        {
            foreach (Process process in processes)
            {
                process.Dispose();
            }
        }
    }
}
