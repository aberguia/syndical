namespace GestionSyndicale.Core.Entities;

public class Poll
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public PollStatus Status { get; set; } = PollStatus.Draft;
    public int CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ClosedOn { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
    public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
}

public enum PollStatus
{
    Draft = 0,
    Published = 1,
    Closed = 2,
    Archived = 3
}
