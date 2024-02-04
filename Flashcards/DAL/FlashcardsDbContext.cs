using Flashcards.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // For authentication/authorization

namespace Flashcards.DAL;

public class FlashcardsDbContext : IdentityDbContext
{
    public FlashcardsDbContext(DbContextOptions<FlashcardsDbContext> options) : base(options)
    {
    }

    public DbSet<Subject> Subjects { get; set; } = default!;
    public DbSet<Deck> Decks { get; set; } = default!;
    public DbSet<Card> Cards { get; set; } = default!;
    public DbSet<Session> Sessions { get; set; } = default!;
    public DbSet<CardResult> CardResults { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }
}