using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views;

public sealed partial class MainWindow : Window
{
    private OverlappedPresenterState _winState = OverlappedPresenterState.Restored;

    private int _winRestoreWidth = 1274;
    private int _winRestoreHeight = 794;
    private int _winRestoreTop = 100;
    private int _winRestoreLeft = 100;

    private readonly ViewModels.MainViewModel _viewModel;
    private readonly IDispatcherService _dispatcherService;

    public MainWindow(IDispatcherService dispatcherService, ViewModels.MainViewModel viewModel)
    {
        _dispatcherService = dispatcherService;
        _viewModel = viewModel;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        //SetTitleBar(AppTitleBar);

        //this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets\\App.ico"));
        this.AppWindow.SetIcon("Assets/App.ico");

        this.AppWindow.Closing += AppWindow_Closing;
        this.Closed += Window_Closed;
        this.SizeChanged += Window_SizeChanged;

        LoadSetting();

        if (this.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = 542;
            presenter.PreferredMinimumHeight = 600;

            if (_winState == OverlappedPresenterState.Maximized)
            {
                this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(_winRestoreLeft, _winRestoreTop, _winRestoreWidth, _winRestoreHeight));
                presenter.Maximize();
            }
            else if (_winState == OverlappedPresenterState.Minimized)
            {
                this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(_winRestoreLeft, _winRestoreTop, _winRestoreWidth, _winRestoreHeight));
            }
            else
            {
                this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(_winRestoreLeft, _winRestoreTop, _winRestoreWidth, _winRestoreHeight));
            }
        }
    }

    private void LoadSetting()
    {
        System.IO.Directory.CreateDirectory(App.AppDataFolder);

        int winHeight;
        int winWidth;
        int winTop;
        int winLeft;

        if (System.IO.File.Exists(App.AppConfigFilePath))
        {
            var xdoc = XDocument.Load(App.AppConfigFilePath);
            //Debug.WriteLine(xdoc.ToString());

            if (xdoc.Root != null)
            {
                // Main window
                var mainWindow = xdoc.Root.Element("MainWindow");
                if (mainWindow != null)
                {
                    var hoge = mainWindow.Attribute("state");
                    if (hoge != null)
                    {
                        if (hoge.Value == "Maximized")
                        {
                            _winState = OverlappedPresenterState.Maximized;
                        }
                        else if (hoge.Value == "Normal")
                        {
                            _winState = OverlappedPresenterState.Restored;
                        }
                        else if (hoge.Value == "Minimized")
                        {
                            // Let's not minimized on startup, so restore it.
                            _winState = OverlappedPresenterState.Restored;
                        }
                    }

                    hoge = mainWindow.Attribute("top");
                    if (hoge != null)
                    {
                        winTop = int.Parse(hoge.Value);
                        _winRestoreTop = winTop;
                    }

                    hoge = mainWindow.Attribute("left");
                    if (hoge != null)
                    {
                        winLeft = int.Parse(hoge.Value);
                        _winRestoreLeft = winLeft;
                    }

                    hoge = mainWindow.Attribute("height");
                    if (hoge != null)
                    {
                        winHeight = int.Parse(hoge.Value);
                        _winRestoreHeight = winHeight;
                    }

                    hoge = mainWindow.Attribute("width");
                    if (hoge != null)
                    {
                        winWidth = int.Parse(hoge.Value);
                        _winRestoreWidth = winWidth;
                    }
                }

                if (_winRestoreWidth < 542)
                {
                    _winRestoreWidth = 542;
                }
                if (_winRestoreHeight < 600)
                {
                    _winRestoreHeight = 600;
                }
                if (_winRestoreTop < 0)
                {
                    _winRestoreTop = 0;
                }
                if (_winRestoreLeft < 0)
                {
                    _winRestoreLeft = 0;
                }

                // BldgEditorWindow element
                var editWindow = xdoc.Root.Element("BldgEditorWindow");
                if (editWindow != null)
                {
                    var hoge = editWindow.Attribute("top");
                    if (hoge != null)
                    {
                        _viewModel.BldgEditorWinTop = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("left");
                    if (hoge != null)
                    {
                        _viewModel.BldgEditorWinLeft = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("height");
                    if (hoge != null)
                    {
                        _viewModel.BldgEditorWinHeight = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("width");
                    if (hoge != null)
                    {
                        _viewModel.BldgEditorWinWidth = int.Parse(hoge.Value);
                    }
                }

                if (_viewModel.BldgEditorWinWidth < 500)
                {
                    _viewModel.BldgEditorWinWidth = 500;
                }
                if (_viewModel.BldgEditorWinHeight < 500)
                {
                    _viewModel.BldgEditorWinHeight = 500;
                }
                if (_viewModel.BldgEditorWinTop < 0)
                {
                    _viewModel.BldgEditorWinTop = 0;
                }
                if (_viewModel.BldgEditorWinLeft < 0)
                {
                    _viewModel.BldgEditorWinLeft = 0;
                }


                // RoomEditorWindow element
                editWindow = xdoc.Root.Element("RoomEditorWindow");
                if (editWindow != null)
                {
                    var hoge = editWindow.Attribute("top");
                    if (hoge != null)
                    {
                        _viewModel.RoomEditorWinTop = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("left");
                    if (hoge != null)
                    {
                        _viewModel.RoomEditorWinLeft = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("height");
                    if (hoge != null)
                    {
                        _viewModel.RoomEditorWinHeight = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("width");
                    if (hoge != null)
                    {
                        _viewModel.RoomEditorWinWidth = int.Parse(hoge.Value);
                    }
                }

                if (_viewModel.RoomEditorWinWidth < 500)
                {
                    _viewModel.RoomEditorWinWidth = 500;
                }
                if (_viewModel.RoomEditorWinHeight < 500)
                {
                    _viewModel.RoomEditorWinHeight = 500;
                }
                if (_viewModel.RoomEditorWinTop < 0)
                {
                    _viewModel.RoomEditorWinTop = 0;
                }
                if (_viewModel.RoomEditorWinLeft < 0)
                {
                    _viewModel.RoomEditorWinLeft = 0;
                }
            }
        }
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        var isCancel = false;

        if (_viewModel.RoomEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.RoomEditorList)
            {
                if (editor.ViewModel is null)
                {
                    Debug.WriteLine("AppWindow_Closing: editor.ViewModel is null");
                    continue;
                }
                if (editor.ViewModel.IsDirty)
                {
                    args.Cancel = true;
                    isCancel = true;
                    editor.Activate();
                    editor.AppWindow.MoveInZOrderAtTop();

                    // Show comfirmation dialog to user to save changes or not.
                    if (editor.Content is Views.Rent.Residentials.Room.ShellPage shell)
                    {
                        await shell.ShowEditorCloseConfirmationDialog();
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.RoomEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    editor.IsAutoClose = true;

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            return;
        }

        if (_viewModel.BldgEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.BldgEditorList)
            {
                if (editor.ViewModel is null)
                {
                    Debug.WriteLine("AppWindow_Closing: editor.ViewModel is null");
                    continue;
                }
                if (editor.ViewModel.IsDirty)
                {
                    args.Cancel = true;
                    isCancel = true;
                    editor.Activate();
                    editor.AppWindow.MoveInZOrderAtTop();

                    // Show comfirmation dialog to user to save changes or not.
                    if (editor.Content is Views.Rent.Residentials.Bldg.ShellPage shell)
                    {
                        await shell.ShowEditorCloseConfirmationDialog();
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.BldgEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    editor.IsAutoClose = true;

                    editor.Close();
                }
            }
        }
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // Save window state and position, etc.
        SaveSetting();

        // TODO:
        //_viewModel.CleanUp();

    }

    private void SaveSetting()
    {
        var winHeight = 794;
        var winWidth = 1274;
        var winTop = 100;
        var winLeft = 100;
        var winState = OverlappedPresenterState.Restored;

        if (this.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            if (presenter.State == OverlappedPresenterState.Maximized)
            {
                winState = OverlappedPresenterState.Maximized;
            }
            else if (presenter.State == OverlappedPresenterState.Minimized)
            {
                winState = OverlappedPresenterState.Minimized;
            }
            else if (presenter.State == OverlappedPresenterState.Restored)
            {
                winState = OverlappedPresenterState.Restored;
            }
            else
            {
                winState = OverlappedPresenterState.Restored;
            }

            // TODO: needs "Window.RestoreBounds Property"
            //if (winState == OverlappedPresenterState.Restored) {}
            winHeight = (int)this.AppWindow.Size.Height;
            winWidth = (int)this.AppWindow.Size.Width;
            winTop = (int)this.AppWindow.Position.Y;
            winLeft = (int)this.AppWindow.Position.X;
        }

        XmlDocument doc = new();
        var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

        // Root Document Element
        var root = doc.CreateElement(string.Empty, "App", string.Empty);
        doc.AppendChild(root);

        XmlAttribute attrs;

        // Main window
        if (this.AppWindow != null)
        {
            // Main window element
            var mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

            // Main window attributes
            attrs = doc.CreateAttribute("width");
            if (winState == OverlappedPresenterState.Restored)
            {
                attrs.Value = winWidth.ToString();
            }
            else
            {
                attrs.Value = _winRestoreWidth.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("height");
            if (winState == OverlappedPresenterState.Restored)
            {
                attrs.Value = winHeight.ToString();
            }
            else
            {
                attrs.Value = _winRestoreHeight.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("top");
            if (winState == OverlappedPresenterState.Restored)
            {
                attrs.Value = winTop.ToString();
            }
            else
            {
                attrs.Value = _winRestoreTop.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("left");
            if (winState == OverlappedPresenterState.Restored)
            {
                attrs.Value = winLeft.ToString();
            }
            else
            {
                attrs.Value = _winRestoreLeft.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("state");
            if (winState == OverlappedPresenterState.Maximized)
            {
                attrs.Value = "Maximized";
            }
            else if (winState == OverlappedPresenterState.Restored)
            {
                attrs.Value = "Normal";

            }
            else if (winState == OverlappedPresenterState.Minimized)
            {
                attrs.Value = "Minimized";
            }
            else
            {
                attrs.Value = "Normal";
            }
            mainWindow.SetAttributeNode(attrs);

            // Set main window element to root.
            root.AppendChild(mainWindow);
        }

        // Editor window
        var editWindow = doc.CreateElement(string.Empty, "BldgEditorWindow", string.Empty);

        // Editor window attributes
        attrs = doc.CreateAttribute("width");
        attrs.Value = _viewModel.BldgEditorWinWidth.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("height");
        attrs.Value = _viewModel.BldgEditorWinHeight.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = _viewModel.BldgEditorWinTop.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = _viewModel.BldgEditorWinLeft.ToString();
        editWindow.SetAttributeNode(attrs);

        // Set editor window element to root.
        root.AppendChild(editWindow);

        // Editor window
        editWindow = doc.CreateElement(string.Empty, "RoomEditorWindow", string.Empty);

        // Editor window attributes
        attrs = doc.CreateAttribute("width");
        attrs.Value = _viewModel.RoomEditorWinWidth.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("height");
        attrs.Value = _viewModel.RoomEditorWinHeight.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = _viewModel.RoomEditorWinTop.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = _viewModel.RoomEditorWinLeft.ToString();
        editWindow.SetAttributeNode(attrs);

        // Set editor window element to root.
        root.AppendChild(editWindow);

        /*
        // Modal window 
        var modalWindow = doc.CreateElement(string.Empty, "ModalWindow", string.Empty);

        // Editor window attributes
        attrs = doc.CreateAttribute("width");
        attrs.Value = _viewModel.ModalWinWidth.ToString();
        modalWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("height");
        attrs.Value = _viewModel.ModalWinHeight.ToString();
        modalWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = _viewModel.ModalWinTop.ToString();
        modalWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = _viewModel.ModalWinLeft.ToString();
        modalWindow.SetAttributeNode(attrs);

        // Set editor window element to root.
        root.AppendChild(modalWindow);
        */


        try
        {
            doc.Save(App.AppConfigFilePath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MainWindow_Closed: " + ex + " while saving : " + App.AppConfigFilePath);
        }
    }

    private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        if (this.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            if (presenter.State == OverlappedPresenterState.Restored)
            {
                _winRestoreHeight = (int)this.AppWindow.Size.Height;
                _winRestoreWidth = (int)this.AppWindow.Size.Width;
                _winRestoreTop = (int)this.AppWindow.Position.Y;
                _winRestoreLeft = (int)this.AppWindow.Position.X;
            }
        }
    }
}
