using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tuan6.Models
{
    public class ProductImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product? Product { get; set; }
    }
}
