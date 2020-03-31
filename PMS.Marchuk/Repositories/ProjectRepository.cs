using Microsoft.EntityFrameworkCore;
using PMS.Marchuk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PMS.Marchuk.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly PmsDbContext _dbContext;

        public ProjectRepository(PmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Attaches Project to Parent Project
        /// </summary>
        /// <param name="mainProjectId">Parent Project Id</param>
        /// <param name="childProjectId">Current Project Id</param>
        /// <returns></returns>
        public PmsResponse AttachProject(Guid mainProjectId, Guid childProjectId)
        {
            var response = new PmsResponse();
            try
            {
                var childProject = _dbContext.Projects.FirstOrDefault(p => p.Id == childProjectId);

                if (childProject == null)
                {
                    response.Errors.Add($"Project with ID = '{childProjectId}' not found.");
                }

                var mainProject = _dbContext.Projects.FirstOrDefault(p => p.Id == mainProjectId);

                if (mainProject == null)
                {
                    response.Errors.Add($"Project with ID = '{mainProjectId}' not found.");
                }

                if (response.Errors.Any())
                {
                    throw new Exception("Validation error.");
                }

                childProject.ParentId = mainProjectId;
                _dbContext.SaveChanges();

                response.EntityId = childProjectId;
                response.Message = $"Project with ID='{mainProjectId}' set as parent for Project ID '{childProjectId}'";
            }
            catch (Exception ex)
            {
                response.Message = "Project attachment error.";
                response.Errors.Add(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Create Project
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public PmsResponse Create(string code, string name)
        {
            var response = new PmsResponse();
            try
            {

                var p = new Project
                {
                    Code = code,
                    Name = name,
                    State = State.Planned,
                    Id = Guid.NewGuid()
                };

                _dbContext.Projects.Add(p);
                int r = _dbContext.SaveChanges();

                response.EntityId = p.Id;
                response.Message = $"Project '{name}' successfully created.";

            }
            catch (Exception ex)
            {
                response.Message = "Project create error";
                response.Errors.Add(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Delete Project
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        public PmsResponse Delete(Guid id)
        {
            var response = new PmsResponse();
            try
            {
                var p = _dbContext.Projects.FirstOrDefault(p => p.Id == id);
                _dbContext.Projects.Remove(p);
                int r = _dbContext.SaveChanges();

                response.EntityId = p.Id;
                response.Message = $"Project {p.Name} successfully deleted.";
            }
            catch (Exception ex)
            {
                response.Message = "Project delete error";
                response.Errors.Add(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Find projects
        /// </summary>
        /// <param name="f">Search expression</param>
        /// <returns></returns>
        public IEnumerable<Project> Find(Expression<Func<Project, bool>> f)
        {
            return _dbContext.Projects.Where(f)
                .Include(x => x.Parent)
                .ToArray();
        }

        /// <summary>
        /// Get Project by Code.
        /// </summary>
        /// <param name="code">Code</param>
        /// <returns></returns>
        public Project GetByCode(string code)
        {
            Project p = Find(x => x.Code.Equals(code)).FirstOrDefault();

            return p;
        }

        /// <summary>
        /// Update Project
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="state">State</param>
        /// <returns></returns>
        public PmsResponse Update(Guid id, string name, State? state)
        {
            var response = new PmsResponse();
            try
            {
                var p = _dbContext.Projects.FirstOrDefault(p => p.Id == id);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    p.Name = name;
                }

                if (state.HasValue)
                {
                    p.State = state.Value;
                }

                int r = _dbContext.SaveChanges();

                response.EntityId = p.Id;
                response.Message = $"Project {p.Name} successfully updated.";
            }
            catch (Exception ex)
            {
                response.Message = "Project update error";
                response.Errors.Add(ex.Message);
            }

            return response;
        }
    }
}
