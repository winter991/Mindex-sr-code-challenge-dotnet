using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.Models
{
    //Create a FK to employee via employeeIdd. This will allow us to link employees to compensations easliy and efficently    
    public class Compensation
    {
        [Key]
        public string CompansationId { get; set; }//primary key to the Compensation table         
        public virtual Employee Employee { get; set; }// link to the employee table, with this we can get the employee associated to the compensation
        public decimal Salary { get; set; }
        public DateTime EffectiveDate { get; set; }
        [ForeignKey("Employee")]
        public string EmployeeId {get;set;}
    }
}
