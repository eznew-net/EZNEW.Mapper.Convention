using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZNEW.Mapper.Convention
{
    internal class AutoMapMapper : EZNEW.Mapper.IMapper
    {
        readonly AutoMapper.IMapper Mapper = null;

        /// <summary>
        /// 转换对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="sourceObj">源对象类型</param>
        /// <returns>目标对象类型</returns>
        public T MapTo<T>(object sourceObj)
        {
            return Mapper.Map<T>(sourceObj);
        }

        public AutoMapMapper(Action<IMapperConfigurationExpression> configuration)
        {
            var mapperConfiguration = new MapperConfiguration(configuration);
            Mapper = mapperConfiguration.CreateMapper();
        }
    }
}
