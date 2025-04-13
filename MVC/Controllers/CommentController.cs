using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Business.Interfaces;
using Business.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using MVC.Hubs;

namespace MVC.Controllers
{
    [Authorize]
    public class CommentController(ICommentService commentService, IHubContext<CommentHub> commentHubContext) : Controller
    {
        private readonly ICommentService _commentService = commentService;
        private readonly IHubContext<CommentHub> _commentHubContext = commentHubContext;

        [HttpGet]
        public async Task<IActionResult> GetProjectComments(Guid projectId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var result = await _commentService.GetCommentsByProjectIdAsync(projectId, userId);

            if (result.Succeeded)
            {
                var comments = result.Result.Select(c => new
                {
                    id = c.Id,
                    content = c.Content,
                    dateFormatted = c.CreatedAt.ToString("MMM d, yyyy h:mm tt"),
                    userId = c.UserId,
                    userName = c.UserName ?? "Unknown User",
                    userImage = c.UserAvatar ?? "/images/default-avatar.svg", 
                    canDelete = c.UserId == userId || User.IsInRole("Admin")  
                });

                return Json(comments);
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
                var commentObj = new
                {
                    id = result.Result.Id,
                    content = result.Result.Content,
                    dateFormatted = result.Result.CreatedAt.ToString("MMM d, yyyy h:mm tt"),
                    userId = result.Result.UserId,
                    userName = result.Result.UserName ?? "Unknown User",
                    userImage = result.Result.UserAvatar ?? "/images/default-avatar.svg",
                    projectId = comment.ProjectId,
                    canDelete = result.Result.UserId == userId || User.IsInRole("Admin")
                };

                await _commentHubContext.Clients.Group($"project-{comment.ProjectId}")
                    .SendAsync("ReceiveComment", commentObj);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(commentObj);  
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
                    await _commentHubContext.Clients.Group($"project-{projectId}")
                          .SendAsync("CommentDeleted", id.ToString());
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