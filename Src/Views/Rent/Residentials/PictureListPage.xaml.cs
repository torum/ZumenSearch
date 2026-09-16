using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class PictureListPage : Page
{
    public ViewModels.Rent.Residentials.PropertyViewModel? ViewModel { get; private set; }

    public PictureListPage()
    {
        InitializeComponent();

    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.PropertyViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.PropertyViewModel;
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
            var openPicker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                // Set options for your file picker
                ViewMode = Microsoft.Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
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

                if (ViewModel.AddNewBuildingPicturesCommand.CanExecute(list))
                {
                    await ViewModel.AddNewBuildingPicturesCommand.ExecuteAsync(list);
                    //await ViewModel.Bldg.AddNewBuildingPictures(list);
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
