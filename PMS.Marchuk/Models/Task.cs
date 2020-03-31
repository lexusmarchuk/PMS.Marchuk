using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Marchuk.Models
{
    /// <summary>
    /// Project Task.
    /// </summary>
    public class Task
    {
        /// <summary>
        /// Task Id.
        /// </summary>
        [Key]
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Task name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parent Task Id.
        /// </summary>
        public Guid? ParentTaskId { get; set; }

        /// <summary>
        /// Parent Task.
        /// </summary>
        [ForeignKey("ParentTaskId")]
        public Task ParentTask { get; set; }

        /// <summary>
        /// Project Id.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Project.
        /// </summary>
        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }

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
