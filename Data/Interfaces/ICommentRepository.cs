using Data.Entities;
using Data.Models;
using Domain.Models;

namespace Data.Interfaces;

public interface ICommentRepository
{
    public Task<RepositoryResult<IEnumerable<Comment>>> GetCommentsByProjectIdAsync(Guid projectId);
    public Task<RepositoryResult<Comment>> CreateCommentAsync(CommentEntity comment, Guid currentUserId);
    public Task<RepositoryResult<bool>> DeleteCommentAsync(Guid commentId, Guid currentUserId);
}
