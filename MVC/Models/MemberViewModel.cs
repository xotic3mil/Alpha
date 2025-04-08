using Business.Dtos;
using Domain.Models;

namespace MVC.Models
{
    public class MemberViewModel
    {
        public IEnumerable<User> Users { get; set; } = new List<User>();

        public UserRegForm Form { get; set; } = new UserRegForm();
    }

}
