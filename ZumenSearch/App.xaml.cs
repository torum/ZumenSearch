using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Text;
using System.IO;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Models.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;

namespace ZumenSearch
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

        public App()
        {
            // 未処理例外の処理
            // UI スレッドで実行されているコードで処理されなかったら発生する（.NET 3.0 より）
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            // バックグラウンドタスク内で処理されなかったら発生する（.NET 4.0 より）
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            // 例外が処理されなかったら発生する（.NET 1.0 より）
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        #region == エラーログ関連 ==

        /// <summary>
        /// UI スレッドで発生した未処理例外を処理します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception as Exception;
            if (ConfirmUnhandledException(exception, "UI スレッド"))
            {
                e.Handled = true;
            }
            else
            {
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// バックグラウンドタスクで発生した未処理例外を処理します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var exception = e.Exception.InnerException as Exception;
            if (ConfirmUnhandledException(exception, "バックグラウンドタスク"))
            {
                e.SetObserved();
            }
            else
            {
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 実行を継続するかどうかを選択できる場合の未処理例外を処理します。
        /// </summary>
        /// <param name="e">例外オブジェクト</param>
        /// <param name="sourceName">発生したスレッドの種別を示す文字列</param>
        /// <returns>継続することが選択された場合は true, それ以外は false</returns>
        bool ConfirmUnhandledException(Exception e, string sourceName)
        {
            var message = $"予期せぬエラーが発生しました。続けて発生する場合は開発者に報告してください。\nプログラムの実行を継続しますか？";
            if (e != null) message += $"\n({e.Message} @ {e.TargetSite.Name})";

            // Logger.Fatal($"未処理例外 ({sourceName})", e); // 適当なログ記録
            SaveErrorLogs(message);

            var result = MessageBox.Show(message, $"未処理例外 ({sourceName})", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// 最終的に処理されなかった未処理例外を処理します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            var message = $"予期せぬエラーが発生しました。";
            if (exception != null) message += $"\n({exception.Message} @ {exception.TargetSite.Name})";

            // Logger.Fatal("未処理例外", exception); // 適当なログ記録
            SaveErrorLogs(message);

            MessageBox.Show(message, "未処理例外", MessageBoxButton.OK, MessageBoxImage.Stop);
            Environment.Exit(1);
        }

        private void SaveErrorLogs(string eText)
        {
            var logFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + "ZumenSearch_Errors.txt";
            var txt = new StringBuilder();
            txt.AppendLine(eText);

            DateTime dt = DateTime.Now;
            string nowString = dt.ToString("yyyy/MM/dd HH:mm:ss");
            txt.AppendLine(nowString);

            File.WriteAllText(logFilePath, txt.ToString());

        }

        #endregion

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

        #region == チャイルドウィンドウ管理 ==

        // Windowの一覧を保持 
        public List<Window> WindowList = new List<Window>();

        // Windowを閉じた時にWindowListからWindowを削除する
        public void RemoveEditWindow(Window editor)
        {
            WindowList.Remove(editor);
        }

        #endregion

    }

}
