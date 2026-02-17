using System;
using System.Linq;
using TaskManagementSystem.Models;
using TaskManagementSystem.Patterns.Factory;
using TaskManagementSystem.Patterns.Observer;
using TaskManagementSystem.Patterns.Command;
using TaskManagementSystem.Patterns.Strategy;
using TaskManagementSystem.Data;

namespace TaskManagementSystem.ConsoleUI
{
    public static class ConsoleApp
    {
        private static DatabaseManager _db;
        private static NotificationManager _notificationManager;
        private static CommandManager _commandManager;
        private static IFilterStrategy _currentFilter;
        private static ISortStrategy _currentSort;

        public static void Run()
        {
            _db = DatabaseManager.Instance;
            _notificationManager = new NotificationManager();
            _commandManager = new CommandManager();
            _currentFilter = new AllTasksStrategy();
            _currentSort = new SortByDateDescStrategy();

            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   TASK MANAGEMENT SYSTEM - DESIGN PATTERNS DEMO (CLI)      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            bool running = true;
            while (running)
            {
                ShowMenu();
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": CreateTask(); break;
                    case "2": ListTasks(); break;
                    case "3": ChangeTaskState(); break;
                    case "4": DeleteTask(); break;
                    case "5": UndoOperation(); break;
                    case "6": RedoOperation(); break;
                    case "7": FilterTasks(); break;
                    case "8": SortTasks(); break;
                    case "9": FilterByDate(); break;
                    case "10": ShowNotifications(); break;
                    case "11": BackupDatabase(); break;
                    case "12": ResetFilters(); break;
                    case "0": running = false; break;
                    default: Console.WriteLine("Invalid choice!"); break;
                }

                if (running)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        MAIN MENU                           ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  1. Create Task (Factory Pattern)                          ║");
            Console.WriteLine("║  2. List All Tasks (Strategy Pattern)                      ║");
            Console.WriteLine("║  3. Change Task State (State Pattern)                      ║");
            Console.WriteLine("║  4. Delete Task                                            ║");
            Console.WriteLine("║  5. Undo Last Operation (Command Pattern)                  ║");
            Console.WriteLine("║  6. Redo Operation (Command Pattern)                       ║");
            Console.WriteLine("║  7. Filter Tasks (Strategy Pattern)                        ║");
            Console.WriteLine("║  8. Sort Tasks (Strategy Pattern)                          ║");
            Console.WriteLine("║  9. Filter by Date Range                                   ║");
            Console.WriteLine("║ 10. Show Notifications (Observer Pattern)                  ║");
            Console.WriteLine("║ 11. Backup Database (Singleton Pattern)                    ║");
            Console.WriteLine("║ 12. Reset All Filters                                      ║");
            Console.WriteLine("║  0. Exit                                                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"\nCurrent Filter: {_currentFilter.Name}");
            Console.WriteLine($"Current Sort: {_currentSort.Name}");
            Console.WriteLine($"Undo: {_commandManager.CanUndo} | Redo: {_commandManager.CanRedo}");
            Console.Write("\nYour choice: ");
        }

        static void CreateTask()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              CREATE TASK (Factory Pattern)                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.WriteLine("Select task type:");
            Console.WriteLine("  1. Regular Task");
            Console.WriteLine("  2. Urgent Task (due in 1 day)");
            Console.WriteLine("  3. Recurring Task (repeats every 7 days)");
            Console.Write("\nType (1-3): ");
            
            var typeChoice = Console.ReadLine();
            TaskType type = typeChoice switch
            {
                "2" => TaskType.Urgent,
                "3" => TaskType.Recurring,
                _ => TaskType.Regular
            };

            Console.Write("Title: ");
            var title = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("❌ Title cannot be empty!");
                return;
            }

            Console.Write("Description (optional): ");
            var description = Console.ReadLine();

            // Factory Method Pattern
            var factory = TaskFactory.GetFactory(type);
            var task = factory.CreateTask(title, description);

            // Observer Pattern
            task.Attach(_notificationManager);

            // Command Pattern
            var command = new CreateTaskCommand(task, _db);
            _commandManager.ExecuteCommand(command);

