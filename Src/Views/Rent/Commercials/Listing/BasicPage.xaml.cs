using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Commercials.Listing;

public sealed partial class BasicPage : Page
{
    public ViewModels.Rent.Commercials.Listing.ListingViewModel? ViewModel { get; private set; }

    public BasicPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = e.Parameter as ViewModels.Rent.Commercials.Listing.ListingViewModel;
        Bindings.Update();
        base.OnNavigatedTo(e);
    }
}