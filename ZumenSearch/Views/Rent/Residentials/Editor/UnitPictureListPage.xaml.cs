using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class UnitPictureListPage : Page
{
    public UnitPictureListViewModel ViewModel
    {
        get;
    }

    public UnitPictureListPage()
    {
        ViewModel = new UnitPictureListViewModel();//App.GetService<RentLivingEditPictureShellViewModel>();
        InitializeComponent();
    }
}
