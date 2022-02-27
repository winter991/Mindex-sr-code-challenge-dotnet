using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using challenge.Models;
using Microsoft.Extensions.Logging;
using challenge.Repositories;
using challenge.API_Models;

namespace challenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public Employee Create(Employee employee)
        {
            if(employee != null)
            {
                _employeeRepository.Add(employee);
                _employeeRepository.SaveAsync().Wait();
            }

            return employee;
        }

        public Employee GetById(string id)
        {
            if(!String.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id);
            }

            return null;
        }
        //
        public ReportingStructure GetReportingStructureForEmployeeID(string id)
        {
            //TODO check to see how we should handle invalid IDs/ employee not found
            if (String.IsNullOrEmpty(id))
            {
                return null; 
            }

            //Get employee if not found just return null
            var employee = _employeeRepository.GetById(id);
            if(employee == null)
            {
                return null;
            }
            //
            int numberOfReports = GetNumberOfReports(employee.DirectReports);

            return new API_Models.ReportingStructure
            {
                employee = employee,
                numberOfReports = numberOfReports
            };
        }

      

        public Employee Replace(Employee originalEmployee, Employee newEmployee)
        {
            if(originalEmployee != null)
            {
                _employeeRepository.Remove(originalEmployee);
                if (newEmployee != null)
                {
                    // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                    _employeeRepository.SaveAsync().Wait();

                    _employeeRepository.Add(newEmployee);
                    // overwrite the new id with previous employee id
                    newEmployee.EmployeeId = originalEmployee.EmployeeId;
                }
                _employeeRepository.SaveAsync().Wait();
            }

            return newEmployee;
        }


        #region private methods
        /// <summary>
        /// Recursivly gets the number of reports for an employee by descending down it's decendants
        /// 
        /// 
        /// </summary>
        /// <param name="employees"></param>
        /// <returns></returns>
        private int GetNumberOfReports(IList<Employee> reports)
        {

            int reportCount=0;
            if(reports == null)
            {
                return reportCount;
            }
            else
            {
                //loop over the direct reports
                // if the direct reports do not have reports of their own we don't need to recurse and can just add to the count instead
                foreach( var report in reports)
                {
                    //increment the number of reports for each direct report
                    ++reportCount;
                    if (report.DirectReports?.Count() > 0)
                    {
                        //recursively calll get number of reports for each direct report that has direct reports for where the original employee is the root
                        reportCount += GetNumberOfReports(report.DirectReports);
                    }
                }
            }
            return reportCount;
        }


        #endregion
    }
}
