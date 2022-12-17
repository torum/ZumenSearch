using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentLivingEditLocationPage : Page
{
    public RentLivingEditLocationViewModel ViewModel
    {
        get;
    }

    public RentLivingEditLocationPage()
    {
        ViewModel = App.GetService<RentLivingEditLocationViewModel>();
        InitializeComponent();
    }
}
