using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Commercials.Listing;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class ListingViewModel
{
    private readonly List<string> _incompletePictureFiles = [];
    private readonly HashSet<string> _handedOffPictureDeletions = new(StringComparer.Ordinal);
    private bool _handedOffToNewParent;
    private bool _hasPendingPictureCleanup;
    private bool _discardPending;
    private bool _isClosing;
    private bool _isSaving;
    private bool _propertyBusy;
    private int _pendingPickers;
    private TaskCompletionSource? _pickersCompleted;
    private readonly Queue<Action> _deferredPropertyMessages = new();

    public ObservableCollection<ListingPicture> Pictures { get; } = [];
    public bool IsClosing => _isClosing;
    public bool IsPropertyBusy => _propertyBusy;
    public bool IsDiscardPending => _discardPending;
    public bool HasPendingFileCleanupAfterSave => _hasPendingPictureCleanup || _hasPendingPdfCleanup;
    public bool IsEditorEnabled => !_isClosing && !_isSaving && !_propertyBusy && !_discardPending;
    public bool HasPendingEditorOperations => _pendingPickers > 0 || PendingPictureTasks().Count > 0;

    public bool ReferencesAttachmentFile(string path) =>
        Pictures.Any(picture => string.Equals(Path.Combine(_listingDataDirectoryPath, picture.ImageFilename), path, StringComparison.OrdinalIgnoreCase)) ||
        Pdfs.Any(pdf => string.Equals(Path.Combine(_listingDataDirectoryPath, pdf.PdfFilename), path, StringComparison.OrdinalIgnoreCase) || string.Equals(Path.Combine(_listingDataDirectoryPath, pdf.ThumbnailFilename), path, StringComparison.OrdinalIgnoreCase));

    public void SetPropertyBusy(bool value)
    {
        _propertyBusy = value;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();
        if (!value && !_isSaving) ApplyDeferredPropertyMessages();
    }

    private bool DeferPropertyMessage(Action action)
    {
        if (!_isSaving && !_propertyBusy) return false;
        _deferredPropertyMessages.Enqueue(action);
        return true;
    }

    private void ApplyDeferredPropertyMessages()
    {
        while (!_isSaving && !_propertyBusy && _deferredPropertyMessages.TryDequeue(out var action)) action();
    }

    private void PopulatePictures()
    {
        _handedOffToNewParent = _unit.PropertyStatus == EntityStatus.New && App.GetService<ViewModels.MainViewModel>().RentCommercialEditorList.Any(editor => editor.Id == _unit.PropertyId && editor.ViewModel?.Units.Any(unit => unit.Id == _unit.Id) == true);
        if (_handedOffToNewParent)
        {
            _committedPdfs.Clear();
            _committedLessors.Clear();
            _committedBrokers.Clear();
            _handedOffPictureDeletions.UnionWith(_unit.PicturesToBeDeleted.Select(picture => picture.Id));
        }
        foreach (var picture in _unit.Pictures)
        {
            picture.BasePath = _listingDataDirectoryPath;
            picture.ParentViewModel = this;
            picture.PropertyChanged += OnListingPictureChanged;
            Pictures.Add(picture);
        }
        _unit.Pictures = Pictures;
    }

    private void OnListingPictureChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ListingPicture picture) return;
        if (e.PropertyName == nameof(ListingPicture.IsMain) && picture.IsMain)
        {
            foreach (var other in Pictures)
            {
                if (other != picture && other.IsMain) other.IsMain = false;
            }
        }
        MarkEdited();
    }

    public void BeginPicker()
    {
        if (_pendingPickers++ == 0) _pickersCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public void EndPicker()
    {
        if (--_pendingPickers == 0)
        {
            _pickersCompleted?.TrySetResult();
            _pickersCompleted = null;
        }
    }

    public void SetClosing(bool value)
    {
        _isClosing = value;
        OnPropertyChanged(nameof(IsEditorEnabled));
    }

    private List<Task> PendingPictureTasks()
    {
        IAsyncRelayCommand[] commands = [AddNewRoomPicturesCommand, DeleteRoomPictureCommand, AddNewRoomPdfsCommand, OpenRoomPdfCommand, DeleteRoomPdfCommand, AddLessorCommand, EditLessorCommand, AddBrokerCommand, EditBrokerCommand];
        return commands.Where(command => command.ExecutionTask is { IsCompleted: false }).Select(command => command.ExecutionTask!).ToList();
    }

    public async Task<bool> WaitForEditorOperationsAsync()
    {
        while (true)
        {
            var tasks = PendingPictureTasks();
            if (_pickersCompleted is not null) tasks.Add(_pickersCompleted.Task);
            if (tasks.Count == 0) return true;
            try { await Task.WhenAll(tasks); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
            {
                Debug.WriteLine($"Failed to complete commercial listing media operation: {ex}");
                ShowAttachmentError($"編集中の処理を完了できませんでした: {ex.Message}");
                return false;
            }
        }
    }

    private void ShowPictureError(string message) => ShowAttachmentError(message);

    private void ShowAttachmentError(string message)
    {
        InfoBarErrorMessage = message;
        IsInfoBarErrorOpen = true;
    }

    private static bool DeleteAttachmentFiles(IEnumerable<string> files, List<string> failures)
    {
        var succeeded = true;
        foreach (var file in files)
        {
            try { File.Delete(file); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to delete commercial listing attachment '{file}': {ex}");
                failures.Add(file);
                succeeded = false;
            }
        }
        return succeeded;
    }

    private static bool DeletePictureFiles(IEnumerable<string> files, List<string> failures) => DeleteAttachmentFiles(files, failures);

    public bool CleanIncompleteAttachments()
    {
        var failures = new List<string>();
        foreach (var file in _incompletePictureFiles.ToList())
        {
            if (DeletePictureFiles(new[] { file }, failures)) _incompletePictureFiles.Remove(file);
        }
        foreach (var file in _incompletePdfFiles.ToList())
        {
            if (DeleteAttachmentFiles(new[] { file }, failures)) _incompletePdfFiles.Remove(file);
        }
        var referencedPdfFiles = Pdfs.Concat(_unit.PdfsToBeDeleted).SelectMany(pdf => new[]
        {
            Path.Combine(_listingDataDirectoryPath, pdf.PdfFilename),
            Path.Combine(_listingDataDirectoryPath, pdf.ThumbnailFilename)
        }).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var tracked in new[] { _unsavedUnitPdfFileList, _unsavedUnitPdfThumbnailFileList })
        {
            foreach (var file in tracked.Where(file => !referencedPdfFiles.Contains(file)).ToList())
            {
                if (DeleteAttachmentFiles(new[] { file }, failures)) tracked.Remove(file);
            }
        }
        if (failures.Count == 0) return true;
        ShowAttachmentError($"読み込みに失敗したファイルを削除できませんでした: {string.Join(", ", failures)}");
        return false;
    }

    public bool DiscardChanges()
    {
        if (_hasPendingPictureCleanup || _hasPendingPdfCleanup)
        {
            ShowAttachmentError("保存済みデータの削除済みファイルが残っています。ファイルの削除を完了するため、もう一度保存してください。");
            return false;
        }
        _discardPending = true;
        OnPropertyChanged(nameof(IsEditorEnabled));
        if (!CleanIncompleteAttachments()) return false;
        var failures = new List<string>();
        foreach (var file in _unsavedUnitPictureFileList.ToList())
        {
            if (DeletePictureFiles(new[] { file }, failures)) _unsavedUnitPictureFileList.Remove(file);
        }
        foreach (var file in _unsavedUnitPdfFileList.ToList())
        {
            if (DeleteAttachmentFiles(new[] { file }, failures)) _unsavedUnitPdfFileList.Remove(file);
        }
        foreach (var file in _unsavedUnitPdfThumbnailFileList.ToList())
        {
            if (DeleteAttachmentFiles(new[] { file }, failures)) _unsavedUnitPdfThumbnailFileList.Remove(file);
        }
        if (_unit.Status == EntityStatus.New && !_handedOffToNewParent && Directory.Exists(_listingDataDirectoryPath))
        {
            try { Directory.Delete(_listingDataDirectoryPath, true); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to delete commercial listing directory '{_listingDataDirectoryPath}': {ex}");
                failures.Add(_listingDataDirectoryPath);
            }
        }
        if (failures.Count > 0)
        {
            ShowPictureError($"変更の破棄中にファイルを削除できませんでした。ファイルを閉じてからもう一度画面を閉じてください: {string.Join(", ", failures)}");
            return false;
        }
        _discardPending = false;
        IsDirty = false;
        return true;
    }

    [RelayCommand]
    private async Task AddNewRoomPictures(List<string> paths)
    {
        if (paths is null || paths.Count == 0) return;
        Directory.CreateDirectory(_listingDataDirectoryPath);
        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            var extension = Path.GetExtension(path);
            if (!new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                ShowPictureError($"画像形式に対応していません: {Path.GetFileName(path)}");
                continue;
            }
            var id = Guid.CreateVersion7().ToString("N");
            var filename = id + extension;
            var destination = Path.Combine(_listingDataDirectoryPath, filename);
            await using (var source = File.OpenRead(path))
            await using (var target = new FileStream(destination, FileMode.CreateNew, FileAccess.Write))
            {
                _incompletePictureFiles.Add(destination);
                await source.CopyToAsync(target);
            }
            var picture = new ListingPicture(id, filename, EntityStatus.New) { BasePath = _listingDataDirectoryPath, ParentViewModel = this };
            picture.PropertyChanged += OnListingPictureChanged;
            Pictures.Add(picture);
            _unsavedUnitPictureFileList.Add(destination);
            _incompletePictureFiles.Remove(destination);
            MarkEdited();
        }
    }

    [RelayCommand]
    private async Task DeleteRoomPicture(ListingPicture picture)
    {
        if (picture is null || !Pictures.Contains(picture) || !IsEditorEnabled) return;
        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この募集物件の写真を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
            if (!Pictures.Remove(picture)) return;
            picture.PropertyChanged -= OnListingPictureChanged;
            picture.ParentViewModel = null;
            _unit.PicturesToBeDeleted.Add(picture);
            MarkEdited();
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            Debug.WriteLine($"Failed to confirm commercial listing picture deletion: {ex}");
            ShowPictureError($"写真の削除を確認できませんでした: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenBlobDirectory()
    {
        if (!Directory.Exists(_listingDataDirectoryPath)) return;
        try { Process.Start("explorer.exe", _listingDataDirectoryPath); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Debug.WriteLine($"Failed to open commercial listing directory: {ex}");
            ShowPictureError($"保存場所を開けませんでした: {ex.Message}");
        }
    }
}
