using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class ZumenListPage : Page
{
    public ZumenListViewModel ViewModel
    {
        get;
    }

    public ZumenListPage()
    {
        ViewModel = new ZumenListViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }
}
