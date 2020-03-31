using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.API.DTO
{
    public class CreateProjectModel
    {
        [Required]
        public string code { get; set; }

        [Required]
        public string name { get; set; }
        public Guid? parentId { get; set; }
    }
}
