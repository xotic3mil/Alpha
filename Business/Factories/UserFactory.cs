using Business.Dtos;
using Business.Models;
using Data.Entities;


namespace Business.Factories
{
    public class UserFactory
    {
        public static UserRegForm Create() => new();

        public static UserEntity Create(UserRegForm form) => new()
        {
            FirstName = form.FirstName,
            LastName = form.LastName,
            Email = form.Email,
            PhoneNumber = form.PhoneNumber,
            UserName = form.Email
    
        };

        public static Users Create(UserEntity entity) => new()
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
        };

        public static UserEntity Create(Users user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UserName = user.Email
        };

        public static UserEntity Update(UserEntity entity, Users user)
        {
            entity.FirstName = user.FirstName;
            entity.LastName = user.LastName;
            entity.Email = user.Email;
            entity.PhoneNumber = user.PhoneNumber;
            entity.UserName = user.Email;
            return entity;
        }
    }
}
