using CAT.AID.Models;
using Microsoft.AspNetCore.Identity;
using CAT.AID.Web.Models;
using CAT.AID.Models;

namespace CAT.AID.Web.Services.Notifications
{
    public interface INotificationService
{
    Task NotifyAssessorAssignment(
        ApplicationUser assessor,
        Assessment assessment,
        DateTime date,
        TimeSpan from,
        TimeSpan to);
}


    public class NotificationService : INotificationService
    {
        private readonly IEmailService _email;
        private readonly ISmsService _sms;

        public NotificationService(IEmailService email, ISmsService sms)
        {
            _email = email;
            _sms = sms;
        }

public async Task NotifyAssessorAssignment(
    ApplicationUser assessor,
    Assessment assessment,
    DateTime date,
    TimeSpan from,
    TimeSpan to)
{
    string message =
$@"Dear {assessor.FullName},

You have been assigned a vocational assessment.

Candidate : {assessment.Candidate.FullName}
Date      : {date:dd MMM yyyy}
Time      : {from:hh\:mm} - {to:hh\:mm}

Please login to CVAT for details.

– NIEPID CVAT";

    if (!string.IsNullOrEmpty(assessor.Email))
    {
        await _email.SendAsync(
            assessor.Email,
            "New Assessment Assigned – CVAT",
            message);
    }

    if (!string.IsNullOrEmpty(assessor.PhoneNumber))
    {
        await _sms.SendAsync(assessor.PhoneNumber, message);
    }
}
    }
}
