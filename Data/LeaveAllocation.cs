using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Data
{
    public class LeaveAllocation
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Number of Days")]
        public int NumberOfDays { get; set; }
        public DateTime DateCreated { get; set; }
        [ForeignKey("EmployeeId")]
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; }
        [ForeignKey("LeaveTypeId")]
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public int Period { get; set; }
    }
}
