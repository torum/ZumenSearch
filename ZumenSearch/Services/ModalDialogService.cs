using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Playlists;
using ZumenSearch.Models;
using ZumenSearch.Services;
using ZumenSearch.ViewModels.Rent.Residentials;
using ZumenSearch.Views.Dialogs;
using ZumenSearch.Views.Rent.Residentials.Editor;

namespace ZumenSearch.Services;

public class ModalDialogService : IModalDialogService
{
    public bool IsDialogOpened {get; private set;}

    public ModalDialogService()
    {
        //
        IsDialogOpened = false;
    }

    public async Task<ContentDialogResult> ShowEditorCloseConfirmationDialog(Window win)
    {
        if (IsDialogOpened)
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

        Debug.WriteLine("await dialog.ShowAsync()");
        IsDialogOpened = true;
        var result = await dialog.ShowAsync();
        IsDialogOpened = false;
        return result;
    }

    public void ShowUnitDialog(ViewModels.Rent.Residentials.ResidentialsViewModel editVM, EditorWindow editWin)
    {

    }

    public async Task<RailLine?> ShowRailLineSelectDialog(Window win)
    {
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
        IsDialogOpened = true;
        var result = await dialog.ShowAsync();
        IsDialogOpened = false;
        if (result == ContentDialogResult.Primary)
        {
            //
            return dialogContent.ViewModel.SelectedRailLine;
        }

        return null;
    }

    public async Task<RailStation?> ShowRailStationSelectDialog(Window win, string railLineCode)
    {
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

        IsDialogOpened = true;
        var result = await dialog.ShowAsync();
        IsDialogOpened = false;

        if (result == ContentDialogResult.Primary)
        {
            //
            return dialogContent.ViewModel.SelectedRailStation;
        }

        return null;
    }


}
