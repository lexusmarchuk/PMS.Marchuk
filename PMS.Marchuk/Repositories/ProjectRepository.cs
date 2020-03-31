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

        public IEnumerable<Project> Find(Expression<Func<Project, bool>> f)
        {
            return _dbContext.Projects.Where(f)
                .Include(x => x.Parent)
                .ToArray();
        }

        public Project GetByCode(string code)
        {
            Project p = Find(x => x.Code.Equals(code)).FirstOrDefault();

            return p;
        }

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
