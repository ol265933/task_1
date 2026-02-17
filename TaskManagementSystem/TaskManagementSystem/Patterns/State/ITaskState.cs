using TaskManagementSystem.Models;

namespace TaskManagementSystem.Patterns.State
{
    public interface ITaskState
    {
        void MoveNext(Task task);
        void MovePrevious(Task task);
    }

    public class CreatedState : ITaskState
    {
        public void MoveNext(Task task)
        {
            task.SetState(new InProgressState());
        }

        public void MovePrevious(Task task)
        {
            // Нельзя вернуться назад из Created
        }
    }

    public class InProgressState : ITaskState
    {
        public void MoveNext(Task task)
        {
            task.SetState(new InReviewState());
        }

        public void MovePrevious(Task task)
        {
            task.SetState(new CreatedState());
        }
    }

    public class InReviewState : ITaskState
    {
        public void MoveNext(Task task)
        {
            task.SetState(new CompletedState());
        }

        public void MovePrevious(Task task)
        {
            task.SetState(new InProgressState());
        }
    }

    public class CompletedState : ITaskState
    {
        public void MoveNext(Task task)
        {
            // Конечное состояние
        }

        public void MovePrevious(Task task)
        {
            task.SetState(new InReviewState());
        }
    }
}
