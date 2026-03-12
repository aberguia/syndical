namespace GestionSyndicale.Core.DTOs.Notes;

public class MemberNoteListDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberFullName { get; set; } = string.Empty;
    public int? BuildingId { get; set; }
    public string? BuildingCodeOrName { get; set; }
    public int? ApartmentId { get; set; }
    public string? ApartmentNumber { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
}

public class CreateMemberNoteDto
{
    public int MemberId { get; set; }
    public string NoteText { get; set; } = string.Empty;
}

public class UpdateMemberNoteDto
{
    public string NoteText { get; set; } = string.Empty;
}

public class MemberLookupForNotesDto
{
    public int MemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ApartmentNumber { get; set; }
    public string? BuildingCodeOrName { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}
