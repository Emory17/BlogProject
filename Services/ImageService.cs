using BlogProject.Services.Interfaces;

namespace BlogProject.Services
{
	public class ImageService : IImageService
	{

		private readonly string? _defaultUserImage = "/img/DefaultUserImage.png";
		private readonly string? _defaultBlogImage = "/img/DefaultBlogImage.png";
		private readonly string? _defaultCategoryImage = "/img/DefaultCategoryImage.png";
		public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, int defaultImage)
		{
			if (fileData == null || fileData.Length == 0)
			{
				switch(defaultImage)
				{
					case 1: return _defaultUserImage;
					case 2: return _defaultBlogImage;
					case 3: return _defaultCategoryImage;
				}
			}

			try
			{
				string? imageBase64Data = Convert.ToBase64String(fileData!);
				imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}");

				return imageBase64Data;

			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
		{
			try
			{
				using MemoryStream memoryStream = new MemoryStream();
				await file!.CopyToAsync(memoryStream);
				byte[] byteFile = memoryStream.ToArray();
				memoryStream.Close();

				return byteFile;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
