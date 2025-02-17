using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Application.Interfaces
{
    public interface IMeasurementConverter
    {
        float ConvertToPoints(float measurementInCm);
    }
}
