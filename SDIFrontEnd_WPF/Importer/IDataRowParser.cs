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

    public class PraccingIssueParser : IDataRowParser<PraccingIssue>
    {
        public PraccingIssue Parse(IDictionary<string, string> row)
        {

            var issue = new PraccingIssue
            {
                IssueNo = Convert.ToInt32(row["IssueNo"]),
                VarNames = (string)row["VarNames"],
                IssueDate = DateTime.TryParse(row["Date"], out var date) ? date : DateTime.Today,
                PinNo = (string)row["Pin"],                
                Description = (string)row["Issue Description"]
            };

            issue.IssueFrom = new Person { Name = row["IssueFrom"] };
            issue.IssueTo = new Person { Name = row["IssueTo"] };

            return issue;
        }
    }
}
