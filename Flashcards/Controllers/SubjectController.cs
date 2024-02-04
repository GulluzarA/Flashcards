using Flashcards.Authorization;
using Flashcards.DAL;
using Flashcards.Logging;
using Flashcards.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Flashcards.Controllers;

public class SubjectController : Controller
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<SubjectController> _logger;

    public SubjectController(ISubjectRepository subjectRepository, UserManager<IdentityUser> userManager,
        IAuthorizationService authorizationService, ILogger<SubjectController> logger)
    {
        _subjectRepository = subjectRepository;
        _userManager = userManager;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    // Default title
    [ViewData] public string Title { get; set; } = "Subjects";

    // Status code for error view
    [TempData] public int? ErrorCode { get; set; }

    // message for error view
    [TempData] public string? ErrorMsg { get; set; }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Get the current user
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogError("{FormatError}",
                    ErrorHandling.FormatLog(ControllerContext, "User not found."));

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "User not found.";

                return View("Errors/StatusCodes");
            }

            // Get all subjects from database
            var subjects = await _subjectRepository.GetAllByUserId(userId);

            // Return the view
            return View(subjects);
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }

    public async Task<IActionResult> Public()
    {
        try
        {
            // Get all public subjects from database
            var subjects = await _subjectRepository.GetAllPublic();
            
            // Return the view
            return View(subjects);
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        try
        {
            // Return the view
            return View(new Subject());
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
    public async Task<IActionResult> Create(Subject subject)
    {
        try
        {
            // Set the owner
            subject.OwnerId = _userManager.GetUserId(User);
            if (subject.OwnerId == null)
            {
                _logger.LogError("{FormatError}",
                    ErrorHandling.FormatLog(ControllerContext, "User not found, creation failed."));

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "User not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("{FormatError} Subject: {@Subject}",
                    ErrorHandling.FormatLog(ControllerContext, "Model is invalid."), subject);
                return View(subject);
            }

            // Create the subject
            await _subjectRepository.Create(subject);

            // Redirect to the subject page
            return RedirectToAction("Index", "Subject", subject.SubjectId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(int subjectId)
    {
        try
        {
            // Get the subject from database
            var subject = await _subjectRepository.GetSubjectById(subjectId);

            // Check if the subject exists
            if (subject == null)
            {
                _logger.LogError("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "subject not found."), subjectId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Subject not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to update the subject
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, subject, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized access."), subjectId);
                return Forbid();
            }

            // Return the view
            return View(subject);
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
    public async Task<IActionResult> Update([Bind("SubjectId,Name,Description,Visibility")] Subject subject)
    {
        try
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("{FormatError} Subject: {@Subject}",
                    ErrorHandling.FormatLog(ControllerContext, "Model is invalid."), subject);
                return View(subject);
            }

            var oldSubject = await _subjectRepository.GetSubjectById(subject.SubjectId);

            if (oldSubject == null)
            {
                _logger.LogError("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "Subject not found."), subject.SubjectId);
                ViewData.ModelState.AddModelError("", "Subject not found");

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Subject not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to update the subject
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, oldSubject, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} Subject: {@Subject}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized update."), oldSubject);
                return Forbid();
            }

            // Update the subject
            oldSubject.Name = subject.Name;
            oldSubject.Description = subject.Description;
            oldSubject.Visibility = subject.Visibility;
            await _subjectRepository.Update(oldSubject);

            // Redirect to the subject page
            return RedirectToAction("Index", "Subject", subject.SubjectId.ToString());
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
    public async Task<IActionResult> Delete(int subjectId)
    {
        try
        {
            // Get the subject from database
            var subject = await _subjectRepository.GetSubjectById(subjectId);

            // Check if the subject exists
            if (subject == null)
            {
                _logger.LogError("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "Subject not found."), subjectId);

                Response.StatusCode = 404;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Subject not found.";

                return View("Errors/StatusCodes");
            }

            // Check if the user is authorized to delete the subject
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, subject, Operations.Delete);
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "Attempt for unauthorized delete."), subjectId);
                return Forbid();
            }

            // Delete the subject
            var result = await _subjectRepository.Delete(subjectId);

            // Check if the subject was deleted
            if (!result)
            {
                _logger.LogError("{FormatError} SubjectId: {SubjectId}",
                    ErrorHandling.FormatLog(ControllerContext, "Subject delete failed."), subjectId);

                Response.StatusCode = 400;
                TempData["ErrorCode"] = Response.StatusCode;
                TempData["ErrorMsg"] = "Couldn't delete subject";

                return View("Errors/StatusCodes");
            }

            // Redirect to the subject page
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }
    }
}