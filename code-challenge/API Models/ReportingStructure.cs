using challenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.API_Models
{
    // returns the employee and it's number of reportss
    //As this is not a DB object created a seperate folder to store api/requests and responses
    public class ReportingStructure
    {
        public Employee employee { get; set; }
        public int numberOfReports { get; set; }

    }
}
