using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using ITC_Contracts;

namespace SDIFrontEnd_WPF.Mappings
{
    public class WordingMapper : IMapper<Wording, WordingDto>
    {
        public WordingMapper() { }

        public Wording MapToEntity(WordingDto dto)
        {
            return new Wording
            {
                WordID = dto.ID,
                Type = (WordingType)dto.Type,
                WordingText = dto.WordingText
            };
        }

        public WordingDto MapToDto(Wording entity)
        {
            return new WordingDto
            {
                ID = entity.WordID,
                Type = (int)entity.Type,
                WordingText = entity.WordingText
            };
        }


    }

    public class ResponseSetMapper : IMapper<ResponseSet, ResponseSetDto>
    {
        public ResponseSetMapper() { }

        public ResponseSet MapToEntity(ResponseSetDto dto)
        {
            return new ResponseSet
            {
                RespSetName = dto.RespSetName,
                Type = (ResponseType)dto.Type,
                RespList = dto.RespList
            };
        }

        public ResponseSetDto MapToDto(ResponseSet entity)
        {
            return new ResponseSetDto
            {
                RespSetName = entity.RespSetName,
                Type = (int)entity.Type,
                RespList = entity.RespList
            };
        }
    }
}
