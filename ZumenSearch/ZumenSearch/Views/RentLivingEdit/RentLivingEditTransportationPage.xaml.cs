using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.RentLivingEdit;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditTransportationPage : Page
{
    public RentLivingEditTransportationViewModel ViewModel
    {
        get;
    }

    public RentLivingEditTransportationPage()
    {
        ViewModel = new RentLivingEditTransportationViewModel();//App.GetService<RentLivingEditTransportationViewModel>();
        InitializeComponent();
    }
}
