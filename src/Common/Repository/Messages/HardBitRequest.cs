using Common.Consts;
using Common.Models;

namespace Common.Repository.Messages
{
    class HardBitRequest: MessageRequest
    {
        public HardBitRequest()
        {
            Meta[MessageKeys.Meta.Handler] = MessageHandlerKeys.HardBit;
        }
    }
}
