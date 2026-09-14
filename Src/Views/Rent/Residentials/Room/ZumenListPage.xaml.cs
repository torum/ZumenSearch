using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Views.Rent.Residentials.Room;

public sealed partial class ZumenListPage : Page
{
    public ViewModels.Rent.Residentials.Room.ListingViewModel? ViewModel { get; private set; }

    public ZumenListPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.ZumenViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Room.ListingViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Room.ListingViewModel;
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

                if (ViewModel.AddNewRoomPdfsCommand.CanExecute(list))
                {
                    await ViewModel.AddNewRoomPdfsCommand.ExecuteAsync(list);
                    //await ViewModel.SetNewUnitPicturesAsync(list);
                }
            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
            }

            button.IsEnabled = true;
        }
    }
}
