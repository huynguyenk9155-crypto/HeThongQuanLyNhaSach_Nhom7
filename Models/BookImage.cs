using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tuan6.Models
{
    public class BookImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty;

        [ForeignKey("Book")]
        public int BookId { get; set; }

        public Book? Book { get; set; }
    }
}
