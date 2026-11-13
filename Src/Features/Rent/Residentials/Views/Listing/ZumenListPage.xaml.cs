using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Views.Rent.Residentials.Listing;

public sealed partial class ZumenListPage : Page
{
    public ViewModels.Rent.Residentials.ListingViewModel? ViewModel { get; private set; }

    public ZumenListPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.ZumenViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Residentials.ListingViewModel vm)
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

    private async void AppBarButtonAddPdf_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (sender is not AppBarButton button) return;

        button.IsEnabled = false;
        ViewModel.BeginPicker();
        try
        {
            var openPicker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                ViewMode = Microsoft.Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            openPicker.FileTypeFilter.Add(".pdf");
            var files = await openPicker.PickMultipleFilesAsync();
            var paths = files.Select(file => file.Path).ToList();
            if (paths.Count > 0 && ViewModel.AddNewRoomPdfsCommand.CanExecute(paths))
            {
                await ViewModel.AddNewRoomPdfsCommand.ExecuteAsync(paths);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to import listing PDFs: {ex}");
            ViewModel.InfoBarErrorMessage = $"PDFを読み込めませんでした: {ex.Message}";
            ViewModel.IsInfoBarErrorOpen = true;
        }
        finally
        {
            ViewModel.EndPicker();
            button.IsEnabled = true;
        }
    }
}
