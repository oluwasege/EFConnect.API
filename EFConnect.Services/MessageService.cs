using EFConnect.Contracts;
using EFConnect.Data;
using EFConnect.Data.Entities;
using EFConnect.Models;
using EFConnect.Models.Message;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Services
{
    public class MessageService : IMessageService
    {
        private readonly EFConnectContext _context;
        private readonly IUserService _userService;

        public MessageService(EFConnectContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                            .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<MessageToReturn>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages                                                                    //  1.
                                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                                .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":                                                                                   //  2a.
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId &&
                        u.RecipientDeleted == false);
                    break;
                case "Outbox":                                                                                  //  2b.
                    messages = messages.Where(u => u.SenderId == messageParams.UserId &&
                        u.SenderDeleted == false);
                    break;
                default:    // "Unread"                                                                         //  2c.
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId &&
                    u.RecipientDeleted == false &&
                    u.IsRead == false);
                    break;
            }

            var messagesToReturn = messages                                                                     //  3.
                                    .Select(u => new MessageToReturn
                                    {
                                        Id = u.Id,
                                        SenderId = u.SenderId,
                                        SenderKnownAs = u.Sender.KnownAs,
                                        SenderPhotoUrl = u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url,
                                        RecipientId = u.RecipientId,
                                        RecipientKnownAs = u.Recipient.KnownAs,
                                        RecipientPhotoUrl = u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url,
                                        Content = u.Content,
                                        IsRead = u.IsRead,
                                        DateRead = u.DateRead,
                                        DateSent = u.DateSent
                                    })
                                    .OrderByDescending(d => d.DateSent);

            return await PagedList<MessageToReturn>                                                             //  4.
                            .CreateAsync(messagesToReturn, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageToReturn>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                                .Where(m => (m.RecipientId == userId &&
                                             m.SenderId == recipientId &&
                                             m.RecipientDeleted == false)
                                            ||
                                            (m.RecipientId == recipientId &&
                                             m.SenderId == userId &&
                                             m.SenderDeleted == false))
                                .Select(u => new MessageToReturn
                                {
                                    Id = u.Id,
                                    SenderId = u.SenderId,
                                    SenderKnownAs = u.Sender.KnownAs,
                                    SenderPhotoUrl = u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url,
                                    RecipientId = u.RecipientId,
                                    RecipientKnownAs = u.Recipient.KnownAs,
                                    RecipientPhotoUrl = u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url,
                                    Content = u.Content,
                                    IsRead = u.IsRead,
                                    DateRead = u.DateRead,
                                    DateSent = u.DateSent
                                })
                                .OrderByDescending(m => m.DateSent)
                                .ToListAsync();

            return messages;
        }

        public async Task<MessageToReturn> CreateMessage(MessageForCreation messageForCreation)
        {
            var sender = await _userService.GetUser(messageForCreation.SenderId);
            var recipient = await _userService.GetUser(messageForCreation.RecipientId);

            var message = new Message
            {
                SenderId = messageForCreation.SenderId,
                RecipientId = messageForCreation.RecipientId,
                DateSent = messageForCreation.DateSent,
                Content = messageForCreation.Content,
                IsRead = false,
                SenderDeleted = false,
                RecipientDeleted = false
            };

            _context.Add(message);

            if (!await SaveAll())
                return null;

            return new MessageToReturn
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderKnownAs = message.Sender.KnownAs,
                SenderPhotoUrl = message.Sender.Photos.FirstOrDefault(p => p.IsMain).Url,
                RecipientId = message.RecipientId,
                RecipientKnownAs = message.Recipient.KnownAs,
                RecipientPhotoUrl = message.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url,
                Content = message.Content,
                IsRead = message.IsRead,
                DateRead = message.DateRead,
                DateSent = message.DateSent
            };
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkMessageAsRead(Message message)
        {
            message.IsRead = true;
            message.DateRead = DateTime.Now;

            return await SaveAll();
        }
    }
}
