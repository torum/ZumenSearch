using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Owners;

namespace ZumenSearch.Views.Rent.Owners;

public sealed partial class OwnersPage : Page
{
    public OwnersViewModel ViewModel
    {
        get;
    }

    public OwnersPage()
    {
        ViewModel = App.GetService<OwnersViewModel>();
        InitializeComponent();
    }
}
