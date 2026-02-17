using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using TaskManagementSystem.Models;
using TaskManagementSystem.Patterns.State;

namespace TaskManagementSystem.Data
{
    // Singleton Pattern для управления БД
    public sealed class DatabaseManager
    {
        private static DatabaseManager _instance;
        private static readonly object _lock = new object();
        private string _connectionString;

        private DatabaseManager()
        {
            string dbPath = "tasks.db";
            _connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string createTasksTable = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        Type INTEGER,
                        AssignedUserId INTEGER,
                        CreatedAt TEXT,
                        DueDate TEXT,
                        RecurringDays INTEGER,
                        StateName TEXT
                    )";
                
                string createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Role INTEGER
                    )";

                using (var command = new SQLiteCommand(createTasksTable, connection))
                {
                    command.ExecuteNonQuery();
                }
                
                using (var command = new SQLiteCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddTask(Task task)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Tasks (Title, Description, Type, AssignedUserId, CreatedAt, DueDate, RecurringDays, StateName)
                                VALUES (@Title, @Description, @Type, @AssignedUserId, @CreatedAt, @DueDate, @RecurringDays, @StateName)";
                
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", task.Title);
                    command.Parameters.AddWithValue("@Description", task.Description ?? "");
                    command.Parameters.AddWithValue("@Type", (int)task.Type);
                    command.Parameters.AddWithValue("@AssignedUserId", task.AssignedUserId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedAt", task.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@DueDate", task.DueDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RecurringDays", task.RecurringDays ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@StateName", task.StateName);
                    command.ExecuteNonQuery();
                    
                    task.Id = (int)connection.LastInsertRowId;
                }
            }
        }

        public void UpdateTask(Task task)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"UPDATE Tasks SET Title=@Title, Description=@Description, Type=@Type, 
                                AssignedUserId=@AssignedUserId, DueDate=@DueDate, RecurringDays=@RecurringDays, 
                                StateName=@StateName WHERE Id=@Id";
                
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", task.Id);
                    command.Parameters.AddWithValue("@Title", task.Title);
                    command.Parameters.AddWithValue("@Description", task.Description ?? "");
                    command.Parameters.AddWithValue("@Type", (int)task.Type);
                    command.Parameters.AddWithValue("@AssignedUserId", task.AssignedUserId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DueDate", task.DueDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RecurringDays", task.RecurringDays ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@StateName", task.StateName);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTask(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Tasks WHERE Id=@Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Task> GetAllTasks()
        {
            var tasks = new List<Task>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Tasks";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var task = new Task
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Type = (TaskType)reader.GetInt32(3),
                            AssignedUserId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                            CreatedAt = DateTime.Parse(reader.GetString(5)),
                            DueDate = reader.IsDBNull(6) ? (DateTime?)null : DateTime.Parse(reader.GetString(6)),
                            RecurringDays = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7)
                        };
                        
                        // Восстановление состояния
                        string stateName = reader.GetString(8);
                        ITaskState state = stateName switch
                        {
                            "CreatedState" => new CreatedState(),
                            "InProgressState" => new InProgressState(),
                            "InReviewState" => new InReviewState(),
                            "CompletedState" => new CompletedState(),
                            _ => new CreatedState()
                        };
                        task.SetState(state);
                        
                        tasks.Add(task);
                    }
                }
            }
            return tasks;
        }

        public void AddUser(User user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Users (Name, Role) VALUES (@Name, @Role)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@Role", (int)user.Role);
                    command.ExecuteNonQuery();
                    user.Id = (int)connection.LastInsertRowId;
                }
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Users";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Role = (UserRole)reader.GetInt32(2)
                        });
                    }
                }
            }
            return users;
        }

        public void BackupDatabase(string backupPath)
        {
            File.Copy("tasks.db", backupPath, true);
        }

        public void RestoreDatabase(string backupPath)
        {
            File.Copy(backupPath, "tasks.db", true);
        }
    }
}
