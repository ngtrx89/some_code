using Common.Interfaces;
using System;

namespace Common.Models
{
    public class User : IUser
    {
        public string Name { get; set; }

        public Guid Id { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
