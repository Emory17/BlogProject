using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Body { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        public DateTime Updated { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? UpdateReason { get; set; }

        public int BlogPostId { get; set; }
        public BlogPost? BlogPost { get; set; }

        [Required]
        public string? AuthorId { get; set; }
        public BlogUser? Author { get; set; }

    }
}
