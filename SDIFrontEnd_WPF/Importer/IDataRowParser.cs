using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public interface IDataRowParser<T>
    {
        T Parse(IDictionary<string, string> row);
    }

    public class TranslationParser : IDataRowParser<Translation>
    {
        public Translation Parse(IDictionary<string, string> row)
        {
            return new Translation
            {
                VarName = row["VarName"],
                TranslationText = row["French"]
            };
        }

       
    }

    public class RenameParser : IDataRowParser<VarNameChange>
    {
        public VarNameChange Parse(IDictionary<string, string> row)
        {
            return new VarNameChange
            {
                OldName = row["Old"],
                NewName = row["New"]
            };
        }


    }
}
