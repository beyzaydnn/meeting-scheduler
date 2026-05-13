using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetingScheduler.Api.Data;
using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Models;

namespace MeetingScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeetingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MeetingsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create(CreateMeetingGroupRequest request)
    {
        var group = new MeetingGroup
        {
            ShareCode = Guid.NewGuid().ToString("N")[..8],
            CreatorName = request.CreatorName,
            Deadline = request.Deadline,
            IncludeWeekends = request.IncludeWeekends,
            SelectAvailable = request.SelectAvailable
        };

        _db.MeetingGroups.Add(group);
        await _db.SaveChangesAsync();

        return Ok(new { group.Id, group.ShareCode });
    }

    [HttpGet("{shareCode}")]
    public async Task<IActionResult> Get(string shareCode)
    {
        var group = await _db.MeetingGroups
            .Include(g => g.Participants)
                .ThenInclude(p => p.Availabilities)
            .FirstOrDefaultAsync(g => g.ShareCode == shareCode);

        if (group is null) return NotFound();

        var response = new MeetingGroupResponse(
            group.Id,
            group.ShareCode,
            group.CreatorName,
            group.Deadline,
            group.IncludeWeekends,
            group.SelectAvailable,
            group.CreatedAt,
            group.Participants.Select(p => new ParticipantResponse(
                p.Id,
                p.Name,
                p.JoinedAt,
                p.Availabilities.Select(a => a.Date).ToList()
            )).ToList());

        return Ok(response);
    }

    [HttpPost("{shareCode}/join")]
    public async Task<IActionResult> Join(string shareCode, JoinMeetingRequest request)
    {
        var group = await _db.MeetingGroups
            .FirstOrDefaultAsync(g => g.ShareCode == shareCode);

        if (group is null) return NotFound();
        if (group.Deadline < DateTime.UtcNow) return BadRequest("Deadline gecmis.");

        var participant = new Participant
        {
            MeetingGroupId = group.Id,
            Name = request.Name,
            Availabilities = request.Dates
                .Select(d => new Availability { Date = d.Date })
                .ToList()
        };

        _db.Participants.Add(participant);
        await _db.SaveChangesAsync();

        return Ok(new { participant.Id });
    }

    [HttpGet("{shareCode}/results")]
    public async Task<IActionResult> GetResults(string shareCode)
    {
        var group = await _db.MeetingGroups
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.ShareCode == shareCode);

        if (group is null) return NotFound();

        var commonDates = await GetCommonDates(shareCode);
        return Ok(new { CommonDates = commonDates, ParticipantCount = group.Participants.Count });
    }

    [HttpGet("{shareCode}/spin")]
    public async Task<IActionResult> Spin(string shareCode)
    {
        var commonDates = await GetCommonDates(shareCode);
        if (commonDates is null) return NotFound();
        if (commonDates.Count == 0) return BadRequest("Ortak musait gun bulunamadi.");

        var selectedDate = commonDates[Random.Shared.Next(commonDates.Count)];
        return Ok(new { SelectedDate = selectedDate });
    }

    private async Task<List<DateTime>?> GetCommonDates(string shareCode)
    {
        var group = await _db.MeetingGroups
            .Include(g => g.Participants)
                .ThenInclude(p => p.Availabilities)
            .FirstOrDefaultAsync(g => g.ShareCode == shareCode);

        if (group is null) return null;

        var participantCount = group.Participants.Count;
        if (participantCount == 0) return new List<DateTime>();

        var allDates = group.Participants
            .SelectMany(p => p.Availabilities.Select(a => a.Date))
            .ToList();

        if (group.SelectAvailable)
        {
            return allDates
                .GroupBy(d => d)
                .Where(g => g.Count() == participantCount)
                .Select(g => g.Key)
                .OrderBy(d => d)
                .ToList();
        }
        else
        {
            var busyDates = allDates.Distinct().ToHashSet();
            var today = DateTime.UtcNow.Date;
            return Enumerable.Range(0, (group.Deadline.Date - today).Days + 1)
                .Select(i => today.AddDays(i))
                .Where(d => !busyDates.Contains(d))
                .Where(d => group.IncludeWeekends || (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday))
                .ToList();
        }
    }
}
