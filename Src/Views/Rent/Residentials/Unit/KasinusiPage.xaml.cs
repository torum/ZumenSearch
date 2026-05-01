using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;


namespace ZumenSearch.Views.Rent.Residentials.Unit;

public sealed partial class KasinusiPage : Page
{
    public ViewModels.Rent.ResidentialsViewModel? ViewModel
    {
        get;
        private set
        {
            if (value != null)
            {
                field = value;

                //_viewModel.EventBackToSummary += (sender, arg) => OnEventBackToSummary(arg);
            }
        }
    }

    public KasinusiPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.KasinusiViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.ResidentialsViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.ResidentialsViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
