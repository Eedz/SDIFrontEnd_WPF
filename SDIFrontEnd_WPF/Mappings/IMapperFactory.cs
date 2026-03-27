using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.Mappings
{
    public interface IMapperFactory
    {
        IMapper<TSource, TDest> Get<TSource, TDest>();
    }

    public class MapperFactory : IMapperFactory
    {
        private readonly IServiceProvider _provider;

        public MapperFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IMapper<TSource, TDest> Get<TSource, TDest>()
        {
            return _provider.GetRequiredService<IMapper<TSource, TDest>>();
        }
    }
}
