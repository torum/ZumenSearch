using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Presentation.Shell.ViewModels;
namespace ZumenSearch.Presentation.Shell.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
