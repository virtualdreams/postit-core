using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using notes.Core.Services;
using notes.Helper;

namespace notes.Controllers
{
	public abstract class BaseController : Controller
	{	
		private readonly UserService UserService;

		public ObjectId UserId => UserService.GetUserId(User.GetUserName());

		public BaseController(UserService userService)
		{
			UserService = userService;
		}
	}
}