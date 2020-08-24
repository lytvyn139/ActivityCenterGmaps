using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ActivityCenter.Validations;

namespace ActivityCenter.Models
{
    public class Activity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required (ErrorMessage = "Name is required")]
        [MinLength(2,ErrorMessage="Name must be at least 2 characters")]
        public string Title { get; set; }


        [Required (ErrorMessage = "Date & Time are required")]
        [DataType(DataType.DateTime)]
        [FutureDate]
        public DateTime Date { get; set; }


        [Required (ErrorMessage = "A number is required")]
        public int DurationInt { get; set; }


        [Required (ErrorMessage = "Select a unit of time.")]
        public string DurationStr { get; set; }


        [Required (ErrorMessage = "Description is required")]
        public string Description { get; set; }

	[Required (ErrorMessage = "Location is required")]
	public string Location { get; set; }

        public int UserId { get; set; }

        public User Planner { get; set; }

        public List<GuestList> Guests { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}