using TRS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
namespace TRS.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index() // default view
        {
            return View(new LoginScreen());
        }

        public IActionResult Login(string username)
        {
            if(!Request.Cookies.ContainsKey(TRS.Constants.USER_COOKIE))
            {
                Response.Cookies.Append(TRS.Constants.USER_COOKIE, username); // add a cookie with username
            }
            //Response.Cookies[TRS.Constants.USER_COOKIE]
            return RedirectToAction("Index", "Home"); // show the daily screen for today (from now on the HomeController will be called   
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete(TRS.Constants.USER_COOKIE); // delete the username cookie
            
            return RedirectToAction("Index","Login"); // return to login screen   
        }
    }
}