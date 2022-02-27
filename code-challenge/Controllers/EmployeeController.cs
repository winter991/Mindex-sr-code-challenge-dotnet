using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using challenge.Services;
using challenge.Models;

namespace challenge.Controllers
{
    [Route("api/employee")]
    public class EmployeeController : Controller
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody]Employee newEmployee)
        {
            _logger.LogDebug($"Recieved employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        //reporting structure is dependant of employee and not a first class object
        // because of this the route would be {id}/reportingStructure
        [HttpGet("{id}/reportingStructure", Name = "getReportingStructure")]
        public IActionResult getReportingStructure(String id)
        {
            _logger.LogDebug($"Received getReportingStructure get request for '{id}'");

            var reportingStructure = _employeeService.GetReportingStructureForEmployeeID(id);

            if (reportingStructure == null)
                return NotFound();

            return Ok(reportingStructure);
        }

        //Since we are createing compensations off of the employeeID. create the route to be POST {id}/compensation
        [HttpPost("{id}/Compensation", Name = "CreateCompensation")]
        public IActionResult CreateCompensation(String id,[FromBody] API_Models.CreateCompensationRequest request)
        {
            _logger.LogDebug($"Received Create Compensation get request for '{id}'");
            // check model state to make sure required fields are filled in
            // createCompensationRequest has validation annotations
            //we need to check them to make sure things are corrrect before callling CreateCompensation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var comp = _employeeService.CreateCompensation(id, request);

            if (comp == null)
                return NotFound();

            return Ok(comp);
        }

        
        [HttpGet("{id}/compensation", Name = "GetCompensation")]
        public IActionResult GetCompensation(String id)
        {
            _logger.LogDebug($"Received getReportingStructure get request for '{id}'");

            var reportingStructure = _employeeService.GetCompensationByEmployeeID(id);

            if (reportingStructure == null)
                return NotFound();

            return Ok(reportingStructure);
        }
    }
}
