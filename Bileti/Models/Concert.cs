using System.ComponentModel.DataAnnotations;

namespace Bileti.Models
{
    public class Concert
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Location { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableTickets { get; set; }
        public DateTime? LastPurchaseAt { get; set; }  

    }

}
