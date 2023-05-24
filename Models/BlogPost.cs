using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace BlogProject.Models
{
    public class BlogPost
    {
        //Primary Key
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }

        [StringLength(600, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Abstract { get; set; }

        [Required]
        public string? Content { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime UpdatedDate { get; set; }

        [Required]
        public string? Slug { get; set; }

        [Display(Name = "Deleted?")]
        public bool IsDeleted { get; set; }

        [Display(Name = "Published?")]
        public bool IsPublished { get; set; }

        //Foreign Key
        public int CategoryId { get; set; }

        // Image Properties
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        // Navigation Properties

        public virtual Category? Category { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();

        public virtual ICollection<BlogLike> Likes { get; set; } = new HashSet<BlogLike>();
    }
}
