using System;
using System.Collections.Generic;
using System.Linq;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Patterns.Strategy
{
    // Strategy Pattern для фильтрации
    public interface IFilterStrategy
    {
        List<Task> Filter(List<Task> tasks);
        string Name { get; }
    }

    public class AllTasksStrategy : IFilterStrategy
    {
        public string Name => "All Tasks";
        public List<Task> Filter(List<Task> tasks)
        {
            return tasks;
        }
    }

    public class UrgentTasksStrategy : IFilterStrategy
    {
        public string Name => "Urgent Only";
        public List<Task> Filter(List<Task> tasks)
        {
            return tasks.Where(t => t.Type == TaskType.Urgent).ToList();
        }
    }

    public class ActiveTasksStrategy : IFilterStrategy
    {
        public string Name => "Active Only";
        public List<Task> Filter(List<Task> tasks)
        {
            return tasks.Where(t => t.StateName != "CompletedState").ToList();
        }
    }

    public class CompletedTasksStrategy : IFilterStrategy
    {
        public string Name => "Completed Only";
        public List<Task> Filter(List<Task> tasks)
        {
            return tasks.Where(t => t.StateName == "CompletedState").ToList();
        }
    }

    public class RecurringTasksStrategy : IFilterStrategy
    {
        public string Name => "Recurring Only";
        public List<Task> Filter(List<Task> tasks)
        {
            return tasks.Where(t => t.Type == TaskType.Recurring).ToList();
        }
    }

    public class DateRangeFilterStrategy : IFilterStrategy
    {
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        public DateRangeFilterStrategy(DateTime startDate, DateTime endDate)
        {
            _startDate = startDate.Date;
            _endDate = endDate.Date.AddDays(1).AddSeconds(-1);
        }

        public string Name => $"Date: {_startDate:yyyy-MM-dd} to {_endDate:yyyy-MM-dd}";
        
        public List<Task> Filter(List<Task> tasks)
        {
            return tasks.Where(t => t.CreatedAt >= _startDate && t.CreatedAt <= _endDate).ToList();
        }
    }

    // Strategy Pattern для сортировки
    public interface ISortStrategy
    {
        List<Task> Sort(List<Task> tasks);
        string Name { get; }
    }

    public class SortByDateDescStrategy : ISortStrategy
    {
        public string Name => "Date (Newest First)";
        public List<Task> Sort(List<Task> tasks)
        {
            return tasks.OrderByDescending(t => t.CreatedAt).ToList();
        }
    }

    public class SortByDateAscStrategy : ISortStrategy
    {
        public string Name => "Date (Oldest First)";
        public List<Task> Sort(List<Task> tasks)
        {
            return tasks.OrderBy(t => t.CreatedAt).ToList();
        }
    }

    public class SortByTypeStrategy : ISortStrategy
    {
        public string Name => "Type";
        public List<Task> Sort(List<Task> tasks)
        {
            return tasks.OrderBy(t => t.Type).ToList();
        }
    }

    public class SortByStateStrategy : ISortStrategy
    {
        public string Name => "State";
        public List<Task> Sort(List<Task> tasks)
        {
            return tasks.OrderBy(t => t.StateName).ToList();
        }
    }

    public class SortByTitleStrategy : ISortStrategy
    {
        public string Name => "Title (A-Z)";
        public List<Task> Sort(List<Task> tasks)
        {
            return tasks.OrderBy(t => t.Title).ToList();
        }
    }

    // Комбинированные стратегии сортировки
    public class SortByTypeAndStateStrategy : ISortStrategy
    {
        public string Name => "Type + State";
        public List<Task> Sort(List<Task> tasks)
        {
            return tasks.OrderBy(t => t.Type).ThenBy(t => t.StateName).ToList();
        }
    }

    public class SortByStateAndTypeStrategy : ISortStrategy
    {
        public string Name => "State + Type";
        public List<Task> Sort(List<Task> tasks)
        {
            return tasks.OrderBy(t => t.StateName).ThenBy(t => t.Type).ToList();
        }
    }

    public class SortByTypeAndDateStrategy : ISortStrategy
    {
        public string Name => "Type + Date";
        public List<Task> Sort(List<Task> tasks)
        {
            return tasks.OrderBy(t => t.Type).ThenByDescending(t => t.CreatedAt).ToList();
        }
    }

    public class SortByStateAndDateStrategy : ISortStrategy
    {
        public string Name => "State + Date";
        public List<Task> Sort(List<Task> tasks)
        {
            return tasks.OrderBy(t => t.StateName).ThenByDescending(t => t.CreatedAt).ToList();
        }
    }
}
