namespace GestionSyndicale.Core.DTOs.Announcements;

public class AnnouncementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? UpdatedOn { get; set; }
}

public class CreateAnnouncementDto
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class UpdateAnnouncementDto
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class AnnouncementListDto
{
    public int TotalCount { get; set; }
    public List<AnnouncementDto> Items { get; set; } = new();
}
