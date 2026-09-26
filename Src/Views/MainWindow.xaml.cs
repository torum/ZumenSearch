using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Xml;
using System.Xml.Linq;
using WinRT.Interop;
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

    public MainWindow(ViewModels.MainViewModel viewModel, IDispatcherService dispatcherService)
    {
        _viewModel = viewModel;
        _dispatcherService = dispatcherService;

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

                // RentResidentialEditorWindow element
                var editWindow = xdoc.Root.Element("RentResidentialEditorWindow");
                if (editWindow != null)
                {
                    var hoge = editWindow.Attribute("top");
                    if (hoge != null)
                    {
                        _viewModel.RentResidentialEditorWinTop = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("left");
                    if (hoge != null)
                    {
                        _viewModel.RentResidentialEditorWinLeft = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("height");
                    if (hoge != null)
                    {
                        _viewModel.RentResidentialEditorWinHeight = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("width");
                    if (hoge != null)
                    {
                        _viewModel.RentResidentialEditorWinWidth = int.Parse(hoge.Value);
                    }
                }

                if (_viewModel.RentResidentialEditorWinWidth < 500)
                {
                    _viewModel.RentResidentialEditorWinWidth = 500;
                }
                if (_viewModel.RentResidentialEditorWinHeight < 500)
                {
                    _viewModel.RentResidentialEditorWinHeight = 500;
                }
                if (_viewModel.RentResidentialEditorWinTop < 0)
                {
                    _viewModel.RentResidentialEditorWinTop = 0;
                }
                if (_viewModel.RentResidentialEditorWinLeft < 0)
                {
                    _viewModel.RentResidentialEditorWinLeft = 0;
                }

                // RentResidentialListingEditorWindow element
                editWindow = xdoc.Root.Element("RentResidentialListingEditorWindow");
                if (editWindow != null)
                {
                    var hoge = editWindow.Attribute("top");
                    if (hoge != null)
                    {
                        _viewModel.RentResidentialListingEditorWinTop = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("left");
                    if (hoge != null)
                    {
                        _viewModel.RentResidentialListingEditorWinLeft = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("height");
                    if (hoge != null)
                    {
                        _viewModel.RentResidentialListingEditorWinHeight = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("width");
                    if (hoge != null)
                    {
                        _viewModel.RentResidentialListingEditorWinWidth = int.Parse(hoge.Value);
                    }
                }

                if (_viewModel.RentResidentialListingEditorWinWidth < 500)
                {
                    _viewModel.RentResidentialListingEditorWinWidth = 500;
                }
                if (_viewModel.RentResidentialListingEditorWinHeight < 500)
                {
                    _viewModel.RentResidentialListingEditorWinHeight = 500;
                }
                if (_viewModel.RentResidentialListingEditorWinTop < 0)
                {
                    _viewModel.RentResidentialListingEditorWinTop = 0;
                }
                if (_viewModel.RentResidentialListingEditorWinLeft < 0)
                {
                    _viewModel.RentResidentialListingEditorWinLeft = 0;
                }

                // RentCommercialEditorWindow element
                editWindow = xdoc.Root.Element("RentCommercialEditorWindow");
                if (editWindow != null)
                {
                    var hoge = editWindow.Attribute("top");
                    if (hoge != null)
                    {
                        _viewModel.RentCommercEditorWinTop = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("left");
                    if (hoge != null)
                    {
                        _viewModel.RentCommercEditorWinLeft = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("height");
                    if (hoge != null)
                    {
                        _viewModel.RentCommercEditorWinHeight = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("width");
                    if (hoge != null)
                    {
                        _viewModel.RentCommercEditorWinWidth = int.Parse(hoge.Value);
                    }
                }

                if (_viewModel.RentCommercEditorWinWidth < 500)
                {
                    _viewModel.RentCommercEditorWinWidth = 500;
                }
                if (_viewModel.RentCommercEditorWinHeight < 500)
                {
                    _viewModel.RentCommercEditorWinHeight = 500;
                }
                if (_viewModel.RentCommercEditorWinTop < 0)
                {
                    _viewModel.RentCommercEditorWinTop = 0;
                }
                if (_viewModel.RentCommercEditorWinLeft < 0)
                {
                    _viewModel.RentCommercEditorWinLeft = 0;
                }

                // LessorEditorWindow element
                editWindow = xdoc.Root.Element("LessorEditorWindow");
                if (editWindow != null)
                {
                    var hoge = editWindow.Attribute("top");
                    if (hoge != null)
                    {
                        _viewModel.LessorEditorWinTop = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("left");
                    if (hoge != null)
                    {
                        _viewModel.LessorEditorWinLeft = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("height");
                    if (hoge != null)
                    {
                        _viewModel.LessorEditorWinHeight = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("width");
                    if (hoge != null)
                    {
                        _viewModel.LessorEditorWinWidth = int.Parse(hoge.Value);
                    }
                }

                if (_viewModel.LessorEditorWinWidth < 500)
                {
                    _viewModel.LessorEditorWinWidth = 500;
                }
                if (_viewModel.LessorEditorWinHeight < 500)
                {
                    _viewModel.LessorEditorWinHeight = 500;
                }
                if (_viewModel.LessorEditorWinTop < 0)
                {
                    _viewModel.LessorEditorWinTop = 0;
                }
                if (_viewModel.LessorEditorWinLeft < 0)
                {
                    _viewModel.LessorEditorWinLeft = 0;
                }

                // BrokerEditorWindow element
                editWindow = xdoc.Root.Element("BrokerEditorWindow");
                if (editWindow != null)
                {
                    var hoge = editWindow.Attribute("top");
                    if (hoge != null)
                    {
                        _viewModel.BrokerEditorWinTop = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("left");
                    if (hoge != null)
                    {
                        _viewModel.BrokerEditorWinLeft = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("height");
                    if (hoge != null)
                    {
                        _viewModel.BrokerEditorWinHeight = int.Parse(hoge.Value);
                    }

                    hoge = editWindow.Attribute("width");
                    if (hoge != null)
                    {
                        _viewModel.BrokerEditorWinWidth = int.Parse(hoge.Value);
                    }
                }

                if (_viewModel.BrokerEditorWinWidth < 500)
                {
                    _viewModel.BrokerEditorWinWidth = 500;
                }
                if (_viewModel.BrokerEditorWinHeight < 500)
                {
                    _viewModel.BrokerEditorWinHeight = 500;
                }
                if (_viewModel.BrokerEditorWinTop < 0)
                {
                    _viewModel.BrokerEditorWinTop = 0;
                }
                if (_viewModel.BrokerEditorWinLeft < 0)
                {
                    _viewModel.BrokerEditorWinLeft = 0;
                }

            }
        }
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        var isCancel = false;

        if (_viewModel.RentResidentialListingEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.RentResidentialListingEditorList)
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

                    IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                    NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                    editor.Activate();
                    editor.AppWindow.MoveInZOrderAtTop();

                    // Show comfirmation dialog to user to save changes or not.
                    if (editor.Content is Views.Rent.Residentials.Listing.ShellPage shell)
                    {
                        await shell.ShowEditorCloseConfirmationDialog();
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.RentResidentialListingEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                    
                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            return;
        }

        if (_viewModel.RentResidentialEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.RentResidentialEditorList)
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

                    IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                    NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                    editor.Activate();
                    editor.AppWindow.MoveInZOrderAtTop();

                    // Show comfirmation dialog to user to save changes or not.
                    if (editor.Content is Views.Rent.Residentials.ShellPage shell)
                    {
                        await shell.ShowEditorCloseConfirmationDialog();
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.RentResidentialEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized

                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            return;
        }


        if (_viewModel.RentCommercEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.RentCommercEditorList)
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

                    IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                    NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                    editor.Activate();
                    editor.AppWindow.MoveInZOrderAtTop();

                    // Show comfirmation dialog to user to save changes or not.
                    if (editor.Content is Views.Rent.Commercials.ShellPage shell)
                    {
                        await shell.ShowEditorCloseConfirmationDialog();
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.RentCommercEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    //editor.IsAutoClose = true;

                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized

                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            return;
        }




        if (_viewModel.LessorEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.LessorEditorList)
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

                    IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                    NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                    editor.Activate();
                    editor.AppWindow.MoveInZOrderAtTop();

                    // Show comfirmation dialog to user to save changes or not.
                    if (editor.Content is Views.Rent.Lessors.ShellPage shell)
                    {
                        await shell.ShowEditorCloseConfirmationDialog();
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.LessorEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized

                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            return;
        }

        if (_viewModel.BrokerEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.BrokerEditorList)
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

                    IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                    NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                    editor.Activate();
                    editor.AppWindow.MoveInZOrderAtTop();

                    // Show comfirmation dialog to user to save changes or not.
                    if (editor.Content is Views.Brokers.ShellPage shell)
                    {
                        await shell.ShowEditorCloseConfirmationDialog();
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.BrokerEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    editor.IsAutoClose = true;

                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized

                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            return;
        }

    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // Save window state and position, etc.
        SaveSetting();

        _viewModel.CleanUp();

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

        // Editor window Bldg
        var editWindow = doc.CreateElement(string.Empty, "RentResidentialEditorWindow", string.Empty);

        // Editor window attributes
        attrs = doc.CreateAttribute("width");
        attrs.Value = _viewModel.RentResidentialEditorWinWidth.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("height");
        attrs.Value = _viewModel.RentResidentialEditorWinHeight.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = _viewModel.RentResidentialEditorWinTop.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = _viewModel.RentResidentialEditorWinLeft.ToString();
        editWindow.SetAttributeNode(attrs);

        // Set editor window element to root.
        root.AppendChild(editWindow);

        // Editor window Room
        editWindow = doc.CreateElement(string.Empty, "RentResidentialListingEditorWindow", string.Empty);

        // Editor window attributes
        attrs = doc.CreateAttribute("width");
        attrs.Value = _viewModel.RentResidentialListingEditorWinWidth.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("height");
        attrs.Value = _viewModel.RentResidentialListingEditorWinHeight.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = _viewModel.RentResidentialListingEditorWinTop.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = _viewModel.RentResidentialListingEditorWinLeft.ToString();
        editWindow.SetAttributeNode(attrs);

        // Set editor window element to root.
        root.AppendChild(editWindow);

        // Editor window Rent Commercial
        editWindow = doc.CreateElement(string.Empty, "RentCommercialEditorWindow", string.Empty);

        // Editor window attributes
        attrs = doc.CreateAttribute("width");
        attrs.Value = _viewModel.RentCommercEditorWinWidth.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("height");
        attrs.Value = _viewModel.RentCommercEditorWinHeight.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = _viewModel.RentCommercEditorWinTop.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = _viewModel.RentCommercEditorWinLeft.ToString();
        editWindow.SetAttributeNode(attrs);

        // Set editor window element to root.
        root.AppendChild(editWindow);

        // Editor window Lessor
        editWindow = doc.CreateElement(string.Empty, "LessorEditorWindow", string.Empty);

        // Editor window attributes
        attrs = doc.CreateAttribute("width");
        attrs.Value = _viewModel.LessorEditorWinWidth.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("height");
        attrs.Value = _viewModel.LessorEditorWinHeight.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = _viewModel.LessorEditorWinTop.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = _viewModel.LessorEditorWinLeft.ToString();
        editWindow.SetAttributeNode(attrs);

        // Set editor window element to root.
        root.AppendChild(editWindow);


        // Editor window Broker
        editWindow = doc.CreateElement(string.Empty, "BrokerEditorWindow", string.Empty);

        // Editor window attributes
        attrs = doc.CreateAttribute("width");
        attrs.Value = _viewModel.BrokerEditorWinWidth.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("height");
        attrs.Value = _viewModel.BrokerEditorWinHeight.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = _viewModel.BrokerEditorWinTop.ToString();
        editWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = _viewModel.BrokerEditorWinLeft.ToString();
        editWindow.SetAttributeNode(attrs);

        // Set editor window element to root.
        root.AppendChild(editWindow);


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

    #region == BringToFront ==

    private static partial class NativeMethods
    {
        internal const int SW_RESTORE = 9; // Restores a minimized window and brings it to the foreground.

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    }

    #endregion
}
