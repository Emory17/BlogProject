using System.Diagnostics;
using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace BlogProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly IBlogService _blogService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IEmailSender _emailService;
		private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IBlogService blogService, UserManager<BlogUser> userManager, IEmailSender emailService, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _blogService = blogService;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int? pageNum)
        {
			int pageSize = 4;
			int page = pageNum ?? 1;

			IPagedList<BlogPost> blogPosts = await _context.BlogPosts.Include(b=>b.Category).ToPagedListAsync(page, pageSize);

			ViewData["ActionName"] = "Index";

			return View(blogPosts);
		}

		public async Task<IActionResult> SearchIndex(string? searchString, int? pageNum)
		{
			int pageSize = 4;
			int page = pageNum ?? 1;

			IPagedList<BlogPost> blogPosts = await _blogService.SearchBlogPosts(searchString).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = "SearchIndex";

            return View(nameof(Index),blogPosts);
		}

		[Authorize]
		public async Task<IActionResult> ContactMe()
		{
			string? blogUserId = _userManager.GetUserId(User);

			if(blogUserId == null)
			{
				return NotFound();
			}

			BlogUser? blogUser = await _context.Users.FirstOrDefaultAsync(u=>u.Id == blogUserId);

			return View(blogUser);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ContactMe([Bind("FirstName,LastName,Email")] BlogUser blogUser, string? message)
		{
			string? swalMessage = "Error: Unable to send email";

			if(ModelState.IsValid)
			{
				try
				{
					string? adminEmail = _configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
					await _emailService.SendEmailAsync(adminEmail!, $"Contact Me Message From - {blogUser.FullName}", message!);
					swalMessage = "Email sent successfully!";
				}
				catch (Exception)
				{

					throw;
				}
			}

			return RedirectToAction("Index", new {swalMessage});
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}