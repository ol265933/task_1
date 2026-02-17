using System;
using System.Collections.Generic;
using TaskManagementSystem.Patterns.State;
using TaskManagementSystem.Patterns.Observer;

namespace TaskManagementSystem.Models
{
    public enum TaskType
    {
        Regular,
        Urgent,
        Recurring
    }

    public enum UserRole
    {
        Administrator,
        Manager,
        Executor
    }

    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskType Type { get; set; }
        public int? AssignedUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int? RecurringDays { get; set; } // Для повторяющихся задач
        
        // State Pattern - текущее состояние
        private ITaskState _state;
        public string StateName => _state?.GetType().Name ?? "CreatedState";

        private List<ITaskObserver> _observers = new List<ITaskObserver>();

        public Task()
        {
            CreatedAt = DateTime.Now;
            _state = new CreatedState();
        }

        // State Pattern methods
        public void SetState(ITaskState state)
        {
            _state = state;
            NotifyObservers($"Task '{Title}' changed to {StateName}");
        }

        public void MoveNext()
        {
            _state?.MoveNext(this);
        }

        public void MovePrevious()
        {
            _state?.MovePrevious(this);
        }

        // Observer Pattern methods
        public void Attach(ITaskObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Detach(ITaskObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers(string message)
        {
            foreach (var observer in _observers)
            {
                observer.Update(this, message);
            }
        }

        public override string ToString()
        {
            return $"[{Type}] {Title} - {StateName} (ID: {Id})";
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UserRole Role { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Role})";
        }
    }
}
