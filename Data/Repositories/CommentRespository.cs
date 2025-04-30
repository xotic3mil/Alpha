using Microsoft.EntityFrameworkCore;
using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Data.Models;

namespace Data.Repositories;

public class CommentRepository(DataContext context) : ICommentRepository
{
    private readonly DataContext _context = context;

    public async Task<RepositoryResult<IEnumerable<Comment>>> GetCommentsByProjectIdAsync(Guid projectId)
    {
        try
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.ProjectId == projectId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new Comment
                {
                    Id = c.Id,
                    ProjectId = c.ProjectId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserName = $"{c.User.FirstName} {c.User.LastName}",
                    UserAvatar = c.User.AvatarUrl ?? "/images/avatar-template-1.svg"
                })
                .ToListAsync();

            return new RepositoryResult<IEnumerable<Comment>>
            {
                Succeeded = true,
                Result = comments
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<IEnumerable<Comment>>
            {
                Succeeded = false,
                Error = $"Error retrieving comments: {ex.Message}"
            };
        }
    }

    public async Task<RepositoryResult<Comment>> CreateCommentAsync(CommentEntity comment, Guid currentUserId)
    {
        try
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(comment.UserId);

            if (user == null)
            {
                return new RepositoryResult<Comment>
                {
                    Succeeded = false,
                    Error = "User not found"
                };
            }

            var result = new Comment
            {
                Id = comment.Id,
                ProjectId = comment.ProjectId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserName = $"{user.FirstName} {user.LastName}",
                UserAvatar = user.AvatarUrl ?? "/images/avatar-template-1.svg",
                IsCurrentUser = comment.UserId == currentUserId
            };

            return new RepositoryResult<Comment>
            {
                Succeeded = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<Comment>
            {
                Succeeded = false,
                Error = $"Error creating comment: {ex.Message}"
            };
        }
    }

    public async Task<RepositoryResult<bool>> DeleteCommentAsync(Guid commentId, Guid currentUserId)
    {
        try
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return new RepositoryResult<bool>
                {
                    Succeeded = false,
                    Error = "Comment not found"
                };
            }

            if (comment.UserId != currentUserId)
            {
                var isAdmin = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == currentUserId && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin"));

                if (!isAdmin)
                {
                    return new RepositoryResult<bool>
                    {
                        Succeeded = false,
                        Error = "You don't have permission to delete this comment"
                    };
                }
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return new RepositoryResult<bool>
            {
                Succeeded = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return new RepositoryResult<bool>
            {
                Succeeded = false,
                Error = $"Error deleting comment: {ex.Message}"
            };
        }
    }
}