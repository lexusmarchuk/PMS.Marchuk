using ExcelLibrary.SpreadSheet;
using PMS.Marchuk.Models;
using PMS.Marchuk.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace PMS.Marchuk
{
    public class TaskUnitOfWork : ITaskUnitOfWork
    {
        IProjectRepository _projectRepository;
        ITaskRepository _taskRepository;

        public TaskUnitOfWork(IProjectRepository projectRepository, ITaskRepository taskRepository)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
        }

        /// <summary>
        /// Changes Task State.
        /// </summary>
        /// <param name="taskId">Task Id.</param>
        /// <param name="state">State</param>
        /// <returns></returns>
        public PmsResponse ChangeTaskState(Guid taskId, State state)
        {
            var response = new PmsResponse();

            try
            {
                if (state == State.Planned)
                {
                    response.Errors.Add($"'{nameof(State.Planned)}' is a default state.");
                }
                else
                {
                    var task = _taskRepository.Find(x => x.Id == taskId);

                    if (!task.Any())
                    {
                        throw new Exception($"Task '{taskId}' not found.");
                    }

                    PmsResponse resp = null;
                    if (state == State.Completed)
                    {
                        var childTasks = _taskRepository.Find(x => x.ParentTaskId == taskId);
                        if (childTasks.Any(x => x.State != State.Completed))
                        {
                            response.Message = "Validation error";
                            response.Errors.Add("Unable to complete task. Not finished subtasks exist.");
                        }
                        else
                        {
                            resp = _taskRepository.SetState(taskId, state);
                        }
                    }
                    else if (state == State.InProgress)
                    {
                        resp = _taskRepository.SetState(taskId, state);
                    }

                    if (resp != null)
                    {
                        UpdateProjectState(task.FirstOrDefault().Project);

                        if (resp.Success)
                        {
                            response.EntityId = resp.EntityId;
                        }
                        else
                        {
                            response.Errors.AddRange(resp.Errors);
                        }
                        response.Message = resp.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = "Validation error.";
                response.Errors.Add(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Update Project State
        /// </summary>
        /// <param name="project"></param>
        public void UpdateProjectState(Project project)
        {
            if (project != null)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    var tasks = _taskRepository.Find(x => x.ProjectId == project.Id);
                    if (!tasks.Any(x => x.State != State.Completed))
                    {
                        var childProjects = _projectRepository.Find(x => x.ParentId == project.Id).ToArray();

                        if (!childProjects.Any(x => x.State != State.Completed))
                        {
                            _projectRepository.Update(project.Id, null, State.Completed, null, DateTime.UtcNow);

                            if (project.ParentId.HasValue
                                && project.ParentId.Value != Guid.Empty)
                            {
                                UpdateProjectState(project.Parent);
                            }
                        }
                    }
                    else if (tasks.Any(x => x.State == State.InProgress))
                    {
                        _projectRepository.Update(project.Id, null, State.InProgress, DateTime.UtcNow, null);

                        if (project.ParentId.HasValue
                            && project.ParentId.Value != Guid.Empty)
                        {
                            UpdateProjectState(project.Parent);
                        }
                    }
                }).Wait();
            }
        }

        /// <summary>
        /// Create Task.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="projectId">Project Id.</param>
        /// <returns></returns>
        public PmsResponse CreateTask(string name, string description, Guid projectId)
        {
            var response = new PmsResponse();
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    response.Errors.Add($"'{nameof(name)}' can not be empty.");
                }

                var project = _projectRepository.Find(x => x.Id == projectId);
                if (!project.Any())
                {
                    response.Errors.Add($"Project Id='{projectId}' not found.");
                }

                if (response.Success)
                {
                    var createResp = _taskRepository.Create(name, description, projectId);

                    if (createResp.Success)
                    {
                        response = createResp;
                        UpdateProjectState(project.First());
                    }
                    else
                    {
                        response.Message = createResp.Message;
                        response.Errors.AddRange(createResp.Errors);
                    }
                }
                else
                {
                    response.Message = "Validation error";
                }
            }
            catch (Exception ex)
            {
                response.Message = "Task create error.";
                response.Errors.Add(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Get Task by Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task GetTask(Guid id)
        {
            var task = _taskRepository.Find(x => x.Id == id);

            return task.FirstOrDefault();
        }

        /// <summary>
        /// Create SubTask.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="parentTaskId"></param>
        /// <returns></returns>
        public PmsResponse CreateSubTask(string name, string description, Guid parentTaskId)
        {
            var response = new PmsResponse();
            var parentTask = GetTask(parentTaskId);

            if (parentTask == null)
            {
                response.Message = "Validation error";
                response.Errors.Add($"Task with Id '{parentTaskId}' not found.");
            }
            else
            {
                response = CreateTask(name, description, parentTask.ProjectId);

                if (response.Success)
                {
                    _taskRepository.Attach(parentTaskId, response.EntityId);
                }
            }

            return response;
        }
    }
}
