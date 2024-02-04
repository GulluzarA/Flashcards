using Flashcards.Models;

namespace Flashcards.DAL;

public interface ICardRepository
{
    Task<Card?> GetCardById(int id);
    Task Create(Card card);
    Task Update(Card card);
    Task<bool> Delete(int id);
}