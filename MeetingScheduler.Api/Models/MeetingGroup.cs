namespace MeetingScheduler.Api.Models;

public class MeetingGroup
{
    public int Id { get; set; }
    public string ShareCode { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public bool IncludeWeekends { get; set; }
    public bool SelectAvailable { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Participant> Participants { get; set; } = new();
}
