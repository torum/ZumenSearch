using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentBussinessPage : Page
{
    public RentBussinessViewModel ViewModel
    {
        get;
    }

    public RentBussinessPage()
    {
        ViewModel = App.GetService<RentBussinessViewModel>();
        InitializeComponent();
    }
}
