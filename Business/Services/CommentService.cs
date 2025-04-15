using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;

namespace Business.Services
{
    public class CommentService(ICommentRepository commentRepository) : ICommentService
    {

        private readonly ICommentRepository _commentRepository = commentRepository;


        public async Task<CommentResult<IEnumerable<Comment>>> GetCommentsByProjectIdAsync(Guid projectId, Guid currentUserId)
        {
            var result = await _commentRepository.GetCommentsByProjectIdAsync(projectId);

            if (result.Succeeded && result.Result != null)
            {
                foreach (var comment in result.Result)
                {
                    comment.IsCurrentUser = comment.UserId == currentUserId;
                }
            }

            return new CommentResult<IEnumerable<Comment>>
            {
                Succeeded = result.Succeeded,
                StatusCode = result.Succeeded ? 200 : 500,
                Error = result.Error,
                Result = result.Result
            };
        }

        public async Task<CommentResult<Comment>> CreateCommentAsync(CommentForm dto, Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                return new CommentResult<Comment>
                {
                    Succeeded = false,
                    StatusCode = 400,
                    Error = "Comment content cannot be empty"
                };
            }

            var commentEntity = new CommentEntity
            {
                ProjectId = dto.ProjectId,
                UserId = currentUserId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _commentRepository.CreateCommentAsync(commentEntity, currentUserId);

            return new CommentResult<Comment>
            {
                Succeeded = result.Succeeded,
                StatusCode = result.Succeeded ? 201 : 500,
                Error = result.Error,
                Result = result.Result
            };
        }

        public async Task<CommentResult> DeleteCommentAsync(Guid commentId, Guid currentUserId)
        {
            var result = await _commentRepository.DeleteCommentAsync(commentId, currentUserId);

            return new CommentResult
            {
                Succeeded = result.Succeeded,
                StatusCode = result.Succeeded ? 200 :
                            result.Error?.Contains("permission") == true ? 403 : 500,
                Error = result.Error
            };
        }




    }
}
