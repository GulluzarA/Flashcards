using Microsoft.AspNetCore.Mvc;

namespace Flashcards.Logging
{
    public static class ErrorHandling
    {
        public static string FormatLog(ControllerContext context, string errorMessage)
        {
            var controllerName = context.ActionDescriptor.ControllerName; 
            var actionName = context.ActionDescriptor.ActionName;
            return $"[{controllerName}Controller/{actionName}] {errorMessage}";
        }

        public static string FormatException(ControllerContext context, string errorMessage, Exception ex)
        {
            var controllerName = context.ActionDescriptor.ControllerName;
            var actionName = context.ActionDescriptor.ActionName;
            var exMessage = ex.Message;
            return $"[{controllerName}Controller/{actionName}] {errorMessage} Exception Message: {exMessage}";
        }

    }
}
