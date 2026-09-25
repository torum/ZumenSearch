using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Models.Rent.Lessors;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials.Listing;

public sealed partial class ListingViewModel : ObservableRecipient, 
    IRecipient<PropertyUpdatedMessage>, 
    IRecipient<PropertyIsUnitOwnershipChangedMessage>,
    IRecipient<LessorUpdatedMessage>,
    IRecipient<BrokerUpdatedMessage>,
    IRecipient<LessorDeletedMessage>,
    IRecipient<BrokerDeletedMessage>
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

    public bool IsPropertyUnitOwnership
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {

            }
        }
    }

    #region == 画面表示関連 ==

    public string WindowTitle
    {
        get
        {
            var str = $"{field}：{_room.PropertyName}";

            if (!string.IsNullOrEmpty(Name))
            {
                str = $"{str}：{Name}";
            }

            if (_room.Status == EnumEntryStatus.New)
            {
                str = $"{str}：新規";
            }
            else
            {
                str = $"{str}：編集";
            }
            /*
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
                //var str = $"{field}：{_room.PropertyName}";

                if (!string.IsNullOrEmpty(Name))
                {
                    str = $"{str}：{Name}";
                }

                if (_room.ListingStatus == EnumListingStatus.New)
                {
                    str = $"{str}：(新規)";
                }
                else
                {
                    str = $"{str}：(編集)";
                }

            }
            */

            return str;
        }
        set
        {
            OnPropertyChanged();
        }
    } = "賃貸住居用";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "募集物件", Page = typeof(Views.Rent.Residentials.Listing.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Listing.BasicPage).FullName! }
    ];

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

                ValidateName();

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

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            text = Helpers.Common.ReplaceZenkakuNumbers(text);
            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                OnPropertyChanged();
            }
            /*
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
            */
        }
    }

    #endregion

    #region == 写真 & PDFプロパティ ==

    public ObservableCollection<Models.Rent.Residentials.Listing.Picture> Pictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    public ObservableCollection<Models.Rent.Residentials.Listing.Pdf> Pdfs
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    #endregion

    #region == 貸主プロパティ ==

    public ObservableCollection<Models.Rent.Lessors.PersonWrapperForListingViewModel> LessorsWrapper
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?
            }
        }
    } = [];

    #endregion

    #region == 宅建業者プロパティ ==

    public ObservableCollection<Models.Brokers.PersonWrapperForPropertyViewModel> BrokersWrapper
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?
            }
        }
    } = [];

    #endregion

    #endregion

    #region == Private variables ==

    private readonly string _id = string.Empty;

    private readonly Models.Rent.Residentials.Listing.Listing _room;

    private readonly string _listingDataDirectoryPath = string.Empty;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedRoomPictureFileList = [];
    private readonly List<string> _unsavedRoomPdfFileList = [];
    private readonly List<string> _unsavedRoomPdfThumbnailFileList = [];

    private readonly CancellationTokenSource _cts = new();

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService? _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public ListingViewModel(
        Models.Rent.Residentials.Listing.Listing room, 
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService, 
        IDataAccessService dataAccessService)
    {
        _room = room;
        _id = room.Id;

        _navigationService = navigationService; // _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.RoomListPage", this, new DrillInNavigationTransitionInfo());
        _dialogService = dialogService;

        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;

        _listingDataDirectoryPath = System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId), _room.Id);

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
            _room.PropertyStatus = building.Status;

            _room.PropertyName = building.Name;
            // Update window title with dummy string.
            WindowTitle = string.Empty;
        }
    }

    public void Receive(LessorUpdatedMessage person)
    {
        var lessor = person.Value;
        if (lessor is null)
        {
            return;
        }

        var psn = LessorsWrapper.FirstOrDefault(r => r.Person.Id.Equals(lessor.Id));
        if (psn is null) return;

        psn.Person = lessor;
    }

    public void Receive(BrokerUpdatedMessage corp)
    {
        var broker = corp.Value;
        if (broker is null)
        {
            return;
        }

        var psn = BrokersWrapper.FirstOrDefault(r => r.Person.Id.Equals(broker.Id));
        if (psn is null) return;

        psn.Person = broker; //= new PersonWrapperForPropertyViewModel(lessor, this);
    }

    public void Receive(PropertyIsUnitOwnershipChangedMessage isUnitOwnership)
    {
        IsPropertyUnitOwnership = isUnitOwnership.Value;

        if (IsPropertyUnitOwnership)
        {
            return;
        }

        if (_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.Listing.ZumenListPage"))
        {
            _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage", this);
        }
        else if (_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.Listing.LessorListPage"))
        {
            _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage", this);
        }
        else if (_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.Listing.BrokerListPage"))
        {
            _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage", this);
        }
    }

    public void Receive(LessorDeletedMessage lessorId)
    {
        var id = lessorId.Value;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        var psn = LessorsWrapper.FirstOrDefault(r => r.Person.Id.Equals(id));
        if (psn is null) return;
        LessorsWrapper.Remove(psn);
    }

    public void Receive(BrokerDeletedMessage lessorId)
    {
        var id = lessorId.Value;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        var psn = BrokersWrapper.FirstOrDefault(r => r.Person.Id.Equals(id));
        if (psn is null) return;
        BrokersWrapper.Remove(psn);
    }

    #endregion

    #region == Public Methods ==

    public void CleanUp()
    {
        // TODO: ?
        foreach (var item in Pictures)
        {
            item.PropertyChanged -= OnPicturePropertyChanged;
        }

        // TODO: ?
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

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage = "部屋名（必須項目）が入力されていません。保存出来ませんでした。";
            IsNameHasError = true;

            return false;
        }

        IsNameHasError = false;
        return true;
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

        // 図面
        foreach (var pdf in Pdfs)
        {
            var existingPdf = _room.Pdfs.FirstOrDefault(r => r.Id == pdf.Id);
            if (existingPdf is not null)
            {
                // Update existing pdf
                var index = _room.Pdfs.IndexOf(existingPdf);
                _room.Pdfs[index] = pdf;

            }
            else
            {
                // Add new pic
                _room.Pdfs.Add(pdf);
            }
        }

        // 貸主
        // 一旦クリアして、Wraperから「建物」の貸主を取り出して追加
        _room.Lessors.Clear();
        foreach (var item in this.LessorsWrapper)
        {
            _room.Lessors.Add(item.Person);
        }

    }

    private bool SaveToNew()
    {
        Debug.WriteLine("(_room.PropertyStatus == EnumPropertyStatus.New) @ListingViewModel on Save. Sending it to Property editor window");
        // Building is unsaved state. So, update it and done (don't save room to DB here because we don't save room without building).

        // TODO: make sure property editor window is exists (opened).

        // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
        WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_room));

        return true;
    }

    private bool SaveAsUpdate()
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

            return false;
        }
        else
        {
            _room.IsModified = false;
            _room.PropertyStatus = EnumEntryStatus.Saved;// just in case.
            _room.Status = EnumEntryStatus.Saved;

            // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
            WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_room));

            return true;
        }
    }

    private void PopulateValues()
    {
        if (_room is null)
        {
            return;
        }

        IsPropertyUnitOwnership = _room.IsPropertyUnitOwnership;

        Name = _room.Name; // Set the value to trigger the setter logic if needed.

        //var test = _room.Chinryou.ToString();
        Chinryou = _room.Chinryou.ToString();


        // TODO: Set other properties for editing..

        // Pictures
        Pictures = new ObservableCollection<Models.Rent.Residentials.Listing.Picture>(_room.Pictures); // create a copy.
        foreach (var item in Pictures)
        {
            item.BasePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId), _room.Id));
            item.ParentViewModel = this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnPicturePropertyChanged;
        }

        Pictures.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.Listing.Picture item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from Pictures");
                    IsDirty = true;

                    item.PropertyChanged -= OnPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Listing.Picture item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to Pictures");
                    IsDirty = true;

                    item.PropertyChanged += OnPicturePropertyChanged;
                }
            }
        };

        // PDFs
        Pdfs = new ObservableCollection<Models.Rent.Residentials.Listing.Pdf>(_room.Pdfs); // create a copy.
        foreach (var item in Pdfs)
        {
            item.BasePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId), _room.Id));
            item.ParentViewModel = this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnPdfPropertyChanged;
        }

        Pdfs.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.Listing.Pdf item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from Pdfs");
                    IsDirty = true;

                    item.PropertyChanged -= OnPdfPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Listing.Pdf item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to Pdfs");
                    IsDirty = true;

                    item.PropertyChanged += OnPdfPropertyChanged;
                }
            }
        };

        // Lessors
        LessorsWrapper = new ObservableCollection<Models.Rent.Lessors.PersonWrapperForListingViewModel>();
        foreach (var item in _room.Lessors)
        {
            LessorsWrapper.Add(new Models.Rent.Lessors.PersonWrapperForListingViewModel(item, this));
        }


        _room.IsModified = false;
        IsDirty = false;
    }

    private void OnPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Listing.Picture picUnit)
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

    private void OnPdfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Listing.Pdf pdfUnit)
        {
            Debug.WriteLine("OnPdfPropertyChanged returned non PdfUnit.");
            return;
        }

        if (pdfUnit.IsModified)
        {
            Debug.WriteLine($"Property {e.PropertyName} changed");
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain"))
            {
                if (pdfUnit.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in Pdfs)
                    {
                        if (item != pdfUnit)
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
        if (_unsavedRoomPictureFileList.Count > 0)
        {
            foreach (var file in _unsavedRoomPictureFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved picture file: {file}");
                    File.Delete(file);
                }
            }

            _unsavedRoomPictureFileList.Clear();
        }

        if (_unsavedRoomPdfFileList.Count > 0)
        {
            foreach (var file in _unsavedRoomPdfFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved pdf file: {file}");
                    File.Delete(file);
                }
            }

            _unsavedRoomPdfFileList.Clear();
        }

        if (_unsavedRoomPdfThumbnailFileList.Count > 0)
        {
            foreach (var file in _unsavedRoomPdfThumbnailFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved pdf thumb picture file: {file}");
                    File.Delete(file);
                }
            }

            _unsavedRoomPdfThumbnailFileList.Clear();
        }

        if (_room.Status == EnumEntryStatus.New)
        {
            if (Directory.Exists(_listingDataDirectoryPath))
            {
                Debug.WriteLine($"Deleting folder: {_listingDataDirectoryPath}");
                Directory.Delete(_listingDataDirectoryPath, true);
            }
        }
    }

    #endregion

    #region == Commands ==

    [RelayCommand]
    public async Task OpenPropertyEditorWindow()
    {
        var vm = App.GetService<ViewModels.MainViewModel>();
        await vm.EditRentResidentialBldgFromId(_room.PropertyId);
    }

    #region == Save ==

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
        if (!ValidateName()) 
        {
            //InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            IsInfoBarErrorOpen = true;

            if (!_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage"))
            {
                _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage", this);
            }

            return;
        }

        // TODO: more.

        SetValues();

        bool saveResult;
        if (_room.PropertyStatus == EnumEntryStatus.New)
        {
            saveResult = SaveToNew();
        }
        else
        {
            saveResult = SaveAsUpdate();
        }

        if (saveResult)
        {
            IsDirty = false;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;
            
            // Update title with dummy value.
            WindowTitle = string.Empty;

            // TODO: 

            // Clean up deleted picture file.
            if (_room.PicturesToBeDeleted.Count > 0)
            {
                foreach (var file in _room.PicturesToBeDeleted)
                {
                    /*
                    if (_room.Pictures.Remove(file))
                    {
                    }
                    */
                    var delFilePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId), _room.Id), file.ImageFilename);
                    File.Delete(delFilePath);
                }

                _room.PicturesToBeDeleted.Clear();
            }

            // Clean up deleted picture file.
            if (_room.PdfsToBeDeleted.Count > 0)
            {
                foreach (var file in _room.PdfsToBeDeleted)
                {
                    /*
                    if (_room.Pdfs.Remove(file))
                    {
                    }
                    */
                    var delFilePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId), _room.Id), file.PdfFilename);
                    File.Delete(delFilePath);
                    delFilePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId), _room.Id), file.PdfFilename);
                    File.Delete(delFilePath);
                }

                _room.PdfsToBeDeleted.Clear();
            }

            _unsavedRoomPictureFileList.Clear();
            _unsavedRoomPdfFileList.Clear();
            _unsavedRoomPdfThumbnailFileList.Clear();
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

    #endregion

    #region == Pics and Pdfs ==

    [RelayCommand(CanExecute = nameof(CanAddNewRoomPictures))]
    public async Task AddNewRoomPictures(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_listingDataDirectoryPath}  @AddNewRoomPictures()");
        
        if (!Directory.Exists(_listingDataDirectoryPath))
        {
            Directory.CreateDirectory(_listingDataDirectoryPath);
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
            string newFilename = newId + extension;
            var destFilePath = Path.Combine(_listingDataDirectoryPath, newFilename);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = File.Create(destFilePath);
            await sourceStream.CopyToAsync(destinationStream);

            var pic = new Models.Rent.Residentials.Listing.Picture(newId, newFilename)
            {
                BasePath = _listingDataDirectoryPath,
                IsNew = true,
                ParentViewModel = this
            };

            Pictures.Add(pic);

            OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeleteRoomPictureCommand.NotifyCanExecuteChanged();

            IsDirty = true;

            // Keep track of unsaved files to delete them when discarding.
            _unsavedRoomPictureFileList.Add(destFilePath);
        }
    }
    private static bool CanAddNewRoomPictures()
    {
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteRoomPicture))]
    private void DeleteRoomPicture(Models.Rent.Residentials.Listing.Picture picUnit)
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
    private static bool CanDeleteRoomPicture(Models.Rent.Residentials.Listing.Picture picUnit)
    {
        return picUnit is not null;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewRoomPdfs))]
    public async Task AddNewRoomPdfs(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_listingDataDirectoryPath}  @AddNewRoomPdfs()");

        if (!Directory.Exists(_listingDataDirectoryPath))
        {
            Directory.CreateDirectory(_listingDataDirectoryPath);
        }

        //List<string> list = [];

        foreach (var filePath in filePathList)
        {
            if (string.IsNullOrEmpty(filePath.Trim()))
            {
                continue;
            }


            // TODO: set max file size?

            string extension = Path.GetExtension(System.IO.Path.GetFileName(filePath));
            if (!extension.Equals(".pdf")) // TODO: check case.
            {
                continue;
            }

            //using var sourceStream = File.Open(file, FileMode.Open);

            StorageFile sfile = await StorageFile.GetFileFromPathAsync(filePath);
            PdfDocument pdfDocument = await PdfDocument.LoadFromFileAsync(sfile);

            if (pdfDocument.PageCount > 0)
            {
                using PdfPage pdfPage = pdfDocument.GetPage(0);
                using var stream = new InMemoryRandomAccessStream();

                // Set screen standard DPI
                //float targetDpi = 96f;
                //float scaleFactor = targetDpi / 72f; 
                //uint calculatedWidth = (uint)Math.Round(pdfPage.Size.Width * scaleFactor);

                var options = new PdfPageRenderOptions
                {
                    // Set the desired target width in pixels (e.g., 1024px)
                    // Aspect ratio is locked; height scales automatically.
                    DestinationWidth = 512//calculatedWidth//1024
                };

                await pdfPage.RenderToStreamAsync(stream, options);

                //var bitmapImage = new BitmapImage();
                //await bitmapImage.SetSourceAsync(stream);

                string newId = Guid.CreateVersion7().ToString("N");
                string newThumbnailFilename = newId + ".bmp";
                var thumbnailDestFilePath = Path.Combine(_listingDataDirectoryPath, newThumbnailFilename);

                using var destinationStream = File.Create(thumbnailDestFilePath);
                using var managedSourceStream = stream.AsStreamForRead();
                await managedSourceStream.CopyToAsync(destinationStream);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedRoomPdfThumbnailFileList.Add(thumbnailDestFilePath);

                string newFilename = newId + extension;

                var pdfDestFilePath = Path.Combine(_listingDataDirectoryPath, newFilename);
                File.Copy(filePath, pdfDestFilePath);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedRoomPdfFileList.Add(pdfDestFilePath);

                var pdf = new Models.Rent.Residentials.Listing.Pdf(newId, newFilename, newThumbnailFilename)
                {
                    BasePath = _listingDataDirectoryPath,
                    IsNew = true,
                    ParentViewModel = this
                };

                Pdfs.Add(pdf);

                OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
                DeleteRoomPdfCommand.NotifyCanExecuteChanged();

                IsDirty = true;
            }
            else
            {
                Debug.WriteLine("0 page.");
            }
        }
    }
    private static bool CanAddNewRoomPdfs()
    {
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteRoomPdf))]
    private void DeleteRoomPdf(Models.Rent.Residentials.Listing.Pdf pdf)
    {
        if (pdf is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

        if (Pdfs.Remove(pdf))
        {
            _room.PdfsToBeDeleted.Add(pdf);
            IsDirty = true;
        }
    }
    private static bool CanDeleteRoomPdf(Models.Rent.Residentials.Listing.Pdf pdf)
    {
        return pdf is not null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenBlobDirectory))]
    public void OpenBlobDirectory()
    {
        if (Directory.Exists(_listingDataDirectoryPath))
        {
            try
            {
                Process.Start("explorer.exe", _listingDataDirectoryPath);
            }
            catch (Exception ex)
            {
                // TODO: show error to user.
                Debug.WriteLine($"Error opening folder: {ex.Message}");
            }
        }
    }
    private bool CanOpenBlobDirectory()
    {
        if (Directory.Exists(_listingDataDirectoryPath))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region == Lessor ==

    [RelayCommand]
    public async Task AddLessor()
    {
        if (_dialogService is null)
        {
            Debug.WriteLine("_dlgService is null");
            return;
        }

        var lessor = await _dialogService.ShowLessorSelectDialog(new ViewModels.Dialogs.LessorSelectViewModel(_dataAccessService, _cts));

        if (lessor is not null)
        {
            var lessorId = lessor.Id;

            // Check if already exists
            var match = LessorsWrapper.FirstOrDefault(x => x.Person.Id.Equals(lessorId));
            if (match is not null)
            {
                Debug.WriteLine($"lessor {lessor.Name} already in the list.");
                return;
            }

            // TODO:
            //var res = await Task.Run(() => _dataAccessService.SelectRentLessorById(lessorId), _cts.Token);
            var res = _dataAccessService.SelectRentLessorById(lessorId);
            if (res.IsError)
            {
                Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

                //ErrorMain = res.Error;
                //IsMainErrorInfoBarVisible = true;

                // TODO: Show error message to user
                return;
            }

            if (res.Person is null)
            {
                Debug.WriteLine($"{lessorId} is null. Cannot open editor.");
                return;
            }


            var asdf = new PersonWrapperForListingViewModel(res.Person, this);

            LessorsWrapper.Add(asdf);

            IsDirty = true;
        }

    }

    [RelayCommand(CanExecute = nameof(CanEditLessor))]
    private void EditLessor(PersonWrapperForListingViewModel lessor)
    {
        if (lessor.Person is not null)
        {
            var mainVm = App.GetService<MainViewModel>();
            if (mainVm.EditRentLessorCommand.CanExecute(lessor.Person as Models.Base.PersonBase))
            {
                mainVm.EditRentLessorCommand.Execute(lessor.Person as Models.Base.PersonBase);
            }
        }
    }
    private static bool CanEditLessor(PersonWrapperForListingViewModel lessor)
    {
        return lessor is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteLessor))]
    public async Task DeleteLessor(PersonWrapperForListingViewModel lessor)
    {
        Debug.WriteLine($"DeleteLessorCommand {lessor.Person.Name}");

        if (lessor is null)
        {
            return;
        }

        if (lessor.Person is null)
        {
            return;
        }

        if (LessorsWrapper.Remove(lessor))
        {
            IsDirty = true;

            _room.LessorsToBeDeleted.Add(lessor.Person);
        }
    }
    private static bool CanDeleteLessor(PersonWrapperForListingViewModel lessor)
    {
        return lessor is not null;
    }


    #endregion

    #endregion

}
