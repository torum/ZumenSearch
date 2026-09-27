using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Listing;

public sealed partial class AppliancesPage : Page
{
    public ViewModels.Rent.Residentials.Listing.ListingViewModel? ViewModel { get; private set; }

    public AppliancesPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.ApplianceViewModel();
        InitializeComponent();
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Listing.ListingViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Listing.ListingViewModel;
            // comment out when x:Bind expressions is added.
            //Bindings.Update();
        }
        base.OnNavigatedTo(e);
    }
}
