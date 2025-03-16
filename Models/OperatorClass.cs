using System;
using System.ComponentModel.DataAnnotations;

namespace TraceabilityV3
{    
    [MetadataType(typeof(OperatorClass))]
    public partial class Operator
    {
    }
    
    public class OperatorClass
    {
        [Display(Name = "Employee ID")]
        public int EmployeeId { get; set; } 

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; } 

        [Display(Name = "Operator Code")]
        public string Code { get; set; } 

        [Display(Name = "Waypoint")]
        public int? WaypointID { get; set; } 
    }
}
