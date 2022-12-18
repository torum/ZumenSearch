using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RealEstateBrokerPage : Page
{
    public RealEstateBrokerViewModel ViewModel
    {
        get;
    }

    public RealEstateBrokerPage()
    {
        ViewModel = App.GetService<RealEstateBrokerViewModel>();
        InitializeComponent();
    }
}