            task.NotifyObservers($"Task '{task.Title}' created successfully");

            Console.WriteLine($"\n✅ Task created: {task}");
        }

        static void ListTasks()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    TASK LIST                               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var allTasks = _db.GetAllTasks();
            var tasks = allTasks.ToList();
            
            // Подписываем на уведомления
            foreach (var task in tasks)
            {
                task.Attach(_notificationManager);
            }

            // Strategy Pattern - применяем фильтр и сортировку
            tasks = _currentFilter.Filter(tasks);
            tasks = _currentSort.Sort(tasks);

            Console.WriteLine($"Total tasks in database: {allTasks.Count}");
            Console.WriteLine($"Showing: {tasks.Count} tasks");
            Console.WriteLine($"Filter: {_currentFilter.Name}");
            Console.WriteLine($"Sort: {_currentSort.Name}");
            Console.WriteLine();

            if (tasks.Count == 0)
            {
                Console.WriteLine("No tasks found with current filters.");
                return;
            }

            Console.WriteLine($"{"ID",-5} {"Type",-12} {"Title",-30} {"State",-20} {"Created",-20}");
            Console.WriteLine(new string('─', 90));

            foreach (var task in tasks)
            {
                var stateDisplay = task.StateName.Replace("State", "");
                var typeIcon = task.Type switch
                {
                    TaskType.Regular => "📄",
                    TaskType.Urgent => "🔥",
                    TaskType.Recurring => "🔄",
                    _ => "📋"
                };

                var title = task.Title.Length > 30 ? task.Title.Substring(0, 27) + "..." : task.Title;
                Console.WriteLine($"{task.Id,-5} {typeIcon} {task.Type,-10} {title,-30} {stateDisplay,-20} {task.CreatedAt:yyyy-MM-dd HH:mm}");
            }
        }

        static void ChangeTaskState()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          CHANGE TASK STATE (State Pattern)                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            ListTasks();

            Console.Write("\nEnter Task ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                return;
            }

            var tasks = _db.GetAllTasks();
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task == null)
            {
                Console.WriteLine("❌ Task not found!");
                return;
            }

            task.Attach(_notificationManager);

            Console.WriteLine($"\nCurrent state: {task.StateName.Replace("State", "")}");
            Console.WriteLine("\n1. Move to Next State");
            Console.WriteLine("2. Move to Previous State");
            Console.Write("\nChoice: ");

            var choice = Console.ReadLine();
            bool isForward = choice == "1";

            // STATE PATTERN + COMMAND PATTERN
            var command = new ChangeTaskStateCommand(task, isForward);
            _commandManager.ExecuteCommand(command);

            _db.UpdateTask(task);

            Console.WriteLine($"✅ New state: {task.StateName.Replace("State", "")}");
        }

        static void DeleteTask()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    DELETE TASK                             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            ListTasks();

            Console.Write("\nEnter Task ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("❌ Invalid ID!");
                return;
            }

            var tasks = _db.GetAllTasks();
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task == null)
            {
                Console.WriteLine("❌ Task not found!");
                return;
            }

            Console.Write($"Are you sure you want to delete '{task.Title}'? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                _db.DeleteTask(id);
                _notificationManager.Update(task, $"Task '{task.Title}' deleted");
                Console.WriteLine("✅ Task deleted successfully!");
            }
        }

        static void UndoOperation()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              UNDO (Command Pattern)                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            if (_commandManager.CanUndo)
            {
                _commandManager.Undo();
                Console.WriteLine("✅ Operation undone!");
            }
            else
            {
                Console.WriteLine("❌ Nothing to undo!");
            }
        }

        static void RedoOperation()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              REDO (Command Pattern)                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            if (_commandManager.CanRedo)
            {
                _commandManager.Redo();
                Console.WriteLine("✅ Operation redone!");
            }
            else
            {
                Console.WriteLine("❌ Nothing to redo!");
            }
        }

        static void FilterTasks()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          FILTER TASKS (Strategy Pattern)                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.WriteLine("Select filter:");
            Console.WriteLine("  1. All Tasks");
            Console.WriteLine("  2. Urgent Tasks Only");
            Console.WriteLine("  3. Active Tasks Only");
            Console.WriteLine("  4. Completed Tasks Only");
            Console.WriteLine("  5. Recurring Tasks Only");
            Console.Write("\nChoice: ");

            // STRATEGY PATTERN
            _currentFilter = Console.ReadLine() switch
            {
                "2" => new UrgentTasksStrategy(),
                "3" => new ActiveTasksStrategy(),
                "4" => new CompletedTasksStrategy(),
                "5" => new RecurringTasksStrategy(),
                _ => new AllTasksStrategy()
            };

            Console.WriteLine($"✅ Filter set to: {_currentFilter.Name}");
        }

        static void SortTasks()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           SORT TASKS (Strategy Pattern)                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.WriteLine("Select sort order:");
            Console.WriteLine("  1. Date (Newest First)");
            Console.WriteLine("  2. Date (Oldest First)");
            Console.WriteLine("  3. By Type");
            Console.WriteLine("  4. By State");
            Console.WriteLine("  5. By Title (A-Z)");
            Console.WriteLine("  6. Type + State (Combined)");
            Console.WriteLine("  7. State + Type (Combined)");
            Console.WriteLine("  8. Type + Date (Combined)");
            Console.WriteLine("  9. State + Date (Combined)");
            Console.Write("\nChoice: ");

            // STRATEGY PATTERN
            _currentSort = Console.ReadLine() switch
            {
                "2" => new SortByDateAscStrategy(),
                "3" => new SortByTypeStrategy(),
                "4" => new SortByStateStrategy(),
                "5" => new SortByTitleStrategy(),
                "6" => new SortByTypeAndStateStrategy(),
                "7" => new SortByStateAndTypeStrategy(),
                "8" => new SortByTypeAndDateStrategy(),
                "9" => new SortByStateAndDateStrategy(),
                _ => new SortByDateDescStrategy()
            };

            Console.WriteLine($"✅ Sort set to: {_currentSort.Name}");
        }

        static void FilterByDate()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║        FILTER BY DATE RANGE (Strategy Pattern)             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.Write("Start date (yyyy-MM-dd) or press Enter for 7 days ago: ");
            var startInput = Console.ReadLine();
            DateTime startDate = string.IsNullOrWhiteSpace(startInput) 
                ? DateTime.Now.AddDays(-7) 
                : DateTime.Parse(startInput);

            Console.Write("End date (yyyy-MM-dd) or press Enter for today: ");
            var endInput = Console.ReadLine();
            DateTime endDate = string.IsNullOrWhiteSpace(endInput) 
                ? DateTime.Now 
                : DateTime.Parse(endInput);

            // STRATEGY PATTERN - Date Range Filter
            _currentFilter = new DateRangeFilterStrategy(startDate, endDate);
            
            Console.WriteLine($"✅ Date filter applied: {_currentFilter.Name}");
        }

        static void ShowNotifications()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         NOTIFICATIONS (Observer Pattern)                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var notifications = _notificationManager.GetNotifications();
            
            if (notifications.Count == 0)
            {
                Console.WriteLine("No notifications yet.");
                return;
            }

            foreach (var notification in notifications.TakeLast(20))
            {
                Console.WriteLine($"• {notification}");
            }

            Console.WriteLine($"\nTotal notifications: {notifications.Count}");
        }

        static void BackupDatabase()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          BACKUP DATABASE (Singleton Pattern)               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var backupName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
            Console.Write($"Backup file name [{backupName}]: ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                backupName = input;
            }

            // SINGLETON PATTERN
            _db.BackupDatabase(backupName);
            Console.WriteLine($"✅ Backup created: {backupName}");
        }

        static void ResetFilters()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                  RESET FILTERS                             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            _currentFilter = new AllTasksStrategy();
            _currentSort = new SortByDateDescStrategy();

            Console.WriteLine("✅ All filters and sorting reset to defaults!");
            Console.WriteLine($"   Filter: {_currentFilter.Name}");
            Console.WriteLine($"   Sort: {_currentSort.Name}");
        }
    }
}
