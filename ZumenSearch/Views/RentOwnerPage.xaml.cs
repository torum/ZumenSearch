using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentOwnerPage : Page
{
    public RentOwnerViewModel ViewModel
    {
        get;
    }

    public RentOwnerPage()
    {
        ViewModel = App.GetService<RentOwnerViewModel>();
        InitializeComponent();
    }
}
