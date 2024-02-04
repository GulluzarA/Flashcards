using Flashcards.Models;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.DAL;

public class SessionRepository : ISessionRepository
{
    private readonly FlashcardsDbContext _db;

    public SessionRepository(FlashcardsDbContext db)
    {
        _db = db;
    }

    public async Task Create(Session session)
    {
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();
    }

    public async Task Update(Session session)
    {
        _db.Sessions.Update(session);
        await _db.SaveChangesAsync();
    }

    public async Task<Session?> GetSessionById(int sessionId)
    {
        return await _db.Sessions.FindAsync(sessionId);
    }

    public async Task<Session?> GetActiveSession(int deckId, string userId)
    {
        // Find a Session that is in progress for the given deck and user
        var sessionId = await (
            from s in _db.Sessions
            where s.IsActive && s.DeckId == deckId && s.UserId == userId
            select s.SessionId).FirstOrDefaultAsync();

        return await _db.Sessions.FindAsync(sessionId);
    }

    public async Task<Card?> GetNextCard(int sessionId)
    {
        // Query to get session from db
        var sessionQuery =
            from s in _db.Sessions
            where s.SessionId == sessionId
            select s;

        // Get the first session
        var session = await sessionQuery.SingleOrDefaultAsync();
        if (session?.Deck.Cards == null) return null;

        // Shuffle the cards in the deck using Random
        session.Deck.Cards = session.Deck.Cards
            .OrderBy(_ => Guid.NewGuid()) // randomize the order
            .ToList();

        var correctCards = session.CardResults
            .Where(cr => cr.Correct)
            .Select(cr => cr.Card)
            .ToList();

        var incorrectCards = session.CardResults
            .Where(cr => !cr.Correct)
            .Select(cr => cr.Card)
            .ToList();

        var lastAnsweredCard = session.CardResults
            .OrderByDescending(cr => cr.CardResultId)
            .Select(cr => cr.Card).Take(1).ToList();

        // Get next unanswered card
        var nextCard = session.Deck.Cards
            .Except(correctCards)
            .Except(incorrectCards)
            .FirstOrDefault();

        // Get next card that was answered incorrectly
        nextCard ??= session.Deck.Cards.Except(correctCards).Except(lastAnsweredCard).FirstOrDefault();

        // Get the next card that was answered correctly even if it is the last card
        nextCard ??= session.Deck.Cards.Except(correctCards).FirstOrDefault();

        return nextCard;
    }
}