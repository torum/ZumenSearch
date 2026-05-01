using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class AppliancePage : Page
{
    //private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

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

    public AppliancePage()
    {
        //ViewModel = new ApplianceViewModel();//App.GetService<RentLivingEditApplianceViewModel>();
        InitializeComponent();


    }



    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.ResidentialsViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.ResidentialsViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
