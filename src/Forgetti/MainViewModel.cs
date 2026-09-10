using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Forgetti.Revit;
using Forgetti.Theming;

namespace Forgetti;

internal abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        Raise(name);
        return true;
    }

    protected void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// A command whose availability is re-checked whenever WPF re-queries commands (focus changes,
/// clicks, key presses). Enough for a window with two buttons; not a framework.
/// </summary>
internal sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
}

internal enum RowState
{
    /// <summary>Has recent files, and can be forgotten.</summary>
    Pending,

    /// <summary>Had nothing to forget when the window opened.</summary>
    Empty,

    /// <summary>Forgotten in this session; a backup exists.</summary>
    Cleared,
}

/// <summary>One Revit version as a row in the list.</summary>
internal sealed class InstallRow : ObservableObject
{
    private int _count;
    private bool _isSelected;
    private bool _canRestore;

    public InstallRow(RevitInstall install, int count)
    {
        Install = install;
        _count = count;
        _isSelected = count > 0;
    }

    public RevitInstall Install { get; }

    public string Name => Install.Name;

    public string IniPath => Install.IniPath;

    public int Count
    {
        get => _count;
        private set
        {
            if (Set(ref _count, value))
            {
                RaiseDerived();
            }
        }
    }

    public bool HasFiles => Count > 0;

    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }

    /// <summary>Whether a backup from this session exists for the row.</summary>
    public bool CanRestore
    {
        get => _canRestore;
        private set
        {
            if (Set(ref _canRestore, value))
            {
                RaiseDerived();
            }
        }
    }

    public RowState State => HasFiles ? RowState.Pending : CanRestore ? RowState.Cleared : RowState.Empty;

    /// <summary>The tag text beside the name. Rendered in spaced capitals, so it is written lower-case here.</summary>
    public string Summary => State switch
    {
        RowState.Pending => Count == 1 ? "1 file" : $"{Count} files",
        RowState.Cleared => "forgotten",
        _ => "empty",
    };

    public void MarkCleared()
    {
        CanRestore = true;
        Count = 0;
        IsSelected = false;
    }

    /// <summary>Re-reads the file. Used after a restore, so the count is what is really there.</summary>
    public void Refresh()
    {
        CanRestore = false;
        Count = RecentFileList.Count(IniPath);
        IsSelected = HasFiles;
    }

    private void RaiseDerived()
    {
        Raise(nameof(HasFiles));
        Raise(nameof(State));
        Raise(nameof(Summary));
    }
}

internal enum StatusKind
{
    None,
    Info,
    Success,
    Error,
}

internal sealed class MainViewModel : ObservableObject
{
    private readonly Func<bool> _isRevitRunning;
    private bool _isRevitRunning_;
    private string _statusMessage = string.Empty;
    private StatusKind _statusKind;

    public MainViewModel(Func<bool> isRevitRunning)
    {
        _isRevitRunning = isRevitRunning;

        ForgetCommand = new RelayCommand(Forget, () => CanForget);
        UndoCommand = new RelayCommand(Undo, () => CanUndo);
        SelectAllCommand = new RelayCommand(() => SelectAll(true));
        SelectNoneCommand = new RelayCommand(() => SelectAll(false));

        Reload();
        RefreshRevitState();
    }

    /// <summary>
    /// A view model showing one row in each state, with nothing behind it. For screenshots and
    /// for checking the look of states that need a real clear to reach.
    /// </summary>
    public static MainViewModel Sample(Func<bool> isRevitRunning)
    {
        MainViewModel sample = new(isRevitRunning);
        sample.Installs.Clear();

        InstallRow pending = new(new RevitInstall("Revit 2027", string.Empty), 5);
        InstallRow forgotten = new(new RevitInstall("Revit 2026", string.Empty), 12);
        forgotten.MarkCleared();
        InstallRow empty = new(new RevitInstall("Revit 2025", string.Empty), 0);

        sample.Installs.Add(pending);
        sample.Installs.Add(forgotten);
        sample.Installs.Add(empty);
        sample.SetStatus(StatusKind.Success, Describe(12, new List<string> { forgotten.Name }));
        sample.Raise(nameof(HasInstalls));
        sample.Raise(nameof(CanUndo));

        return sample;
    }

