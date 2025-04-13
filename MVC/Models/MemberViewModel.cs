using Business.Dtos;
using Data.Entities;
using Domain.Models;

namespace MVC.Models
{
    public class MemberViewModel
    {
        public IEnumerable<User> Users { get; set; } = new List<User>();

        public UserEntity User { get; set; }

        public UserRegForm Form { get; set; } = new UserRegForm();
    }

}
