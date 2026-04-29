using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;

namespace SDIFrontEnd_WPF.ViewModels
{
    public class QuestionCandidateViewModel
    {
        private readonly QuestionCandidate _candidate;

        public QuestionCandidateViewModel(QuestionCandidate candidate)
        {
            _candidate = candidate;
        }
    }
}
