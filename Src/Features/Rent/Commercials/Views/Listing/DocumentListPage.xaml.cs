using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Views.Rent.Commercials.Listing;

public sealed partial class DocumentListPage : Page
{
    public ViewModels.Rent.Commercials.ListingViewModel? ViewModel { get; private set; }

    public DocumentListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Commercials.ListingViewModel viewModel)
        {
            ViewModel = viewModel;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

    private async void AppBarButtonAddPdf_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel is null || sender is not AppBarButton button) return;

        button.IsEnabled = false;
        ViewModel.BeginPicker();
        try
        {
            var picker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                ViewMode = Microsoft.Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add(".pdf");
            var files = await picker.PickMultipleFilesAsync();
            var paths = files.Select(file => file.Path).ToList();
            if (paths.Count > 0 && ViewModel.AddNewRoomPdfsCommand.CanExecute(paths))
            {
                await ViewModel.AddNewRoomPdfsCommand.ExecuteAsync(paths);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to import commercial listing PDFs: {ex}");
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
