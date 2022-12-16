using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentLivingEditBuildingPage : Page
{
    public RentLivingEditBuildingViewModel ViewModel
    {
        get;
    }

    public RentLivingEditBuildingPage()
    {
        ViewModel = App.GetService<RentLivingEditBuildingViewModel>();
        InitializeComponent();
    }
}
