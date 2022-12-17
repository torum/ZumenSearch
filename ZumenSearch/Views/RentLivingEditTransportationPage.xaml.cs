using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentLivingEditTransportationPage : Page
{
    public RentLivingEditTransportationViewModel ViewModel
    {
        get;
    }

    public RentLivingEditTransportationPage()
    {
        ViewModel = App.GetService<RentLivingEditTransportationViewModel>();
        InitializeComponent();
    }
}
