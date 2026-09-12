using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Views.Dialogs;

namespace ZumenSearch.Services;

public class ModalDialogService : IModalDialogService
{
    private XamlRoot? _xamlRoot;
    private bool _isDialogOpened;
    private readonly List<Window> _ownerWindowList; // I made the service transient, so this won't be needed. But needs to be checked lator.

    public ModalDialogService()
    {
        _isDialogOpened = false;
        _ownerWindowList = new List<Window>();
    }

    public void Initialize(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public async Task<ContentDialogResult> ShowEditorCloseConfirmationDialog()
    {
        if (_isDialogOpened)// && (_ownerWindowList.IndexOf(win) > -1)
        {
            Debug.WriteLine("ModalDialogService: _isDialogOpened");
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            return ContentDialogResult.None;
        }

        if (_xamlRoot is null)
        {
            Debug.WriteLine("ModalDialogService: _xamlRoot is null. Not initialized");
            return ContentDialogResult.None;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = _xamlRoot,
            Title = "保存の確認",
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "保存して閉じる",
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = "変更を破棄して閉じる",
            CloseButtonText = "キャンセル",
            Content = "編集画面の変更内容が保存されていません。"
        };

        //Debug.WriteLine("await dialog.ShowAsync()");

        _isDialogOpened = true;
        //_ownerWindowList.Add(win);
        var result = await dialog.ShowAsync();
        _isDialogOpened = false;
        //_ownerWindowList.Remove(win);
        return result;
    }


    public async Task<ContentDialogResult> ShowLeaveUnitDirtyConfirmationDialog()
    {
        if (_isDialogOpened)
        {
            System.Diagnostics.Debug.WriteLine("_isDialogOpened");
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            return ContentDialogResult.None;
        }
        /*
        if (_isDialogOpened && (_ownerWindowList.IndexOf(win) > -1))
        {
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            return ContentDialogResult.None;
        }

        if (win is null)
        {
            return ContentDialogResult.None;
        }

        if (win.Content is null)
        {
            return ContentDialogResult.None;
        }
        */

        if (_xamlRoot is null)
        {
            return ContentDialogResult.None;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = _xamlRoot,
            Title = "保存の確認（部屋）",
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "保存して移動する",
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = "変更を破棄して移動する",
            CloseButtonText = "キャンセル",
            Content = "部屋の変更内容が保存されていません。"
        };

        //Debug.WriteLine("await dialog.ShowAsync()");

        _isDialogOpened = true;
        //_ownerWindowList.Add(win);
        var result = await dialog.ShowAsync();
        _isDialogOpened = false;
        //_ownerWindowList.Remove(win);
        return result;
    }

    public async Task<RailLine?> ShowRailLineSelectDialog()
    {
        if (_isDialogOpened)// && (_ownerWindowList.IndexOf(win) > -1)
        {
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            System.Diagnostics.Debug.WriteLine("_isDialogOpened @ShowRailLineSelectDialog");
            return null;
        }

        if (_xamlRoot is null)
        {
            System.Diagnostics.Debug.WriteLine("_xamlRoot is null @ShowRailLineSelectDialog");
            return null;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = _xamlRoot,
            Title = "路線の選択",
            IsPrimaryButtonEnabled = false,
            PrimaryButtonText = "確定",
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "キャンセル",
            Content = new Views.Dialogs.RailLineSelectPage(new ViewModels.Transportation.RailLineSelectViewModel(new DataAccessTransportationService()))
        };

        if (dialog.Content is not RailLineSelectPage dialogContent)
        {
            return null;
        }

        dialogContent.ViewModel.SelectionChanged += (sender, e) =>
        {
            if ((e is not null) && (e is RailLine rl))
            {
                //dialogContent.ViewModel.SelectedRailLine
                dialog.IsPrimaryButtonEnabled = true;
            }
        };
        /*
        dialogContent.SelectionList.SelectionChanged += (s, e) =>
        {
            if (dialogContent.SelectionList.SelectedItem is RailLine)
            {
                dialog.IsPrimaryButtonEnabled = true;
            }
        };
        */
        _isDialogOpened = true;
        //_ownerWindowList.Add(win);
        var result = await dialog.ShowAsync();
        _isDialogOpened = false;
        //_ownerWindowList.Remove(win);
        if (result == ContentDialogResult.Primary)
        {
            //
            return dialogContent.ViewModel.SelectedRailLine;
        }

        return null;
    }

    public async Task<RailStation?> ShowRailStationSelectDialog(string railLineCode)
    {
        if (_isDialogOpened)// && (_ownerWindowList.IndexOf(win) > -1)
        {
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            return null;
        }

        if (_xamlRoot is null)
        {
            return null;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = _xamlRoot,
            Title = "駅の選択",
            IsPrimaryButtonEnabled = false,
            PrimaryButtonText = "確定",
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "キャンセル",
            Content = new Views.Dialogs.RailStationSelectPage(new ViewModels.Transportation.RailStationSelectViewModel(new DataAccessTransportationService(), railLineCode))
        };

        if (dialog.Content is not RailStationSelectPage dialogContent)
        {
            return null;
        }

        dialogContent.ViewModel.SelectionChanged += (sender, e) =>
        {
            if (e is not null and RailStation rs)
            {
                //dialogContent.ViewModel.SelectedRailStation
                dialog.IsPrimaryButtonEnabled = true;
            }
        };

        _isDialogOpened = true;
        //_ownerWindowList.Add(win);
        var result = await dialog.ShowAsync();
        //_ownerWindowList.Remove(win);
        _isDialogOpened = false;

        if (result == ContentDialogResult.Primary)
        {
            //
            return dialogContent.ViewModel.SelectedRailStation;
        }

        return null;
    }


}
