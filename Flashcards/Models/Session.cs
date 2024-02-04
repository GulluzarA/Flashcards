namespace Flashcards.Models;

public class Session
{
    public int SessionId { get; set; }
    public string UserId { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    // navigation properties
    public virtual Deck Deck { get; set; } = default!;
    public int DeckId { get; set; }
    public virtual List<CardResult> CardResults { get; set; } = new();
}