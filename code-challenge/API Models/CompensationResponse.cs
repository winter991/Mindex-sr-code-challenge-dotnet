using challenge.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.API_Models
{
    public class CompensationResponse
    {
        //Create reponse object to only return fields that we want to.
        //  employee is the first+ last name of the employee record
        // keep this conisistant with ReportingStructure
        public string employee { get; set; }
        public decimal salary { get; set; }
        public DateTime effectiveDate { get; set; }
        //used to pass back to the controlller that an error has occured so we can pass that back to the user
        // don't return this in the response to the caller
        [JsonIgnore]
        public string errorMessage { get; set; }
     
    }
}
