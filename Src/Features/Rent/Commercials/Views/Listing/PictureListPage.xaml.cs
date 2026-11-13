using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Commercials.Listing;

public sealed partial class PictureListPage : Page
{
    public ViewModels.Rent.Commercials.ListingViewModel? ViewModel { get; private set; }

    public PictureListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Commercials.ListingViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }
        base.OnNavigatedTo(e);
    }

    private async void AppBarButtonAddPicture_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel is null || sender is not AppBarButton button) return;
        button.IsEnabled = false;
        ViewModel.BeginPicker();
        try
        {
            var picker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                ViewMode = Microsoft.Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".webp");
            var files = await picker.PickMultipleFilesAsync();
            var paths = files.Select(file => file.Path).ToList();
            if (paths.Count > 0 && ViewModel.AddNewRoomPicturesCommand.CanExecute(paths)) await ViewModel.AddNewRoomPicturesCommand.ExecuteAsync(paths);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to import commercial listing pictures: {ex}");
            ViewModel.InfoBarErrorMessage = $"画像を読み込めませんでした: {ex.Message}";
            ViewModel.IsInfoBarErrorOpen = true;
        }
        finally
        {
            ViewModel.EndPicker();
            button.IsEnabled = true;
        }
    }
}
