using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class UnitPictureListPage : Page
{
    //private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    private ViewModels.Rent.Residentials.Editor.EditorViewModel? _viewModel;
    public ViewModels.Rent.Residentials.Editor.EditorViewModel? ViewModel
    {
        get => _viewModel;
        private set
        {
            if (value != null)
            {
                //
                _viewModel = value;
            }
        }
    }

    public UnitPictureListPage()
    {
        //ViewModel = new UnitPictureListViewModel();//App.GetService<RentLivingEditPictureShellViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Editor.EditorViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Editor.EditorViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
