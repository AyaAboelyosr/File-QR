using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileQR.Application.Interfaces;
using FileQR.Domain.Entities;

namespace FileQR.Infrastructure.Services
{
    public class QRSettingsService : IQRSettingsService
    {
        public IEnumerable<QRSetting> QRSettings { get; }

        public QRSettingsService()
        {
            // Initialize QRSettings (e.g., from a database or configuration)
            QRSettings = new List<QRSetting>
        {
            new QRSetting { ID = 1, ShowArabicNames = true, QRShowAuthBy = true, QRShowIssueDate = true, QRShowIssuedFor = true, QRShowLink = true, MessageToShowInImage = "Authorized By" },
            // Add more QR settings as needed
        };
        }
    }
}
