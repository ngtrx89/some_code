using System;

namespace Common.Interfaces
{
    public interface IUser
    {
        string Name { get; set; }
        Guid Id { get; set; }
    }
}
