using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDIFrontEnd_WPF.ViewModels;
namespace SDIFrontEnd_WPF
{
    public interface IWindowService
    {
        void ShowQuestionImporterWindow();
        void ShowSearchWindow(List<PraccingIssue> issues);
        void ShowImageWindow(PraccingImage image);
    }
}
