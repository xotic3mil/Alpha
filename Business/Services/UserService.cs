using Business.Dtos;
using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Domain.Extensions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Business.Services;

public class UserService(IUserRepository userRepository, UserManager<UserEntity> userManager, RoleManager<RoleEntity> roleManager ) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly RoleManager<RoleEntity> _roleManager = roleManager;


    public async Task<UserResult> GetUsersAsync()
    {
        var result = await _userRepository.GetAllAsync();
        return result.MapTo<UserResult>();
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
        var user = await _userManager.FindByEmailAsync(form.Email);
        if (user != null)
            return new UserResult { Succeeded = false, StatusCode = 409, Error = "User already exists." };

        var userform = UserFactory.Create(form);
        var result = await _userManager.CreateAsync(userform, form.Password);

        return result.Succeeded
            ? new UserResult { Succeeded = true, StatusCode = 201 }
            : new UserResult { Succeeded = false, StatusCode = 500, Error = "Failed to create user." };
    }


    public async Task<UserResult> UpdateUser(Users user)
    {
        var userEntity = await _userManager.FindByIdAsync(user.Id.ToString());
        if (userEntity == null)
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "User doesn't exists." };

        userEntity = UserFactory.Update(userEntity, user);

        var result = await _userManager.UpdateAsync(userEntity);

        return result.Succeeded
            ? new UserResult { Succeeded = true, StatusCode = 200 }
            : new UserResult { Succeeded = false, StatusCode = 500, Error = "Failed to update user." };
    }

}
