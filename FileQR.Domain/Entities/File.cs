using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Domain.Entities
{
    public class File
    {
        public int Id { get; set; }
        public string Path { get; set; } = string.Empty;
        public int? QRSettingId { get; set; }
        public QRSetting? QRSetting { get; set; }

        public string? IssuedFor { get; set; }
        public int? AuthRequiredFrom { get; set; }
        public User? AuthRequiredFromUser { get; set; }
        public bool ShowFirstPage { get; set; }
        public bool ShowLastPage { get; set; }
        public bool ShowMiddlePages { get; set; }

        public float FirstPagePosLeft { get; set; }
        public float FirstPagePosBottom { get; set; }
        public float FirstPagePosWidth { get; set; }

        public float LastPagePosLeft { get; set; }
        public float LastPagePosBottom { get; set; }
        public float LastPagePosWidth { get; set; }

        public float MiddlePagePosLeft { get; set; }
        public float MiddlePagePosBottom { get; set; }
        public float MiddlePagePosWidth { get; set; }
    }
}
