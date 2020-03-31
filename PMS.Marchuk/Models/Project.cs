using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Marchuk.Models
{
    /// <summary>
    /// Project.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Project Id.
        /// </summary>
        [Key]
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Parent Project Id.
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Parent Project.
        /// </summary>
        [ForeignKey("ParentId")]        
        public Project Parent { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Finish date.
        /// </summary>
        public DateTime FinishDate { get; set; }

        /// <summary>
        /// State.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public State State { get; set; }
    }
}
