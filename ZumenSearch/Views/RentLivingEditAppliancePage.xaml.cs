using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentLivingEditAppliancePage : Page
{
    public RentLivingEditApplianceViewModel ViewModel
    {
        get;
    }

    public RentLivingEditAppliancePage()
    {
        ViewModel = App.GetService<RentLivingEditApplianceViewModel>();
        InitializeComponent();
    }
}
