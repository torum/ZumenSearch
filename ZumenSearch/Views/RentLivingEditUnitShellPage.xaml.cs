using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentLivingEditUnitShellPage : Page
{
    public RentLivingEditUnitShellViewModel ViewModel
    {
        get;
    }

    public RentLivingEditUnitShellPage()
    {
        ViewModel = App.GetService<RentLivingEditUnitShellViewModel>();
        InitializeComponent();
    }
}
