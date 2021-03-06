using challenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.Services
{
    public interface IEmployeeService
    {
        API_Models.CompensationResponse CreateCompensation(string id, API_Models.CreateCompensationRequest request);
        API_Models.CompensationResponse GetCompensationByEmployeeID(string id);
        Employee GetById(String id);
        Employee Create(Employee employee);
        Employee Replace(Employee originalEmployee, Employee newEmployee);
        API_Models.ReportingStructure GetReportingStructureForEmployeeID(string id);
    }
}
