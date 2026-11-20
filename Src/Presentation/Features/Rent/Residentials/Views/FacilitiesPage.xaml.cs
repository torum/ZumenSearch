using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Views;

public sealed partial class FacilitiesPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel? ViewModel { get; private set; }

    public FacilitiesPage()
    {
        //ViewModel = new ApplianceViewModel();//App.GetService<RentLivingEditApplianceViewModel>();
        InitializeComponent();

    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel vm)
        {
            //_editorShell = e.Parameter as ZumenSearch.Presentation.Features.Rent.Residentials.Views.EditorShell;
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
