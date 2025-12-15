using CAT.AID.Models;
using CAT.AID.Web.Models;

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
}
