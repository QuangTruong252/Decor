using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    public class BulkDeleteDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one ID must be provided")]
        public List<int> Ids { get; set; }
    }
}
