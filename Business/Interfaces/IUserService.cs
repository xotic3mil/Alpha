using Business.Dtos;
using Domain.Models;

namespace Business.Interfaces;

public interface IUserService
{

    public Task<UserResult> AddUserToRole(string userId, string roleName);
    public Task<UserResult> RemoveUserFromRole(string userId, string roleName);
    public Task<UserResult> CreateUser(UserRegForm form);
    public Task<UserResult> UpdateUser(User user);
    public Task<UserResult<List<User>>> GetUsersAsync();
    public Task<UserResult<User>> DeleteUserAsync(Guid id);

}
