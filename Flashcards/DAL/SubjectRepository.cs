using Flashcards.Models;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.DAL;

public class SubjectRepository : ISubjectRepository
{
    private readonly FlashcardsDbContext _db;

    public SubjectRepository(FlashcardsDbContext db)
    {
        _db = db;
    }
    
    public async Task Create(Subject subject)
    {
        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync();
    }
    
    public async Task<Subject?> GetSubjectById(int id)
    {
        return await _db.Subjects.FindAsync(id);
    }
    
    public async Task<IEnumerable<Subject>> GetAllByUserId(string userId)
    {
        return await _db.Subjects.Where(s => s.OwnerId == userId).ToListAsync();
    }

    public async Task<IEnumerable<Subject>> GetAllPublic()
    {
        return await _db.Subjects
            .Where(s => s.Decks != null && s.Visibility == SubjectVisibility.Public).ToListAsync();
    }

    public async Task Update(Subject subject)
    {
        _db.Subjects.Update(subject);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> Delete(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null)
        {
            return false;
        }

        _db.Subjects.Remove(subject);
        await _db.SaveChangesAsync();
        return true;
    }
}