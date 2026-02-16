namespace HelpDeskApp.ViewModels.Models.Ticket
{
    public class TicketDetailsVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Category { get; set; } = null!;
        public bool IsCreator { get; set; }
        public string CreatorId { get; set; }
    }
}
