using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.RentLivingEdit.Units;

namespace ZumenSearch.Views.RentLivingEdit.Units;

public sealed partial class RentLivingEditUnitEditStatusPage : Page
{
    public RentLivingEditUnitEditStatusViewModel ViewModel
    {
        get;

    }
    public RentLivingEditUnitEditStatusPage()
    {
        ViewModel = new RentLivingEditUnitEditStatusViewModel();
        InitializeComponent();
    }
}
