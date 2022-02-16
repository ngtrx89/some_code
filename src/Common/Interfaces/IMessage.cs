using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface IMessage
    {
        Dictionary<string, string> Meta { get; }
        IUser From { get; set; }
        IUser To { get; set; }
        Dictionary<string, object> Payload { get; }
    }
}
