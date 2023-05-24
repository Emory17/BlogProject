using BlogProject.Models;

namespace BlogProject.Services.Interfaces
{
    public interface IBlogService
    {

        public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync();
        public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int? count);
        public Task<IEnumerable<BlogPost>> GetRecentBlogPostsAsync();
        public Task<IEnumerable<BlogPost>> GetRecentBlogPostsAsync(int? count);
		public Task AddTagsToBlogPostAsync(string tagNames, int? blogPostId);

		public Task<IEnumerable<Category>> GetCategoriesAsync();

        public Task<bool> IsTagOnBlogPostAsync(int? tagId, int? blogPostId);
        public Task RemoveAllBlogPostTagsAsync(int? blogPostId);

        public IEnumerable<BlogPost> SearchBlogPosts(string? searchString);
        public Task<bool> ValidateSlugAsync(string? title, int? blogPostId);

        public Task<bool> UserLikedBlogAsync(int blogPostId, string blogUserId);

        public Task<List<Tag>> GetTagsAsync();

    }
}
