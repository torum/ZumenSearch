using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Services.Contracts;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class BrokerSearchPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    private readonly INavigationService _navigationService;

    public BrokerSearchPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();

        this.Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.TextBoxName.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }
}
