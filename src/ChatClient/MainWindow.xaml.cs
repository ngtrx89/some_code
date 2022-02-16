using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Common.Interfaces;
using Common.Helpers;
using Common.Models;
using Common.Tcp;
using Common.Repository.Messages;
using Common.Consts;
using ChatClient.Tcp.OutcomeReqeuestProcessor;
using ChatClient.PeerProcessing;
using System.Windows.Controls;
using System.Collections.Generic;
using Common.Logger;
using System.Windows.Threading;

namespace ChatClient
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string DefaultHost = "127.0.0.1";
        public const string DefaultPort = "8001";
        private TcpClientWorker _tcpWorker;
        private IUser _user;
        private readonly RequestProcessContainerStorage _outRequestStorage = new RequestProcessContainerStorage();
        private readonly CacheMessageStorage _messageStorage = new CacheMessageStorage();
        private readonly List<IUser> _chachedUsers = new List<IUser>();
        private IUser _receiver = null;
        public MainWindow()
        {
            _user = new User()
            {
                Id = GuidHelper.GenerateTimeBasedGuid(DateTime.Now)
            };
            InitializeComponent();
            txtbHost.Text = DefaultHost;
            txtbPort.Text = DefaultPort;
            lstbUserList.SelectionChanged += lstbUserList_OnSelected;
            txtSendinput.KeyUp += txtSendinput_KeyUp;
            ManageControlsOnStop();
            this.ResizeMode = ResizeMode.NoResize;
            txtName.MaxLength = 20;
            Logger.Instance.OnLogErrorConsumed += LogEventHandler;
            Logger.Instance.OnLogSystemInfoConsumed += LogEventHandler;
            Logger.Instance.OnLogResponseConsumed += LogEventHandler;
            Logger.Instance.Start();
        }

        
        private void ManageControlsOnStart()
        {
            btnConnect.IsEnabled = false;
            txtbHost.IsEnabled = false;
            txtbPort.IsEnabled = false;
            txtName.IsEnabled = false;
            btnRefreshUserList.IsEnabled = true;
            txtSendinput.IsEnabled = true;
            btnSend.IsEnabled = true;
            btnDisconnect.IsEnabled = true;
        }

        private void ManageControlsOnStop()
        {
            btnConnect.IsEnabled = true;
            txtbHost.IsEnabled = true;
            txtbPort.IsEnabled = true;
            txtName.IsEnabled = true;
            btnRefreshUserList.IsEnabled = false;
            txtSendinput.IsEnabled = false;
            btnSend.IsEnabled = false;
            btnDisconnect.IsEnabled = false;
            lstbUserList.Items.Clear();
        }
        private void Start()
        {
            ManageControlsOnStart();
            string host;
            try
            {
                host = IPAddress.Parse(txtbHost.Text).ToString();
            }
            catch (FormatException)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.SystemInfo,
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
            catch (FormatException)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.SystemInfo,
                    Message = "Port should be correct"
                });
                ManageControlsOnStop();
                return;
            }

            if(txtName.Text.Trim().Length == 0)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.SystemInfo,
                    Message = "Name should be entered(whitespaces doesn't count)"
                });
                ManageControlsOnStop();
                return;
            }

            try
            {
                var client = new TcpClient();
                client.Connect(host, port);
                _tcpWorker = new TcpClientWorker(client);
                _tcpWorker.OnMessageReceived += OnMessageReceivedHandler;
                _tcpWorker.OnConnectionClosed += OnConnectionClosedHandler;
                _tcpWorker.Start();
                var registerMessage = MessageRepository.GetMessage(ERepositoryMessagesSet.Register);
                _user.Name = txtName.Text;
                registerMessage.From = _user;
                _outRequestStorage.CreateNewRequestProcessor(registerMessage as MessageRequest, RegisterSuccessHandler, RegisterErrorHandler);
                _tcpWorker.SendMessage(registerMessage);
            }
            catch(Exception e)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = e.Message
                });
                ManageControlsOnStop();
            }
        }

        private void RegisterSuccessHandler(object sender, EventArgs e)
        {
            try
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Message = "Registration success",
                    Type = LoggerMessageTypes.ResponseMessage
                });
            }
            catch (Exception ex)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Message = ex.Message,
                    Type = LoggerMessageTypes.RuntimeError
                });
            }
        }

        private void RegisterErrorHandler(object sender, EventArgs e)
        {
            if (e is ErrorMessageArgs errArgs)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Message = errArgs.ErrorMessage,
                    Type = LoggerMessageTypes.ResponseMessage
                });
                Dispatcher.Invoke(() => Stop(false));
            }
                
        }

        private void OnConnectionClosedHandler(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.SystemInfo,
                    Message = "Connection closed"
                });
                ManageControlsOnStop();
            });
        }
        private void Stop(bool polite = true)
        {
            try
            {
                if(polite)
                {
                    var msg = MessageRepository.GetMessage(ERepositoryMessagesSet.Unregister);
                    msg.From = _user;
                    _tcpWorker.SendMessage(msg);
                }
                _tcpWorker.Stop();
            }catch(Exception e)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = e.Message
                });
            }
            finally
            {
                ManageControlsOnStop();
            }

        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void btnRefreshUserList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnRefreshUserList.IsEnabled = false;
                var request = MessageRepository.GetMessage(ERepositoryMessagesSet.UserListRequest);
                request.From = _user;
                _outRequestStorage.CreateNewRequestProcessor(request as MessageRequest, RefreshUserListSuccessHadler, RefreshUserListErrorHadler);
                _tcpWorker.SendMessage(request);
            }
            catch(Exception ex)
            {
                btnRefreshUserList.IsEnabled = true;
                Logger.Instance.PushMessage(new LogMessage
                {
                    Message = ex.Message,
                    Type = LoggerMessageTypes.RuntimeError
                });
            }
            
        }

        private void OnMessageReceivedHandler(object sender, EventArgs e)
        {
            if (e is MessageReceivedArgs messageArgs)
                if (messageArgs.Message.Meta[MessageKeys.Meta.Type] == MessageType.Response)
                    _outRequestStorage.PushResponseMessage(messageArgs.Message as Message);
                else if (messageArgs.Message.Meta[MessageKeys.Meta.Handler] == MessageHandlerKeys.TextMessage)
                    Dispatcher.Invoke(() =>
                    {
                        _messageStorage.PushMessageFrom(messageArgs.Message);
                        if(null != _receiver && _receiver.Id == messageArgs.Message.From.Id)
                            lstbMessages.Items.Add($"{DateTime.Now} {messageArgs.Message.From}: {messageArgs.Message.Payload[MessageKeys.Payload.Message]}");
                    });
        }

        private void RefreshUserListSuccessHadler(object sender, EventArgs e)
        {
            if (e is UserListEventArgs lstArgs)
                Dispatcher.Invoke(() =>
               {
                   lstbUserList.Items.Clear();
                   _chachedUsers.Clear();
                   _chachedUsers.AddRange(lstArgs.UserList);
                   foreach (var user in lstArgs.UserList)
#if DEBUG
                        lstbUserList.Items.Add($"{user.Name} {user.Id}");
#else
                        lstbUserList.Items.Add($"{user.Name}");
#endif
                   btnRefreshUserList.IsEnabled = true;
               });
        }

        private void RefreshUserListErrorHadler(object sender, EventArgs e)
        {
            if (e is ErrorMessageArgs errArgs)
                Logger.Instance.PushMessage(new LogMessage
                {
                    Message = errArgs.ErrorMessage,
                    Type = LoggerMessageTypes.ResponseMessage
                });
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }
        private void SendMessage()
        {
            try
            {
                btnSend.IsEnabled = false;
                txtSendinput.IsEnabled = false;
                if (null == _receiver)
                {
                    Logger.Instance.PushMessage(new LogMessage
                    {
                        Type = LoggerMessageTypes.SystemInfo,
                        Message = "User should be chosen before send message"
                    });
                    return;
                }
                if(txtSendinput.Text.Trim().Length == 0)
                {
                    Logger.Instance.PushMessage(new LogMessage
                    {
                        Type = LoggerMessageTypes.SystemInfo,
                        Message = "Message should contains some characters(whitespace doesn't count)"
                    });
                    return;
                }

                var msg = MessageRepository.GetMessage(ERepositoryMessagesSet.Text);
                msg.From = _user;
                msg.To = _receiver;
                msg.Payload[MessageKeys.Payload.Message] = txtSendinput.Text;
                _outRequestStorage.CreateNewRequestProcessor(msg as MessageRequest, OnTextMessageSuccessHandler, OnTextMessageErrorHandler);
                _tcpWorker.SendMessage(msg);
                
                
            }
            catch(Exception exc)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = exc.Message
                });
            }
            finally
            {
                btnSend.IsEnabled = true;
                txtSendinput.IsEnabled = true;
            }
        }

        private void OnTextMessageSuccessHandler(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    txtSendinput.Text = "";
                    btnSend.IsEnabled = true;
                    if (e is TextmessageEventArgs msg)
                    {
                        _messageStorage.PushMessageTo(msg.Message);
                        lstbMessages.Items.Add($"{DateTime.Now} {msg.Message.From}: {msg.Message.Payload[MessageKeys.Payload.Message]}");
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = exc.Message
                });
            }
        }

        private void OnTextMessageErrorHandler(object sender, EventArgs e)
        {
            try
            {
                if (e is ErrorMessageArgs eArgs)
                    Logger.Instance.PushMessage(new LogMessage
                    {
                        Type = LoggerMessageTypes.ResponseMessage,
                        Message = eArgs.ErrorMessage
                    });
                Dispatcher.Invoke(() =>
                {
                    btnSend.IsEnabled = true;
                    
                });
            }
            catch (Exception exc)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = exc.Message
                });
            }
        }

        private void lstbUserList_OnSelected(object sender, EventArgs e)
        {
            if (sender is ListBox userListBoxt)
            {
                if (userListBoxt.SelectedIndex < 0)
                    return;
                _receiver = _chachedUsers[userListBoxt.SelectedIndex];
                lblChatWithUser.Content = $"Chat: {_receiver}";
                lstbMessages.Items.Clear();
                foreach(var cachedMsg in _messageStorage.GetMessages(_receiver.Id))
                {
                    lstbMessages.Items.Add(cachedMsg);
                }
            }
        }

        private void txtSendinput_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) 
                return;
            SendMessage();
            e.Handled = true;
        }

        private void LogEventHandler(LogMessage message)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{message.Time}\n{message.Type}\n{message.Message}");
            });
        }
    }
}
