using FileQR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Application.Interfaces
{
    public interface IQRSettingsService
    {
       
        IEnumerable<QRSetting> QRSettings { get; }
    }
}
