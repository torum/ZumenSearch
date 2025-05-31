using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class AppliancePage : Page
{
    public ApplianceViewModel ViewModel
    {
        get;
    }

    public AppliancePage()
    {
        ViewModel = new ApplianceViewModel();//App.GetService<RentLivingEditApplianceViewModel>();
        InitializeComponent();
    }
}
