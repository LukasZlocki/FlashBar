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
            var repository = new JsonApplicationRepository(Path.Combine(AppContext.BaseDirectory, "apps.json"));
            var viewModel = new MainViewModel(
                new ApplicationSearchService(repository),
                new ApplicationLaunchService(new ProcessApplicationLauncher()));

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