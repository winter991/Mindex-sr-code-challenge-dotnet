using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using challenge.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using challenge.Data;

namespace challenge.Repositories
{
    public class EmployeeRespository : IEmployeeRepository
    {
        private readonly EmployeeContext _employeeContext;
        private readonly ILogger<IEmployeeRepository> _logger;

        public EmployeeRespository(ILogger<IEmployeeRepository> logger, EmployeeContext employeeContext)
        {
            _employeeContext = employeeContext;
           
            _logger = logger;
        }
        // Included compensation in the EmployeeRepository. This allows us easier saving of the context scope and avoids having to save employee and Compensation info separately. 
        // This could be replaced with a unit of work, but that seems overkill for this.
        public Compensation Add(Compensation compensation)
        {
            compensation.CompansationId = Guid.NewGuid().ToString();
            _employeeContext.Compensation.Add(compensation);
            return compensation;
        }
        public Compensation GetCompensationByEmployeeID(string id)
        {
            return _employeeContext.Compensation.Include(x=>x.Employee.DirectReports).FirstOrDefault(e => e.EmployeeId == id);

        }
        public Employee Add(Employee employee)
        {
            employee.EmployeeId = Guid.NewGuid().ToString();
            _employeeContext.Employees.Add(employee);
            return employee;
        }

        public Employee GetById(string id)
        {
            //fixed issue where employee direct reports are not loaded when the employee is 
            //materialize the list so that all directReports are loaded
            // this will allow the direct reports( and the direct reports direct reports) to be loaded and returned to the calller 
            //Depending on table size, there may be a performace hit with this as we are loading the table all at once and then pullling off the single employee.

            return _employeeContext.Employees.ToList().SingleOrDefault(e => e.EmployeeId == id);
           
        }

        public Task SaveAsync()
        {
            return _employeeContext.SaveChangesAsync();
        }

        public Employee Remove(Employee employee)
        {
            return _employeeContext.Remove(employee).Entity;
        }
    }
}
