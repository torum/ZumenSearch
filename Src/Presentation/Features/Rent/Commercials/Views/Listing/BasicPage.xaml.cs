using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing;

public sealed partial class BasicPage : Page
{
    private bool _initialized;

    public BasicPage()
    {
        InitializeComponent();

        this.Loaded += Page_Loaded;
    }

    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel? ViewModel { get; private set; }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Init();
    }

    private void Init()
    {
        if (_initialized) return;

        _initialized = true;

        this.TextBoxName.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }
        base.OnNavigatedTo(e);
    }
}
