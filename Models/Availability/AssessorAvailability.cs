namespace CAT.AID.Web.Models.Availability
{
    public class AssessorAvailability
    {
        public int Id { get; set; }

        public string AssessorId { get; set; }
        public ApplicationUser Assessor { get; set; }

        public DateOnly Date { get; set; }

        public TimeSpan SlotFrom { get; set; }
        public TimeSpan SlotTo { get; set; }

        public bool IsBooked { get; set; }
    }
}
