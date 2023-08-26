using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.ServerShared.Utils
{
    public class SqlUtils
    {
        public static B MapTo<A, B>(A obj) where A : class, new() where B : class, new()
        {
            MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<A, B>());

            IMapper mapper = config.CreateMapper();

            return mapper.Map<B>(obj);
        }
    }
}
