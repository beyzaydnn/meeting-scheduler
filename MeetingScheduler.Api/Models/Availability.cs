namespace MeetingScheduler.Api.Models;

public class Availability
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public DateTime Date { get; set; }
    public Participant Participant { get; set; } = null!;
}
