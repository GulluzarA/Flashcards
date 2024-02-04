using Flashcards.Authorization;
using Flashcards.DAL;
using Flashcards.Logging;
using Flashcards.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flashcards.Controllers;

public class DeckController : Controller
{
    private readonly IDeckRepository _deckRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<DeckController> _logger;


    public DeckController(IDeckRepository deckRepository, ISubjectRepository subjectRepository,
        IAuthorizationService authorizationService, ILogger<DeckController> logger)
    {
        _deckRepository = deckRepository;
        _subjectRepository = subjectRepository;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    // Default title
    [ViewData] public string Title { get; set; } = "Deck";
    // Status code for error view
    [TempData] public int? ErrorCode { get; set; }
    // message for error view
    [TempData] public string? ErrorMsg { get; set; }

    public async Task<IActionResult> Details(int deckId)
    {
        try
        {
            // Get the deck
            var deck = await _deckRepository.GetDeckById(deckId);

            // Check if the deck exists
            if (deck == null)
            {
                _logger.LogError("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Deck not found."), deckId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Deck not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to view the deck
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, deck.Subject, Operations.Read);

            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), deckId);
                return Forbid();
            }

            return View(deck);
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(int subjectId)
    {
        try
        {
            // Check if the subject exists
            var subject = await _subjectRepository.GetSubjectById(subjectId);
            if (subject == null) 
            {
                _logger.LogError("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "Subject not found."), subjectId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Subject not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to create the deck for the subject
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, subject, Operations.Create);

            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), subjectId);
                return Forbid();
            }

            // Assigns subjectId for deck
            var deck = new Deck()
            {
                SubjectId = subjectId
            };
            return View(deck);
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Deck deck)
    {
        try
        {
            // Check if the model is valid
            if (!ModelState.IsValid) 
            {
                _logger.LogWarning("{FormatError} Deck: {@Deck}",
                    ErrorHandling.FormatLog(ControllerContext, "Model is invalid."), deck);
                return View(deck);
            } 

            // Check if the subject exists
            deck.Subject = await _subjectRepository.GetSubjectById(deck.SubjectId);
            if (deck.Subject == null)
            {
                _logger.LogError("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "Subject not found."), deck.SubjectId);

                Response.StatusCode = 400;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Subject not found.";

                return View("Errors/StatusCodes");
            } 
            
            // Check if the user is authorized to create a deck for the subject
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, deck.Subject, Operations.Create);
            if (!authorizationResult.Succeeded) 
            {
                _logger.LogWarning("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized creation."), deck.SubjectId);
                return Forbid();
            } 

            // Create the deck
            await _deckRepository.Create(deck);

            // Redirect to the deck page
            return RedirectToAction("Details", "Deck", new { deckId = deck.DeckId });
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(int deckId)
    {
        try 
        {
            // Get the deck from database
            var deck = await _deckRepository.GetDeckById(deckId);

            // Check if the deck exists
            if (deck == null)
            {
                _logger.LogError("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Deck not found."), deckId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Deck not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to update the deck
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, deck, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized update."), deckId);
                return Forbid();
            }

            return View(deck);
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([Bind("DeckId,Name,Description")] Deck deck)
    {
        try
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("{FormatError} Deck: {@Deck}",
                    ErrorHandling.FormatLog(ControllerContext, "Model is invalid."), deck);
                return View(deck);
            }

            // Get the old deck from database
            var oldDeck = await _deckRepository.GetDeckById(deck.DeckId);

            if (oldDeck == null)
            {
                _logger.LogError("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Deck not found."), deck.DeckId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Deck not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to update the deck
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, oldDeck, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized update."), oldDeck.DeckId);
                return Forbid();
            }

            // Update the deck
            oldDeck.Name = deck.Name;
            oldDeck.Description = deck.Description;
            await _deckRepository.Update(oldDeck);

            // Redirect to the deck page
            return RedirectToAction("Details", "Deck", new { deckId = oldDeck.DeckId });
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int deckId)
    {
        try
        {
            // Get the deck from database
            var deck = await _deckRepository.GetDeckById(deckId);

            // Check if the deck exists
            if (deck == null)
            {
                _logger.LogError("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Deck not found."), deckId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Deck not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to delete the deck
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, deck, Operations.Delete);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized delete."), deckId);
                return Forbid();
            }

            // Delete the deck
            var result = await _deckRepository.Delete(deckId);

            // Check if the deck was deleted
            if (!result)
            {
                _logger.LogError("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Deck delete failed."), deckId);

                Response.StatusCode = 400;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Couldn't delete deck.";

                return View("Errors/StatusCodes");
            }

            return RedirectToAction("Index", "Subject", deck.SubjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }
}