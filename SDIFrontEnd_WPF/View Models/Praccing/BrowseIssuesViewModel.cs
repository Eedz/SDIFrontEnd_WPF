using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITC_Services;
using ITCLib;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace SDIFrontEnd_WPF.ViewModels
{
    // list the incoming issues and allow the user to select one
    // user can also search for issues by varname and recipient
    public partial class BrowseIssuesViewModel : ViewModelBase
    {
        private const int PageSize = 4;

        [ObservableProperty]
        private ObservableCollection<PraccingIssue> pagedIssues;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand), nameof(PreviousPageCommand))]
        private int currentPage = 1;


        public Action? CloseWindow { get; set; }

        [ObservableProperty]
        public List<PraccingIssue> issues;


        public ObservableCollection<PraccingIssue> AllIssues { get; } = new();
        private List<PraccingIssue> _filteredIssues = new();

        public int TotalPages => (int)Math.Ceiling((double)_filteredIssues.Count / PageSize);

       

        public string? CurrentSurvey { get => Issues.FirstOrDefault()?.Survey.SurveyCode; }
        public string? Instructions { get => 
                "Enter the variable name and inteded recipient on the left to check if the issues already been submitted. " +
                "You can go to any of the issues by clicking the Select button. \r\n" +
                "If the issue is not found below, click Create New to start a new issue.";
        }

        [ObservableProperty]
        private string? varNameCriteria;
        [ObservableProperty]
        private Person recipients;

        public BrowseIssuesViewModel(List<PraccingIssue> issueList)
        {
            issues = issueList;

            AllIssues = new ObservableCollection<PraccingIssue>(issueList);
            _filteredIssues = AllIssues.ToList();

            FilterIssues();

            OnPropertyChanged(nameof(CurrentSurvey));
        }

        partial void OnVarNameCriteriaChanged(string? value)
        {
            CurrentPage = 1;
            FilterIssues();
        }

        partial void OnRecipientsChanged(Person value)
        {
            CurrentPage = 1;
            FilterIssues();
        }

        private void FilterIssues()
        {
            if (string.IsNullOrWhiteSpace(VarNameCriteria))
                _filteredIssues = AllIssues.ToList();
            else
                _filteredIssues = AllIssues
                    .Where(i =>
                        (!string.IsNullOrEmpty(i.VarNames) && i.VarNames.Contains(VarNameCriteria, StringComparison.OrdinalIgnoreCase)) &&
                       (Recipients == null || i.IssueTo == Recipients))
                    .ToList();

            UpdatePagedIssues();
            NextPageCommand.NotifyCanExecuteChanged();
        }

        private void UpdatePagedIssues()
        {
            var items = _filteredIssues
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            PagedIssues = new ObservableCollection<PraccingIssue>(items);
            OnPropertyChanged(nameof(TotalPages));
        }

        [RelayCommand (CanExecute= nameof(CanGoNext))]
        private void NextPage()
        {
            if (CanGoNext())
            {
                CurrentPage++;
                UpdatePagedIssues();
            }
        }
        [RelayCommand (CanExecute =nameof(CanGoPrevious))]
        private void PreviousPage()
        {
            if (CanGoPrevious())
            {
                CurrentPage--;
                UpdatePagedIssues();
            }
        }

        private bool CanGoNext() => CurrentPage < TotalPages;
        private bool CanGoPrevious() => CurrentPage > 1;

        [RelayCommand]
        private void SelectIssue(PraccingIssue issue)
        {
            // Send the selected issue
            WeakReferenceMessenger.Default.Send(new IssueSelectedMessage(issue));

            // Close the chooser window (bonus)
            var win = System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            win?.Close();
        }

        [RelayCommand]
        private void CreateNew()
        {
            // Send a message to create a new issue
            WeakReferenceMessenger.Default.Send(new IssueSelectedMessage(new PraccingIssue() ));
            // Close the chooser window (bonus)
            var win = System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            win?.Close();
            
        }
    }


}
