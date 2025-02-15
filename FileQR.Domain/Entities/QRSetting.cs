using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Domain.Entities
{
    public class QRSetting
    {
        public int ID { get; set; }
        public bool ShowArabicNames { get; set; }
        public bool QRShowAuthBy { get; set; }
        public bool QRShowIssueDate { get; set; }
        public bool QRShowIssuedFor { get; set; }
        public bool QRShowLink { get; set; }
        public float QRLeft { get; set; }
        public float QRBottom { get; set; }
        public float QRWidth { get; set; }
        public string MessageToShowInImage { get; set; } = string.Empty;
    }
}
