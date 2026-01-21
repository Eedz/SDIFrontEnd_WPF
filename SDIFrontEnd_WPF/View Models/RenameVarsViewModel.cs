using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITC_DataAccess;
using ITC_Services;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class RenameVarsViewModel : ViewModelBase
    {
        private readonly IVarNameService _varNameService;
        private readonly ISurveyService _surveyService;
        private readonly IFileDialogService _fileDialogService;

        public List<Survey> SurveyList { get; set; }
        
        public ObservableCollection<RenameVarItem> RenameList { get; set; } = new ObservableCollection<RenameVarItem>();

        public VarNameChangeViewModel VarChangeDetails { get; set; } // container for each rename operation. most data remains constant and we will change the old/new names for each rename to be performed

        public bool UseVarName { get; set; } = true;
        public bool UseFile { get; set; }

        public string SourceFilePath { get; set; }


        public RenameVarsViewModel(IVarNameService varNameService, ISurveyService surveyService, IFileDialogService fileDialogService, VarNameChangeViewModel changeDetails)
        {
            DisplayName = "Rename Variables";

            _varNameService = varNameService;
            _surveyService = surveyService;
            _fileDialogService = fileDialogService;

            SurveyList = _surveyService.GetAllSurveys();
            RenameList.CollectionChanged += RenameList_CollectionChanged;
            VarChangeDetails = changeDetails;
        }

        private void RenameList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.NewItems == null)
                return;

            foreach (RenameVarItem item in e.NewItems)
            {
                item.PropertyChanged += RenameVarItem_PropertyChanged;
            }
            
        }

        private void RenameVarItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not RenameVarItem item)
                return;

            // Only recompute when relevant fields change
            if (e.PropertyName is nameof(RenameVarItem.OldVarName) or nameof(RenameVarItem.NewVarName))
            {
                LoadItemInfo(item);
            }
        }

        private void LoadItemInfo(RenameVarItem item)
        {
            if (string.IsNullOrWhiteSpace(item.OldVarName) && string.IsNullOrWhiteSpace(item.NewVarName))
            {
                item.VarLabel = string.Empty;
                return;
            }

            VariableName oldVarInfo = _varNameService.GetVariableInfo(item.OldVarName);
            VariableName newVarInfo = _varNameService.GetVariableInfo(item.NewVarName);

            string varlabel = (oldVarInfo != null ? $"{oldVarInfo.VarName}: {oldVarInfo.VarLabel}" : $"{item.OldVarName}:[new variable]") +
                            "\r\n" +
                            (newVarInfo != null ? $"{newVarInfo.VarName}: {newVarInfo.VarLabel}" : $"{item.NewVarName}:[new variable]");

            item.VarLabel = varlabel;

            List<Survey> surveysWithOldVar = new List<Survey>();
            List<Survey> surveysWithNewVar = new List<Survey>();

            if (UseVarName) 
            {           
                surveysWithOldVar = _surveyService.GetSurveysWithVarName(item.OldVarName);
                surveysWithNewVar = _surveyService.GetSurveysWithVarName(item.NewVarName);
            }
            else
            {
                surveysWithOldVar = _surveyService.GetSurveysWithRefVarName(item.OldVarName);
                surveysWithNewVar = _surveyService.GetSurveysWithRefVarName(item.NewVarName);
            }

            item.SurveysAffected = string.Join(", ", surveysWithOldVar.Where(x=>!x.Locked));
            var both = surveysWithOldVar.Intersect(surveysWithNewVar).ToList();
            item.SurveysBoth = string.Join(", ", both);
            
            var lockedSurveys = surveysWithOldVar.Where(x => x.Locked);
            item.SurveysLocked = string.Join(", ", lockedSurveys);
            OnPropertyChanged(nameof(RenameList));
        }

        public partial class RenameVarItem : ObservableObject
        {
            [ObservableProperty]
            string oldVarName;
            [ObservableProperty]
            string newVarName;
            [ObservableProperty]
            string varLabel;
            [ObservableProperty]
            string surveysAffected;
            [ObservableProperty]
            string surveysBoth;
            [ObservableProperty]
            string surveysLocked;
        }

        [RelayCommand]
        private void BrowseForFile()
        {
            SourceFilePath = _fileDialogService.OpenFile("Text Files|*.txt|Word Files|*.docx");
        }

        [RelayCommand]
        private void ImportRenames()
        {
            var tableImporter = new WordTableImporter(SourceFilePath);
            var parser = new RenameParser();
            var importer = new WordImporter<VarNameChange>(tableImporter, parser);

            foreach (var  d in importer.Import())
            {
                RenameVarItem item = new RenameVarItem()                
                {
                    OldVarName = d.OldName,
                    NewVarName = d.NewName
                };
                LoadItemInfo(item);
                RenameList.Add(item);
            }
        }

        [RelayCommand]
        private void PerformRenames()
        {
            VarNameChange changeInfo = this.VarChangeDetails.GetChangeObject;

            int i = 0;
        }
    }
}
