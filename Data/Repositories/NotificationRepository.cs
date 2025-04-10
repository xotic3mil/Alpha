using Data.Contexts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;

namespace Data.Repositories
{
    public class NotificationRepository(DataContext context) : INotificationRepository
    {
        private readonly DataContext _context = context;



        public async Task<IEnumerable<NotificationEntity>> GetUnreadForAdminsAsync()
        {
            return await _context.Notifications
                .Where(n => n.ForAdminsOnly && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationEntity>> GetUnreadForProjectManagersAsync()
        {
            return await _context.Notifications
                .Where(n => (n.ForAdminsOnly || n.ForProjectManagersOnly) && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationEntity>> GetUnreadForUserAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => (n.RecipientId == userId || !n.ForAdminsOnly) && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountForAdminsAsync()
        {
            return await _context.Notifications
                .CountAsync(n => n.ForAdminsOnly && !n.IsRead);
        }

        public async Task<int> GetUnreadCountForProjectManagersAsync()
        {
            return await _context.Notifications
                .CountAsync(n => (n.ForAdminsOnly || n.ForProjectManagersOnly) && !n.IsRead);
        }

        public async Task<int> GetUnreadCountForUserAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => (n.RecipientId == userId || !n.ForAdminsOnly) && !n.IsRead);
        }

        public async Task<NotificationEntity> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task CreateAsync(NotificationEntity notification)
        {
            if (notification.Id == Guid.Empty)
            {
                notification.Id = Guid.NewGuid();
            }

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}
