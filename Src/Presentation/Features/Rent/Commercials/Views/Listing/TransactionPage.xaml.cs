using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing;

public sealed partial class TransactionPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel? ViewModel { get; private set; }

    public TransactionPage()
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
