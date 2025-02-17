using FileQR.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileQR.Application.DTOs;

namespace FileQR.Infrastructure.Services
{
    public class FileManager : IFileManager
    {
        private readonly string _basePath;

        public FileManager(string basePath)
        {
            _basePath = basePath;

            // Ensure the base directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<BlobResult?> DownloadFileAsync(string filePath, string containerName)
        {
            try
            {
                string fullPath = Path.Combine(_basePath, containerName, filePath);

                if (!System.IO.File.Exists(fullPath))
                    return null;

                var fileStream = System.IO.File.OpenRead(fullPath);
                return new BlobResult
                {
                    FileStream = fileStream,
                    Metadata = new BlobMetadata
                    {
                        ContentType = GetContentType(fullPath)
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to download file from local storage.", ex);
            }
        }

        public async Task UploadFileAsync(Stream fileStream, string filePath, string containerName, string contentType)
        {
            try
            {
                string containerPath = Path.Combine(_basePath, containerName);

                if (!Directory.Exists(containerPath))
                {
                    Directory.CreateDirectory(containerPath);
                }

                string fullPath = Path.Combine(containerPath, filePath);

                using (var outputFileStream = System.IO.File.Create(fullPath))
                {
                    await fileStream.CopyToAsync(outputFileStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload file to local storage.", ex);
            }
        }

        private string GetContentType(string filePath)
        {
            // You can use a library like `MimeMapping` to get the MIME type based on the file extension
            return "application/octet-stream"; // Default MIME type
        }
    }
}
