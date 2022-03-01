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

        //Creates a compensation for a given customer
        //Takes a employee ID and the salary and effectiveDate of the compensation
        //returns the employee,salary,effectiveDate
        //salary and effectiveDate are required and checked by model validation attributes
        public API_Models.CompensationResponse CreateCompensation (string id,CreateCompensationRequest request)           
        {
            // Since the response does not contain a  unique identifier for the compensation, we can assumme that there is only a single compensation
            //Therefore if an employee already has a compensation will return an error stating that a compensation has been for this employee

            //handle error cases for if the request object is not provided or if the employee does not exist for the provided ID
            if (request == null)
            {
                _logger.LogError("Create Compensation request object is null");
                return null;
            }
            var employee = _employeeRepository.GetById(id);
            if(employee == null)
            {
                _logger.LogError(String.Format("Could not find employee for employeeID {0}",id));
            }
            if ( _employeeRepository.GetCompensationByEmployeeID(id) != null)
            {
                _logger.LogError(String.Format("A compensation alrready exists for  employeeID", id));
                //  don't throw an exception as that willl be treated a 500 internal server error
                // this allows the user to take corrective action and fix the request
                // in this case they would need to do an update(not implemented) of the compensation
                var errorComp = new API_Models.CompensationResponse
                {
                    errorMessage = String.Format("A compensation record already exists for  employeeID", id)
                };
                return errorComp;
            }
            //create new commpenation  and assign it to the emmployee. This will overrrite 
            var compensation = new Compensation
            {
                Employee = employee,
                Salary = request.Salary.Value,
                EffectiveDate = request.EffectiveDate.Value
            };

           
          //Compensation should now be linked to the emplloyee by employeID FK
            _employeeRepository.Add(compensation);
            _employeeRepository.SaveAsync().Wait();
            //Convert thhe result to the response model and send it back
            var result = new API_Models.CompensationResponse
            {
                effectiveDate = compensation.EffectiveDate,
                salary = compensation.Salary,
                employee = String.Format("{0} {1}", employee.FirstName, employee.LastName),
            };
            return result;

        }
        public API_Models.CompensationResponse GetCompensationByEmployeeID (string id)
        {
            //et the 
            API_Models.CompensationResponse result = null;
            if (!String.IsNullOrEmpty(id))
            {
                var compensation = _employeeRepository.GetCompensationByEmployeeID(id);
                if(compensation != null)
                {
                     result = new API_Models.CompensationResponse
                    {
                        effectiveDate = compensation.EffectiveDate,
                        salary = compensation.Salary,
                        employee = String.Format("{0} {1}", compensation?.Employee.FirstName, compensation?.Employee.LastName)
                    };
                }
            }

            return result;

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
        // for a given employeeID determine the number of employees that report under it
        // takes a string emplyeeID and returns the employe(first+ last)
        // for example employeeId: 16a596ae-edd3-4847-99fe-c4518e82c86f
        // returns employee:John Lennon, numberOfReports:4 
        // 
        public ReportingStructure GetReportingStructureForEmployeeID(string id)
        {
            
            if (String.IsNullOrEmpty(id))
            {
                return null; 
            }

            //Get employee, if not found, just return null
            var employee = _employeeRepository.GetById(id);
            if(employee == null)
            {
                return null;
            }
            //we should already have the employee's direct reports(and their direct reports) based on the employee object structure and how the employee was loaded
            int numberOfReports = GetNumberOfReports(employee.DirectReports);

            return new API_Models.ReportingStructure
            {
                employee = String.Format("{0} {1}",employee.FirstName,employee.LastName),
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
      
        //Recursivly gets the number of reports for an employee by descending down it's decendants       
        private int GetNumberOfReports(IList<Employee> reports)
        {
            int reportCount=0;
            if(reports == null)//we've reached the end of the employee reporting tree
            {
                return reportCount;
            }
            else
            {
                //loop over the direct reports
                // if the direct reports do not have reports of their own, we don't need to recurse and can just add to the count instead
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
