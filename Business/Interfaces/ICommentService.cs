using Business.Dtos;
using Domain.Models;


namespace Business.Interfaces
{
    public interface ICommentService
    {

        Task<CommentResult<IEnumerable<Comment>>> GetCommentsByProjectIdAsync(Guid projectId, Guid currentUserId);
        Task<CommentResult<Comment>> CreateCommentAsync(CommentForm dto, Guid currentUserId);
        Task<CommentResult> DeleteCommentAsync(Guid commentId, Guid currentUserId);

    }
}
