using System;
using System.Collections.Generic;
using System.Text;
using Common.Models;
using Common.Consts;

namespace Common.Repository.Messages
{
    class ErrorResponse : MessageResponse
    {
        public ErrorResponse()
        {
            Meta[MessageKeys.Meta.Status] = MessageStatus.Error;
        }
    }
}
