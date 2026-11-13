using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Commercials.Listing;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class ListingViewModel
{
    private readonly List<string> _incompletePdfFiles = [];
    private bool _hasPendingPdfCleanup;

    public ObservableCollection<ListingPdf> Pdfs { get; } = [];
    public bool HasPendingPdfCleanup => _hasPendingPdfCleanup;

    private bool HasPendingPdfEdits()
    {
        return _unit.PdfsToBeDeleted.Count > 0 || Pdfs.Count != _committedPdfs.Count ||
            Pdfs.Any(pdf => !_committedPdfs.Contains(pdf.Id) || pdf.IsModified);
    }

    private void CaptureCommittedPdfs(IEnumerable<ListingPdf> pdfs) => CaptureCommittedPdfIds(pdfs.Select(pdf => pdf.Id));

    private void CaptureCommittedPdfs(IEnumerable<Models.Rent.Commercials.CommercialListingPdfDraft> pdfs) => CaptureCommittedPdfIds(pdfs.Select(pdf => pdf.Id));

    private void CaptureCommittedPdfIds(IEnumerable<string> pdfIds)
    {
        _committedPdfs.Clear();
        _committedPdfs.UnionWith(pdfIds);
    }

    private void PopulatePdfs()
    {
        foreach (var pdf in _unit.Pdfs)
        {
            pdf.BasePath = _listingDataDirectoryPath;
            pdf.ParentViewModel = this;
            pdf.PropertyChanged += OnListingPdfChanged;
            Pdfs.Add(pdf);
        }
        _unit.Pdfs = Pdfs;
    }

    private void OnListingPdfChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not ListingPdf pdf) return;
        if (e.PropertyName == nameof(ListingPdf.IsMain) && pdf.IsMain)
        {
            foreach (var other in Pdfs)
            {
                if (other != pdf && other.IsMain) other.IsMain = false;
            }
        }
        MarkPdfEdited();
    }

    private bool HasMultipleMainPdfs => Pdfs.Count(pdf => pdf.IsMain) > 1;

    private void ReportMultipleMainPdfs()
    {
        ShowAttachmentError(IsPropertyUnitOwnership ? "メインの図面は1件だけ選択してください。" : "募集物件のメインの図面は1件だけ選択してください。建物画面で区分所有を有効にすると図面を編集できます。");
        if (IsPropertyUnitOwnership) _navigationService.NavigateTo(DocumentListPageName, this);
    }

    [RelayCommand]
    private async Task AddNewRoomPdfs(List<string> paths)
    {
        if (paths is null || paths.Count == 0 || !IsPropertyUnitOwnership) return;

        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            var extension = Path.GetExtension(path);
            if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ShowAttachmentError($"PDFファイルを選択してください: {Path.GetFileName(path)}");
                continue;
            }

            try
            {
                Directory.CreateDirectory(_listingDataDirectoryPath);
                var sourceFile = await StorageFile.GetFileFromPathAsync(path);
                var document = await PdfDocument.LoadFromFileAsync(sourceFile);
                if (document.PageCount == 0)
                {
                    ShowAttachmentError($"PDFにページがありません: {Path.GetFileName(path)}");
                    continue;
                }

                using var page = document.GetPage(0);
                using var stream = new InMemoryRandomAccessStream();
                await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = 512 });

                var id = Guid.CreateVersion7().ToString("N");
                var thumbnailFilename = id + ".bmp";
                var thumbnailPath = Path.Combine(_listingDataDirectoryPath, thumbnailFilename);
                using (var destination = new FileStream(thumbnailPath, FileMode.CreateNew, FileAccess.Write))
                {
                    _incompletePdfFiles.Add(thumbnailPath);
                    using var source = stream.AsStreamForRead();
                    await source.CopyToAsync(destination);
                }

                var filename = id + extension;
                var pdfPath = Path.Combine(_listingDataDirectoryPath, filename);
                await using (var source = File.OpenRead(path))
                await using (var destination = new FileStream(pdfPath, FileMode.CreateNew, FileAccess.Write))
                {
                    _incompletePdfFiles.Add(pdfPath);
                    await source.CopyToAsync(destination);
                }

                var pdf = new ListingPdf(id, filename, thumbnailFilename, EntityStatus.New)
                {
                    BasePath = _listingDataDirectoryPath,
                    ParentViewModel = this
                };
                pdf.PropertyChanged += OnListingPdfChanged;
                Pdfs.Add(pdf);
                _unsavedUnitPdfThumbnailFileList.Add(thumbnailPath);
                _unsavedUnitPdfFileList.Add(pdfPath);
                _incompletePdfFiles.Remove(thumbnailPath);
                _incompletePdfFiles.Remove(pdfPath);
                MarkPdfEdited();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
            {
                Debug.WriteLine($"Failed to import commercial listing PDF '{path}': {ex}");
                ShowAttachmentError($"PDFを読み込めませんでした: {Path.GetFileName(path)}。{ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async Task OpenRoomPdf(ListingPdf pdf)
    {
        if (pdf is null || !Pdfs.Contains(pdf) || !IsEditorEnabled || !IsPropertyUnitOwnership) return;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(_listingDataDirectoryPath, pdf.PdfFilename));
            if (!await Windows.System.Launcher.LaunchFileAsync(file))
            {
                ShowAttachmentError("PDFを開けませんでした。PDFを開くアプリが設定されているか確認してください。");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to open commercial listing PDF: {ex}");
            ShowAttachmentError($"PDFを開けませんでした: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteRoomPdf(ListingPdf pdf)
    {
        if (pdf is null || !Pdfs.Contains(pdf) || !IsEditorEnabled || !IsPropertyUnitOwnership) return;

        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この募集物件の図面を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
            if (!Pdfs.Remove(pdf)) return;

            pdf.PropertyChanged -= OnListingPdfChanged;
            pdf.ParentViewModel = null;
            _unit.PdfsToBeDeleted.Add(pdf);
            MarkPdfEdited();
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            Debug.WriteLine($"Failed to confirm commercial listing PDF deletion: {ex}");
            ShowAttachmentError($"図面の削除を確認できませんでした: {ex.Message}");
        }
    }
}
