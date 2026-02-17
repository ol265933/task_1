using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.Data;
using TaskManagementSystem.Patterns.Observer;
using TaskManagementSystem.Patterns.Command;
using TaskManagementSystem.Patterns.Strategy;

namespace TaskManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Регистрируем сервисы как Singleton
            builder.Services.AddSingleton<DatabaseManager>(sp => DatabaseManager.Instance);
            builder.Services.AddSingleton<NotificationManager>();
            builder.Services.AddSingleton<CommandManager>();
            builder.Services.AddSingleton<IFilterStrategy, AllTasksStrategy>();
            builder.Services.AddSingleton<ISortStrategy, SortByDateDescStrategy>();
            
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            app.UseStaticFiles();
            app.UseRouting();
            app.MapControllers();
            
            app.MapGet("/", () => Results.Redirect("/home"));

            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   TASK MANAGEMENT SYSTEM - WEB UI                          ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║   Open: http://localhost:5000                              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            app.Run("http://localhost:5000");
        }
    }
}
