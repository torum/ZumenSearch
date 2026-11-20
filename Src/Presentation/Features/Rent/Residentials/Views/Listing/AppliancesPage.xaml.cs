using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing;

public sealed partial class AppliancesPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel? ViewModel { get; private set; }

    public AppliancesPage()
    {
        //ViewModel = new ViewZumenSearch.Presentation.Features.Rent.Residentials.Models.Editor.Modal.ApplianceViewModel();
        InitializeComponent();
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }
        base.OnNavigatedTo(e);
    }
}
