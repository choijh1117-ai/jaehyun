using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfEdit.Core;
using PdfEditApp.Services;

namespace PdfEditApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PdfTaskService _service = new();
    private CancellationTokenSource? _cts;

    public ObservableCollection<PdfOperationType> OperationTypes { get; } = new()
    {
        PdfOperationType.Merge,
        PdfOperationType.Split,
        PdfOperationType.Rotate,
        PdfOperationType.Reorder
    };

    public ObservableCollection<string> InputFiles { get; } = new();
    public ObservableCollection<string> Logs { get; } = new();

    [ObservableProperty]
    private PdfOperationType _selectedOperation = PdfOperationType.Merge;

    [ObservableProperty]
    private string _qpdfPath = string.Empty;

    [ObservableProperty]
    private string _outputFolder = string.Empty;

    [ObservableProperty]
    private string _outputFileName = "output.pdf";

    [ObservableProperty]
    private string _splitOutputPattern = "output-%d.pdf";

    [ObservableProperty]
    private int _rotateAngle = 90;

    [ObservableProperty]
    private string _pageOrder = "1-";

    [ObservableProperty]
    private string? _selectedInputFile;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _isBusy;

    public MainViewModel()
    {
        _qpdfPath = App.Configuration["Qpdf:Path"] ?? string.Empty;
    }

    [RelayCommand]
    private void AddFiles()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true)
        {
            foreach (var file in dialog.FileNames)
            {
                if (!InputFiles.Contains(file))
                {
                    InputFiles.Add(file);
                }
            }
        }
    }

    [RelayCommand]
    private void RemoveSelectedFile()
    {
        if (SelectedInputFile is null)
        {
            return;
        }

        InputFiles.Remove(SelectedInputFile);
    }

    [RelayCommand]
    private void BrowseOutputFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select output folder"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            OutputFolder = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private void BrowseQpdf()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "qpdf.exe|qpdf.exe|Executable files (*.exe)|*.exe"
        };

        if (dialog.ShowDialog() == true)
        {
            QpdfPath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void ClearLogs() => Logs.Clear();

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        IsBusy = true;
        Progress = 0;
        AppendLog("Starting task...");

        try
        {
            var request = new PdfTaskRequest
            {
                OperationType = SelectedOperation,
                QpdfPath = QpdfPath,
                InputFiles = InputFiles.ToList(),
                OutputFolder = OutputFolder,
                OutputFileName = OutputFileName,
                SplitOutputPattern = SplitOutputPattern,
                RotateAngle = RotateAngle,
                PageOrder = PageOrder
            };

            var progress = new Progress<double>(value => Progress = value);
            var logSink = new UiLogSink(AppendLog);

            await _service.RunAsync(request, _cts.Token, progress, logSink);
            AppendLog("Completed.");
        }
        catch (OperationCanceledException)
        {
            AppendLog("Cancelled.");
        }
        catch (Exception ex)
        {
            AppendLog($"Error: {ex.Message}");
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanStart() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    private bool CanCancel() => IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        StartCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    private void AppendLog(string message)
    {
        var stamp = DateTime.Now.ToString("HH:mm:ss");
        Logs.Add($"[{stamp}] {message}");
    }
}
