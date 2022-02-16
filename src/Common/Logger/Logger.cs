using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Common.Consts;

namespace Common.Logger
{
    public class Logger
    {
        public delegate void LogEvent(LogMessage lMessage);
        public event LogEvent OnLogMessageConsumed;
        public event LogEvent OnLogResponseConsumed;
        public event LogEvent OnLogErrorConsumed;
        public event LogEvent OnLogSystemInfoConsumed;

        #region Private fields
        private static Logger _instance = new Logger();
        private readonly ConcurrentQueue<LogMessage> _queue = new ConcurrentQueue<LogMessage>();
        private bool _isLoggerStarted = false;
        #endregion  Private fields

        #region Constructors
        private Logger() {}
        #endregion Constructors

        #region Public props
        public static Logger Instance => _instance;
        #endregion Public props

        #region Public methods
        public void PushMessage(LogMessage message)
        {
            if (!_isLoggerStarted)
                return;
            _queue.Enqueue(message);
        }

        public async void Start()
        {
            await Task.Run(() =>
            {
                _isLoggerStarted = true;
                while (_isLoggerStarted)
                {
                    try
                    {
                        if(_queue.Count < 0)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        LogMessage message;
                        if(!_queue.TryDequeue(out message))
                            continue;

                        switch (message.Type)
                        {
                            case LoggerMessageTypes.TextMessage: { OnLogMessageConsumed?.Invoke(message); break; }
                            case LoggerMessageTypes.RuntimeError: { OnLogErrorConsumed?.Invoke(message); break; }
                            case LoggerMessageTypes.ResponseMessage: { OnLogResponseConsumed?.Invoke(message); break; }
                            case LoggerMessageTypes.SystemInfo: { OnLogSystemInfoConsumed?.Invoke(message); break; }
                            default: throw new ArgumentOutOfRangeException($"There is no any handler for {message.Type}");
                        }
                        
                    }
                    catch(Exception e)
                    {
                        OnLogErrorConsumed?.Invoke(new LogMessage
                        {
                            Type = LoggerMessageTypes.RuntimeError,
                            Message = e.Message
                        }); ;
                    }
                }
                _queue.Clear();
            });
        }

        public void Stop()
        {
            _isLoggerStarted = false;
        }
        #endregion Public methods


        #region Private methods
        #endregion Private methods
    }
}
