using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Views.Dialogs;

namespace ZumenSearch.Services;

public class ModalDialogService : IModalDialogService
{
    private bool _isDialogOpened;
    private readonly List<Window> _ownerWindowList;

    public ModalDialogService()
    {
        //
        _isDialogOpened = false;
        _ownerWindowList = new List<Window>();
    }

    public async Task<ContentDialogResult> ShowEditorCloseConfirmationDialog(Window win)
    {
        if (_isDialogOpened)// && (_ownerWindowList.IndexOf(win) > -1)
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

        var dialog = new ContentDialog
        {
            XamlRoot = win.Content.XamlRoot,
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


    public async Task<ContentDialogResult> ShowLeaveUnitDirtyConfirmationDialog(XamlRoot root)
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

        if (root is null)
        {
            return ContentDialogResult.None;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = root,
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

    public async Task<RailLine?> ShowRailLineSelectDialog(Window win)
    {
        if (_isDialogOpened)// && (_ownerWindowList.IndexOf(win) > -1)
        {
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            return null;
        }

        if (win is null)
        {
            return null;
        }

        if (win.Content is null)
        {
            return null;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = win.Content.XamlRoot,
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

    public async Task<RailStation?> ShowRailStationSelectDialog(Window win, string railLineCode)
    {
        if (_isDialogOpened)// && (_ownerWindowList.IndexOf(win) > -1)
        {
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            return null;
        }

        if (win is null)
        {
            return null;
        }

        if (win.Content is null)
        {
            return null;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = win.Content.XamlRoot,
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
