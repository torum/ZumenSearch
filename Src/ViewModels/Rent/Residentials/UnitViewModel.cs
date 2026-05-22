using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials;

internal sealed partial class UnitViewModel : ObservableObject
{
    #region == Public Properties ==
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty {  get; private set; }
    
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
                _mainViewModel.WindowTitle = string.Empty;
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

    internal ObservableCollection<PictureUnit> UnitPictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenUnitBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    
    #endregion

    #region == Private variables ==

    private ViewModels.Rent.Residentials.MainViewModel _mainViewModel;
    private Models.Rent.Residentials.UnitResidential? _unit;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedUnitPictureFileList = [];

    #endregion

    public UnitViewModel(ViewModels.Rent.Residentials.MainViewModel vm, IDataAccessService dataAccessService)
    {
        _mainViewModel = vm;
        _dataAccessService = dataAccessService;

        //_unit = new Room(Guid.CreateVersion7().ToString("N"));
    }

    #region == Private Methods ==

    private void SetValuesToUnit()
    {
        if (!IsDirty)
        {
            return;
        }

        if (_unit is null)
        {
            Debug.WriteLine("_unit is null. Can't set room.");
            return;
        }

        if (string.IsNullOrEmpty(Name))
        {
            // TODO: Show InfoBar?
            HasErrors = true;
            return;
        }

        _unit.Name = Name;

        if (int.TryParse(Chinryou, out var result))
        {
            if (result > -1)
            {
                _unit.Chinryou = result;
            }
            else
            {
                Debug.WriteLine("整数変換に失敗。（マイナス）");
            }
        }



        // TODO: More.


        // 写真
        //_unit.UnitPictures = UnitPictures;
        foreach (var pic in UnitPictures)
        {
            var existingPic = _unit.UnitPictures.FirstOrDefault(r => r.Id == pic.Id);
            if (existingPic is not null)
            {
                // Update existing pic
                var index = _unit.UnitPictures.IndexOf(existingPic);
                _unit.UnitPictures[index] = pic;

            }
            else
            {
                // Add new pic
                _unit.UnitPictures.Add(pic);
            }
        }

    }

    private void UpdateBldg()
    {
        if (_unit is null)
        {
            Debug.WriteLine("_unit is null. Can't save room.");
            return;
        }

        var existingRoom = _mainViewModel.Bldg.Rooms.FirstOrDefault(r => r.Id == _unit.Id);
        if (existingRoom is not null)
        {
            // Update existing room
            var index = _mainViewModel.Bldg.Rooms.IndexOf(existingRoom);
            _mainViewModel.Bldg.Rooms[index] = _unit;
        }
        else
        {
            // Add new room
            _mainViewModel.Bldg.Rooms.Add(_unit);
        }


    }

    private void PopulateUnitValues()
    {
        if (_unit is null)
        {
            return;
        }

        Name = _unit.Name; // Set the value to trigger the setter logic if needed.

        //var test = _unit.Chinryou.ToString();
        Chinryou = _unit.Chinryou.ToString();


        // TODO: Set other properties for editing..

        // Pictures
        UnitPictures = new ObservableCollection<Models.Rent.Residentials.PictureUnit>(_unit.UnitPictures); // create a copy.
        foreach (var item in UnitPictures)
        {
            item.ParentViewModel = _mainViewModel;//this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnUnitPicturePropertyChanged;
        }

        UnitPictures.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.PictureUnit item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from UnitPictures");
                    IsDirty = true;

                    item.PropertyChanged -= OnUnitPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.PictureUnit item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to UnitPictures");
                    IsDirty = true;

                    item.PropertyChanged += OnUnitPicturePropertyChanged;
                }
            }
        };

        // TODO: PDFs

        _unit.IsModified = false;
        IsDirty = false;
    }

    private void OnUnitPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ZumenSearch.Models.Rent.Residentials.PictureUnit picUnit)
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
        if (_unsavedUnitPictureFileList.Count > 0)
        {
            foreach (var file in _unsavedUnitPictureFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved picture file: {file}");
                    File.Delete(file);
                }
            }

            _unsavedUnitPictureFileList.Clear();
        }
    }

    #endregion

    #region == Public Methods ==

    internal void SetEditUnit(Models.Rent.Residentials.UnitResidential room)//SetEditUnit //PopulateUnitValues
    {
        _unit = room;

        // TODO: reset all ..

        PopulateUnitValues();

        // Reset errors
        NameHasError = false;
        // TODO: more.
        HasErrors = false;

        _unit.IsModified = false;
        IsDirty = false;
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

            var pic = new Models.Rent.Residentials.PictureUnit(newId, destFilePath)
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

    }

    public void DiscardChanges()
    {
        DiscardUnsavedFiles();

        _unit = null;
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

        if (_unit is null)
        {
            Debug.WriteLine("_unit is null. Can't save room.");
            return;
        }

        // Validate input.
        ValidateName(Name);
        // TODO: more.
        if (HasErrors)
        {
            // TODO: Show InfoBar.
            _mainViewModel.InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            _mainViewModel.IsInfoBarErrorOpen = true;

            HasErrors = false;
            return;
        }


        SetValuesToUnit();

        if (_mainViewModel.Bldg.EntryStatus == EnumEntryStatus.New)
        {
            // update Bldg and done.
            UpdateBldg();

            IsDirty = false;
        }
        else
        {
            // save room directry to db.
            // 
            var resInsert = _dataAccessService.UpsertRentResidentialUnit(_mainViewModel.Id, _unit);
            if (resInsert.IsError)
            {
                Debug.WriteLine("Error on UpsertRentResidentialUnit. @Save() in Residentials.MainViewModel");
                Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

                // TODO: return error object.
                return;
            }
            else
            {
                UpdateBldg();

                // Clean up deleted picture file.
                if (_unit.UnitPicturesToBeDeleted.Count > 0)
                {
                    foreach (var file in _unit.UnitPicturesToBeDeleted)
                    {
                        if (_unit.UnitPictures.Remove(file))
                        {
                            File.Delete(file.ImageLocation);
                        }
                    }

                    _unit.UnitPicturesToBeDeleted.Clear();
                }

                _unsavedUnitPictureFileList.Clear();

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

    [RelayCommand(CanExecute = nameof(CanOpenUnitBlobDirectory))]
    public void OpenUnitBlobDirectory()
    {
        if (Directory.Exists(_mainViewModel.EntryDataDirectoryPath))
        {
            try
            {
                Process.Start("explorer.exe", _mainViewModel.EntryDataDirectoryPath);
            }
            catch (Exception ex)
            {
                // TODO: show error to user.
                Debug.WriteLine($"Error opening folder: {ex.Message}");
            }
        }
    }
    private bool CanOpenUnitBlobDirectory()
    {
        if (Directory.Exists(_mainViewModel.EntryDataDirectoryPath))
        {
            return true;
        }

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteUnitPicture))]
    private void DeleteUnitPicture(Models.Rent.Residentials.PictureUnit picUnit)
    {
        if (_unit is null)
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
            if (_unit.UnitPictures.Remove(picUnit))
            {
                
            }
            */
            _unit.UnitPicturesToBeDeleted.Add(picUnit);
            IsDirty = true;
        }
    }
    private bool CanDeleteUnitPicture(Models.Rent.Residentials.PictureUnit picUnit)
    {
        return picUnit is not null;
    }

    #endregion
}
