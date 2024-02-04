namespace Flashcards.Models;

public class CardResult
{
    public int CardResultId { get; set; }
    public bool Correct { get; set; }
    // navigation properties
    public virtual Session Session { get; set; } = default!;
    public int SessionId { get; set; }
    public virtual Card Card { get; set; } = default!;
    public int CardId { get; set; }
}