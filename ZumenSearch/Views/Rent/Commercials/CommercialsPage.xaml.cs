using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Commercials;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class CommercialsPage : Page
{
    public CommercialsViewModel ViewModel
    {
        get;
    }

    public CommercialsPage()
    {
        ViewModel = App.GetService<CommercialsViewModel>();
        InitializeComponent();
    }
}
