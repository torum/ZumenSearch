using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class LocationPage : Page
{
    public LocationViewModel ViewModel
    {
        get;
    }

    public LocationPage()
    {
        ViewModel = new LocationViewModel();//App.GetService<RentLivingEditLocationViewModel>();
        InitializeComponent();
    }
}
