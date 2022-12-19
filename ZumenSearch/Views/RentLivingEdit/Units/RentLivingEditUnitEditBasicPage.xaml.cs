using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.RentLivingEdit.Units;

namespace ZumenSearch.Views.RentLivingEdit.Units;

public sealed partial class RentLivingEditUnitEditBasicPage : Page
{
    public RentLivingEditUnitEditBasicViewModel ViewModel
    {
        get;

    }
    public RentLivingEditUnitEditBasicPage()
    {
        ViewModel = new RentLivingEditUnitEditBasicViewModel();
        InitializeComponent();
    }
}
