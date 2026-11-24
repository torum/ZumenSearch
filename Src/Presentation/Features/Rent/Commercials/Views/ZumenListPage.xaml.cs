using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views;

public sealed partial class ZumenListPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel? ViewModel { get; private set; }

    public ZumenListPage()
    {
        //ViewModel = new ZumenListViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel vm)
        {
            ViewModel = vm;
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
                Debug.WriteLine("PickMultipleFilesAsync: Operation cancelled. @AppBarButtonAddPdf_Click");
                return;
            }

            if (ViewModel.AddNewPropertyPdfsCommand.CanExecute(paths))
            {
                await ViewModel.AddNewPropertyPdfsCommand.ExecuteAsync(paths);
                //await ViewModel.SetNewPropertyPdfsAsync(paths);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to import property PDFs: {ex}");
            ViewModel.ShowSimpleErrorInfoBar("PDFの読み込みエラー", $"PDFを読み込めませんでした: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            button.IsEnabled = true;
        }
    }
}
