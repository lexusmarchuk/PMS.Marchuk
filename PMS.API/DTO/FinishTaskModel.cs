﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.API.DTO
{
    public class FinishTaskModel
    {
        [Required]
        public Guid Id { get; set; }
    }
}
