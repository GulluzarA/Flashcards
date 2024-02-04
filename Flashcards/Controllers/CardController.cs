using Flashcards.Authorization;
using Flashcards.DAL;
using Flashcards.Logging;
using Flashcards.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flashcards.Controllers;

public class CardController : Controller
{
    private readonly ICardRepository _cardRepository;
    private readonly IDeckRepository _deckRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<CardController> _logger;

    public CardController(ICardRepository cardRepository, IDeckRepository deckRepository,
        IAuthorizationService authorizationService, ILogger<CardController> logger)
    {
        _cardRepository = cardRepository;
        _deckRepository = deckRepository;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    // Default title
    [ViewData] public string Title { get; set; } = "Cards";
    // Status code for error view
    [TempData] public int? ErrorCode { get; set; }
    // message for error view
    [TempData] public string? ErrorMsg { get; set; }

    [HttpGet]
    public async Task<IActionResult> Create(int deckId)
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
                User, deck.Subject, Operations.Create);

            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), deckId);
                return Forbid();
            }

            var card = new Card()
            {
                DeckId = deckId
            };
            return View(card);
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
    public async Task<IActionResult> Create(Card card)
    {
        try
        {
            // Get the deck
            card.Deck = await _deckRepository.GetDeckById(card.DeckId);

            // Check if the deck exists
            if (card.Deck == null)
            {
                _logger.LogError("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Deck not found."), card.DeckId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Deck not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("{FormatError} Card: {@Card}",
                    ErrorHandling.FormatLog(ControllerContext, "Model is invalid."), card);
                return View(card);
            }
            
            // Check if the user is authorized to create a card in the deck
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, card, Operations.Create);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} DeckId: {DeckId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized creation."), card.DeckId);
                return Forbid();
            }

            // Create the card
            await _cardRepository.Create(card);

            // Redirect to the deck page
            return RedirectToAction("Details", "Deck", new { deckId = card.DeckId });
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(int cardId)
    {
        try 
        {
            // Get the card
            var card = await _cardRepository.GetCardById(cardId);

            // Check if the card exists
            if (card == null)
            {
                _logger.LogError("{FormatError} CardId: {CardId}",
                    ErrorHandling.FormatLog(ControllerContext, "Card not found."), cardId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Card not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to update the card
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, card, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} CardId: {CardId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), cardId);
                return Forbid();
            }
            return View(card);
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
    public async Task<IActionResult> Update([Bind("CardId,Front,Back")] Card card)
    {
        try
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("{FormatError} Card: {@Card}",
                    ErrorHandling.FormatLog(ControllerContext, "Model is invalid."), card);
                return View(card);
            }
            
            // Get the old card
            var oldCard = await _cardRepository.GetCardById(card.CardId);
            if (oldCard == null)
            {
                _logger.LogError("{FormatError} CardId: {CardId}",
                    ErrorHandling.FormatLog(ControllerContext, "Card not found."), card.CardId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Card not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to update the card
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, oldCard, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} CardId: {CardId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized update."), oldCard.CardId);
                return Forbid();
            }

            // Update the card
            oldCard.Front = card.Front;
            oldCard.Back = card.Back;
            await _cardRepository.Update(oldCard);

            // Redirect to the deck page
            return RedirectToAction("Details", "Deck", new { deckId = oldCard.DeckId });
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }

    }

    // Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int cardId)
    {
        try
        {
            var card = await _cardRepository.GetCardById(cardId);

            // Check if the card exists
            if (card == null)
            {
                _logger.LogError("{FormatError} CardId: {CardId}",
                    ErrorHandling.FormatLog(ControllerContext, "Card not found."), cardId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Card not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to delete the card
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, card, Operations.Delete);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} CardId: {CardId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized delete."), cardId);
                return Forbid();
            }

            // Delete the card
            var result = await _cardRepository.Delete(cardId);

            // Check if the card was deleted
            if (!result)
            {
                _logger.LogError("{FormatError} CardId: {CardId}",
                    ErrorHandling.FormatLog(ControllerContext, "Card delete failed."), cardId);
                Response.StatusCode = 400;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Couldn't delete card.";

                return View("Errors/StatusCodes");
            }

            // Redirect to the deck page
            return RedirectToAction("Details", "Deck", new { card.DeckId });
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }
}