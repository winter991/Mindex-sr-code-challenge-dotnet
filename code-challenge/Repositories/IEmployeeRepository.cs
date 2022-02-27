using challenge.Models;
using System;
using System.Threading.Tasks;

namespace challenge.Repositories
{
    public interface IEmployeeRepository
    {
        Compensation Add( Compensation compensation);
        Compensation GetCompensationByEmployeeID(String id);
        Employee GetById(String id);
        Employee Add(Employee employee);
        Employee Remove(Employee employee);       
        Task SaveAsync();
    }
}