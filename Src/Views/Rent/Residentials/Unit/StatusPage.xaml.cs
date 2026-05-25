using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;


namespace ZumenSearch.Views.Rent.Residentials.Unit;

internal sealed partial class StatusPage : Page
{
    public ViewModels.Rent.Residentials.MainViewModel? ViewModel { get; private set; }

    public StatusPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.StatusViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.MainViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.MainViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
