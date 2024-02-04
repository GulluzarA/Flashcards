using System.ComponentModel.DataAnnotations;

namespace Flashcards.Models;

public class Deck
{
    public int DeckId { get; set; }

    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ.  \-]{2,50}", ErrorMessage = "The title must contain 2 to 50 characters."),
     Display(Name = "Deck title")]
    public string Name { get; set; } = default!;

    [StringLength(150, ErrorMessage = "Max length is 150.")]
    public string? Description { get; set; }
    
    // navigation property
    public virtual Subject? Subject { get; set; }
    public int SubjectId { get; set; }

    // navigation property
    public virtual List<Card>? Cards { get; set; }
}