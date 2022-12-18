using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.RentLivingEdit;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditLocationPage : Page
{
    public RentLivingEditLocationViewModel ViewModel
    {
        get;
    }

    public RentLivingEditLocationPage()
    {
        ViewModel = new RentLivingEditLocationViewModel();//App.GetService<RentLivingEditLocationViewModel>();
        InitializeComponent();
    }
}
