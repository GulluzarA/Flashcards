using System.ComponentModel.DataAnnotations;

namespace Flashcards.Models;

public class Subject
{
    public int SubjectId { get; set; }
    
    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ.  \-]{2,50}", ErrorMessage = "The subject name must contain 2 to 50 characters.")]
    [Display(Name = "Subject name")]
    public string Name { get; set; } = string.Empty;
    
    public string? OwnerId { get; set; }
    
    [StringLength(150, ErrorMessage = "Max length is 150.")]
    public string? Description { get; set; }
    
    public SubjectVisibility Visibility { get; set; } = SubjectVisibility.Private;
    
    // navigation property
    public virtual List<Deck>? Decks { get; set; }
}

public enum SubjectVisibility
{
    Public,
    Private
}