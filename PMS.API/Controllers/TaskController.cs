using Microsoft.AspNetCore.Mvc;
using PMS.API.DTO;
using PMS.Marchuk;
using PMS.Marchuk.Models;
using System;

namespace PMS.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IUnitOfWork _workUnit;

        public TaskController(IUnitOfWork workUnit)
        {
            _workUnit = workUnit;
        }

        [HttpPost]
        public ActionResult Create([FromBody]CreateTaskModel model)
        {
            var response = _workUnit.CreateTask(model.Name, model.Description, model.ProjectId);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("subtask")]
        public ActionResult CreateSubTask([FromBody]CreateSubTaskModel model)
        {
            var response = _workUnit.CreateSubTask(model.Name, model.Description, model.ParentTaskId);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("start")]
        public ActionResult StartTask([FromBody]StartTaskModel model)
        {
            PmsResponse response = _workUnit.ChangeTaskState(model.Id, State.InProgress);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("finish")]
        public ActionResult FinishTask([FromBody]FinishTaskModel model)
        {
            PmsResponse response = _workUnit.ChangeTaskState(model.Id, State.Completed);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet]
        public ActionResult Get([FromQuery] Guid id)
        {
            PMS.Marchuk.Models.Task response = _workUnit.GetTask(id);

            if (response == null)
            {
                return NotFound("Task not found");
            }

            return Ok(response);
        }
    }
}