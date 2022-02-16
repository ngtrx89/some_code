using Common.Interfaces;
using Common.Models;
using Common.Consts;
using Common.Tcp;
using Common.Repository.Messages;
using Common.Logger;
using System;
using System.Linq;

namespace ChatServer.Tcp
{
    internal static class MessageProcessor
    {
        #region Private fields
        #endregion  Private fields

        #region Constructors
        #endregion Constructors

        #region Public props
        #endregion Public props

        #region Public methods
        public static void ProcessMessage(IMessage msg, UserStorage.UserStorage storage, TcpClientWorker client)
        {
            if (null == storage)
                return;
            Message response;

            if (msg is Message message)
                try
                {
                    switch (message.Meta[MessageKeys.Meta.Handler])
                    {
                        case MessageHandlerKeys.Register:
                            {
                                storage.RegisterUser(new Models.UserContainter(message.From, client));
                                response = MessageRepository.GetMessage(ERepositoryMessagesSet.OkResponse) as Message;
                                response.MessageId = message.MessageId;
                                client.SendMessage(response);
                                break;
                            }
                        case MessageHandlerKeys.UserList:
                            {
                                const int UserCountInTheChunk = 5;
                                
                                IUser[] headSlice;
                                IUser[] tailSlice = storage.GetRegisteredUsers().ToArray();
                                var chunkCount = (long)Math.Ceiling(((float)tailSlice.Length) / UserCountInTheChunk);
                                var counter = 1;

                                response = MessageRepository.GetMessage(ERepositoryMessagesSet.UserListResponse) as Message;
                                response.MessageId = message.MessageId;
                                response.Payload.Add(MessageKeys.Payload.UserListChunksCount, chunkCount);

                                client.SendMessage(response);

                                while(tailSlice.Length > 0)
                                {
                                    if(tailSlice.Length < UserCountInTheChunk)
                                    {
                                        headSlice = tailSlice;
                                        tailSlice = new IUser[0];
                                    }
                                    else
                                    {
                                        headSlice = tailSlice[..UserCountInTheChunk];
                                        tailSlice = tailSlice[UserCountInTheChunk..];
                                    }
                                    response = MessageRepository.GetMessage(ERepositoryMessagesSet.UserListResponse) as Message;
                                    response.MessageId = message.MessageId;
                                    response.Payload.Add(MessageKeys.Payload.ChunkNumber, counter++);
                                    response.Payload.Add(MessageKeys.Payload.ChunkData, headSlice);
                                    client.SendMessage(response);
                                }
                                client.SendMessage(response);
                                break;
                            }
                        case MessageHandlerKeys.TextMessage:
                            {
                                Logger.Instance.PushMessage(new LogMessage
                                {
                                    Type = LoggerMessageTypes.TextMessage,
                                    Message = $"\"{message.From}\" \"{message.Payload[MessageKeys.Payload.Message]}\""
                                });
                                storage.SendMessageToUser(message);
                                response = MessageRepository.GetMessage(ERepositoryMessagesSet.OkResponse) as Message;
                                response.MessageId = message.MessageId;
                                client.SendMessage(response);
                                break;
                            }
                        case MessageHandlerKeys.Unregister:
                            {
                                storage.UnregisterUser(message.From.Name, message.From.Id);
                                break;
                            }
                        case MessageHandlerKeys.HardBit:
                            {
                                break;
                            }
                        default:
                            {
                                response = MessageRepository.GetMessage(ERepositoryMessagesSet.NotValidMessageHandler) as Message;
                                response.MessageId = message.MessageId;
                                client.SendMessage(response);
                                break;
                            }
                    }
                }
                catch (Exception e) {
                    response = MessageRepository.GetMessage(ERepositoryMessagesSet.ErrorResponse) as Message;
                    response.MessageId = message.MessageId;
                    response.Payload[MessageKeys.Payload.Message] = e.Message;
                    client?.SendMessage(response);
                }


        }
        #endregion Public methods


        #region Private methods
        #endregion Private methods
    }
}
