using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.ViewModels;


namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;

public sealed partial class GyousyaPage : Page
{
    private ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel? _viewModel;
    public ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel? ViewModel
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

    public GyousyaPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.GyousyaViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
