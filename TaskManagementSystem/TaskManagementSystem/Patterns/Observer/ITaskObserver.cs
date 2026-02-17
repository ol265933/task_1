using System;
using System.Collections.Generic;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Patterns.Observer
{
    // Observer Pattern
    public interface ITaskObserver
    {
        void Update(Task task, string message);
    }

    public class NotificationManager : ITaskObserver
    {
        private List<string> _notifications = new List<string>();

        public List<string> GetNotifications()
        {
            return new List<string>(_notifications);
        }

        public void Update(Task task, string message)
        {
            string notification = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _notifications.Add(notification);
            Console.WriteLine(notification);
        }

        public void ClearNotifications()
        {
            _notifications.Clear();
        }
    }
}
