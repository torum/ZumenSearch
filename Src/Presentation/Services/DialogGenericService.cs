using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models.Transportation;
using ZumenSearch.Services;
using ZumenSearch.Views.Services.Contracts;
using ZumenSearch.Views.Dialogs;

namespace ZumenSearch.Views.Services;

public class DialogGenericService : IDialogGenericService
{
    private XamlRoot? _xamlRoot;
    private Window? _window;
    private bool _isDialogOpened;
    private bool _isInitialized;

    public DialogGenericService()
    {
        _isDialogOpened = false;
        _isInitialized = false;
    }

    public void Initialize(XamlRoot xamlRoot, Window window)
    {
        _xamlRoot = xamlRoot;
        _window = window;
        _isInitialized = true;
    }

    public async Task<ContentDialogResult> ShowEditorCloseConfirmationDialog()
    {
        if (_isDialogOpened)
        {
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            Debug.WriteLine("DialogGenericService: _isDialogOpened @ShowEditorCloseConfirmationDialog");
            return ContentDialogResult.None;
        }

        if (_xamlRoot is null || !_isInitialized)
        {
            Debug.WriteLine("ModalDialogService: _xamlRoot is null or not initialized.");
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

        _isDialogOpened = true;
        try
        {
            return await dialog.ShowAsync();
        }
        finally
        {
            _isDialogOpened = false;
        }
    }

    public async Task<ContentDialogResult> ShowDeleteConfirmationDialog(string message)
    {
        if (_isDialogOpened || _xamlRoot is null || !_isInitialized)
        {
            Debug.WriteLine("DialogGenericService: _isDialogOpened or _xamlRoot is null or not initialized @ShowDeleteConfirmationDialog");
            return ContentDialogResult.None;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = _xamlRoot,
            Title = "削除の確認",
            PrimaryButtonText = "削除",
            CloseButtonText = "キャンセル",
            DefaultButton = ContentDialogButton.Close,
            Content = message
        };

        _isDialogOpened = true;
        try
        {
            return await dialog.ShowAsync();
        }
        finally
        {
            _isDialogOpened = false;
        }
    }

    public async Task<Models.SearchResult.PersonSearchResultItem?> ShowLessorSelectDialog(ViewModels.Dialogs.LessorSelectViewModel viewModel)
    {
        if (_isDialogOpened)
        {
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            Debug.WriteLine("DialogGenericService: _isDialogOpened @ShowLessorSelectDialog");
            return null;
        }

        if (_xamlRoot is null || !_isInitialized)
        {
            Debug.WriteLine("_xamlRoot is null or not initialized @ShowLessorSelectDialog");
            return null;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = _xamlRoot,
            Title = "貸主の選択",
            IsPrimaryButtonEnabled = false,
            PrimaryButtonText = "確定",
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "キャンセル",
            Content = new Views.Dialogs.LessorSelectPage(viewModel)
        };

        if (dialog.Content is not LessorSelectPage dialogContent)
        {
            return null;
        }

        dialogContent.ViewModel.SelectionChanged += (sender, e) =>
        {
            if ((e is not null) && (e is Models.SearchResult.PersonSearchResultItem rl))
            {
                dialog.IsPrimaryButtonEnabled = true;
            }
        };

        _isDialogOpened = true;
        try
        {
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return dialogContent.ViewModel.SelectedLessor;
            }
        }
        finally
        {
            _isDialogOpened = false;
        }

        return null;
    }

    public async Task<Models.SearchResult.PersonSearchResultItem?> ShowBrokerSelectDialog(ViewModels.Dialogs.BrokerSelectViewModel viewModel)
    {
        if (_isDialogOpened)
        {
            Debug.WriteLine("DialogGenericService: _isDialogOpened @ShowBrokerSelectDialog");
            return null;
        }

        if (_xamlRoot is null || !_isInitialized)
        {
            Debug.WriteLine("_xamlRoot is null or not initialized @ShowBrokerSelectDialog");
            return null;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = _xamlRoot,
            Title = "宅建業者の選択",
            IsPrimaryButtonEnabled = false,
            PrimaryButtonText = "確定",
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "キャンセル",
            Content = new Views.Dialogs.BrokerSelectPage(viewModel)
        };

        if (dialog.Content is not BrokerSelectPage dialogContent)
        {
            return null;
        }

        dialogContent.ViewModel.SelectionChanged += (sender, e) =>
        {
            if ((e is not null) && (e is Models.SearchResult.PersonSearchResultItem))
            {
                dialog.IsPrimaryButtonEnabled = true;
            }
        };

        _isDialogOpened = true;
        try
        {
            var result = await dialog.ShowAsync();
            _isDialogOpened = false;

            if (result == ContentDialogResult.Primary)
            {
                return dialogContent.ViewModel.SelectedBroker;
            }
        }
        finally
        {
            _isDialogOpened = false;
        }

        return null;
    }

    public async Task<RailLine?> ShowRailLineSelectDialog()
    {
        if (_isDialogOpened)
        {
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            Debug.WriteLine("DialogGenericService: _isDialogOpened @ShowRailLineSelectDialog");
            return null;
        }

        if (_xamlRoot is null || !_isInitialized)
        {
            System.Diagnostics.Debug.WriteLine("_xamlRoot is null or not initialized @ShowRailLineSelectDialog");
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
            Content = new Views.Dialogs.RailLineSelectPage(new ViewModels.Dialogs.RailLineSelectViewModel(new DataAccessTransportationService()))
        };

        if (dialog.Content is not RailLineSelectPage dialogContent)
        {
            return null;
        }

        dialogContent.ViewModel.SelectionChanged += (sender, e) =>
        {
            if ((e is not null) && (e is RailLine rl))
            {
                dialog.IsPrimaryButtonEnabled = true;
            }
        };

        _isDialogOpened = true;
        try
        {
            var result = await dialog.ShowAsync();
            _isDialogOpened = false;
            if (result == ContentDialogResult.Primary)
            {
                return dialogContent.ViewModel.SelectedRailLine;
            }
        }
        finally
        {
            _isDialogOpened = false;
        }

        return null;
    }

    public async Task<RailStation?> ShowRailStationSelectDialog(string railLineCode)
    {
        if (_isDialogOpened)
        {
            // Prevents COM exepction causing by attempt to show multiple dialogs. (Window's close button is enabled even tho dialog is shown)
            Debug.WriteLine("DialogGenericService: _isDialogOpened @ShowRailStationSelectDialog");
            return null;
        }

        if (_xamlRoot is null || !_isInitialized)
        {
            Debug.WriteLine("_xamlRoot is null or not initialized @ShowRailStationSelectDialog");
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
            Content = new Views.Dialogs.RailStationSelectPage(new ViewModels.Dialogs.RailStationSelectViewModel(new DataAccessTransportationService(), railLineCode))
        };

        if (dialog.Content is not RailStationSelectPage dialogContent)
        {
            return null;
        }

        dialogContent.ViewModel.SelectionChanged += (sender, e) =>
        {
            if (e is not null and RailStation rs)
            {
                dialog.IsPrimaryButtonEnabled = true;
            }
        };

        _isDialogOpened = true;
        try
        {
            var result = await dialog.ShowAsync();
            _isDialogOpened = false;

            if (result == ContentDialogResult.Primary)
            {
                return dialogContent.ViewModel.SelectedRailStation;
            }
        }
        finally
        {
            _isDialogOpened = false;
        }

        return null;
    }
}
