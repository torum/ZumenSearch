using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.RentLivingEdit;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditAppliancePage : Page
{
    public RentLivingEditApplianceViewModel ViewModel
    {
        get;
    }

    public RentLivingEditAppliancePage()
    {
        ViewModel = new RentLivingEditApplianceViewModel();//App.GetService<RentLivingEditApplianceViewModel>();
        InitializeComponent();
    }
}
