using Microsoft.EntityFrameworkCore;
using PMS.Marchuk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PMS.Marchuk.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly PmsDbContext _dbContext;

        public TaskRepository(PmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Attach Task to Parent Task
        /// </summary>
        /// <param name="mainTaskId">Parent Task</param>
        /// <param name="childTaskId">Current Task</param>
        /// <returns></returns>
        public PmsResponse Attach(Guid mainTaskId, Guid childTaskId)
        {
            var response = new PmsResponse();
            try
            {
                var child = _dbContext.Tasks.FirstOrDefault(p => p.Id == childTaskId);
                child.ParentTaskId = mainTaskId;
                _dbContext.Update(child);
                _dbContext.SaveChanges();

                response.EntityId = childTaskId;
                response.Message = $"Task with ID='{mainTaskId}' set as parent for Task ID '{childTaskId}'";
            }
            catch (Exception ex)
            {
                response.Message = "Task attachment error.";
                response.Errors.Add(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Create Task.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="projectId">Project Id</param>
        /// <returns></returns>
        public PmsResponse Create(string name, string description, Guid projectId)
        {
            var response = new PmsResponse();
            try
            {
                var task = new Task
                {
                    Name = name,
                    Description = description,
                    State = State.Planned,
                    Id = Guid.NewGuid(),
                    ProjectId = projectId
                };

                _dbContext.Tasks.Add(task);
                int r = _dbContext.SaveChanges();

                response.EntityId = task.Id;
                response.Message = $"Task '{name}' successfully created.";
            }
            catch (Exception ex)
            {
                response.Message = "Task create error";
                response.Errors.Add(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Delete Task
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        public PmsResponse Delete(Guid id)
        {
            var response = new PmsResponse();
            try
            {
                var p = _dbContext.Tasks.FirstOrDefault(p => p.Id == id);
                _dbContext.Tasks.Remove(p);
                int r = _dbContext.SaveChanges();

                response.EntityId = p.Id;
                response.Message = $"Task {p.Name} successfully deleted.";
            }
            catch (Exception ex)
            {
                response.Message = "Task delete error";
                response.Errors.Add(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Find Tasks
        /// </summary>
        /// <param name="f">Search expression</param>
        /// <returns></returns>
        public IEnumerable<Task> Find(Expression<Func<Task, bool>> f)
        {
            return _dbContext.Tasks.Where(f)
                .Include(x => x.Project)
                .Include(x => x.ParentTask)
                .ToArray();
        }

        /// <summary>
        /// Sets Task State
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public PmsResponse SetState(Guid id, State state)
        {
            var response = new PmsResponse();
            try
            {
                var task = _dbContext.Tasks.FirstOrDefault(t => t.Id == id);

                task.State = state;

                if (state == State.Completed)
                {
                    task.FinishDate = DateTime.UtcNow;
                }
                else if (state == State.InProgress)
                {
                    task.StartDate = DateTime.UtcNow;
                }

                _dbContext.Update(task);
                int r = _dbContext.SaveChanges();

                response.EntityId = task.Id;
                response.Message = $"Task '{id}' state changed to {state}.";
            }
            catch (Exception ex)
            {
                response.Message = "Error";
                response.Errors.Add(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Updates Task
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <returns></returns>
        public PmsResponse Update(Guid id, string name, string description)
        {
            var response = new PmsResponse();
            try
            {
                var task = _dbContext.Tasks.FirstOrDefault(p => p.Id == id);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    task.Name = name;
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    task.Description = description;
                }

                _dbContext.Update(task);
                int r = _dbContext.SaveChanges();

                response.EntityId = task.Id;
                response.Message = $"Task {task.Name} successfully updated.";
            }
            catch (Exception ex)
            {
                response.Message = "Task update error";
                response.Errors.Add(ex.Message);
            }

            return response;
        }
    }
}
