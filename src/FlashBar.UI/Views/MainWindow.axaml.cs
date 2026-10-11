using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using FlashBar.UI.ViewModels;

namespace FlashBar.UI.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _searchAnimationTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(400),
    };
    private int _searchAnimationDotCount = 1;

    public MainWindow()
    {
        InitializeComponent();
        _searchAnimationTimer.Tick += OnSearchAnimationTimerTick;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        SearchTextBox.Focus();
        _searchAnimationTimer.Start();
    }

    protected override void OnClosed(EventArgs e)
    {
        _searchAnimationTimer.Stop();
        base.OnClosed(e);
    }

    private void OnSearchAnimationTimerTick(object? sender, EventArgs e)
    {
        if (DataContext is not MainViewModel { IsSearching: true })
        {
            _searchAnimationDotCount = 1;
            SearchingStatusText.Text = "Searching app folder.";
            return;
        }

        _searchAnimationDotCount = _searchAnimationDotCount % 3 + 1;
        SearchingStatusText.Text = $"Searching app folder{new string('.', _searchAnimationDotCount)}";
    }

    private void OnApplicationDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.LaunchSelectedCommand.Execute(null);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
            return;
        }

        base.OnKeyDown(e);
    }
}