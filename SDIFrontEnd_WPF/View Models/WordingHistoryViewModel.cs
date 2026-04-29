using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITCLib;

namespace SDIFrontEnd_WPF.ViewModels
{
    public class WordingHistoryViewModel : ViewModelBase
    {

        private readonly AuditWording _wording;

        public int ID => _wording.ID;
        public string WordingType => _wording.WordingType;
        public string Wording => _wording.Wording;
        public DateTime UpdateDate => _wording.UpdateDate;
        public string UserName => _wording.UserName;
        public AuditEntryType ChangeType => _wording.ChangeType;    
        public WordingHistoryViewModel(AuditWording wording)
        {
            DisplayName = "Wording History";
            _wording = wording;

        }

    }
}
