using System;
using System.IO;
using Avalonia.Media.Imaging;
using FlashBar.Domain;

namespace FlashBar.UI.ViewModels;

public sealed class ApplicationItemViewModel : ViewModelBase
{
    public ApplicationItemViewModel(ApplicationEntry application)
    {
        Application = application;
        Icon = LoadIcon(application.IconPath);
    }

    public ApplicationEntry Application { get; }

    public string Name => Application.Name;

    public string Initial => Name[..1].ToUpperInvariant();

    public Bitmap? Icon { get; }

    public bool HasIcon => Icon is not null;

    private static Bitmap? LoadIcon(string? path)
    {
        if (path is null || !File.Exists(path))
        {
            return null;
        }

        try
        {
            return new Bitmap(path);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
