using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Application.DTOs
{
    public class BlobResult
    {
        public Stream FileStream { get; set; }
        public BlobMetadata Metadata { get; set; }
    }
}
