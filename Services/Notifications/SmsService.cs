namespace CAT.AID.Web.Services.Notifications
{
    public interface ISmsService
    {
        Task SendAsync(string mobile, string message);
    }

    public class SmsService : ISmsService
    {
        public Task SendAsync(string mobile, string message)
        {
            // TODO: integrate NIC / Twilio / Govt SMS gateway
            Console.WriteLine($"SMS to {mobile}: {message}");
            return Task.CompletedTask;
        }
    }
}
