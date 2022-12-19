using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.RentLivingEdit;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditZumenListPage : Page
{
    public RentLivingEditZumenListViewModel ViewModel
    {
        get;
    }

    public RentLivingEditZumenListPage()
    {
        ViewModel = new RentLivingEditZumenListViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }
}
