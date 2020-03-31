using System;
using System.ComponentModel.DataAnnotations;

namespace PMS.API.DTO
{
    public class CreateTaskModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        [Required]
        public Guid ProjectId { get; set; }
    }
}
