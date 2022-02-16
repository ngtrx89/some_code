using System;
using System.Collections.Generic;
using System.Linq;
using Common.Interfaces;
using ChatServer.Models;
using Common.Models;

namespace ChatServer.UserStorage
{
    internal partial class UserStorage
    {
        public delegate void UserRegistrationAction(string userInfo);
        public event UserRegistrationAction OnUserRegistered;
        public event UserRegistrationAction OnUserUnregistered;
        public readonly object _listLock = new object();

        #region Private fields
        private readonly List<UserContainter> _userList = new List<UserContainter>();
        private bool _uniqueById = false;
        #endregion  Private fields
        #region Constructors

        public UserStorage() {
        }

        #endregion Constructors

        #region Public props
        public bool UniqueById { get => _uniqueById; set => _uniqueById = value; }
        #endregion Public props


        #region Public methods
        public void RegisterUser(UserContainter user)
        {
            lock (_listLock)
            {
                if (IsUserRegiseterd(user))
                    throw new ArgumentException("User already registered");
                user.OnConnectionClosed += OnClientClosedConnectionHandler;
                _userList.Add(user);
                OnUserRegistered?.Invoke($"{user.Name} {user.Id}");
            }
        }
        public void UnregisterUser(UserContainter user)
        {
            lock (_listLock)
                if (IsUserRegiseterd(user))
                {
                    _userList.Remove(user);
                    user.StopTcpWorker();
                    OnUserUnregistered?.Invoke($"{user.Name} {user.Id}");
                }
        }

        public void UnregisterUser(string name, Guid userId) {
            try
            {
                UnregisterUser((from user in _userList where user.Id == userId && user.Name == name select user).First());
            }
            catch (ArgumentException)
            {
            }
        }

        public void SendMessageToUser(IMessage message)
        {
            lock (_listLock)
            {
                if (IsUserRegiseterd(message.To.Id))
                    _userList.Find(u => message.To.Id == u.Id).SendMessage(message);
                else
                    throw new Exception($"{message.To} not connected");
            }
                
        }

        public bool IsUserRegiseterd(UserContainter user)
        {
            if (UniqueById)
                return _userList.Count(u => u.Id == user.Id) > 0;
            else 
                return _userList.Count(u => u.Name == user.Name) > 0;

        }
        public bool IsUserRegiseterd(Guid userGuid) {
            try
            {
                var user = _userList.Find(u => userGuid == u.Id);
                return null != user;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void FlushUsers()
        {
            lock (_userList)
                _userList.Clear();
        }

        public IEnumerable<IUser> GetRegisteredUsers()
        {
            lock (_listLock)
                return from uc in _userList select new User { Id = uc.Id, Name = uc.Name };
        }

        ~UserStorage()
        {
            FlushUsers();
        }
        #endregion Public methods


        #region Private methods
        private void OnClientClosedConnectionHandler(object sender, EventArgs e)
        {
            if (sender is UserContainter user)
                if(IsUserRegiseterd(user))
                    UnregisterUser(user);
        }
        #endregion Private methods
    }
}
