using Domain.Models;
using System.Collections.Generic;

namespace MVC.Models
{
    public class HomeViewModel
    {
        public int ProjectCount { get; set; }
        public int UserCount { get; set; }
        public int StatusCount { get; set; }
        public int RoleCount { get; set; }
        public int CustomerCount { get; set; }
        public int ServiceCount { get; set; }

        public List<string> ProjectStatusLabels { get; set; } = new List<string>();
        public List<int> ProjectStatusCounts { get; set; } = new List<int>();

        public List<ActivityItem> RecentActivities { get; set; } = new List<ActivityItem>();
        public List<TopCustomerDto> TopCustomers { get; set; } = new List<TopCustomerDto>();
        public List<Service> RecentServices { get; set; } = new List<Service>();
    }

    public class ActivityItem
    {
        public string UserName { get; set; }
        public string UserAvatarUrl { get; set; }
        public string Action { get; set; }
        public string TimeAgo { get; set; }
    }

    public class TopCustomerDto
    {
        public string CompanyName { get; set; }
        public int ProjectCount { get; set; }
        public decimal TotalBudget { get; set; }
        public string Status { get; set; }
        public string StatusClass { get; set; } // For the badge color classes
    }
}