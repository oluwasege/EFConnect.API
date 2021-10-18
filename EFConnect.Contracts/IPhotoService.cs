using EFConnect.Data.Entities;
using EFConnect.Models.Photo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Contracts
{
    public interface IPhotoService
    {
        Task<PhotoForReturn> AddPhotoForUser(int userId, PhotoForCreation photoDto);
        Task<bool> SaveAll();
        Task<PhotoForReturn> GetPhoto(int id);
        Task<Photo> GetMainPhotoForUser(int userId);
        Task<bool> SetMainPhotoForUser(int userId, PhotoForReturn photo);
        object DeletePhotoFromCloudinary(string publicId);
        Task<Photo> GetPhotoEntity(int id);
    }
}
