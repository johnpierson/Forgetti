using System.Windows;
using Forgetti.Theming;

namespace Forgetti;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LaunchOptions options = LaunchOptions.Parse(e.Args);

        // The palette is written before any window exists, so the first frame is already themed.
        Theme.Apply(Resources, options.Dark);

        MainWindow window = new(options);
        MainWindow = window;
        window.Show();
    }
}
