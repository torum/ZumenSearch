using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;


namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing;

public sealed partial class BrokerListPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel? ViewModel { get; private set; }

    public BrokerListPage()
    {
        InitializeComponent();
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
