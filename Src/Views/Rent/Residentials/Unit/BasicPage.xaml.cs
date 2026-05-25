using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Unit;

internal sealed partial class BasicPage : Page
{
    public ViewModels.Rent.Residentials.MainViewModel? ViewModel { get; private set; }

    private bool _initialized;

    public BasicPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.SummaryViewModel();
        InitializeComponent();

    }
    private void Init()
    {
        if (_initialized) return;

        _initialized = true;

        this.TextBoxName.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.MainViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.MainViewModel;

            if (!_initialized)
            {
                //Init();
            }
        }

        base.OnNavigatedTo(e);
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Init();
    }
}
