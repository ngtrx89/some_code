using ChatServer.Models;
using Common.Logger;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer.UserStorage
{
    partial class UserStorage
    {
        internal class UserTimeoutWatchdog
        {
            public const int DefaultTimeOut = 300;//seconds

            public event EventHandler OnUserUnregistered;

            #region Private fields
            private readonly UserStorage _storage;
            private bool _isWatchDogStarted = false;
            #endregion  Private fields

            #region Constructors
            public UserTimeoutWatchdog(UserStorage storage)
            {
                _storage = storage ?? throw new NullReferenceException("Storage should be an object, not null");
            }
            #endregion Constructors

            #region Public props
            public int TimeOutSeconds { get; private set; } = DefaultTimeOut;
            #endregion Public props

            #region Public methods
            /// <summary>
            /// 
            /// </summary>
            /// <param name="timeoutSeconds">Sleep time for watchdog in seconds</param>
            public async void Start(int timeoutSeconds)
            {
                TimeOutSeconds = timeoutSeconds;
                _isWatchDogStarted = true;

                await Task.Run(() =>
                {
                    var timeout = TimeOutSeconds * 10;// think as .01 of TimeOut is good for polling sleep time
                    while (_isWatchDogStarted)
                    {
                        try
                        {
                            _storage?._userList?.AsParallel().ForAll(userContainer =>
                            {
                                try
                                {
                                    if (IsUserOutOfTime(userContainer))
                                    {
                                        _storage?.UnregisterUser(userContainer);
                                        OnUserUnregistered?.Invoke(null, new UserUnregisteredArgs(userContainer.Name, userContainer.Id));
                                    }
                                        
                                }
                                catch(Exception e)
                                {
                                    Logger.Instance.PushMessage(new LogMessage
                                    {
                                        Type = Common.Consts.LoggerMessageTypes.RuntimeError,
                                        Message = e.Message
                                    });
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.PushMessage(new LogMessage
                            {
                                Type = Common.Consts.LoggerMessageTypes.RuntimeError,
                                Message = e.Message
                            });
                        }
                        finally
                        {
                            Thread.Sleep(timeout);
                        }
                    }
                });
            }
            public void Stop()
            {
                _isWatchDogStarted = false;
            }
            #endregion Public methods


            #region Private methods
            private bool IsUserOutOfTime(UserContainter user)
            {
                if (null == user)
                    return false;
                return (DateTime.Now - user.LastActivityTime).TotalSeconds > TimeOutSeconds;
                
            }
            #endregion Private methods
        }
    }
}
