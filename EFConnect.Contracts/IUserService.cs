using EFConnect.Data.Entities;
using EFConnect.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Contracts
{
    public interface IUserService
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAll();
        Task<IEnumerable<UserForList>> GetUsers();
        Task<UserForDetail> GetUser(int id);
        Task<bool> UpdateUser(int id, UserForUpdate userForUpdate);
        Task<User> GetUserEntity(int id);
        Task<Follow> GetFollow(int userId, int recipientId);
    }
}
