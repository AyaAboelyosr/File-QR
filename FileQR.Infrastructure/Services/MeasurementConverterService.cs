using FileQR.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileQR.Infrastructure.Services
{
    public class MeasurementConverterService : IMeasurementConverter
    {
        public float ConvertToPoints(float measurementInCm)
        {
            float dpi = 72;
            return measurementInCm * dpi / 2.54f;
        }
    }
}
