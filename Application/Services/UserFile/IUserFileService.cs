using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.UserFile;

public interface IUserFileService
{
    Task<string> SaveFileAsync(IFormFile file, string folder, string userId);
    void DeleteFile(string fileUrl);
    bool IsValidFile(IFormFile file, string[] allowedTypes, long maxSizeBytes);
    bool IsValidAvatar(IFormFile file);
    bool IsValidAttachment(IFormFile file);
}
