using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

internal sealed partial class BasicPage : Page
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

    public enum RentLivingKinds
    {

        Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
    }

    public BasicPage()
    {
        //Debug.WriteLine("Views.Rent.Residentials.Editor.BasicPage init!");

        //ViewModel = new ViewModels.Rent.Residentials.Editor.SummaryViewModel();//App.GetService<RentLivingEditBuildingViewModel>();

        InitializeComponent();

        /*
        ViewModel.EventEditStructure += (sender, arg) => OnEventEditStructure(arg);
        ViewModel.EventEditLocation += (sender, arg) => OnEventEditLocation(arg);
        ViewModel.EventEditTransportation += (sender, arg) => OnEventEditTransportation(arg);
        ViewModel.EventEditAppliance += (sender, arg) => OnEventEditAppliance(arg);
        */


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
