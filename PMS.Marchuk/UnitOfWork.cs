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
    public class UnitOfWork : IUnitOfWork
    {
        IProjectRepository _projectRepository;
        ITaskRepository _taskRepository;

        public UnitOfWork(IProjectRepository projectRepository, ITaskRepository taskRepository)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
        }

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
                            _projectRepository.Update(project.Id, null, State.Completed);

                            if (project.ParentId.HasValue
                                && project.ParentId.Value != Guid.Empty)
                            {
                                UpdateProjectState(project.Parent);
                            }
                        }
                    }
                    else if (tasks.Any(x => x.State == State.InProgress))
                    {
                        _projectRepository.Update(project.Id, null, State.InProgress);

                        if (project.ParentId.HasValue
                            && project.ParentId.Value != Guid.Empty)
                        {
                            UpdateProjectState(project.Parent);
                        }
                    }
                }).Wait();
            }
        }

        public PmsResponse CreateProject(string code, string name, Guid? parentId)
        {
            PmsResponse response = new PmsResponse();
            var project = _projectRepository.Find(x => x.Code.Equals(code));
            if (project.Any())
            {
                response.Message = "Validation error";
                response.Errors.Add($"Project with code '{code}' already exists.");
            }
            else
            {
                if (parentId.HasValue)
                {
                    var parentProject = _projectRepository.Find(x => x.Id == parentId.Value);

                    if (!parentProject.Any())
                    {
                        response.Message = "Validation error";
                        response.Errors.Add($"Parent Project not found.");
                    }

                    if (response.Success)
                    {
                        response = _projectRepository.Create(code, name);
                        if (response.Success)
                        {
                            _projectRepository.AttachProject(parentId.Value, response.EntityId);
                        }
                    }
                }
                else
                {
                    response = _projectRepository.Create(code, name);
                }
            }

            return response;
        }

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

        public Project GetProjectByCode(string code)
        {
            return _projectRepository.GetByCode(code);
        }

        public Task GetTask(Guid id)
        {
            var task = _taskRepository.Find(x => x.Id == id);

            return task.FirstOrDefault();
        }

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

        public byte[] GenerateReport()
        {
            var stream = new MemoryStream();

            Workbook workbook = new Workbook();
            Worksheet worksheet = new Worksheet("Projects");

            int row = 0;
            worksheet.Cells[row, 0] = new Cell("Project.Id");
            worksheet.Cells[row, 1] = new Cell("Project.Code");
            worksheet.Cells[row, 2] = new Cell("Project.Name");
            worksheet.Cells[row, 3] = new Cell("Project.State");
            worksheet.Cells[row, 4] = new Cell("Project.StartDate");
            worksheet.Cells[row, 5] = new Cell("Project.FinishDate");
            worksheet.Cells[row, 6] = new Cell("Project.ParentId");
            worksheet.Cells[row, 7] = new Cell("Task.Id");
            worksheet.Cells[row, 8] = new Cell("Task.Name");
            worksheet.Cells[row, 9] = new Cell("Task.Description");
            worksheet.Cells[row, 10] = new Cell("Task.State");
            worksheet.Cells[row, 11] = new Cell("Task.StartDate");
            worksheet.Cells[row, 12] = new Cell("Task.FinishDate");
            worksheet.Cells[row, 13] = new Cell("Task.ParentTaskId");

            foreach (var proj in _projectRepository.Find(x => x.Id != Guid.Empty))
            {
                row++;
                worksheet.Cells[row, 0] = new Cell(proj.Id.ToString());
                worksheet.Cells[row, 1] = new Cell(proj.Code);
                worksheet.Cells[row, 2] = new Cell(proj.Name);
                worksheet.Cells[row, 3] = new Cell(proj.State.ToString());
                worksheet.Cells[row, 4] = new Cell(proj.StartDate.ToString());
                worksheet.Cells[row, 5] = new Cell(proj.FinishDate.ToString());
                worksheet.Cells[row, 6] = new Cell(proj.ParentId.HasValue ? proj.ParentId.Value.ToString() : string.Empty);
                foreach (var task in _taskRepository.Find(t => t.ProjectId == proj.Id))
                {
                    row++;
                    worksheet.Cells[row, 0] = new Cell(proj.Id.ToString());

                    worksheet.Cells[row, 7] = new Cell(task.Id.ToString());
                    worksheet.Cells[row, 8] = new Cell(task.Name.ToString());
                    worksheet.Cells[row, 9] = new Cell(task.Description.ToString());
                    worksheet.Cells[row, 10] = new Cell(task.State.ToString());
                    worksheet.Cells[row, 11] = new Cell(task.StartDate.ToString());
                    worksheet.Cells[row, 12] = new Cell(task.FinishDate.ToString());
                    worksheet.Cells[row, 13] = new Cell(task.ParentTaskId.HasValue? task.ParentTaskId.ToString(): string.Empty);
                    
                }                
            }

            workbook.Worksheets.Add(worksheet);
            workbook.Save(stream);

            return stream.ToArray();
        }
    }
}
