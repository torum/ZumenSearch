using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Brokers;

namespace ZumenSearch.Views.Brokers;

public sealed partial class BrokersPage : Page
{
    public BrokersViewModel ViewModel
    {
        get;
    }

    public BrokersPage()
    {
        ViewModel = App.GetService<BrokersViewModel>();
        InitializeComponent();
    }
}
