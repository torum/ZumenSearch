using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentParkingPage : Page
{
    public RentParkingViewModel ViewModel
    {
        get;
    }

    public RentParkingPage()
    {
        ViewModel = App.GetService<RentParkingViewModel>();
        InitializeComponent();
    }
}
