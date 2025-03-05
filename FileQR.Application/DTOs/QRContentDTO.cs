using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Application.DTOs
{
    public class QRContentDTO
    {
        public bool ShowArabicNames { get; set; }
        public bool QRShowAuthBy { get; set; }
        public bool QRShowIssueDate { get; set; }
        public bool QRShowIssuedFor { get; set; }
        public bool QRShowLink { get; set; }
        public string AuthRequiredFromUser { get; set; }
        public string IssuedFor { get; set; }
        public string MessageToShowInImage { get; set; }
        public float QRLeft { get; set; }
        public float QRBottom { get; set; }
        public float QRWidth { get; set; }
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
