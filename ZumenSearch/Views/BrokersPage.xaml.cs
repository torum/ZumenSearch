using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class BrokersPage : Page
{
    private BrokersViewModel ViewModel
    {
        get;
    }

    public BrokersPage()
    {
        ViewModel = App.GetService<BrokersViewModel>();
        InitializeComponent();
    }
}
