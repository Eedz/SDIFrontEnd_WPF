using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITCLib;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Configuration;

namespace SDIFrontEnd_WPF
{
    public partial class HomeViewModel :ViewModelBase
    {
        private readonly IApiSurveyService apiSurveyService; // Service for managing surveys via API calls

        private string BackupPath = ConfigurationManager.AppSettings["BackupPath"] ?? string.Empty;
        private string AutoSurveysPath = ConfigurationManager.AppSettings["AutoSurveysPath"] ?? string.Empty;

        [ObservableProperty]
        private string? backupStatusMessage;
        [ObservableProperty]
        private string? autoSurveysStatusMessage;

        public HomeViewModel(IApiSurveyService apiSurveyService)
        {
            this.apiSurveyService = apiSurveyService;
            backupStatusMessage = "Loading...";
            autoSurveysStatusMessage = "Loading...";
            _ = Load(); // TODO move this call to a command or run it from the mainwindowviewmodel
        }

        public async Task Load()
        {
            if (!string.IsNullOrEmpty(BackupPath))
                BackupStatusMessage = await BackupStatus();
            if (!string.IsNullOrEmpty(AutoSurveysPath))
                AutoSurveysStatusMessage = await AutoSurveysStatus();
        }

        private async Task<string> BackupStatus()
        {
            DateTime lastWorkDay = DateTime.Today;
            if (DateTime.Today.Date.DayOfWeek == DayOfWeek.Monday)
                lastWorkDay = lastWorkDay.AddDays(-3);
            else
                lastWorkDay = lastWorkDay.AddDays(-1);

            string path = BackupPath + lastWorkDay.Date.ToString("yyyy-MM-dd") + ".7z";
            if (!File.Exists(path))
                return "Backup for yesterday (" + lastWorkDay.ShortDate() + ") missing.";
            else
                return "Backup: OK";
        }

        private async Task<string> AutoSurveysStatus()
        {
            DateTime lastWorkDay = DateTime.Today;
            if (DateTime.Today.Date.DayOfWeek == DayOfWeek.Monday)
                lastWorkDay = lastWorkDay.AddDays(-3);
            else
                lastWorkDay = lastWorkDay.AddDays(-1);

            List<Survey> changedSurveys = await apiSurveyService.GetChangedSurveys(lastWorkDay);

            if (changedSurveys.Count() == 0)
            {
                return "No surveys were changed " + lastWorkDay.Date.DayOfWeek + " (" + lastWorkDay.ToString("d") + ").";
            }

            int count = 0;
            foreach (Survey s in changedSurveys)
            {
                string lastWkDay = AutoSurveysPath + s.SurveyCode + ", " + lastWorkDay.Date.ToString("ddMMMyyyy").Replace("-", "") + ".docx";
                string today = AutoSurveysPath + s.SurveyCode + ", " + lastWorkDay.ToString("ddMMMyyyy").Replace("-", "") + ".docx";
                if (!File.Exists(lastWkDay) && (!File.Exists(today)))
                    count++;
            }

            if (count > 0)
            {
                return "Automatic Surveys for " + lastWorkDay.Date.DayOfWeek + " (" + lastWorkDay.ToString("d") + ") are missing.";
            }
            else
            {
                string status = $"Surveys changed yesterday: {string.Join(", ", changedSurveys.Select(x => x.SurveyCode))}";
                return status;
            }
        }
    }
}
