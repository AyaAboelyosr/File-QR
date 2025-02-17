using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileQR.Application.DTOs;

namespace FileQR.Application.Interfaces
{
    public interface IFileManager
    {
        
        Task<BlobResult?> DownloadFileAsync(string filePath, string containerName);
        Task UploadFileAsync(Stream fileStream, string filePath, string containerName, string contentType);
    }
}
