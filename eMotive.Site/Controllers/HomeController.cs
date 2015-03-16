﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Pages;
using eMotive.Models.Objects.StatusPages;
using Extensions;
using ServiceStack.Mvc;

namespace eMotive.SCE.Controllers
{//TODO: Add extra partial pages to the work db version. Add group selection to user creation.

    public class HomeController : ServiceStackController
    {
        private readonly ISessionManager signupManager;
        private readonly IPartialPageManager pageManager;
        private readonly IUserManager userManager;
        private readonly IGroupManager groupManager;

        public HomeController(IPartialPageManager _pageManager, IUserManager _userManager, ISessionManager _signupManager, IGroupManager _groupManager)
        {
            pageManager = _pageManager;
            userManager = _userManager;
            signupManager = _signupManager;
            groupManager = _groupManager;
        }

        [Authorize(Roles = "SCE")]
        public ActionResult Index()
        {
            var user = userManager.Fetch(User.Identity.Name);

            var replacements = new Dictionary<string, string>(4)
                    {
                        {"#forename#", user.Forename},
                        {"#surname#", user.Surname}
                    };

            var homeView = signupManager.FetchHomeView(User.Identity.Name);

            Dictionary<string, PartialPage> pageSections;



                if (homeView.HasSignedUp)
                    pageSections = pageManager.FetchPartials(new[] { "SCE-Home-Header-Signed", "SCE-Home-Footer-Signed" }).ToDictionary(k => k.Key, v => v);
                else
                    pageSections = pageManager.FetchPartials(new[] { "SCE-Home-Header-Unsigned", "SCE-Home-Footer-Unsigned" }).ToDictionary(k => k.Key, v => v);
            

            var modifiedSections = pageSections.ToDictionary(section => section.Key.Substring(section.Key.IndexOf('-') + 1), section => section.Value);


            homeView.PageSections = modifiedSections;

            if (replacements.HasContent())
            {
                foreach (var partial in homeView.PageSections.Values)
                {
                    var sb = new StringBuilder(partial.Text);

                    foreach (var replacment in replacements)
                    {
                        sb.Replace(replacment.Key, replacment.Value);
                    }

                    partial.Text = sb.ToString();
                }
            }
            return View(homeView);
        }

        public FileResult DownloadCalanderAppointment(int id)
        {
            var homeView = signupManager.FetchHomeView(User.Identity.Name);
            if (homeView == null)
                throw new HttpException(404, "The requested file was not found");

            var cal = new iCalendar();

            var appointment = cal.Create<Event>();
            appointment.Summary = "Birmingham Admission Interview";
            appointment.Location = "The College of Medical and Dental Sciences, The University of Birmingham";
          //  appointment.Start = new iCalDateTime(homeView.SignUpDate);
          //  appointment.End = new iCalDateTime(homeView.SignUpDate);
          //  appointment.Duration = TimeSpan.FromHours(2);

            var serializer = new iCalendarSerializer(cal);

            var fs = new MemoryStream();

            serializer.Serialize(cal, fs, System.Text.Encoding.UTF8);
            fs.Seek(0, 0);

            return File(fs, "text/calendar", "BirminghamAdmissionInterview.ics");
        }

        public ActionResult Error()
        {
            ErrorView errorView;
            if (TempData["CriticalErrors"] != null)
            {
                errorView = TempData["CriticalErrors"] as ErrorView;
                TempData["CriticalErrors"] = TempData["CriticalErrors"];
            }
            else
            {
                errorView = new ErrorView
                    {
                        ControllerName = "Home",
                        Errors = new[] {"An error occurred."}
                    };
            }


            return View(errorView);
        }

        public ActionResult Success()
        {
            if (TempData["SuccessView"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var successView = TempData["SuccessView"] as SuccessView;
            TempData["SuccessView"] = TempData["SuccessView"];

            return View(successView);

        }

        [Authorize(Roles = "SCE")]
        public ActionResult ContactUs()
        {
            var contactUsPage = pageManager.Fetch("ContactUs");
            return View(contactUsPage);
        }
    }
}