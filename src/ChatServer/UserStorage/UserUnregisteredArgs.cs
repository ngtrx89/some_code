using System;

namespace ChatServer.UserStorage
{
    internal class UserUnregisteredArgs: EventArgs
    {
        public string Name { get; }
        public Guid Id { get; }
        public UserUnregisteredArgs(string name, Guid id)
        {
            Name = name;
            Id = id;
        }
    }
}
