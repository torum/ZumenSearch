using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class PictureListPage : Page
{
    public PictureListViewModel ViewModel
    {
        get;
    }

    public PictureListPage()
    {
        ViewModel = new PictureListViewModel();//App.GetService<RentLivingEditPictureShellViewModel>();
        InitializeComponent();
    }
}
