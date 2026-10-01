using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class ZumenListPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ViewModel { get; private set; }

    public ZumenListPage()
    {
        //ViewModel = new ZumenListViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Commercials.PropertyViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Commercials.PropertyViewModel;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

    private async void AppBarButtonAddPdf_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel is null || sender is not AppBarButton button)
        {
            return;
        }

        button.IsEnabled = false;

        try
        {
            var openPicker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                ViewMode = Microsoft.Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            openPicker.FileTypeFilter.Add(".pdf");

            // Open the picker for the user to pick a file
            var files = await openPicker.PickMultipleFilesAsync();
            if (files.Count <= 0)
            {
                Debug.WriteLine("Operation cancelled.");
                return;
            }
            var paths = files
                .Select(file => file.Path)
                .ToList();

            if (ViewModel.AddNewBuildingPdfsCommand.CanExecute(paths))
            {
                await ViewModel.AddNewBuildingPdfsCommand.ExecuteAsync(paths);
                //await ViewModel.SetNewBuildingPdfsAsync(paths);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in AppBarButtonAddPdf_Click: {ex.Message}");
        }
        finally
        {
            button.IsEnabled = true;
        }
    }
}
