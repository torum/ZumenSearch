using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.RentLivingEdit;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditPictureListPage : Page
{
    public RentLivingEditPictureListViewModel ViewModel
    {
        get;
    }

    public RentLivingEditPictureListPage()
    {
        ViewModel = new RentLivingEditPictureListViewModel();//App.GetService<RentLivingEditPictureShellViewModel>();
        InitializeComponent();
    }
}
