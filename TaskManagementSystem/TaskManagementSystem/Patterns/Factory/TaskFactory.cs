using System;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Patterns.Factory
{
    // Factory Method Pattern
    public abstract class TaskFactory
    {
        public abstract Task CreateTask(string title, string description);

        public static TaskFactory GetFactory(TaskType type)
        {
            switch (type)
            {
                case TaskType.Regular:
                    return new RegularTaskFactory();
                case TaskType.Urgent:
                    return new UrgentTaskFactory();
                case TaskType.Recurring:
                    return new RecurringTaskFactory();
                default:
                    throw new ArgumentException("Unknown task type");
            }
        }
    }

    public class RegularTaskFactory : TaskFactory
    {
        public override Task CreateTask(string title, string description)
        {
            return new Task
            {
                Title = title,
                Description = description,
                Type = TaskType.Regular
            };
        }
    }

    public class UrgentTaskFactory : TaskFactory
    {
        public override Task CreateTask(string title, string description)
        {
            return new Task
            {
                Title = title,
                Description = description,
                Type = TaskType.Urgent,
                DueDate = DateTime.Now.AddDays(1) // Срочная - через 1 день
            };
        }
    }

    public class RecurringTaskFactory : TaskFactory
    {
        public override Task CreateTask(string title, string description)
        {
            return new Task
            {
                Title = title,
                Description = description,
                Type = TaskType.Recurring,
                RecurringDays = 7 // Повторяется каждую неделю
            };
        }
    }
}
