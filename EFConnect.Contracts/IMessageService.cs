using EFConnect.Data.Entities;
using EFConnect.Models;
using EFConnect.Models.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Contracts
{
    public interface IMessageService
    {
        Task<Message> GetMessage(int id);
        Task<PagedList<MessageToReturn>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageToReturn>> GetMessageThread(int userId, int recipientId);
        Task<MessageToReturn> CreateMessage(MessageForCreation messageForCreation);
        Task<bool> SaveAll();
        Task<bool> MarkMessageAsRead(Message message);
    }
}
