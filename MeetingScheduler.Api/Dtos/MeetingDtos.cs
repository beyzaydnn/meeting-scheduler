namespace MeetingScheduler.Api.Dtos;

public record CreateMeetingGroupRequest(
    string CreatorName,
    DateTime Deadline,
    bool IncludeWeekends,
    bool SelectAvailable);

public record JoinMeetingRequest(
    string Name,
    List<DateTime> Dates);

public record MeetingGroupResponse(
    int Id,
    string ShareCode,
    string CreatorName,
    DateTime Deadline,
    bool IncludeWeekends,
    bool SelectAvailable,
    DateTime CreatedAt,
    List<ParticipantResponse> Participants);

public record ParticipantResponse(
    int Id,
    string Name,
    DateTime JoinedAt,
    List<DateTime> Dates);
