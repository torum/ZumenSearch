using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Services;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials.Room;

public sealed partial class MainViewModel : ObservableObject
{
    #region == Public Properties ==

    public string Id => _id;

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "部屋", Page = typeof(Views.Rent.Residentials.Room.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Room.BasicPage).FullName! }
    ];

    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    public string WindowTitle
    {
        get
        {
            if (string.IsNullOrEmpty(_room.PropertyName))
            {
                string str;

                if (_room.ListingStatus == EnumListingStatus.New)
                {
                    str = $"{field} (新規)";
                }
                else
                {
                    str = $"{field} (編集)";
                }

                return str;
            }
            else
            {
                var str = $"{field}：{_room.PropertyName}";

                if (!string.IsNullOrEmpty(Name))
                {
                    str = $"{str}：{Name}";
                }

                if (_room.ListingStatus == EnumListingStatus.New)
                {
                    str = $"{str} (新規)";
                }
                else
                {
                    str = $"{str} (編集)";
                }

                return str;
            }
        }
        set
        {
            OnPropertyChanged();
        }
    } = "賃貸住居用";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    /*
    public bool IsDirty
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
    */

    [ObservableProperty]
    public partial bool HasErrors { get; private set; }


    public string Name
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;

                ValidateName(value);

                // Update title with dummy value.
                WindowTitle = string.Empty;
            }

            /*
            if (field == value) return;
            if (value is null) return;
            if (string.IsNullOrWhiteSpace(value))
            {
                NameErrorMessage = "Can not empty.";
                NameHasError = true;
                return;
            }

            field = value;
            IsDirty = true;
            NameHasError = false;

            // Update title with dummy value.
            _mainViewModel.WindowTitle = string.Empty;

            OnPropertyChanged();
            */
        }
    }

    [ObservableProperty]
    public partial bool NameHasError { get; private set; }

    [ObservableProperty]
    public partial string NameErrorMessage { get; private set; } = string.Empty;

    private void ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            NameErrorMessage = "部屋名（必須項目）を入力してください";
            NameHasError = true;

            HasErrors = true;
        }
        else
        {
            NameHasError = false;
        }
    }

    public string Chinryou
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            if (value.Equals("0"))
            {
                field = "";
                return;
            }

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                //field = string.Empty;
            }

            OnPropertyChanged();
        }
    }

    public ObservableCollection<Models.Rent.Residentials.Room.Picture> UnitPictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenRoomBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    #endregion

    #region == Private variables ==

    private readonly string _id = string.Empty;

    private readonly Models.Rent.Residentials.Room.Listing _room;

    // TODO: Use WeakReferenceMessenger from CommunityToolkit.Mvvm (aka MVVM Toolkit) to send the selected search result from the MainWindow to this ViewModel.
    // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger
    // Holding a reference to the parent ViewModel (Bldg.MainViewModel) (only IF opened by it) to allow communication between the windows.
    public ViewModels.Rent.Residentials.Bldg.MainViewModel? ParentViewModel { get; private set; }

    // TODO: Use WeakReferenceMessenger from CommunityToolkit.Mvvm (aka MVVM Toolkit) to send the selected search result from the MainWindow to this ViewModel.
    // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger
    // Holds the selected search result from the MainWindow which is used to update title/name and other properties.
    private Models.Rent.Residentials.ListingSearchResultItem? _selectedSearchResult;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedPictureFileList = [];

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDispatcherService _dispatcherService;
    private IModalDialogService? _dlgService;
    private INavigationGenericService? _navService;

    #endregion

    public MainViewModel(Models.Rent.Residentials.Room.Listing room, IDispatcherService dispatcherService, IDataAccessService dataAccessService)
    {
        _room = room;
        _id = room.Id;

        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;

        PopulateValues();

        // Reset errors
        NameHasError = false;
        // TODO: more.

        HasErrors = false;

        _room.IsModified = false;
        IsDirty = false;
    }

    #region == Private Methods ==

    private void SetValues()
    {
        if (!IsDirty)
        {
            return;
        }

        if (_room is null)
        {
            Debug.WriteLine("_room is null. Can't set room.");
            return;
        }

        if (string.IsNullOrEmpty(Name))
        {
            // TODO: Show InfoBar?
            HasErrors = true;
            return;
        }

        _room.Name = Name;

        if (int.TryParse(Chinryou, out var result))
        {
            if (result > -1)
            {
                _room.Chinryou = result;
            }
            else
            {
                Debug.WriteLine("整数変換に失敗。（マイナス）");
            }
        }



        // TODO: More.


        // 写真
        //_room.UnitPictures = UnitPictures;
        foreach (var pic in UnitPictures)
        {
            var existingPic = _room.Pictures.FirstOrDefault(r => r.Id == pic.Id);
            if (existingPic is not null)
            {
                // Update existing pic
                var index = _room.Pictures.IndexOf(existingPic);
                _room.Pictures[index] = pic;

            }
            else
            {
                // Add new pic
                _room.Pictures.Add(pic);
            }
        }

    }

    // TODO: Use WeakReferenceMessenger from CommunityToolkit.Mvvm (aka MVVM Toolkit) to send the selected search result from the MainWindow to this ViewModel.
    // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger
    private bool UpdateBldg()
    {
        if (_room is null)
        {
            Debug.WriteLine("_room is null. Can't update room in the parent vm.");
            return false;
        }

        if (ParentViewModel is null)
        {
            Debug.WriteLine("ParentViewModel is null. Can't update room in the parent vm.");
            return false;
        }

        var existingRoom = ParentViewModel.Rooms.FirstOrDefault(r => r.Id == _room.Id);
        if (existingRoom is not null)
        {
            // Update existing room
            var index = ParentViewModel.Rooms.IndexOf(existingRoom);
            ParentViewModel.Rooms[index] = _room;
        }
        else
        {
            // Add new room
            ParentViewModel.Rooms.Add(_room);
        }


        //Debug.WriteLine($"Room {_room.Name} updated in Building {ParentViewModel?.Name}");
        return true;
    }

    private void PopulateValues()
    {
        if (_room is null)
        {
            return;
        }

        Name = _room.Name; // Set the value to trigger the setter logic if needed.

        //var test = _room.Chinryou.ToString();
        Chinryou = _room.Chinryou.ToString();


        // TODO: Set other properties for editing..

        // Pictures
        UnitPictures = new ObservableCollection<Models.Rent.Residentials.Room.Picture>(_room.Pictures); // create a copy.
        foreach (var item in UnitPictures)
        {
            item.ParentViewModel = this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnUnitPicturePropertyChanged;
        }

        UnitPictures.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.Room.Picture item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from UnitPictures");
                    IsDirty = true;

                    item.PropertyChanged -= OnUnitPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Room.Picture item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to UnitPictures");
                    IsDirty = true;

                    item.PropertyChanged += OnUnitPicturePropertyChanged;
                }
            }
        };

        // TODO: PDFs

        _room.IsModified = false;
        IsDirty = false;
    }

    private void OnUnitPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Room.Picture picUnit)
        {
            Debug.WriteLine("OnUnitPicturePropertyChanged returned non PictureUnit.");
            return;
        }

        if (picUnit.IsModified)
        {
            Debug.WriteLine($"Property {e.PropertyName} changed");
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain"))
            {
                if (picUnit.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in UnitPictures)
                    {
                        if (item != picUnit)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
        }
    }

    private void DiscardUnsavedFiles()
    {
        if (_unsavedPictureFileList.Count > 0)
        {
            foreach (var file in _unsavedPictureFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved picture file: {file}");
                    File.Delete(file);
                }
            }

            _unsavedPictureFileList.Clear();
        }
    }

    #endregion

    #region == Public Methods ==

    public void SetNavigationService(INavigationGenericService nav)
    {
        _navService = nav;
    }

    public void SetDialogService(IModalDialogService dialog)
    {
        _dlgService = dialog;
    }

    // TODO: Use WeakReferenceMessenger from CommunityToolkit.Mvvm (aka MVVM Toolkit) to send the selected search result from the MainWindow to this ViewModel.
    // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger
    public void SetParentViewModel(ViewModels.Rent.Residentials.Bldg.MainViewModel parentVM)
    {
        // When created by the parent ViewModel (Bldg.MainViewModel), set a reference to it to allow communication between the windows.
        ParentViewModel = parentVM;
    }

    // TODO: Use WeakReferenceMessenger from CommunityToolkit.Mvvm (aka MVVM Toolkit) to send the selected search result from the MainWindow to this ViewModel.
    // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger
    public void SetSearchResult(Models.Rent.Residentials.ListingSearchResultItem? searchResult)
    {
        // When created by main window, store the selected search result to update title/name and other properties in the main window.
        _selectedSearchResult = searchResult;
    }

    // TODO: Convert this to command
    public void LeavingUnitCleanUp()
    {
        foreach (var item in UnitPictures)
        {
            item.PropertyChanged -= OnUnitPicturePropertyChanged;
        }

        UnitPictures.Clear();

        //IsDirty = false;
    }

    // TODO: change these below to commands.

    public async Task SetNewUnitPicturesAsync(List<string> filePathList)
    {
        /*
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_mainViewModel.EntryDataDirectoryPath}  @SetNewUnitPicturesAsync()");

        if (!Directory.Exists(_mainViewModel.EntryDataDirectoryPath))
        {
            Directory.CreateDirectory(_mainViewModel.EntryDataDirectoryPath);
        }

        List<string> list = [];

        foreach (var filePath in filePathList)
        {
            if (string.IsNullOrEmpty(filePath.Trim()))
            {
                continue;
            }

            // TODO: check file ext for valid image type.
            // TODO: set max file size?

            // TODO: Create thumbnail image?


            using var sourceStream = File.Open(filePath, FileMode.Open);

            string newId = Guid.CreateVersion7().ToString("N");
            string extension = Path.GetExtension(System.IO.Path.GetFileName(filePath));
            var destFilePath = Path.Combine(_mainViewModel.EntryDataDirectoryPath, newId + extension);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = File.Create(destFilePath);
            await sourceStream.CopyToAsync(destinationStream);

            var pic = new Models.Rent.Residentials.Unit.PictureUnit(newId, destFilePath)
            {
                IsNew = true,
                ParentViewModel = _mainViewModel
            };

            UnitPictures.Add(pic);

            OpenUnitBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeleteUnitPictureCommand.NotifyCanExecuteChanged();

            IsDirty = true;

            // Keep track of unsaved files to delete them when discarding.
            _unsavedUnitPictureFileList.Add(destFilePath);
        }
        */
    }

    public void DiscardChanges()
    {
        DiscardUnsavedFiles();

        //_room = null;
        IsDirty = false;
    }

    #endregion

    #region == Commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        if (!IsDirty)
        {
            return;
        }

        if (_room is null)
        {
            Debug.WriteLine("_room is null. Can't save room.");
            return;
        }

        // Validate input.
        ValidateName(Name);
        // TODO: more.
        if (HasErrors)
        {
            // TODO: Show InfoBar.
            InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            IsInfoBarErrorOpen = true;

            HasErrors = false;
            return;
        }

        SetValues();

        // TODO:

        /*
        if (_building is null)
        {
            Debug.WriteLine("_building is null. Can't save room.");
            return;
        }
        */

        if ((_room.PropertyStatus == EnumPropertyStatus.New) && (ParentViewModel is not null))
        {
            // update Bldg and done.
            if (UpdateBldg())
            {
                ParentViewModel?.SetIsDirty(true);

                IsDirty = false;
            }
        }
        else
        { 
            var resInsert = _dataAccessService.UpsertRentResidentialListing(_room.PropertyId, _room);
            if (resInsert.IsError)
            {
                Debug.WriteLine("Error on UpsertRentResidentialUnit. @Save() in Residentials.MainViewModel");
                Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

                // TODO: return error object.
                return;
            }
            else
            {
                _room.IsModified = false;
                _room.IsNew = false;
                _room.ListingStatus = EnumListingStatus.Saved;

                UpdateBldg();

                // Clean up deleted picture file.
                if (_room.PicturesToBeDeleted.Count > 0)
                {
                    foreach (var file in _room.PicturesToBeDeleted)
                    {
                        if (_room.Pictures.Remove(file))
                        {
                            File.Delete(file.ImageLocation);
                        }
                    }

                    _room.PicturesToBeDeleted.Clear();
                }

                _unsavedPictureFileList.Clear();

                IsDirty = false;
            }
        }

        // Update the selected search result's values such asname if it exists.
        _selectedSearchResult?.Name = Name;
    }   
    private bool CanSave()
    {
        if (IsDirty)
        {
            return true;
        }
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanOpenRoomBlobDirectory))]
    public void OpenRoomBlobDirectory()
    {
        /*
        if (Directory.Exists(_mainViewModel.PropertyDataDirectoryPath))
        {
            try
            {
                Process.Start("explorer.exe", _mainViewModel.PropertyDataDirectoryPath);
            }
            catch (Exception ex)
            {
                // TODO: show error to user.
                Debug.WriteLine($"Error opening folder: {ex.Message}");
            }
        }
        */
    }

    private bool CanOpenRoomBlobDirectory()
    {
        /*
        if (Directory.Exists(_mainViewModel.EntryDataDirectoryPath))
        {
            return true;
        }

        return false;
        */
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteRoomPicture))]
    private void DeleteRoomPicture(Models.Rent.Residentials.Room.Picture picUnit)
    {
        if (_room is null)
        {
            return;
        }

        if (picUnit is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

        if (UnitPictures.Remove(picUnit))
        {
            // TODO: should I? Prob no.
            /*
            if (_room.UnitPictures.Remove(picUnit))
            {
                
            }
            */
            _room.PicturesToBeDeleted.Add(picUnit);
            IsDirty = true;
        }
    }
    private bool CanDeleteRoomPicture(Models.Rent.Residentials.Room.Picture picUnit)
    {
        return picUnit is not null;
    }

    #endregion
}
