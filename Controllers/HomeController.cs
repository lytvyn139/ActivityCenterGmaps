using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ActivityCenter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Activity = ActivityCenter.Models.Activity;

namespace ActivityCenter.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext { get; set; }

        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        public User GetUser()
        {
            return dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
        }
        
        // ************************** MY ROUTES START ********************************

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("registration")]
        public IActionResult Registration()
        {
             return View("registration");
        }

        /////////////////////////////////////////////
        ///             LOGIN BLOCK
        /////////////////////////////////////////////

        [HttpPost("process/registration")]
        public IActionResult ProcessRegistration(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("registration");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                    dbContext.Add(newUser);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetInt32("UserId", newUser.UserId);
                    return Redirect("/dashboard");
                }
            }
            else
            {
                return View("registration");
            }
        }


        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }


        [HttpPost("process/login")]
        public IActionResult ProcessLogin(Login newLog)
        {
            if (ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == newLog.LoginEmail);
                if (userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                else
                {
                    var hasher = new PasswordHasher<Login>();
                    var result = hasher.VerifyHashedPassword(newLog, userInDb.Password, newLog.LoginPassword);
                    if (result == 0)
                    {
                        ModelState.AddModelError("LoginPassword", "Invalid Email/Password");
                        return View("Index");
                    }
                    else
                    {
                        Console.Write("ACCESS GRANTED \n");
                        HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            else
            {
                return View("Index");
            }
        }

        /////////////////////////////////////////////
        ///             DASH_BOARD
        /////////////////////////////////////////////

        [HttpGet("dashboard")]
        public IActionResult Dashboard(Login newLog)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect("/");
            }
            ViewBag.User = userInDb;
            List<Models.Activity> AllActivities = dbContext.Activities
                .Include(g => g.Planner)
                .Include(g => g.Planner)
                .Include(g => g.Guests)
                .ThenInclude(gl => gl.Participants)
                .Where(g => g.Date >= DateTime.Now)
                .OrderBy(g => g.Date)
                .ToList();
            return View(AllActivities);
        }
        
        /////////////////////////////////////////////
        ///             ACTIVITIES
        /////////////////////////////////////////////

        [HttpGet("activity/new")]
        public IActionResult NewActivity()
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            return View();
        }
        
        /////////////////////////////////////////////
        ///             CREATE ACTIVITIES
        /////////////////////////////////////////////

        [HttpPost("activity/create")]
        public IActionResult CreateActivity(Activity newActivity)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            if(ModelState.IsValid)
            {
                newActivity.UserId = userInDb.UserId;
                dbContext.Activities.Add(newActivity);
                dbContext.SaveChanges();
                GuestList g = new GuestList();
                g.UserId = userInDb.UserId;
                g.ActivityId = newActivity.ActivityId;
                dbContext.GuestLists.Add(g);
                dbContext.SaveChanges();
                Console.Write("NEW ACTIVITY CREATED \n");
                return Redirect($"/activity/{newActivity.ActivityId}");
            }
            return View("NewActivity");
        }

        /////////////////////////////////////////////
        ///             SHOW ACTIVITIES
        /////////////////////////////////////////////

        [HttpGet("activity/{activityId}")]
        public IActionResult DisplayActivity(int activityId)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            ViewBag.User = userInDb;
            Activity displaying = dbContext.Activities
                .Include(w => w.Guests)
                .ThenInclude(gl => gl.Participants)
                .Include(w => w.Planner)
                .FirstOrDefault(w => w.ActivityId == activityId );
            return View(displaying);
        }

        /////////////////////////////////////////////
        ///             DELETE ACTIVITIES
        /////////////////////////////////////////////

        [HttpGet("activity/{activityId}/delete")]
        public IActionResult DeleteActivity(int activityId)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            Activity delete = dbContext.Activities.FirstOrDefault(w => w.ActivityId == activityId);
            dbContext.Activities.Remove(delete);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        /////////////////////////////////////////////
        ///             ACTIVITIES TOGGLE  
        /////////////////////////////////////////////
        [HttpGet("activity/{activityId}/{status}")]
        public IActionResult ToggleStatus(int activityId, string status)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            if(status == "join")
            {
                GuestList newStatus = new GuestList();
                newStatus.UserId = userInDb.UserId;
                newStatus.ActivityId = activityId;
                dbContext.GuestLists.Add(newStatus);
            }
            else if(status == "leave")
            {
                GuestList leave = dbContext.GuestLists.FirstOrDefault(gl => gl.UserId == userInDb.UserId && gl.ActivityId == activityId);
                dbContext.GuestLists.Remove(leave);
            }
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        // public IActionResult Privacy()
        // {
        //     return View();
        // }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        // }
    }
}
