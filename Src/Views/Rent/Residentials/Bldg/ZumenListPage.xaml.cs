using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class ZumenListPage : Page
{
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

    public ZumenListPage()
    {
        //ViewModel = new ZumenListViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.MainViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.MainViewModel;
        }

        base.OnNavigatedTo(e);
    }

    private async void AppBarButtonAddPdf_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (sender is AppBarButton button)
        {
            button.IsEnabled = false;
            var openPicker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);

            // Set options for your file picker
            openPicker.ViewMode = Microsoft.Windows.Storage.Pickers.PickerViewMode.List;
            openPicker.SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".pdf");

            // Open the picker for the user to pick a file
            var files = await openPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                List<string> list = [];
                foreach (var file in files)
                {
                    list.Add(file.Path);
                }

                await ViewModel.Bldg.SetNewBuildingPdfsAsync(list);
            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
            }

            button.IsEnabled = true;

        }

    }
}
