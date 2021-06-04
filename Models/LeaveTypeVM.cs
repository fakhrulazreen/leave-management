using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveTypeVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required]
        [Range(1,25,ErrorMessage = "Please enter a valid number")]
        [Display(Name = "Default Number of Days")]
        public int DefaultDays { get; set; } //name must be same as data.leavetype for mapping
        [Display(Name="Date Created")]
        public DateTime? DateCreated { get; set; } //is nuablle
    }

    //can have multiple classes depends on purpose and programming style
    //public class DetailsLeaveTypeVM
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    [Display(Name = "Date Created")]
    //    public DateTime DateCreated { get; set; }
    //}

    //public class CreateLeaveTypeVM
    //{
    //    [Required]
    //    public string Name { get; set; }
    //}
}
