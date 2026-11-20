using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Infrastructure.Configuration.Contracts;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Presentation.Services.Contracts;
using ZumenSearch.Presentation.Shell.ViewModels;

namespace ZumenSearch.Presentation.Search.Views.Rent;

public sealed partial class LessorSearchPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    private readonly INavigationService _navigationService;

    public LessorSearchPage()
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
