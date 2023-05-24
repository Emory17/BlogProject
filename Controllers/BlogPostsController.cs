using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogProject.Data;
using BlogProject.Models;
using Microsoft.AspNetCore.Authorization;
using BlogProject.Services.Interfaces;
using BlogProject.Helpers;
using BlogProject.Services;

namespace BlogProject.Controllers
{
    [Authorize]
    public class BlogPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlogService _blogService;
        private readonly IImageService _imageService;

        public BlogPostsController(ApplicationDbContext context, IBlogService blogService, IImageService imageService)
        {
            _context = context;
            _blogService = blogService;
            _imageService = imageService;
        }

        // GET: BlogPosts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BlogPosts.Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: BlogPosts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .Include(b => b.Category)
                .Include(b => b.Tags)
                .Include(b=>b.Comments)
                .ThenInclude(c=>c.Author)
                .FirstOrDefaultAsync(m => m.Slug == slug);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Create
        public IActionResult Create()
        {
            BlogPost blogPost = new BlogPost();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View(blogPost);
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Abstract,Content,CreatedDate,UpdatedDate,Slug,IsDeleted,IsPublished,CategoryId,ImageFile,ImageData,ImageType")] BlogPost blogPost, string? stringTags)
        {
            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {
                if(!await _blogService.ValidateSlugAsync(blogPost.Title, blogPost.Id))
                {
                    ModelState.AddModelError("Title", "A similar Title/Slug is already in use.");

                    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
                    return View(blogPost);
                }

                blogPost.Slug = StringHelper.BlogPostSlug(blogPost.Title);

				if (blogPost.ImageFile != null)
				{
					blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);
					blogPost.ImageType = blogPost.ImageFile.ContentType;
				}

				blogPost.CreatedDate = DateTime.UtcNow;

				if (!string.IsNullOrEmpty(stringTags))
				{
					await _blogService.AddTagsToBlogPostAsync(stringTags, blogPost.Id);
				}

				_context.Add(blogPost);
                await _context.SaveChangesAsync();

				return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _context.BlogPosts.Include(b=>b.Tags).FirstOrDefaultAsync(b=>b.Id == id);

            if (blogPost == null)
            {
                return NotFound();
            }

            List<string> tagNames = blogPost.Tags.Select(t=>t.Name!).ToList();
            ViewData["Tags"] = string.Join(", ", tagNames) + ", ";

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Abstract,Content,CreatedDate,UpdatedDate,Slug,IsDeleted,IsPublished,CategoryId,ImageFile,ImageData,ImageType")] BlogPost blogPost, string? stringTags)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {
                try
                {
					blogPost.CreatedDate = DateTime.SpecifyKind(blogPost.CreatedDate, DateTimeKind.Utc);
                    blogPost.UpdatedDate = DateTime.UtcNow;

                    if (!await _blogService.ValidateSlugAsync(blogPost.Title, blogPost.Id))
                    {
                        ModelState.AddModelError("Title", "A similar Title/Slug is already in use.");

                        ViewData["Tags"] = stringTags != null && stringTags.Trim().EndsWith(',') ? stringTags : stringTags + ", ";
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
                        return View(blogPost);
                    }

                    blogPost.Slug = StringHelper.BlogPostSlug(blogPost.Title);

					if (blogPost.ImageFile != null)
					{
						blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);
						blogPost.ImageType = blogPost.ImageFile.ContentType;
					}

					_context.Update(blogPost);
                    await _context.SaveChangesAsync();

                    await _blogService.RemoveAllBlogPostTagsAsync(blogPost.Id);

                    if (!string.IsNullOrEmpty(stringTags))
                    {
                        await _blogService.AddTagsToBlogPostAsync(stringTags, blogPost.Id);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { slug = blogPost.Slug });
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        public async Task<IActionResult> LikeBlogPost(int blogPostId, string blogUserId)
        {
            BlogUser? blogUser = await _context.Users.Include(u=>u.BlogLikes).FirstOrDefaultAsync(u=>u.Id == blogUserId);
            bool result = false;
            BlogLike blogLike = new();

            if(blogUser != null)
            {
                if(!blogUser.BlogLikes.Any(b => b.BlogPostId == blogPostId))
                {
                    blogLike = new BlogLike()
                    {
                        BlogPostId = blogPostId,
                        IsLiked = true,
                    };

                    blogUser.BlogLikes.Add(blogLike);
                }
                else
                {
                    blogLike = await _context.BlogLikes.FirstOrDefaultAsync(b => b.BlogPostId == blogPostId && b.BlogUserId == blogUserId);

                    blogLike.IsLiked = !blogLike.IsLiked;
                }
                result = blogLike.IsLiked;
                await _context.SaveChangesAsync();
            }
            return Json(new
            {
                isLiked = result,
                count = _context.BlogLikes.Where(b => b.BlogPostId == blogPostId && b.IsLiked).Count()
            });
        }

        // GET: BlogPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BlogPosts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BlogPosts'  is null.");
            }
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                _context.BlogPosts.Remove(blogPost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogPostExists(int id)
        {
            return (_context.BlogPosts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
