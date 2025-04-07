using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces;
using Business.Dtos;
using System.Security.Claims;

namespace MVC.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectComments(Guid projectId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var result = await _commentService.GetCommentsByProjectIdAsync(projectId, userId);

            if (result.Succeeded)
            {
                return Json(result.Result);
            }

            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommentForm comment)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }

                return BadRequest(ModelState);
            }

            Console.WriteLine($"Creating comment for project: {comment.ProjectId}");

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var result = await _commentService.CreateCommentAsync(comment, userId);

            if (result.Succeeded)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(result.Result);
                }
                return RedirectToAction("Index", "Project");
            }

            ModelState.AddModelError("", result.Error ?? "Failed to add comment");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return BadRequest(ModelState);
            }
            return RedirectToAction("Index", "Project");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid projectId)
        {
            Console.WriteLine($"Attempting to delete comment {id} for project {projectId}");

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var result = await _commentService.DeleteCommentAsync(id, userId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }
                return StatusCode(result.StatusCode, result.Error);
            }

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Project");
            }

            ViewBag.ErrorMessage = result.Error;
            return RedirectToAction("Index", "Project");
        }
    }
}