using Business.Dtos;
using Business.Models;


namespace Business.Interfaces;

public interface IUserService
{

    public Task<UserResult> AddUserToRole(string userId, string roleName);
    public Task<UserResult> RemoveUserFromRole(string userId, string roleName);
    public Task<UserResult> CreateUser(UserRegForm form);
    public Task<UserResult> UpdateUser(Users user);

}
