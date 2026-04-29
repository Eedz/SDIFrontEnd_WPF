using ITC_Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.Mappings
{
    public interface IMapper<TEntity, TDto>
    {
        TEntity MapToEntity(TDto dto);
        TDto MapToDto (TEntity entity);

        //void MapToExisting(TDto dto, TEntity entity);
       
    }
}
