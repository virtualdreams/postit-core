using Microsoft.AspNetCore.Mvc;
using notes.Core.Interfaces;
using notes.Core.Models;
using notes.Extensions;

namespace notes.ViewComponents
{
	public abstract class BaseViewComponent : ViewComponent
	{
		private readonly IUserService UserService;

		protected User CurrentUser => UserService.GetByNameAsync(UserClaimsPrincipal.GetUserName()).Result;

		public BaseViewComponent(IUserService user)
		{
			UserService = user;
		}
	}
}