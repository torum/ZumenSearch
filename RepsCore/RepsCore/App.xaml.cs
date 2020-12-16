using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using RepsCore.Views;
using RepsCore.ViewModels;
using RepsCore.ViewModels.Classes;

namespace RepsCore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            // テスト用
            //ChangeTheme("DefaultTheme");
            //ChangeTheme("LightTheme");
            //ChangeTheme("DarkTheme");

            #region == 二重起動防止 ==

            if (_mutexOn)
            {
                this.mutex = new Mutex(true, UniqueMutexName, out bool isOwned);
                this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

                // So, R# would not give a warning that this variable is not used.
                GC.KeepAlive(this.mutex);

                if (isOwned)
                {
                    // Spawn a thread which will be waiting for our event
                    var thread = new Thread(
                        () =>
                        {
                            while (this.eventWaitHandle.WaitOne())
                            {
                                Current.Dispatcher.BeginInvoke(
                                    (Action)(() => ((MainWindow)Current.MainWindow).BringToForeground()));
                            }
                        });

                    // It is important mark it as background otherwise it will prevent app from exiting.
                    thread.IsBackground = true;

                    thread.Start();
                    return;
                }

                // Notify other instance so it could bring itself to foreground.
                this.eventWaitHandle.Set();

                // Terminate this instance.
                this.Shutdown();

            }

            #endregion
        }

        #region == 二重起動防止 ==
        /// <summary> Check and bring to front if already exists.</summary>
        /// 
        // 二重起動防止 on/off
        private bool _mutexOn = true;

        /// <summary>The event mutex name.</summary>
        private const string UniqueEventName = "{e9bf8024-c2a4-4513-87b8-d7537bffd6a5}";

        /// <summary>The unique mutex name.</summary>
        private const string UniqueMutexName = "{dc7b6684-5244-49df-8ca5-8c903579a3b8}";

        /// <summary>The event wait handle.</summary>
        private EventWaitHandle eventWaitHandle;

        /// <summary>The mutex.</summary>
        private Mutex mutex;

        #endregion

        #region == テーマ切り替え ==

        public void ChangeTheme(string themeName)
        {
            ResourceDictionary _themeDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x.Source == new Uri("pack://application:,,,/Themes/DefaultTheme.xaml"));
            if (_themeDict != null)
            {
                _themeDict.Clear();
            }
            else
            {
                // 新しいリソース・ディクショナリを追加
                _themeDict = new ResourceDictionary();
                Application.Current.Resources.MergedDictionaries.Add(_themeDict);
            }

            // テーマをリソース・ディクショナリのソースに指定
            string themeUri = String.Format("pack://application:,,,/Themes/{0}.xaml", themeName);
            _themeDict.Source = new Uri(themeUri);
        }

        #endregion

        #region == ウィンドウ関連 ==

        /// <summary> Hold a list of windows here.</summary>
        public List<Window> WindowList = new List<Window>();

        /// <summary> Create or BringToFront an Editor Window.</summary>
        public void CreateOrBringToFrontEditorWindow(BlogEntryEventArgs arg)
        {
            if (arg == null)
                return;

            if (String.IsNullOrEmpty(arg.Id))
                return;

            string id = arg.Id;

            App app = App.Current as App;

            // The key is to use app.WindowList here.
            //foreach (var w in app.Windows)
            foreach (var w in app.WindowList)
            {
                if (!(w is RentBuilding))
                    continue;

                if ((w as RentBuilding).DataContext == null)
                    continue;

                if (!((w as RentBuilding).DataContext is BuildingViewModel))
                    continue;

                if (id == ((w as RentBuilding).DataContext as BuildingViewModel).Id)
                {
                    //w.Activate();

                    if ((w as RentBuilding).WindowState == WindowState.Minimized || (w as Window).Visibility == Visibility.Hidden)
                    {
                        //w.Show();
                        (w as RentBuilding).Visibility = Visibility.Visible;
                        (w as RentBuilding).WindowState = WindowState.Normal;
                    }

                    (w as RentBuilding).Activate();
                    //(w as EditorWindow).Topmost = true;
                    //(w as EditorWindow).Topmost = false;
                    (w as RentBuilding).Focus();

                    return;
                }
            }
            
            var win = new RentBuilding
            {
                DataContext = new BuildingViewModel(id)
            };

            app.WindowList.Add(win);

            // We can't use Show() or set win.Owner = this. 
            // Try minimized and resotre a child window then close it. An owner window minimizes itself.
            //win.Owner = this;
            //win.Show();
            win.ShowInTaskbar = true;
            win.ShowActivated = true;
            win.Visibility = Visibility.Visible;
            win.Activate();
            
        }

        public void RemoveEditorWindow(RentBuilding editor)
        {
            WindowList.Remove(editor);
        }


        #endregion

    }

}
