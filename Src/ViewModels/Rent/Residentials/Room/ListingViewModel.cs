using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Services;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials.Room;

public sealed partial class ListingViewModel : ObservableRecipient, IRecipient<PropertyUpdatedMessage>, IRecipient<PropertyIsUnitOwnershipChangedMessage>
{
    #region == Public Properties ==

    //public string Id => _id;

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

    #region == 画面表示関連 ==

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

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "部屋", Page = typeof(Views.Rent.Residentials.Room.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Room.BasicPage).FullName! }
    ];

    public bool IsUnitOwnershipVisible
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {

            }
        }
    }

    #endregion

    #region == エラー関連 ==

    /*
    [ObservableProperty]
    public partial bool HasErrors { get; private set; }
    */

    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsNameHasError { get; private set; }

    #endregion

    #region == 基本プロパティ == 

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

    #endregion

    #region == 契約条件 == 

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

    #endregion

    #region == 写真プロパティ ==

    public ObservableCollection<Models.Rent.Residentials.Room.Picture> Pictures
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

    #endregion

    #region == Private variables ==

    private readonly string _id = string.Empty;

    private readonly Models.Rent.Residentials.Room.Listing _room;

    private readonly string _propertyDataDirectoryPath = string.Empty;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedPictureFileList = [];

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IModalDialogService? _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public ListingViewModel(
        Models.Rent.Residentials.Room.Listing room, 
        INavigationGenericService navigationService,
        IModalDialogService dialogService,
        IDispatcherService dispatcherService, 
        IDataAccessService dataAccessService)
    {
        _room = room;
        _id = room.Id;

        _navigationService = navigationService; // _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.Bldg.RoomListPage", this, new DrillInNavigationTransitionInfo());
        _dialogService = dialogService;

        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;

        _propertyDataDirectoryPath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.AppDataPictureFolder, "Rent"), "Residential_Building"), _room.PropertyId);

        PopulateValues();

        // Reset errors
        IsNameHasError = false;
        // TODO: more.

        //HasErrors = false;

        _room.IsModified = false;
        IsDirty = false;

        // Ready to receive messages.
        this.IsActive = true;
    }

    #region == Messages ==

    public void Receive(PropertyUpdatedMessage property)
    {
        var building = property.Value;
        if (building is not null) 
        {
            _room.PropertyStatus = building.PropertyStatus;

            _room.PropertyName = building.Name;
            // Update window title with dummy string.
            WindowTitle = string.Empty;
        }
    }

    public void Receive(PropertyIsUnitOwnershipChangedMessage isUnitOwnership)
    {
        IsUnitOwnershipVisible = isUnitOwnership.Value;
    }


    #endregion

    #region == Public Methods ==

    public void CleanUp()
    {
        foreach (var item in Pictures)
        {
            item.PropertyChanged -= OnPicturePropertyChanged;
        }

        Pictures.Clear();

        // Unsubscribe
        //WeakReferenceMessenger.Default.UnregisterAll(this);
        //or
        this.IsActive = false;
    }

    public void DiscardChanges()
    {
        DiscardUnsavedFiles();

        //_room = null;
        IsDirty = false;
    }

    #endregion

    #region == Private Methods ==

    private bool ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            InfoBarErrorMessage = "部屋名（必須項目）が入力されていません。保存出来ませんでした。";
            IsNameHasError = true;

            return true;
        }

        IsNameHasError = false;
        return false;
    }

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
        //_room.Pictures = Pictures;
        foreach (var pic in Pictures)
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
        Pictures = new ObservableCollection<Models.Rent.Residentials.Room.Picture>(_room.Pictures); // create a copy.
        foreach (var item in Pictures)
        {
            item.ParentViewModel = this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnPicturePropertyChanged;
        }

        Pictures.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.Room.Picture item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from Pictures");
                    IsDirty = true;

                    item.PropertyChanged -= OnPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Room.Picture item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to Pictures");
                    IsDirty = true;

                    item.PropertyChanged += OnPicturePropertyChanged;
                }
            }
        };

        // TODO: PDFs

        _room.IsModified = false;
        IsDirty = false;
    }

    private void OnPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Room.Picture picUnit)
        {
            Debug.WriteLine("OnPicturePropertyChanged returned non PictureUnit.");
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
                    foreach (var item in Pictures)
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
        if (ValidateName(Name)) 
        {
            //InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            IsInfoBarErrorOpen = true;
            return;
        }

        // TODO: more.

        SetValues();

        // TODO:

        if (_room.PropertyStatus == EnumPropertyStatus.New)
        {
            Debug.WriteLine("(_room.PropertyStatus == EnumPropertyStatus.New) @ListingViewModel on Save");
            Debug.WriteLine($"_room.ListingStatus = {_room.ListingStatus}");
            // Building window is open and unsaved state. So, update it and done (don't save room here because we don't save save room without building).

            // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
            WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_room));

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            IsDirty = false;
        }
        else
        { 
            var resInsert = _dataAccessService.UpsertRentResidentialListing(_room.PropertyId, _room);
            if (resInsert.IsError)
            {
                Debug.WriteLine("Error on UpsertRentResidentialUnit. @Save() in Residentials.MainViewModel");
                Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

                // TODO: fix format.
                var errText = resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent;
                InfoBarErrorMessage = errText;
                IsInfoBarErrorOpen = true;

                return;
            }
            else
            {
                // Clear error infobar.
                IsInfoBarErrorOpen = false;

                _room.IsModified = false;
                _room.PropertyStatus = EnumPropertyStatus.Saved;// just in case.
                _room.ListingStatus = EnumListingStatus.Saved;

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

                // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
                WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_room));

                IsDirty = false;
            }
        }

    }   
    private bool CanSave()
    {
        if (IsDirty)
        {
            return true;
        }
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewRoomPictures))]
    public async Task AddNewRoomPictures(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_propertyDataDirectoryPath}  @AddNewRoomPictures()");
        
        if (!Directory.Exists(_propertyDataDirectoryPath))
        {
            Directory.CreateDirectory(_propertyDataDirectoryPath);
        }

        //List<string> list = [];

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
            var destFilePath = Path.Combine(_propertyDataDirectoryPath, newId + extension);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = File.Create(destFilePath);
            await sourceStream.CopyToAsync(destinationStream);

            var pic = new Models.Rent.Residentials.Room.Picture(newId, destFilePath)
            {
                IsNew = true,
                ParentViewModel = this
            };

            Pictures.Add(pic);

            OpenRoomBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeleteRoomPictureCommand.NotifyCanExecuteChanged();

            IsDirty = true;

            // Keep track of unsaved files to delete them when discarding.
            _unsavedPictureFileList.Add(destFilePath);
        }
    }
    private static bool CanAddNewRoomPictures()
    {
        return true;
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
        if (Directory.Exists(_propertyDataDirectoryPath))
        {
            return true;
        }

        return false;
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

        if (Pictures.Remove(picUnit))
        {
            // TODO: should I? Prob no.
            /*
            if (_room.Pictures.Remove(picUnit))
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
