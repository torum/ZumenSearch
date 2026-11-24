using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing;

public sealed partial class ZumenListPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel? ViewModel { get; private set; }

    public ZumenListPage()
    {
        //ViewModel = new ViewZumenSearch.Presentation.Features.Rent.Commercials.Models.Editor.Modal.ZumenViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel vm)
        {
            //_editorShell = e.Parameter as ZumenSearch.Presentation.Features.Rent.Commercials.Views.Editor.EditorShell;
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
        try
        {
            var openPicker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                ViewMode = Microsoft.Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            openPicker.FileTypeFilter.Add(".pdf");
            if (!ViewModel.TryBeginPicker()) return;
            List<string> paths;
            try
            {
                var files = await openPicker.PickMultipleFilesAsync();
                paths = files.Select(file => file.Path).ToList();
            }
            finally
            {
                ViewModel.EndPicker();
            }

            if (paths.Count > 0 && ViewModel.AddNewListingPdfsCommand.CanExecute(paths))
            {
                await ViewModel.AddNewListingPdfsCommand.ExecuteAsync(paths);
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
            button.IsEnabled = true;
        }
    }
}
