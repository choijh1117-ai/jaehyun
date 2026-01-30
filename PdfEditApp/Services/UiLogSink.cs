using System.Windows;
using PdfEdit.Core;

namespace PdfEditApp.Services;

public sealed class UiLogSink : ILogSink
{
    private readonly Action<string> _append;

    public UiLogSink(Action<string> append)
    {
        _append = append;
    }

    public void WriteLine(string message)
    {
        if (Application.Current?.Dispatcher is { } dispatcher)
        {
            dispatcher.Invoke(() => _append(message));
        }
        else
        {
            _append(message);
        }
    }
}
