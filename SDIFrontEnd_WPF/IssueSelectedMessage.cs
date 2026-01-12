using CommunityToolkit.Mvvm.Messaging.Messages;
using ITCLib;

public class IssueSelectedMessage : ValueChangedMessage<PraccingIssue>
{
    public IssueSelectedMessage(PraccingIssue value) : base(value) { }
}
