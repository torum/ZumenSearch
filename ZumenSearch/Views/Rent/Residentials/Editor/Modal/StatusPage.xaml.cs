using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels;


namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;

public sealed partial class StatusPage : Page
{
    public ViewModels.Rent.Residentials.Editor.Modal.StatusViewModel ViewModel
    {
        get;
    }

    public StatusPage()
    {
        ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.StatusViewModel();
        InitializeComponent();
    }
}
