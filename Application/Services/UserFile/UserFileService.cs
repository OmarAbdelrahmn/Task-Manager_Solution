using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.UserFile;

public class UserFileService(IWebHostEnvironment env) : IUserFileService
{

    // allowed image types for avatars
    private static readonly string[] ImageTypes =
        ["image/jpeg", "image/png", "image/webp"];

    // allowed file types for task/message attachments
    private static readonly string[] AttachmentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/gif",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain",
        "application/zip"
    ];

    private const long MaxAvatarSize = 5 * 1024 * 1024;       // 2MB
    private const long MaxAttachmentSize = 20 * 1024 * 1024;  // 20MB

    public async Task<string> SaveFileAsync(IFormFile file, string folder, string userId)
    {
        // folder = "avatars" | "task-files" | "message-files"
        var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", folder , userId);

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // store with a GUID name to avoid collisions
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsFolder, storedFileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        // return relative URL — e.g. /uploads/avatars/abc123.jpg
        return $"/uploads/{folder}/{userId}/{storedFileName}";
    }

    public void DeleteFile(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        // convert URL to physical path
        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(env.WebRootPath, relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public bool IsValidFile(IFormFile file, string[] allowedTypes, long maxSizeBytes)
    {
        if (file.Length == 0 || file.Length > maxSizeBytes)
            return false;

        return allowedTypes.Contains(file.ContentType.ToLower());
    }

    // convenience helpers
    public bool IsValidAvatar(IFormFile file) =>
        IsValidFile(file, ImageTypes, MaxAvatarSize);

    public bool IsValidAttachment(IFormFile file) =>
        IsValidFile(file, AttachmentTypes, MaxAttachmentSize);
}

