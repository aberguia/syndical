namespace GestionSyndicale.Core.Entities;

public class PollVote
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public int PollOptionId { get; set; }
    public int AdherentId { get; set; }
    public DateTime VotedOn { get; set; }

    // Navigation properties
    public Poll Poll { get; set; } = null!;
    public PollOption PollOption { get; set; } = null!;
    public User Adherent { get; set; } = null!;
}
