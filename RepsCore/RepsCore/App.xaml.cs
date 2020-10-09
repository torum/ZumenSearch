using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using RepsCore.Views;

namespace RepsCore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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

        /// <summary> Check and bring to front if already exists.</summary>
        private void AppOnStartup(object sender, StartupEventArgs e)
        {
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
        }
    }

}
