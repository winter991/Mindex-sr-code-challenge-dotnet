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
            return _employeeContext.Compensation.Include("Employee").FirstOrDefault(e => e.EmployeeId == id);

        }
        public Employee Add(Employee employee)
        {
            employee.EmployeeId = Guid.NewGuid().ToString();
            _employeeContext.Employees.Add(employee);
            return employee;
        }

        public Employee GetById(string id)
        {

            //for some reason this is not returning the employyee.DirectReports
            //when I debug and check the results it works as expected
            //materialize the list so that all directReports are loaded and we don't need to call getByID them when traversing the tree
            //There is a performace hit with this as we are loading the table all at once.

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
