using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class FacilitiesPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ViewModel { get; private set; }

    public FacilitiesPage()
    {
        //ViewModel = new ApplianceViewModel();//App.GetService<RentLivingEditApplianceViewModel>();
        InitializeComponent();

    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Commercials.PropertyViewModel vm)
        {
            //_editorShell = e.Parameter as Views.Rent.Commercials.EditorShell;
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