    public ObservableCollection<InstallRow> Installs { get; } = new();

    public bool HasInstalls => Installs.Count > 0;

    public bool IsRevitRunning
    {
        get => _isRevitRunning_;
        private set => Set(ref _isRevitRunning_, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => Set(ref _statusMessage, value);
    }

    public StatusKind StatusKind
    {
        get => _statusKind;
        private set => Set(ref _statusKind, value);
    }

    /// <summary>Paper or ink. Flipping it repaints the window through the theme's dynamic resources.</summary>
    public bool IsDark
    {
        get => Theme.IsDark;
        set
        {
            if (Theme.IsDark != value)
            {
                Theme.Apply(Application.Current.Resources, value);
                Raise();
            }
        }
    }

    public bool CanForget => !IsRevitRunning && Installs.Any(row => row.IsSelected && row.HasFiles);

    public bool CanUndo => Installs.Any(row => row.CanRestore);

    public ICommand ForgetCommand { get; }

    public ICommand UndoCommand { get; }

    public ICommand SelectAllCommand { get; }

    public ICommand SelectNoneCommand { get; }

    public void Reload()
    {
        Installs.Clear();

        foreach (RevitInstall install in RevitInstall.Discover())
        {
            int count;

            try
            {
                count = RecentFileList.Count(install.IniPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // A file that cannot be read cannot be cleared either; show it as empty rather
                // than hide the version, so the user can see it was found.
                count = 0;
            }

            Installs.Add(new InstallRow(install, count));
        }

        Raise(nameof(HasInstalls));
        Raise(nameof(CanUndo));
    }

    /// <summary>Polled by the window, so the banner clears itself when Revit is closed.</summary>
    public void RefreshRevitState()
    {
        IsRevitRunning = _isRevitRunning();
    }

    private void Forget()
    {
        List<InstallRow> targets = Installs.Where(row => row.IsSelected && row.HasFiles).ToList();
        List<string> cleared = new();
        List<string> failures = new();
        int files = 0;

        foreach (InstallRow row in targets)
        {
            try
            {
                files += RecentFileList.Clear(row.IniPath);
                row.MarkCleared();
                cleared.Add(row.Name);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                failures.Add($"{row.Name}: {ex.Message}");
            }
        }

        if (failures.Count > 0)
        {
            SetStatus(StatusKind.Error, string.Join(" ", failures));
        }
        else
        {
            SetStatus(StatusKind.Success, Describe(files, cleared));
        }

        Raise(nameof(CanUndo));
    }

    private static string Describe(int files, List<string> names)
    {
        string count = files == 1 ? "1 file" : $"{files} files";

        return names.Count == 1
            ? $"Forgot {count} in {names[0]}."
            : $"Forgot {count} across {names.Count} Revit versions.";
    }

    private void Undo()
    {
        List<string> failures = new();

        foreach (InstallRow row in Installs.Where(row => row.CanRestore).ToList())
        {
            try
            {
                RecentFileList.Restore(row.IniPath);
                row.Refresh();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                failures.Add($"{row.Name}: {ex.Message}");
            }
        }

        if (failures.Count > 0)
        {
            SetStatus(StatusKind.Error, string.Join(" ", failures));
        }
        else
        {
            SetStatus(StatusKind.Info, "Put back. Revit remembers again.");
        }

        Raise(nameof(CanUndo));
    }

    private void SelectAll(bool selected)
    {
        foreach (InstallRow row in Installs.Where(row => row.HasFiles))
        {
            row.IsSelected = selected;
        }
    }

    private void SetStatus(StatusKind kind, string message)
    {
        StatusKind = kind;
        StatusMessage = message;
    }
}
