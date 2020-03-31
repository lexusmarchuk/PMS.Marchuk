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
    public class ProjectUnitOfWork : IProjectUnitOfWork
    {
        IProjectRepository _projectRepository;
        ITaskRepository _taskRepository;

        public ProjectUnitOfWork(IProjectRepository projectRepository, ITaskRepository taskRepository)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
        }
        /// <summary>
        /// Create Project.
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="name">Name</param>
        /// <param name="parentId">Parent Id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get Project By Code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Project GetProjectByCode(string code)
        {
            return _projectRepository.GetByCode(code);
        }

        /// <summary>
        /// Generte Excel report.
        /// </summary>
        /// <returns></returns>
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
