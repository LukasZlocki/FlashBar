using CommunityToolkit.Mvvm.ComponentModel;

namespace FlashBar.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;
}
