using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TaskManagementSystem.Models;
using TaskManagementSystem.Patterns.Factory;
using TaskManagementSystem.Patterns.Observer;
using TaskManagementSystem.Patterns.Command;
using TaskManagementSystem.Patterns.Strategy;
using TaskManagementSystem.Data;

namespace TaskManagementSystem.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly DatabaseManager _db;
        private readonly NotificationManager _notificationManager;
        private readonly CommandManager _commandManager;
        private static IFilterStrategy _currentFilter = new AllTasksStrategy();
        private static ISortStrategy _currentSort = new SortByDateDescStrategy();

        public HomeController(
            DatabaseManager db,
            NotificationManager notificationManager,
            CommandManager commandManager)
        {
            _db = db;
            _notificationManager = notificationManager;
            _commandManager = commandManager;
        }

        [HttpGet("home")]
        public IActionResult Index()
        {
            var tasks = _db.GetAllTasks();
            foreach (var task in tasks)
            {
                task.Attach(_notificationManager);
            }

            tasks = _currentFilter.Filter(tasks);
            tasks = _currentSort.Sort(tasks);

            ViewBag.Tasks = tasks;
            ViewBag.AllTasksCount = _db.GetAllTasks().Count;
            ViewBag.Notifications = _notificationManager.GetNotifications().TakeLast(10).Reverse().ToList();
            ViewBag.CanUndo = _commandManager.CanUndo;
            ViewBag.CanRedo = _commandManager.CanRedo;
            ViewBag.CurrentFilterName = _currentFilter.Name;
            ViewBag.CurrentSortName = _currentSort.Name;
            ViewBag.CurrentFilterId = GetFilterId(_currentFilter);
            ViewBag.CurrentSortId = GetSortId(_currentSort);

            return View();
        }

        private int GetFilterId(IFilterStrategy filter)
        {
            return filter switch
            {
                UrgentTasksStrategy => 1,
                ActiveTasksStrategy => 2,
                CompletedTasksStrategy => 3,
                RecurringTasksStrategy => 4,
                DateRangeFilterStrategy => 5,
                _ => 0
            };
        }

        private int GetSortId(ISortStrategy sort)
        {
            return sort switch
            {
                SortByDateAscStrategy => 1,
                SortByTypeStrategy => 2,
                SortByStateStrategy => 3,
                SortByTitleStrategy => 4,
                SortByTypeAndStateStrategy => 5,
                SortByStateAndTypeStrategy => 6,
                SortByTypeAndDateStrategy => 7,
                SortByStateAndDateStrategy => 8,
                _ => 0
            };
        }

        [HttpPost("create")]
        public IActionResult CreateTask(string title, string description, int taskType)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return RedirectToAction("Index");
            }

            var type = (TaskType)taskType;
            
            // Factory Method Pattern
            var factory = TaskFactory.GetFactory(type);
            var task = factory.CreateTask(title, description);

            // Observer Pattern
            task.Attach(_notificationManager);

            // Command Pattern
            var command = new CreateTaskCommand(task, _db);
            _commandManager.ExecuteCommand(command);

            task.NotifyObservers($"Task '{task.Title}' created successfully");

            return RedirectToAction("Index");
        }

        [HttpPost("change-state")]
        public IActionResult ChangeState(int id, bool forward)
        {
            var tasks = _db.GetAllTasks();
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task != null)
            {
                task.Attach(_notificationManager);

                // State Pattern + Command Pattern
                var command = new ChangeTaskStateCommand(task, forward);
                _commandManager.ExecuteCommand(command);

                _db.UpdateTask(task);
            }

            return RedirectToAction("Index");
        }

        [HttpPost("delete")]
        public IActionResult DeleteTask(int id)
        {
            var tasks = _db.GetAllTasks();
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task != null)
            {
                _db.DeleteTask(id);
                _notificationManager.Update(task, $"Task '{task.Title}' deleted");
            }

            return RedirectToAction("Index");
        }

        [HttpPost("undo")]
        public IActionResult Undo()
        {
            if (_commandManager.CanUndo)
            {
                _commandManager.Undo();
            }
            return RedirectToAction("Index");
        }

        [HttpPost("redo")]
        public IActionResult Redo()
        {
            if (_commandManager.CanRedo)
            {
                _commandManager.Redo();
            }
            return RedirectToAction("Index");
        }

        [HttpPost("filter")]
        public IActionResult SetFilter(int filterType)
        {
            // Strategy Pattern
            _currentFilter = filterType switch
            {
                1 => new UrgentTasksStrategy(),
                2 => new ActiveTasksStrategy(),
                3 => new CompletedTasksStrategy(),
                4 => new RecurringTasksStrategy(),
                _ => new AllTasksStrategy()
            };

            return RedirectToAction("Index");
        }

        [HttpPost("filter-date")]
        public IActionResult SetDateFilter(string startDate, string endDate)
        {
            if (DateTime.TryParse(startDate, out DateTime start) && 
                DateTime.TryParse(endDate, out DateTime end))
            {
                _currentFilter = new DateRangeFilterStrategy(start, end);
            }
            else
            {
                _currentFilter = new AllTasksStrategy();
            }

            return RedirectToAction("Index");
        }

        [HttpPost("sort")]
        public IActionResult SetSort(int sortType)
        {
            // Strategy Pattern
            _currentSort = sortType switch
            {
                1 => new SortByDateAscStrategy(),
                2 => new SortByTypeStrategy(),
                3 => new SortByStateStrategy(),
                4 => new SortByTitleStrategy(),
                5 => new SortByTypeAndStateStrategy(),
                6 => new SortByStateAndTypeStrategy(),
                7 => new SortByTypeAndDateStrategy(),
                8 => new SortByStateAndDateStrategy(),
                _ => new SortByDateDescStrategy()
            };

            return RedirectToAction("Index");
        }

        [HttpPost("backup")]
        public IActionResult Backup()
        {
            var backupName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
            _db.BackupDatabase(backupName);
            
            var task = new Task { Title = "System", Description = "Backup" };
            _notificationManager.Update(task, $"Backup created: {backupName}");

            return RedirectToAction("Index");
        }

        [HttpPost("reset-filters")]
        public IActionResult ResetFilters()
        {
            _currentFilter = new AllTasksStrategy();
            _currentSort = new SortByDateDescStrategy();
            return RedirectToAction("Index");
        }
    }
}
