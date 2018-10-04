using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using notes.Core.Models;
using notes.Core.Services;
using notes.Helper;

namespace notes.Controllers
{
	public abstract class BaseController : Controller
	{
		private readonly UserService UserService;

		public ObjectId UserId => UserService.GetUserId(User.GetUserName()).Result;
		public UserSettings UserSettings => UserService.GetUserSettings(UserId).Result;

		public BaseController(UserService userService)
		{
			UserService = userService;
		}
	}
}