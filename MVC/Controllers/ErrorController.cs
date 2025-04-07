using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            switch (statusCode)
            {
                case 404:
                    errorViewModel.Title = "Page Not Found";
                    errorViewModel.Message = "Sorry, the page you are looking for doesn't exist.";
                    errorViewModel.StatusCode = statusCode;
                    break;
                case 403:
                    errorViewModel.Title = "Access Denied";
                    errorViewModel.Message = "You don't have permission to access this resource.";
                    errorViewModel.StatusCode = statusCode;
                    break;
                default:
                    errorViewModel.Title = "Error";
                    errorViewModel.Message = "An error occurred while processing your request.";
                    errorViewModel.StatusCode = statusCode;
                    break;
            }

            return View("Error", errorViewModel);
        }

        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Title = "Error",
                Message = "An unexpected error occurred. Please try again later.",
                Path = exceptionDetails?.Path
            };

            return View(errorViewModel);
        }
    }
}