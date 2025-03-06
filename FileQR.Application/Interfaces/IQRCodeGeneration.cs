using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Application.Interfaces
{
    public interface IQRCodeGeneration
    {
        Bitmap GenerateQRCodeImage(string text);
    }
}