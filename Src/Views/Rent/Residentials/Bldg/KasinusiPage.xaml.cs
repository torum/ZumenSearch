using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class KasinusiPage : Page
{
    //private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    public ViewModels.Rent.Residentials.MainViewModel? ViewModel
    {
        get;
        private set
        {
            if (value != null)
            {
                field = value;
            }
        }
    }

    public KasinusiPage()
    {
        //ViewModel = new KasinusiViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.MainViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            //ViewModel = _editorShell?.ViewModel as ViewModels.Rent.Residentials.Editor.EditorViewModel;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.MainViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
