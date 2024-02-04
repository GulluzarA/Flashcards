using Flashcards.Models;

namespace Flashcards.DAL;

public interface ISessionRepository
{
    Task Create(Session session);
    Task Update(Session session);
    Task<Session?> GetSessionById(int sessionId);
    Task<Session?> GetActiveSession(int deckId, string userId);
    Task<Card?> GetNextCard(int sessionId);
}

