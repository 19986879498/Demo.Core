using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Core.IFactory
{
   public interface IJhEmrService
    {
        string GetEmrData(string hoscode, string clincNO);
    }
}
