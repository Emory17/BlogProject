using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tag Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }

        public ICollection<BlogPost> BlogPosts = new HashSet<BlogPost>();
    }
}
