using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FlashBar.Infrastructure;
using FlashBar.Service;
using FlashBar.UI.ViewModels;
using FlashBar.UI.Views;

namespace FlashBar.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FlashBar",
                "apps.json");
            var repository = new JsonApplicationRepository(
                settingsPath,
                Path.Combine(AppContext.BaseDirectory, "apps.json"));
            var viewModel = new MainViewModel(
                new ApplicationSearchService(repository, new WindowsApplicationDiscoveryService()),
                new ApplicationLaunchService(new ProcessApplicationLauncher(), repository));

            var window = new MainWindow
            {
                DataContext = viewModel,
            };
            viewModel.LaunchSucceeded += (_, _) => window.Close();

            desktop.MainWindow = window;
            _ = viewModel.RefreshAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}