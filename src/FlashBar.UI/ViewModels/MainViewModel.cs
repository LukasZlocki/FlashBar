using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlashBar.Service;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlashBar.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IApplicationSearchService _searchService;
    private readonly IApplicationLaunchService _launchService;
    private CancellationTokenSource? _searchCancellation;

    public MainViewModel(IApplicationSearchService searchService, IApplicationLaunchService launchService)
    {
        _searchService = searchService;
        _launchService = launchService;
    }

    public event EventHandler? LaunchSucceeded;

    public ObservableCollection<ApplicationItemViewModel> Applications { get; } = [];

    [ObservableProperty]
    public partial ApplicationItemViewModel? SelectedApplication { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    public partial string? StatusMessage { get; set; }

    [ObservableProperty]
    public partial bool IsSearching { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public bool HasApplications => Applications.Count > 0;

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

    partial void OnSearchTextChanged(string value) => _ = RefreshAsync();

    public async Task RefreshAsync()
    {
        _searchCancellation?.Cancel();
        var cancellation = _searchCancellation = new CancellationTokenSource();
        Applications.Clear();
        SelectedApplication = null;
        StatusMessage = null;
        IsSearching = false;
        OnPropertyChanged(nameof(HasApplications));

        try
        {
            var results = await _searchService.SearchAsync(
                SearchText,
                cancellation.Token,
                () => IsSearching = true);
            cancellation.Token.ThrowIfCancellationRequested();
            if (_searchCancellation != cancellation)
            {
                return;
            }

            foreach (var application in results)
            {
                Applications.Add(new ApplicationItemViewModel(application));
            }

            SelectedApplication = Applications.FirstOrDefault();
            StatusMessage = null;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            if (_searchCancellation == cancellation)
            {
                Applications.Clear();
                SelectedApplication = null;
                StatusMessage = $"Could not load applications: {exception.Message}";
            }
        }
        finally
        {
            if (_searchCancellation == cancellation)
            {
                IsSearching = false;
                _searchCancellation = null;
            }

            cancellation.Dispose();
            OnPropertyChanged(nameof(HasApplications));
        }
    }

    [RelayCommand]
    private void SelectPrevious() => MoveSelection(-1);

    [RelayCommand]
    private void SelectNext() => MoveSelection(1);

    [RelayCommand]
    private async Task LaunchSelectedAsync()
    {
        var application = SelectedApplication;
        if (application is null)
        {
            return;
        }

        try
        {
            await _launchService.LaunchAsync(application.Application);
            StatusMessage = null;
            LaunchSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (ApplicationPersistenceException exception)
        {
            StatusMessage = $"Started {application.Name}, but could not save it: {exception.InnerException?.Message ?? exception.Message}";
        }
        catch (Exception exception)
        {
            StatusMessage = $"Could not start {application.Name}: {exception.Message}";
        }
    }

    private void MoveSelection(int offset)
    {
        if (Applications.Count == 0)
        {
            return;
        }

        var currentIndex = SelectedApplication is null ? -1 : Applications.IndexOf(SelectedApplication);
        var nextIndex = Math.Clamp(currentIndex + offset, 0, Applications.Count - 1);
        SelectedApplication = Applications[nextIndex];
    }
}
