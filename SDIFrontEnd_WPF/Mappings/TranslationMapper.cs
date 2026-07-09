using ITC_Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;

namespace SDIFrontEnd_WPF.Mappings
{
    public  class TranslationMapper : IMapper<Translation, TranslationDto>
    {
        public Translation MapToEntity(TranslationDto dto)
        {
            return  new Translation() 
            { 
                ID = dto.ID, 
                QID = dto.QID, 
                Bilingual = dto.Bilingual,
                TranslationText = dto.TranslationText,
                LanguageName = new Language() { 
                    ID = dto.LanguageID, 
                    LanguageName = dto.LanguageName 
                }
            };
        }

        public TranslationDto MapToDto(Translation translation)
        {
            return new TranslationDto()
            {
                ID = translation.ID,
                QID = translation.QID,
                Bilingual = translation.Bilingual,
                LanguageID = translation.LanguageName.ID,
                LanguageName = translation.LanguageName.LanguageName,
                TranslationText = translation.TranslationText
            };
        }
    }
}
