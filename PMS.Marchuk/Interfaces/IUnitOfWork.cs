using PMS.Marchuk.Models;
using System;
using System.IO;

namespace PMS.Marchuk
{
    public interface IUnitOfWork
    {
        PmsResponse CreateProject(string code, string name, Guid? parentId);

        PmsResponse ChangeTaskState(Guid taskId, State state);

        void UpdateProjectState(Project project);

        PmsResponse CreateTask(string name, string description, Guid projectId);

        PmsResponse CreateSubTask(string name, string description, Guid parentTaskId);
        Project GetProjectByCode(string code);
        byte[] GenerateReport();
        Task GetTask(Guid id);
    }
}