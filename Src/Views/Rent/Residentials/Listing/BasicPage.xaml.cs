using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Listing;

public sealed partial class BasicPage : Page
{
    private bool _initialized;

    public BasicPage()
    {
        InitializeComponent();

        this.Loaded += Page_Loaded;
    }

    public ViewModels.Rent.Residentials.ListingViewModel? ViewModel { get; private set; }

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
        if (e.Parameter is ViewModels.Rent.Residentials.ListingViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
