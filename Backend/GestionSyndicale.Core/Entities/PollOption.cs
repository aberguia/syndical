namespace GestionSyndicale.Core.Entities;

public class PollOption
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    // Navigation properties
    public Poll Poll { get; set; } = null!;
    public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
}
