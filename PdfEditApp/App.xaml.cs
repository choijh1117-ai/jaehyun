using System.Windows;
using Microsoft.Extensions.Configuration;

namespace PdfEditApp;

public partial class App : Application
{
    public static IConfiguration Configuration { get; private set; } = new ConfigurationBuilder().Build();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    }
}
