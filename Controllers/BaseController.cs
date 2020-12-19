using Microsoft.AspNetCore.Mvc;
using notes.Core.Interfaces;
using notes.Core.Models;
using notes.Extensions;

namespace notes.Controllers
{
	public abstract class BaseController : Controller
	{
		private readonly IUserService UserService;

		protected User CurrentUser => UserService.GetByNameAsync(User.GetUserName()).Result;

		protected int PageSize => CurrentUser.Items;

		public BaseController(IUserService userService)
		{
			UserService = userService;
		}
	}
}