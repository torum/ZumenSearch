using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentLivingEditZumenPage : Page
{
    public RentLivingEditZumenViewModel ViewModel
    {
        get;
    }

    public RentLivingEditZumenPage()
    {
        ViewModel = App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }
}
