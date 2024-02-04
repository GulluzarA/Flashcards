using Flashcards.Authorization;
using Flashcards.DAL;
using Flashcards.Logging;
using Flashcards.Models;
using Flashcards.viewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Flashcards.Controllers;

public class PracticeController : Controller
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IDeckRepository _deckRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<PracticeController> _logger;

    public PracticeController(IDeckRepository deckRepository, UserManager<IdentityUser> userManager,
        IAuthorizationService authorizationService, ISessionRepository sessionRepository,
        ILogger<PracticeController> logger)
    {
        _sessionRepository = sessionRepository;
        _deckRepository = deckRepository;
        _authorizationService = authorizationService;
        _userManager = userManager;
        _logger = logger;
    }

    // Default title
    [ViewData] public string Title { get; set; } = "Practice";

    // Status code for error view
    [TempData] public int? ErrorCode { get; set; }

    // message for error view
    [TempData] public string? ErrorMsg { get; set; }

    [HttpGet]
    public async Task<IActionResult> Index(int deckId)
    {
        try
        {
            // Check if there is a session in progress
            var userId = _userManager.GetUserId(User);
            var session = await _sessionRepository.GetActiveSession(deckId, userId);

            //  Check if user is authenticated
            if (userId == null)
            {
                _logger.LogError("{FormatError}",
                    ErrorHandling.FormatLog(ControllerContext, "User not found"));
                return Forbid();
            }

            // Create a new session if there is no active session
            if (session == null)
            {
                // Check if the deck exists
                var deck = await _deckRepository.GetDeckById(deckId);
                if (deck == null)
                {
                    _logger.LogError("{FormatError} DeckId: {DeckId}",
                        ErrorHandling.FormatLog(ControllerContext, "Deck not found."), deckId);

                    Response.StatusCode = 404;
                    TempData["ErrorCode"] = Response.StatusCode;
                    TempData["ErrorMsg"] = "Deck not found.";

                    return View("Errors/StatusCodes");
                }

                // Check if the user is authorized to read the deck
                var deckAuthorizationResult = await _authorizationService.AuthorizeAsync(User, deck, Operations.Read);
                if (!deckAuthorizationResult.Succeeded)
                {
                    _logger.LogWarning("{FormatError} DeckId: {DeckId}",
                        ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), deckId);
                    return Forbid();
                }

                // Check if cards exists
                if (deck.Cards != null)
                {
                    // Check if there are cards in the deck
                    if (!deck.Cards.Any())
                    {
                        _logger.LogError("{FormatError} DeckId: {DeckId}",
                            ErrorHandling.FormatLog(ControllerContext, "No cards in Deck."), deckId);

                        Response.StatusCode = 404;
                        TempData["ErrorCode"] = Response.StatusCode;
                        TempData["ErrorMsg"] = "No cards in Deck.";

                        return View("Errors/StatusCodes");
                    }
                }

                // Create the session
                session = new Session
                {
                    Deck = deck,
                    DeckId = deck.DeckId,
                    UserId = _userManager.GetUserId(User),
                };

                // Add the session to the database
                await _sessionRepository.Create(session);
            }

            // Check if the user is authorized to read the session
            var sessionAuthorizationResult = await _authorizationService.AuthorizeAsync(User, session, Operations.Read);
            if (!sessionAuthorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} Session: {@Session}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), session);
                return Forbid();
            }

            // Get next card
            var nextCard = await _sessionRepository.GetNextCard(session.SessionId);

            // Return active session
            return View(new PracticeViewModel(session.SessionId, nextCard, session.Deck.Name));
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
    public async Task<IActionResult> Post(int sessionId, int cardId, bool gotCorrect)
    {
        try
        {
            // Get the session and card
            var session = await _sessionRepository.GetSessionById(sessionId);
            var card = session?.Deck.Cards?.FirstOrDefault(c => c.CardId == cardId);

            // check is session exists.
            if (session == null)
            {
                _logger.LogError("{FormatError} SessionId: {sessionId}",
                    ErrorHandling.FormatLog(ControllerContext, "Session not found."), sessionId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Session not found.";

                return View("Errors/StatusCodes");
            }

            // check if card exists
            if (card == null)
            {
                _logger.LogError("{FormatError} CardId: {CardId}",
                    ErrorHandling.FormatLog(ControllerContext, "Card not found."), cardId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Card not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to edit the session
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, session, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} Session: {@Session}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), session);
                return Forbid();
            }

            // Add the card result
            session.CardResults.Add(new CardResult
                { CardId = cardId, SessionId = session.SessionId, Correct = gotCorrect });
            await _sessionRepository.Update(session);

            // Return to index
            return RedirectToAction("Index", new { deckId = session.DeckId });
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Summary(int sessionId)
    {
        try
        {
            // Get session
            var session = await _sessionRepository.GetSessionById(sessionId);
            if (session == null)
            {
                _logger.LogError("{FormatError} SessionId: {sessionId}",
                    ErrorHandling.FormatLog(ControllerContext, "Session not found."), sessionId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Session not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to view the session
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, session, Operations.Read);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} Session: {@Session}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), session);
                return Forbid();
            }
            
            // If the session is active, redirect to the index page
            if (session.IsActive)
            {
                return RedirectToAction("Index", new { deckId = session.DeckId });
            }

            return View(session);
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
    public async Task<IActionResult> Finish(int sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetSessionById(sessionId);
            if (session == null)
            {
                _logger.LogError("{FormatError} SessionId: {sessionId}",
                    ErrorHandling.FormatLog(ControllerContext, "Session not found."), sessionId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Session not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to edit the session
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, session, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} Session: {@Session}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), session);
                return Forbid();
            }

            session.IsActive = false;
            await _sessionRepository.Update(session);

            // Redirect to the result page
            return RedirectToAction("Summary", "Practice", new { sessionId = session.SessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }
}