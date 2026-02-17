using System.Collections.Generic;
using TaskManagementSystem.Models;
using TaskManagementSystem.Data;

namespace TaskManagementSystem.Patterns.Command
{
    // Command Pattern для Undo/Redo
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class CreateTaskCommand : ICommand
    {
        private Task _task;
        private DatabaseManager _db;

        public CreateTaskCommand(Task task, DatabaseManager db)
        {
            _task = task;
            _db = db;
        }

        public void Execute()
        {
            _db.AddTask(_task);
        }

        public void Undo()
        {
            _db.DeleteTask(_task.Id);
        }
    }

    public class ChangeTaskStateCommand : ICommand
    {
        private Task _task;
        private string _oldState;
        private bool _isForward;

        public ChangeTaskStateCommand(Task task, bool isForward)
        {
            _task = task;
            _oldState = task.StateName;
            _isForward = isForward;
        }

        public void Execute()
        {
            if (_isForward)
                _task.MoveNext();
            else
                _task.MovePrevious();
        }

        public void Undo()
        {
            if (_isForward)
                _task.MovePrevious();
            else
                _task.MoveNext();
        }
    }

    public class CommandManager
    {
        private Stack<ICommand> _undoStack = new Stack<ICommand>();
        private Stack<ICommand> _redoStack = new Stack<ICommand>();

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // Очищаем redo при новой операции
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
            }
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }
}
