using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Views;

public sealed partial class PictureListPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel? ViewModel { get; private set; }

    public PictureListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

    private async void AppBarButtonAddPicture_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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
                SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".webp");

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

            if (paths.Count == 0)
            {
                Debug.WriteLine("PickMultipleFilesAsync: Operation cancelled. @AppBarButtonAddPicture_Click");
                return;
            }

            if (ViewModel.AddNewPropertyPicturesCommand.CanExecute(paths))
            {
                await ViewModel.AddNewPropertyPicturesCommand.ExecuteAsync(paths);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to import property pictures: {ex}");
            ViewModel.ShowSimpleErrorInfoBar("画像の読み込みエラー", $"画像を読み込めませんでした: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            button.IsEnabled = true;
        }
    }
}
