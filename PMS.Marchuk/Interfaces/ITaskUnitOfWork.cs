using PMS.Marchuk.Models;
using System;
using System.IO;

namespace PMS.Marchuk
{
    public interface ITaskUnitOfWork
    {
        PmsResponse ChangeTaskState(Guid taskId, State state);
        void UpdateProjectState(Project project);
        PmsResponse CreateTask(string name, string description, Guid projectId);
        PmsResponse CreateSubTask(string name, string description, Guid parentTaskId);
        Task GetTask(Guid id);
    }
}