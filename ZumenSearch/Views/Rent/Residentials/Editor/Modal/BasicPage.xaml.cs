using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;

public sealed partial class BasicPage : Page
{
    private ViewModels.Rent.Residentials.Modal.ModalViewModel? _viewModel;
    public ViewModels.Rent.Residentials.Modal.ModalViewModel? ViewModel
    {
        get => _viewModel;
        private set
        {
            if (value != null)
            {
                _viewModel = value;

                //_viewModel.EventBackToSummary += (sender, arg) => OnEventBackToSummary(arg);
            }
        }
    }

    public BasicPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.SummaryViewModel();
        InitializeComponent();

    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Modal.ModalViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Modal.ModalViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
