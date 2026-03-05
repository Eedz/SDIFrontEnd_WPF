using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;

namespace SDIFrontEnd_WPF
{
    public class ReferenceDataStore
    {
        public ObservableCollection<VarNameLabel> DomainLabels { get; set; } = new();
        public ObservableCollection<VarNameLabel> TopicLabels { get; set; } = new();
        public ObservableCollection<VarNameLabel> ContentLabels { get; set; } = new();
        public ObservableCollection<VarNameLabel> ProductLabels { get; set; } = new();
        public ObservableCollection<CommentType> CommentTypes { get; set; } = new();
        public ObservableCollection<Person> Personnel { get; set; } = new();
        public ObservableCollection<Language> Languages { get; set; } = new();

        public ObservableCollection<SurveyMode> Modes { get; set; } = new();
        public ObservableCollection<UserState> UserStates { get; set; } = new();
        public ObservableCollection<ScreenedProduct> ScreenedProducts { get; set; } = new();

        public ObservableCollection<SurveyCohort> Cohorts { get; set; } = new ();


        public ReferenceDataStore() 
        { 

        }


    }
}
