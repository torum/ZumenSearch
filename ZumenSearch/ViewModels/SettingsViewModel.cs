using System;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using ZumenSearch.Helpers;
using Microsoft.Windows.ApplicationModel.Resources;

namespace ZumenSearch.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private static readonly ResourceLoader _resourceLoader = new();

    private string _versionDescription;

    public string VersionDescription
    {
        get => _versionDescription;
        private set => SetProperty(ref _versionDescription, value);
    }

    /*
    private readonly IThemeSelectorService _themeSelectorService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    public ICommand SwitchThemeCommand
    {
        get;
    }
    */

    public SettingsViewModel()
    {
        _versionDescription = GetVersionDescription();
    }

    /*
    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);

                    // WeakReferenceMessenger
                    var thm = ElementTheme.ToString().ToLower();
                    // send message to other windows (Editor windows)
                    WeakReferenceMessenger.Default.Send(new ThemeChangedMessage(thm));
                }
            });
    }
    */

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        var verName =_resourceLoader.GetString("AppDisplayName");

        return $"{verName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
