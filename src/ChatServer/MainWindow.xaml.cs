using System;
using System.Net;
using System.Windows;
using ChatServer.Tcp;
using ChatServer.UserStorage;
using Common.Interfaces;
using Common.Logger;
using Common.Tcp;
using Common.Consts;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using Common.Helpers;
using System.Text;

namespace ChatServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string DefaultPort = "8001";

        private UserStorage.UserStorage _storage = new UserStorage.UserStorage();
        private UserStorage.UserStorage.UserTimeoutWatchdog _watchDog;
        private TcpServer _tcpServer = null;
        private readonly List<NetworkInterfaceView> _interfaces = new List<NetworkInterfaceView>();
        private readonly FileWriterHelper _systemFwHelper = new FileWriterHelper($"system_{DateTime.Now.ToString("dd-MM-yyyy")}.log");
        private readonly FileWriterHelper _messageFwHelper = new FileWriterHelper($"messages_{DateTime.Now.ToString("dd - MM - yyyy")}.log");

        private int WatchDogTimeout { 
            get
            {
                return (int)cmbxTimeout.SelectedItem;
            }
        }

        public MainWindow()
        {
            _watchDog = new UserStorage.UserStorage.UserTimeoutWatchdog(_storage);
            InitializeComponent();
            cmbxTimeout.ItemsSource = new [] { 10, 60, 120, 300 };
            cmbxTimeout.SelectedItem = 60;
            txtbPort.Text = DefaultPort;
            btnStopServer.IsEnabled = false;
            this.ResizeMode = ResizeMode.NoResize;
            _storage.OnUserRegistered += OnUserRegisteredHandler;
            _storage.OnUserUnregistered += OnUserUnregisteredHandler;
            Logger.Instance.OnLogErrorConsumed += OnLoggerItemConsumedHandler;
            Logger.Instance.OnLogErrorConsumed += SystemLogHandler;
            Logger.Instance.OnLogMessageConsumed+= OnLoggerItemConsumedHandler;
            Logger.Instance.OnLogMessageConsumed+= MessageLogHandler;
            Logger.Instance.OnLogResponseConsumed += OnLoggerItemConsumedHandler;
            Logger.Instance.OnLogSystemInfoConsumed += OnLoggerItemConsumedHandler;
            Logger.Instance.OnLogSystemInfoConsumed += SystemLogHandler;
            Logger.Instance.Start();
            
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                   ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                   ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    var niv = new NetworkInterfaceView(ni);
                    _interfaces.Add(niv);
                    cmbxHost.Items.Add(niv);
                }
            }
            var nivAny = new NetworkInterfaceView("Any", IPAddress.Any);
            _interfaces.Add(nivAny);
            cmbxHost.Items.Add(nivAny);
            cmbxHost.SelectedIndex = cmbxHost.Items.Count - 1;
        }

        private void ManageControlsOnStart()
        {
            btnStartServer.IsEnabled = false;
            cmbxTimeout.IsEnabled = false;
            cmbxHost.IsEnabled = false;
            chckAllowDupNames.IsEnabled = false;
            txtbPort.IsEnabled = false;
            btnStopServer.IsEnabled = true;
        }

        private void ManageControlsOnStop()
        {
            btnStartServer.IsEnabled = true;
            cmbxTimeout.IsEnabled = true;
            cmbxHost.IsEnabled = true;
            chckAllowDupNames.IsEnabled = true;
            txtbPort.IsEnabled = true;
            btnStopServer.IsEnabled = false;
        }

        private void StartServer()
        {
            
            string host;
            ManageControlsOnStart();
            try
            {
                host = _interfaces[cmbxHost.SelectedIndex].Address.ToString();
            }
            catch (Exception)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = "Host should be correct"
                });
                ManageControlsOnStop();
                return;
            }

            int port;
            try
            {
                port = Int32.Parse(txtbPort.Text);
            }
            catch (Exception)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = "Port should be correct"
                });
                ManageControlsOnStop();
                return;
            }

            try
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.SystemInfo,
                    Message = "Server started"
                });
                if (null != _tcpServer && _tcpServer.IsStarted)
                    _tcpServer.Stop();
                _tcpServer = new TcpServer(host, port);
                _tcpServer.OnClientConnected += OnTcpClientConnectedHandler;
                _tcpServer.OnServerStartError += OnServerStartErrorHandler;
                _storage.UniqueById = chckAllowDupNames.IsChecked.Value;
                _tcpServer.Start();
                _watchDog.Start(WatchDogTimeout);
            }
            catch (Exception e)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = e.Message
                });
                ManageControlsOnStop();
            }
        }

        private void OnServerStartErrorHandler(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    StopServer();
                });

            }
            catch(Exception ex)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = ex.Message
                });
            }
       }

        private void StopServer()
        {
            try
            {
                ManageControlsOnStop();
                _tcpServer?.Stop();
                _watchDog?.Stop();
                _storage.FlushUsers();
                ClearUserlist();
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.SystemInfo,
                    Message = "Server stopped"
                });
            }
            catch(Exception e)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = e.Message
                });
            }
            
        }

        private void OnTcpClientConnectedHandler(object sender, EventArgs e)
        {
            if (e is ClientConnectedEventArgs connectedArgs)
            {
                connectedArgs.Client.OnMessageReceived += OnMessageReceivedHandler;
                connectedArgs.Client.Start();
            }
        }

        private void OnMessageReceivedHandler(object tcpClient, EventArgs e)
        {
            if (e is MessageReceivedArgs messageArgs)
                try
                {
                    HandleMessage(tcpClient as TcpClientWorker, messageArgs.Message);
                }
                catch(Exception exc)
                {
                    Logger.Instance.PushMessage(new LogMessage
                    {
                        Type = LoggerMessageTypes.RuntimeError,
                        Message = exc.Message
                    });
                }
        }

        private void HandleMessage(TcpClientWorker client, IMessage message)
        {
            MessageProcessor.ProcessMessage(message, _storage, client);
        }

        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
        }

        private void btnStopServer_Click(object sender, RoutedEventArgs e)
        {
            StopServer();
        }

        private void OnLoggerItemConsumedHandler(LogMessage message)
        {
            try
            {
                Dispatcher.Invoke(() => lstbLogInfo.Items.Add($"{message.Time} {Enum.GetName(typeof(LoggerMessageTypes), message.Type)}: {message.Message}"));

            }
            catch (Exception ex)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = ex.Message
                });
            }
        }

        internal void OnUserRegisteredHandler(string userInfo)
        {
            try
            {
                Dispatcher.Invoke(() => lstbUsers.Items.Add(userInfo));
            }
            catch (Exception ex)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = ex.Message
                });
            }
        }
        internal void OnUserUnregisteredHandler(string userInfo)
        {
            try
            {
                Dispatcher.Invoke(() => lstbUsers.Items.Remove(userInfo));
            }
            catch (Exception e)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = e.Message
                });
            }
        }

        private void ClearUserlist()
        {
            lstbUsers.Items.Clear();
        }

        private void SystemLogHandler(LogMessage message)
        {
            try
            {
                _systemFwHelper.WriteText($"{message.Time} {message.Message}\n");
            }
            catch (Exception e)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = e.Message
                });
            }
        }

        private void MessageLogHandler(LogMessage message)
        {
            try
            {
                _messageFwHelper.WriteText($"{message.Time} {message.Message}\n");
            }
            catch (Exception e)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = e.Message
                });
            }
        }
    }
}
