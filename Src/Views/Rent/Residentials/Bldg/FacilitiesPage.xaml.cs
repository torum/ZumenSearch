using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

internal sealed partial class FacilitiesPage : Page
{
    public ViewModels.Rent.Residentials.MainViewModel? ViewModel { get; private set; }

    public FacilitiesPage()
    {
        //ViewModel = new ApplianceViewModel();//App.GetService<RentLivingEditApplianceViewModel>();
        InitializeComponent();


    }



    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.MainViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.MainViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
