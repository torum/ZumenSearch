using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Commercials.Listing;

public sealed partial class BrokerListPage : Page
{
    public ViewModels.Rent.Commercials.ListingViewModel? ViewModel { get; private set; }

    public BrokerListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Commercials.ListingViewModel viewModel)
        {
            ViewModel = viewModel;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
