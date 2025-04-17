using Business.Dtos;
using Data.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Business.Interfaces;

public interface IUserService
{

    public Task<UserResult> AddUserToRole(string userId, string roleName);
    public Task<UserResult> RemoveUserFromRole(string userId, string roleName);
    public Task<UserResult> CreateUser(UserRegForm form);
    public Task<UserResult> UpdateUser(User user);
    public Task<UserResult<List<User>>> GetUsersAsync();
    public Task<UserResult<User>> DeleteUserAsync(Guid id);
    public Task<UserResult<UserEntity>> CreateUserWithRoleAsync(UserRegForm form, string role = "User");
    public Task<string> ProcessAvatarImageAsync(IFormFile userImage, string webRootPath);


}
