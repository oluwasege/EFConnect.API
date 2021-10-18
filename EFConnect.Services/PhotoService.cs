using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EFConnect.Contracts;
using EFConnect.Data;
using EFConnect.Data.Entities;
using EFConnect.Helpers;
using EFConnect.Models.Photo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFConnect.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IUserService _userService;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;
        private readonly EFConnectContext _context;

        public PhotoService(IUserService userService,
            IOptions<CloudinarySettings> cloudinaryConfig,
            EFConnectContext context)
        {
            _context = context;
            _userService = userService;
            _cloudinaryConfig = cloudinaryConfig;

            Account account = new(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        [Obsolete]
        public async Task<PhotoForReturn> AddPhotoForUser(int userId, PhotoForCreation photoDto)
        {
            var user = await _context                                   //  1.
                        .Users
                        .Where(u => u.Id == userId)
                        .FirstOrDefaultAsync();

            var file = photoDto.File;                                   //  2.

            var uploadResult = new ImageUploadResult();                 //  3.

            if (file.Length > 0)                                        //  4.
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()           //  *
                                        .Width(500).Height(500)
                                        .Crop("fill")
                                        .Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);    //  5.
                }
            }

            photoDto.Url = uploadResult.Uri.ToString();                 //  4. (cont'd)
            photoDto.PublicId = uploadResult.PublicId;                  //  4. (cont'd)

            var photo = new Photo                                       //  6.
            {
                Url = photoDto.Url,
                Description = "",
                DateAdded = photoDto.DateAdded,
                PublicId = photoDto.PublicId,
                User = user
            };

            if (!photo.User.Photos.Any(m => m.IsMain))                  //  7.
                photo.IsMain = true;

            user.Photos.Add(photo);
            await SaveAll();                                            //  8.

            return new PhotoForReturn                                   //  9.
            {
                Id = photo.Id,
                Url = photo.Url,
                Description = photo.Description,
                DateAdded = photo.DateAdded,
                IsMain = photo.IsMain,
                PublicId = photo.PublicId,
            };
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<PhotoForReturn> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            var photoForReturn = new PhotoForReturn
            {
                Id = photo.Id,
                Url = photo.Url,
                Description = photo.Description,
                DateAdded = photo.DateAdded,
                IsMain = photo.IsMain,
                PublicId = photo.PublicId,
            };

            return photoForReturn;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<bool> SetMainPhotoForUser(int userId, PhotoForReturn photo)
        {
            var currentMainPhoto = await GetMainPhotoForUser(userId);

            if (currentMainPhoto != null)
                currentMainPhoto.IsMain = false;

            _context.Photos.FirstOrDefault(p => p.Id == photo.Id).IsMain = true;

            return await SaveAll();
        }

        public async Task<Photo> GetPhotoEntity(int id)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        }

        public object DeletePhotoFromCloudinary(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            return _cloudinary.Destroy(deleteParams).Result;
        }
    }
}
