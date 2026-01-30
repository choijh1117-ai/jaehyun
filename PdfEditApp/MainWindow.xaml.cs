using System.Windows;
using PdfEditApp.ViewModels;

namespace PdfEditApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
