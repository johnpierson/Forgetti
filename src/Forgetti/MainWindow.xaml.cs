using System;
using System.Windows;
using System.Windows.Threading;
using Forgetti.Revit;
using Forgetti.Ui;

namespace Forgetti;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly LaunchOptions _options;
    private readonly DispatcherTimer _revitPoll;

    internal MainWindow(LaunchOptions options)
    {
        InitializeComponent();

        _options = options;

        Func<bool> isRevitRunning = options.PretendRevitRunning ? () => true : RevitProcess.IsRunning;
        _viewModel = options.Sample ? MainViewModel.Sample(isRevitRunning) : new MainViewModel(isRevitRunning);
        DataContext = _viewModel;

        // Revit rewrites Revit.ini on exit, so the window watches for it closing and lets the
        // user carry on without relaunching.
        _revitPoll = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(2) };
        _revitPoll.Tick += (_, _) => _viewModel.RefreshRevitState();
        _revitPoll.Start();
        Closed += (_, _) => _revitPoll.Stop();

        if (options.ScreenshotPath is not null)
        {
            ContentRendered += OnContentRenderedForScreenshot;
        }
    }

    private void OnContentRenderedForScreenshot(object? sender, EventArgs e)
    {
        ContentRendered -= OnContentRenderedForScreenshot;

        // ContextIdle: after layout has settled and every binding has delivered its value.
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
        {
            Screenshot.Save(this, _options.ScreenshotPath!);
            Application.Current.Shutdown();
        }));
    }
}
