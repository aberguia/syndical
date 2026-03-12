namespace GestionSyndicale.Core.DTOs.Polls;

public class PollDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime? ClosedOn { get; set; }
    public List<PollOptionDto> Options { get; set; } = new();
    public List<PollResultDto> Results { get; set; } = new();
}

public class PollOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class PollResultDto
{
    public int OptionId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public decimal Percentage { get; set; }
}

public class CreatePollDto
{
    public string Question { get; set; } = string.Empty;
    public List<CreatePollOptionDto> Options { get; set; } = new();
}

public class CreatePollOptionDto
{
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class UpdatePollDto
{
    public string Question { get; set; } = string.Empty;
    public List<UpdatePollOptionDto> Options { get; set; } = new();
}

public class UpdatePollOptionDto
{
    public int? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class PollListDto
{
    public int TotalCount { get; set; }
    public List<PollDto> Items { get; set; } = new();
}

public class PollVoteDto
{
    public int PollId { get; set; }
    public int PollOptionId { get; set; }
    public bool HasVoted { get; set; }
    public int? MyVoteOptionId { get; set; }
}

public class PortalPollDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime? ClosedOn { get; set; }
    public List<PollOptionDto> Options { get; set; } = new();
    public List<PollResultDto> Results { get; set; } = new();
    public bool HasVoted { get; set; }
    public int? MyVoteOptionId { get; set; }
    public int TotalVotes { get; set; }
}
