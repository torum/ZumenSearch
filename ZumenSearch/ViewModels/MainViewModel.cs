using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using ZumenSearch.Extensions.AbstractFactory;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels
{
    public partial class MainViewModel : ObservableRecipient
    {
        #region == Window management ==

        private readonly IAbstractFactory<Views.Rent.Residentials.Editor.EditorShell> _editorFactory;

        // Marking EditorList as readonly to fix IDE0044
        public readonly List<Views.Rent.Residentials.Editor.EditorWindow> EditorList = [];

        // Editor window position and size
        public int EditorWinWidth = 1366;
        public int EditorWinHeight = 768;
        public int EditorWinLeft = 130;
        public int EditorWinTop = 130;

        #endregion

        #region == Navigation ==

        private bool _isBackEnabled = true; // Replace field with backing field for MVVMTK0045
        public bool IsBackEnabled // Implement partial property for AOT compatibility
        {
            get => _isBackEnabled;
            set => SetProperty(ref _isBackEnabled, value);
        }

        // Replacing [ObservableProperty] with a partial property to fix MVVMTK0045
        private object? _selected;
        public object? Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }


        #endregion

        public MainViewModel(IAbstractFactory<Views.Rent.Residentials.Editor.EditorShell> editorFactory)
        {
            _editorFactory = editorFactory;
        }

        public void CreateNewResidentialsEditorWindow()
        {
            Debug.WriteLine("AddNew command executed!");

            Views.Rent.Residentials.Editor.EditorShell editorShell = _editorFactory.Create();
            var editorWindow = editorShell.EditorWin;

            if (editorWindow == null)
            {
                // EditorWin should be initialized in the EditorShell constructor.
                throw new ArgumentNullException(nameof(editorWindow));
            }

            MainViewModel mainShellViewModel = App.GetService<MainViewModel>();
            // Add to the list of editor windows.
            mainShellViewModel.EditorList.Add(editorWindow);

            // Window state and position.
            //editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainShellViewModel.EditorWinLeft, mainShellViewModel.EditorWinTop, mainShellViewModel.EditorWinWidth, mainShellViewModel.EditorWinHeight));
            // TEMP:
            editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainShellViewModel.EditorWinLeft, mainShellViewModel.EditorWinTop, 1366, 768));
            if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
            }

            //editorWindow.AppWindow.Show();
            editorWindow.Activate();


        }


    }
}
