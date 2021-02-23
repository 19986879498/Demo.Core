using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Core.Methods
{
    public class MomeryCacheService
    {
        private static  IMemoryCache memoryCache = null;
         static MomeryCacheService()
        {
            if (memoryCache==null)
            {
                memoryCache = new MemoryCache(new MemoryCacheOptions());
            }
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public static void Set(string Key,object Value)
        {
            memoryCache.Set(Key, Value, TimeSpan.FromMinutes(1));
        }
        public static object Read(string Key)
        {
            return memoryCache.Get(Key);
        }
    }
}
