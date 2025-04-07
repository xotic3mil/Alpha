using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using Domain.Models;

namespace MVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IProjectsService _projectService;
        private readonly IStatusTypeService _statusService;
        private readonly IServicesService _serviceService;
        private readonly ICustomersService _customerService;
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<RoleEntity> _roleManager;

        public HomeController(
            IProjectsService projectService,
            IStatusTypeService statusService,
            IServicesService serviceService,
            ICustomersService customerService,
            UserManager<UserEntity> userManager,
            RoleManager<RoleEntity> roleManager)
        {
            _projectService = projectService;
            _statusService = statusService;
            _serviceService = serviceService;
            _customerService = customerService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel();

            var projectsResult = await _projectService.GetProjectsAsync();
            if (projectsResult.Succeeded)
            {
                var projects = projectsResult.Result?.ToList() ?? new List<Project>();
                model.ProjectCount = projects.Count;

                var statusGroups = projects
                    .GroupBy(p => p.Status?.StatusName ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count());

                model.ProjectStatusLabels = new List<string> { "Completed", "In Progress", "On Hold", "Delayed" };
                model.ProjectStatusCounts = model.ProjectStatusLabels
                    .Select(label => statusGroups.ContainsKey(label) ? statusGroups[label] : 0)
                    .ToList();
            }

            model.UserCount = await _userManager.Users.CountAsync();

            var statusResult = await _statusService.GetStatusesAsync();
            model.StatusCount = statusResult.Succeeded ? statusResult.Result?.Count() ?? 0 : 0;

            model.RoleCount = await _roleManager.Roles.CountAsync();

            var customersResult = await _customerService.GetCustomersAsync();
            if (customersResult.Succeeded)
            {
                var customers = customersResult.Result?.ToList() ?? new List<Domain.Models.Customer>();
                model.CustomerCount = customers.Count;

                var servicesResult = await _serviceService.GetServicesAsync();
                if (servicesResult.Succeeded)
                {
                    var services = servicesResult.Result?.ToList() ?? new List<Domain.Models.Service>();
                    model.ServiceCount = services.Count;

                    model.RecentServices = services
                        .OrderByDescending(s => s.Id) 
                        .Take(5)
                        .ToList();
                }
                model.RecentActivities = new List<ActivityItem>
            {
                new ActivityItem {
                    UserName = "John Doe",
                    UserAvatarUrl = "/images/avatar-template-1.svg",
                    Action = "created a new project",
                    TimeAgo = "5 minutes ago"
                },
                new ActivityItem {
                    UserName = "Jane Smith",
                    UserAvatarUrl = "/images/avatar-template-1.svg",
                    Action = "updated the project status",
                    TimeAgo = "1 hour ago"
                },
                new ActivityItem {
                    UserName = "Mike Johnson",
                    UserAvatarUrl = "/images/avatar-template-1.svg",
                    Action = "added a new customer",
                    TimeAgo = "3 hours ago"
                },
                new ActivityItem {
                    UserName = "Sara Wilson",
                    UserAvatarUrl = "/images/avatar-template-1.svg",
                    Action = "added a new service",
                    TimeAgo = "1 day ago"
                }
            };

               
            }
            return View(model);
        }
    }

}