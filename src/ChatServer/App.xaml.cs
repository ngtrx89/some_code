using Common.Logger;
using System.Windows;
using System.Windows.Threading;

namespace ChatServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += OnUnhendledExceptionRised;
        }
        private void OnUnhendledExceptionRised(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Instance.PushMessage(new LogMessage
            {
                Type = Common.Consts.LoggerMessageTypes.RuntimeError,
                Message = e.Exception.Message
            }); ;
            e.Handled = true;
        }
    }
}
