using Common.Interfaces;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Common.JsonConverters;
using Common.Consts;

namespace Common.Models
{
    public class Message: IMessage
    {
        //Info in production all string codes should be replaced with byte codes to reduce size of message
        [JsonConverter(typeof(ConcreteTypeConverter<User>))]
        public IUser From { get; set; }
        [JsonConverter(typeof(ConcreteTypeConverter<User>))]
        public IUser To { get; set; }
        public virtual Guid MessageId { get; set; }
        public Dictionary<string, object> Payload { get; internal set; }
        public Dictionary<string, string> Meta { get; internal set; }
        public Message()
        {
            Meta = new Dictionary<string, string>();
            Payload = new Dictionary<string, object>();
        }
    }
}
