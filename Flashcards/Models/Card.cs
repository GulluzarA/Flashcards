using System.ComponentModel.DataAnnotations;

namespace Flashcards.Models;

public class Card
{
    public int CardId { get; set; }
    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ.  \-]{2,120}", ErrorMessage = "The front must contain 2 to 160 characters."),
     Display(Name = "Card front ")]
    public string Front { get; set; } = string.Empty;
    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ.  \-]{2,120}", ErrorMessage = "The back must contain 2 to 160 characters."),
     Display(Name = "Card back")]
    public string Back { get; set; } = string.Empty;
  
    
    // navigation property
    public virtual Deck? Deck { get; set; }
    public int DeckId { get; set; }
    public virtual List<CardResult>? CardResults { get; set; }
}