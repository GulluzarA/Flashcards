using Flashcards.Models;

namespace Flashcards.viewModels;

public class PracticeViewModel
{
    public int SessionId;
    public string? Title;
    public Card? Card;

    public PracticeViewModel(int sessionId, Card? card, string? title)
    {
        SessionId = sessionId;
        Card = card;
        Title = title;
    }
}