using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class TransportationPage : Page
{
    public TransportationViewModel ViewModel
    {
        get;
    }

    public TransportationPage()
    {
        ViewModel = new TransportationViewModel();//App.GetService<RentLivingEditTransportationViewModel>();
        InitializeComponent();
    }
}
