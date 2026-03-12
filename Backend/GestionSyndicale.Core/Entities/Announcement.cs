namespace GestionSyndicale.Core.Entities;

public class Announcement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public AnnouncementStatus Status { get; set; } = AnnouncementStatus.Draft;
    public int CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? UpdatedByUserId { get; set; }
    public DateTime? UpdatedOn { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public User? UpdatedBy { get; set; }
}

public enum AnnouncementStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}
