using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System.Diagnostics;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class PictureListPage : Page
{
    //private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

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

    public PictureListPage()
    {
        InitializeComponent();

    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.MainViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.MainViewModel;
        }

        base.OnNavigatedTo(e);
    }

    /*
    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (sender.SelectedItem is not Models.Rent.Residentials.PictureBldg picbldg)
        {
            return;
        }

        //Debug.WriteLine($"PictureListPage ItemsView_SelectionChanged SelectedItem Changed to {picbldg.PictureType.Label}");

        ViewModel?.Bldg.SelectedBuildingPicture = picbldg;//sender.SelectedItem as Models.Rent.Residentials.PictureBuilding;
    }
    */

    private async void AppBarButtonAddPicture_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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
            openPicker.SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".webp");

            // Open the picker for the user to pick a file
            var files = await openPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                List<string> list = [];
                foreach (var file in files)
                {
                    list.Add(file.Path);
                }

                await ViewModel.Bldg.SetNewBuildingPicturesAsync(list);
            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
            }

            button.IsEnabled = true;

        }

    }
}
