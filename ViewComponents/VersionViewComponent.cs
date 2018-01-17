using Microsoft.AspNetCore.Mvc;
using notes.Core.Services;

namespace notes.ViewComponents
{
	public class VersionViewComponent : BaseViewComponent
	{
		private readonly UserService UserService;

		public VersionViewComponent(UserService user)
			: base(user)
		{
			UserService = user;
		}

		public IViewComponentResult Invoke() => Content($"{Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion}");
	}
}