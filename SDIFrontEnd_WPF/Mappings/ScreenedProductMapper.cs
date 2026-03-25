using ITC_Contracts;
using ITCLib;

namespace SDIFrontEnd_WPF.Mappings
{
    public class ScreenedProductMapper : IMapper<SurveyScreenedProduct, SurveyScreenedProductDto>
    {
        public SurveyScreenedProduct MapToEntity(SurveyScreenedProductDto dto)
        {
            return new SurveyScreenedProduct
            {
                SurveyScreenedProductID = dto.SurveyScreenedProductID,
                ID = dto.ScreenedProductID,
                Product = new ScreenedProduct() { ID = dto.ScreenedProductID, ProductName = dto.ScreenedProductName ?? string.Empty }
            };
        }

        public SurveyScreenedProductDto MapToDto(SurveyScreenedProduct entity)
        {
            return new SurveyScreenedProductDto
            {
                SurveyScreenedProductID = entity.SurveyScreenedProductID,
                ScreenedProductID = entity.ID,
                ScreenedProductName = entity.Product.ProductName
            };
        }

        public void MapToExisting(SurveyScreenedProductDto dto, SurveyScreenedProduct entity)
        {
        }
    }
}