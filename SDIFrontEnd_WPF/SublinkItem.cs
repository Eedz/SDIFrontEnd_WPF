using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public class SublinkItem
    {
        public string DisplayName { get; set; }
        public string Key { get; set; }
        public MenuCategory Category { get; set; }

        public SublinkItem(string displayName, string key, MenuCategory category)
        {
            DisplayName = displayName;
            Key = key;
            Category = category;
        }

        public override string ToString() => DisplayName;

    }

    public class SurveySublinkItem : SublinkItem
    {
        public int SurveyId { get; set; }
        
        public SurveySublinkItem(string displayName, string key, MenuCategory category, int surveyId)
            : base(displayName, key, category)
        {
            SurveyId = surveyId;
        
        }
        
    }

}
