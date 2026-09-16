using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;


namespace ZumenSearch.Views.Rent.Residentials.Listing;

public sealed partial class BrokerListPage : Page
{
    public ViewModels.Rent.Residentials.Listing.ListingViewModel? ViewModel { get; private set; }

    public BrokerListPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.GyousyaViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Listing.ListingViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Listing.ListingViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
