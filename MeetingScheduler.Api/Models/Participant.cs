namespace MeetingScheduler.Api.Models;

public class Participant
{
    public int Id { get; set; }
    public int MeetingGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public MeetingGroup MeetingGroup { get; set; } = null!;
    public List<Availability> Availabilities { get; set; } = new();
}
