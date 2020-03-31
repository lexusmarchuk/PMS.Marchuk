using Microsoft.AspNetCore.Mvc;
using PMS.API.DTO;
using PMS.Marchuk;

namespace PMS.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IUnitOfWork _workUnit;

        public ProjectController(IUnitOfWork workUnit)
        {
            _workUnit = workUnit;
        }

        [HttpGet("report")]
        public IActionResult Report()
        {
            byte[] report = _workUnit.GenerateReport();
            var fileName = $"report.xls";
            var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(report, mimeType, fileName);
        }

        [HttpGet]
        public ActionResult Get([FromQuery]string code)
        {
            var project = _workUnit.GetProjectByCode(code);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        [HttpPost]
        public ActionResult Create([FromBody]CreateProjectModel model)
        {
            var response = _workUnit.CreateProject(model.code, model.name, model.parentId);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
