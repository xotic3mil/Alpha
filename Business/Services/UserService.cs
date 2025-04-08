using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Business.Services;

public class UserService(IUserRepository userRepository, UserManager<UserEntity> userManager, RoleManager<RoleEntity> roleManager ) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly RoleManager<RoleEntity> _roleManager = roleManager;


    public async Task<UserResult<List<User>>> GetUsersAsync()
    {
        try
        {
            var userEntities = await _userManager.Users.ToListAsync();

            var users = userEntities.Select(entity =>
            {
                var user = entity.MapTo<User>();
                if (string.IsNullOrEmpty(user.AvatarUrl)) { user.AvatarUrl = "/images/member-template-1.svg"; }
                return user;
            }).ToList();

            return new UserResult<List<User>> { Succeeded = true, StatusCode = 200, Result = users };
        }
        catch (Exception ex)
        {
            return new UserResult<List<User>> { Succeeded = false, StatusCode = 500, Error = $"Failed to retrieve users: {ex.Message}" };
        }
    }

    public async Task<UserResult> AddUserToRole(string userId, string roleName) 
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        return new UserResult { Succeeded = false, StatusCode = 404, Error = "Role doesn't exists." };

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        return new UserResult { Succeeded = false, StatusCode = 404, Error = "User doesn't exists." };

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded
            ? new UserResult { Succeeded = true, StatusCode = 200 }
            : new UserResult { Succeeded = false, StatusCode = 500, Error = "Failed to add user to role." };
    }

    public async Task<UserResult> RemoveUserFromRole(string userId, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "Role doesn't exists." };
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "User doesn't exists." };
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded
            ? new UserResult { Succeeded = true, StatusCode = 200 }
            : new UserResult { Succeeded = false, StatusCode = 500, Error = "Failed to remove user from role." };
    }

    public async Task<UserResult> CreateUser(UserRegForm form)
    {
        if (form == null)
            return new UserResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };

        var userEntity = form.MapTo<UserEntity>();
        userEntity.UserName = form.Email;
        userEntity.NormalizedUserName = form.Email?.ToUpper();

        var result = await _userManager.CreateAsync(userEntity, form.Password);

        return result.Succeeded
            ? new UserResult { Succeeded = true, StatusCode = 201 }
            : new UserResult { Succeeded = false, StatusCode = 500, Error = $"Failed to create user" };
    }


    public async Task<UserResult> UpdateUser(User user)
    {
        var userEntity = await _userManager.FindByIdAsync(user.Id.ToString());
        if (userEntity == null)
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "User doesn't exists." };

        var result = await _userManager.UpdateAsync(userEntity);

        return result.Succeeded
            ? new UserResult { Succeeded = true, StatusCode = 200 }
            : new UserResult { Succeeded = false, StatusCode = 500, Error = "Failed to update user." };
    }

    public async Task<UserResult<User>> DeleteUserAsync(Guid id)
    {
        var userEntity = await _userManager.FindByIdAsync(id.ToString());

        if (userEntity == null)
        {
            return new UserResult<User> { Succeeded = false, StatusCode = 404, Error = $"User with ID '{id}' not found." };
        }

        var deleteResponse = await _userManager.DeleteAsync(userEntity);

        return deleteResponse.Succeeded
            ? new UserResult<User> { Succeeded = true, StatusCode = 200 }
            : new UserResult<User> { Succeeded = false, StatusCode = 500, Error = "Failed to delete user." };
    }

}
