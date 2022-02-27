using challenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.API_Models
{
    public class CompensationResponse
    {
        //Create reponse object to only return fields that we want to.
        public Employee Employee { get; set; }
        public decimal Salary { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
