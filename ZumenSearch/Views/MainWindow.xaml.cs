using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views
{
    public sealed partial class MainWindow : Window
    {

        private readonly MainViewModel _viewModel = App.GetService<MainViewModel>();

        // Window position and size
        // TODO: Change this lator.1920x1080
        private int winRestoreWidth = 1920;//1024;
        // TODO: Change this lator.
        private int winRestoreHeight = 1080;//768;
        private int winRestoreTop = 100;
        private int winRestoreleft = 100;

        private OverlappedPresenterState winState = OverlappedPresenterState.Restored;

        public MainWindow()
        {
            InitializeComponent();

            this.Title = "ZumenSearch Main";

            LoadSetting();

            // Restore window size and position
            Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
            if (appWindow != null)
            {
                // Window state
                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    if (winState == OverlappedPresenterState.Maximized)
                    {
                        // Sets restore size and position.
                        //appWindow.MoveAndResize(new Windows.Graphics.RectInt32(winRestoreleft, winRestoreTop, winRestoreWidth, winRestoreHeight));
                        // Maximize the window.
                        //presenter.Maximize();

                        // TODO: TEMP
                        appWindow.Move(new Windows.Graphics.PointInt32(winRestoreleft, winRestoreTop));
                        appWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1080));
                        presenter.IsResizable = false;
                    }
                    else if (winState == OverlappedPresenterState.Minimized)
                    {
                        // This should not happen, but just in case.
                        //presenter.Restore();
                        // Sets restore size and position.
                        //appWindow.MoveAndResize(new Windows.Graphics.RectInt32(winRestoreleft, winRestoreTop, winRestoreWidth, winRestoreHeight));

                        // TODO: TEMP
                        appWindow.Move(new Windows.Graphics.PointInt32(winRestoreleft, winRestoreTop));
                        appWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1080));
                        presenter.IsResizable = false;
                    }
                    else
                    {
                        // Sets restore size and position.
                        //appWindow.MoveAndResize(new Windows.Graphics.RectInt32(winRestoreleft, winRestoreTop, winRestoreWidth, winRestoreHeight));

                        // TODO: TEMP
                        appWindow.Move(new Windows.Graphics.PointInt32(winRestoreleft, winRestoreTop));
                        appWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1080));
                        presenter.IsResizable = false;
                    }
                }

                // TODO: Check editor window CanClose before closing the main window.
                appWindow.Closing += (s, a) =>
                {

                    // TODO: Currently, WinUI3 does not have "App.Current?.Windows". So, we cannot loop through all windows.

                    // Temporary workaround for closing all editor windows when the main window is closed.
                    if (_viewModel.EditorList.Count > 0)
                    {
                        a.Cancel = true;

                        foreach (var editor in _viewModel.EditorList)
                        {
                            editor.Activate();
                        }
                    }

                    /*
                    // Loop window list and close all windows.           
                    foreach (var editor in _viewModel.EditorList)
                    {
                        // TODO: check if the editor can be closed or not. Keep deleted window list.
                        //editorEindow.CanClose

                        editor.IsAutoClose = true;
                        editor.Close();
                    }
                    // TODO: when close is canceled.
                    //a.Cancel = true; // Prevents closing the window immediately.    
                    a.Cancel = false;
                    */
                };
            }
        }
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            //
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Save window state and position, etc.
            SaveSetting();

            // TODO:
            //_viewModel.CleanUp();

            // Save error logs.
            var app = App.Current as App;
            app?.SaveErrorLog();
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
            if (appWindow != null)
            {
                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    if (presenter.State == OverlappedPresenterState.Maximized)
                    {
                    }
                    else if (presenter.State == OverlappedPresenterState.Minimized)
                    {
                    }
                    else
                    {
                        winRestoreHeight = (int)appWindow.Size.Height;
                        winRestoreWidth = (int)appWindow.Size.Width;
                        winRestoreTop = (int)appWindow.Position.Y;
                        winRestoreleft = (int)appWindow.Position.X;
                    }
                }
            }
        }

        private void SaveSetting()
        {
            #region == Save setting ==

            int winHeight = 480;
            int winWidth = 640;
            int winTop = 100;
            int winLeft = 100;
            OverlappedPresenterState winState = OverlappedPresenterState.Restored;
            bool isDebugSaveLog = true;

            var app = App.Current as App;
            if (app is not null)
            {
                isDebugSaveLog = app.IsSaveErrorLog;
            }

            // Window size, position and state.
            Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
            if (appWindow != null)
            {
                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    if (presenter.State == OverlappedPresenterState.Maximized)
                    {
                        winState = OverlappedPresenterState.Maximized;
                    }
                    else if (presenter.State == OverlappedPresenterState.Minimized)
                    {
                        winState = OverlappedPresenterState.Restored;
                    }
                    else
                    {
                        winState = OverlappedPresenterState.Restored;
                    }

                    // TODO: needs "Window.RestoreBounds Property"
                    //if (winState == OverlappedPresenterState.Restored) {}
                    winHeight = (int)appWindow.Size.Height;
                    winWidth = (int)appWindow.Size.Width;
                    winTop = (int)appWindow.Position.Y;
                    winLeft = (int)appWindow.Position.X;
                }
            }

            XmlDocument doc = new();
            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            // Root Document Element
            var root = doc.CreateElement(string.Empty, "App", string.Empty);
            doc.AppendChild(root);

            //XmlAttribute attrs = doc.CreateAttribute("Version");
            //attrs.Value = _appVer;
            //root.SetAttributeNode(attrs);

            XmlAttribute attrs;

            // Main window
            if (App.MainWindow != null)
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
                    attrs.Value = winRestoreWidth.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("height");
                if (winState == OverlappedPresenterState.Restored)
                {
                    attrs.Value = winHeight.ToString();
                }
                else
                {
                    attrs.Value = winRestoreHeight.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("top");
                if (winState == OverlappedPresenterState.Restored)
                {
                    attrs.Value = winTop.ToString();
                }
                else
                {
                    attrs.Value = winRestoreTop.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("left");
                if (winState == OverlappedPresenterState.Restored)
                {
                    attrs.Value = winLeft.ToString();
                }
                else
                {
                    attrs.Value = winRestoreleft.ToString();
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
                mainWindow.SetAttributeNode(attrs);

                // Set main window element to root.
                root.AppendChild(mainWindow);
            }

            // Editor window
            // Editor window element
            var editWindow = doc.CreateElement(string.Empty, "EditorWindow", string.Empty);

            // Editor window attributes
            attrs = doc.CreateAttribute("width");
            attrs.Value = _viewModel.EditorWinWidth.ToString();
            editWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("height");
            attrs.Value = _viewModel.EditorWinHeight.ToString();
            editWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("top");
            attrs.Value = _viewModel.EditorWinTop.ToString();
            editWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("left");
            attrs.Value = _viewModel.EditorWinLeft.ToString();
            editWindow.SetAttributeNode(attrs);

            // Set editor window element to root.
            root.AppendChild(editWindow);

            // Options
            var xOpts = doc.CreateElement(string.Empty, "Opts", string.Empty);
            attrs = doc.CreateAttribute("IsDebugSaveLog");
            attrs.Value = isDebugSaveLog.ToString();
            xOpts.SetAttributeNode(attrs);

            root.AppendChild(xOpts);

            try
            {
                doc.Save(App.AppConfigFilePath);
            }
            //catch (System.IO.FileNotFoundException) { }
            catch (Exception ex)
            {
                Debug.WriteLine("MainWindow_Closed: " + ex + " while saving : " + App.AppConfigFilePath);
            }

            #endregion
        }

        private void LoadSetting()
        {
            System.IO.Directory.CreateDirectory(App.AppDataFolder);

            int winHeight;
            int winWidth;
            int winTop;
            int winLeft;

            bool isDebugSaveLog = true;

            if (System.IO.File.Exists(App.AppConfigFilePath))
            {
                var xdoc = XDocument.Load(App.AppConfigFilePath);
                //Debug.WriteLine(xdoc.ToString());

                // Main window
                //if (App.MainWindow != null && xdoc.Root != null)
                if (xdoc.Root != null)
                {
                    // Main window element
                    var mainWindow = xdoc.Root.Element("MainWindow");
                    if (mainWindow != null)
                    {
                        var hoge = mainWindow.Attribute("state");
                        if (hoge != null)
                        {
                            if (hoge.Value == "Maximized")
                            {
                                winState = OverlappedPresenterState.Maximized;
                            }
                            else if (hoge.Value == "Normal")
                            {
                                winState = OverlappedPresenterState.Restored;
                            }
                            else if (hoge.Value == "Minimized")
                            {
                                winState = OverlappedPresenterState.Restored;
                            }
                        }

                        hoge = mainWindow.Attribute("top");
                        if (hoge != null)
                        {
                            winTop = int.Parse(hoge.Value);
                            winRestoreTop = winTop;
                        }

                        hoge = mainWindow.Attribute("left");
                        if (hoge != null)
                        {
                            winLeft = int.Parse(hoge.Value);
                            winRestoreleft = winLeft;
                        }

                        hoge = mainWindow.Attribute("height");
                        if (hoge != null)
                        {
                            winHeight = int.Parse(hoge.Value);
                            winRestoreHeight = winHeight;
                        }

                        hoge = mainWindow.Attribute("width");
                        if (hoge != null)
                        {
                            winWidth = int.Parse(hoge.Value);
                            winRestoreWidth = winWidth;
                        }

                    }

                    // Editor window element
                    var editWindow = xdoc.Root.Element("EditorWindow");
                    if (editWindow != null)
                    {
                        var hoge = editWindow.Attribute("top");
                        if (hoge != null)
                        {
                            _viewModel.EditorWinTop = int.Parse(hoge.Value);
                        }

                        hoge = editWindow.Attribute("left");
                        if (hoge != null)
                        {
                            _viewModel.EditorWinLeft = int.Parse(hoge.Value);
                        }

                        hoge = editWindow.Attribute("height");
                        if (hoge != null)
                        {
                            _viewModel.EditorWinHeight = int.Parse(hoge.Value);
                        }

                        hoge = editWindow.Attribute("width");
                        if (hoge != null)
                        {
                            _viewModel.EditorWinWidth = int.Parse(hoge.Value);
                        }
                    }

                    // Options
                    var opts = xdoc.Root.Element("Opts");
                    if (opts != null)
                    {
                        var xvalue = opts.Attribute("IsDebugSaveLog");
                        if (xvalue != null)
                        {
                            if (!string.IsNullOrEmpty(xvalue.Value))
                            {
                                isDebugSaveLog = xvalue.Value == "True";
                            }
                        }
                    }
                }
            }

            // Apply settings

            // Options
            var app = App.Current as App;
            if (app is not null)
            {
                app.IsSaveErrorLog = isDebugSaveLog;
            }
        }
    }
}
