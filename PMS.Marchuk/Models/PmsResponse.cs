using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMS.Marchuk.Models
{
    /// <summary>
    /// Resonse.
    /// </summary>
    public class PmsResponse
    {
        /// <summary>
        /// Is Success.
        /// </summary>
        public bool Success { get { return !Errors.Any(); } }

        /// <summary>
        /// Entity Id.
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Message. 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error messages.
        /// </summary>
        public List<string> Errors { get; set; }

        public PmsResponse()
        {
            Errors = new List<string>();
        }
    }
}
