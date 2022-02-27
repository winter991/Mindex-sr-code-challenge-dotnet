using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.API_Models
{
    public class CreateCompensationRequest
    {
        //TODO find out if we need to require Salary and effective date. for now require it. 
        //Reuquire that salary and effective date be provided

        [Required]
        public decimal? Salary { get; set; }
        [Required]
        public DateTime? EffectiveDate { get; set; }
    }
}
