using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Reports.Users;
using eMotive.Models.Objects.Signups;
using eMotive.Services.Interfaces;
using Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ServiceStack.Mvc;

namespace eMotive.SCE.Areas.Admin.Controllers
{
    [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin, UGC")]
    public class ReportsController : ServiceStackController
    {
        private readonly IReportService reportService;
        private readonly IDocumentManagerService documentManager;
        private readonly IGroupManager groupManager;
        private readonly ISessionManager signupManager;
        private readonly IUserManager userManager;
        private readonly INotificationService notificationService;
        private readonly IFormManager formManager;
        private readonly IEmailService emailService;
        private readonly IeMotiveConfigurationService configurationService;
        private readonly string CONTENT_TYPE;

        public ReportsController(IReportService _reportService, IDocumentManagerService _documentManager, ISessionManager _signupManager, IGroupManager _groupManager, IUserManager _userManager, INotificationService _notificationService, IFormManager _formManager, IEmailService _emailService, IeMotiveConfigurationService _configurationService)
        {
            reportService = _reportService;
            documentManager = _documentManager;
            signupManager = _signupManager;
            userManager = _userManager;
            notificationService = _notificationService;
            groupManager = _groupManager;
            formManager = _formManager;
            emailService = _emailService;
            configurationService = _configurationService;

            CONTENT_TYPE = documentManager.FetchMimeTypeForExtension("xlxs").Type;
        }

        public ActionResult Index()
        {
            var signupAdminView = new AdminSignupView
            {
                Signups = signupManager.FetchAllM(),
                LoggedInUser = userManager.Fetch(User.Identity.Name)
            };

           // var sites = formManager.FetchFormList("Sites");

           // ViewBag.Sites = sites;

            return View(signupAdminView);
        }

        public FileStreamResult AllSCEs()
        {
            var users = reportService.FetchAllSCEs();
            //var sces = 
            if (users.HasContent())
            {
                using (var xlPackage = new ExcelPackage())
                {
                    const string reportName = "All SCEs Report";
                    var worksheet = xlPackage.Workbook.Worksheets.Add(reportName);

                    var password = Guid.NewGuid().ToString("N");
                    xlPackage.Encryption.IsEncrypted = true;
                    xlPackage.Encryption.Password = password;

                    SendPassword(password, reportName);
                    worksheet.Cells[1,1].Value = "This report contains personal details. Please ensure that it is kept confidential.";
                    using (var r = worksheet.Cells["A1:X1"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(122, 255, 94, 0));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    int x = 2;
                    using (var r = worksheet.Cells["A2:X2"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    //18
                    int i = 0;
                    var trustDict = formManager.FetchFormList("Trusts").Collection.ToDictionary(k => k.Value, v=> v.Text);
                    var gradeDict = formManager.FetchFormList("Grade").Collection.ToDictionary(k => k.Value, v => v.Text);
                   // var siteDict = formManager.FetchFormList("Sites").Collection.ToDictionary(k => k.Value, v => v.Text);
                    var groupDict = groupManager.FetchGroups().ToDictionary(k => k.ID, v => v.Name);

                    worksheet.Cells[x, ++i].Value = "Username";
                    worksheet.Cells[x, ++i].Value = "Title";
                    worksheet.Cells[x, ++i].Value = "Forename";
                    worksheet.Cells[x, ++i].Value = "Surname";
                    worksheet.Cells[x, ++i].Value = "GMC Number";
                    worksheet.Cells[x, ++i].Value = "Main Specialty";
                    worksheet.Cells[x, ++i].Value = "Email";
                    worksheet.Cells[x, ++i].Value = "SecretaryEmail";
                    worksheet.Cells[x, ++i].Value = "OtherEmail";
                    worksheet.Cells[x, ++i].Value = "Trust";
                    worksheet.Cells[x, ++i].Value = "Grade";
                    worksheet.Cells[x, ++i].Value = "Address 1";
                    worksheet.Cells[x, ++i].Value = "Address 2";
                    worksheet.Cells[x, ++i].Value = "City";
                    worksheet.Cells[x, ++i].Value = "Region";
                    worksheet.Cells[x, ++i].Value = "Postcode";
                    worksheet.Cells[x, ++i].Value = "Phone Work";
                    worksheet.Cells[x, ++i].Value = "Phone Mobile";
                    worksheet.Cells[x, ++i].Value = "Phone Other";
                    worksheet.Cells[x, ++i].Value = "Trained Date";
                    worksheet.Cells[x, ++i].Value = "Trained Within 3 Years";
                    worksheet.Cells[x, ++i].Value = "Enabled";

                  //  if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                  //  {
                        worksheet.Cells[x, ++i].Value = "Notes";
               //     }

                    x++;

                    foreach (var user in users)
                    {
                        if (!user.Enabled)
                        {
                            using (var r = worksheet.Cells[string.Concat("A", x, ":X", x)])
                            {
                                r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 203, 203));
                            }
                        }

                        i = 0;
                        worksheet.Cells[x, ++i].Value = user.Username;
                        worksheet.Cells[x, ++i].Value = user.Title;
                        worksheet.Cells[x, ++i].Value = user.Forename;
                        worksheet.Cells[x, ++i].Value = user.Surname;
                        worksheet.Cells[x, ++i].Value = user.GMCNumber;
                        worksheet.Cells[x, ++i].Value = groupDict[user.MainSpecialty];
                        worksheet.Cells[x, ++i].Value = user.Email;
                        worksheet.Cells[x, ++i].Value = user.SecretaryEmail;
                        worksheet.Cells[x, ++i].Value = user.OtherEmail;
                        worksheet.Cells[x, ++i].Value = trustDict[user.Trust];
                        worksheet.Cells[x, ++i].Value = gradeDict[user.Grade];
                        worksheet.Cells[x, ++i].Value = user.Address1;
                        worksheet.Cells[x, ++i].Value = user.Address2;
                        worksheet.Cells[x, ++i].Value = user.City;
                        worksheet.Cells[x, ++i].Value = user.Region;
                        worksheet.Cells[x, ++i].Value = user.Postcode;
                        worksheet.Cells[x, ++i].Value = user.PhoneWork;
                        worksheet.Cells[x, ++i].Value = user.PhoneMobile;
                        worksheet.Cells[x, ++i].Value = user.PhoneOther;
                        worksheet.Cells[x, ++i].Value = user.DateTrained.ToString("D");
                        worksheet.Cells[x, ++i].Value = user.Trained ? "Yes" : "No";
 
                     //   worksheet.Cells[x, ++i].Value = user;
                        worksheet.Cells[x, ++i].Value = user.Enabled ? "Yes" : "No";

                      //  if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                      //  {
                            worksheet.Cells[x, ++i].Value = user.Notes;
                      //  }

                        x++;
                    }
                   
                    
                    return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE) { FileDownloadName = string.Format("{0}.xlsx", reportName) };

                }
            }

            return null;
        }

        public FileStreamResult AllObservers()
        {
            var loggedInUser = userManager.Fetch(User.Identity.Name);

            var users = reportService.FetchAllObservers();
          //  var groupsDict = groupManager.FetchGroups().ToDictionary(k => k.ID, v => v.Name);
            if (users.HasContent())
            {
                using (var xlPackage = new ExcelPackage())
                {
                    const string REPORT_NAME = "All Observers Report";
                    var worksheet = xlPackage.Workbook.Worksheets.Add(REPORT_NAME);

                    int x = 1;
                    using (var r = worksheet.Cells["A1:S1"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //18
                    int i = 0;

                    worksheet.Cells[x, ++i].Value = "Username";
                    worksheet.Cells[x, ++i].Value = "Title";
                    worksheet.Cells[x, ++i].Value = "Forename";
                    worksheet.Cells[x, ++i].Value = "Surname";
                    worksheet.Cells[x, ++i].Value = "Email";
                    worksheet.Cells[x, ++i].Value = "SecretaryEmail";
                    worksheet.Cells[x, ++i].Value = "OtherEmail";
                    worksheet.Cells[x, ++i].Value = "PhoneWork";
                    worksheet.Cells[x, ++i].Value = "PhoneMobile";
                    worksheet.Cells[x, ++i].Value = "PhoneOther";
                    worksheet.Cells[x, ++i].Value = "Trained";
                    worksheet.Cells[x, ++i].Value = "Enabled";

                    if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                    {
                        worksheet.Cells[x, ++i].Value = "Notes";
                    }

                    x++;

                    foreach (var user in users)
                    {
                        if (!user.Enabled)
                        {
                            using (var r = worksheet.Cells[string.Concat("A", x, ":S", x)])
                            {
                                r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 203, 203));
                            }
                        }

                     //   var specialty = "Unknown";
                       // groupsDict.TryGetValue(user.MainSpecialty, out specialty);

                        i = 0;
                        worksheet.Cells[x, ++i].Value = user.Username;
                        worksheet.Cells[x, ++i].Value = user.Title;
                        worksheet.Cells[x, ++i].Value = user.Forename;
                        worksheet.Cells[x, ++i].Value = user.Surname;
                        worksheet.Cells[x, ++i].Value = user.Email;
                        worksheet.Cells[x, ++i].Value = user.SecretaryEmail;
                        worksheet.Cells[x, ++i].Value = user.OtherEmail;
                        worksheet.Cells[x, ++i].Value = user.PhoneWork;
                        worksheet.Cells[x, ++i].Value = user.PhoneMobile;
                        worksheet.Cells[x, ++i].Value = user.PhoneOther;
                        worksheet.Cells[x, ++i].Value = user.Trained ? "Yes" : "No";
                        worksheet.Cells[x, ++i].Value = user.Enabled ? "Yes" : "No";

                        if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                        {
                            worksheet.Cells[x, ++i].Value = user.Notes;
                        }

                        x++;
                    }

                    return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE) { FileDownloadName = string.Format("{0}.xlsx", REPORT_NAME) };

                }
            }

            return null;
        }

        public FileStreamResult AllInterviewersAndObservers()
        {
            var loggedInUser = userManager.Fetch(User.Identity.Name);

            var users = reportService.FetchAllInterviewersAndObservers();
            //  var groupsDict = groupManager.FetchGroups().ToDictionary(k => k.ID, v => v.Name);
            if (users.HasContent())
            {
                using (var xlPackage = new ExcelPackage())
                {
                    const string REPORT_NAME = "All Interviewers and Observers Report";
                    var worksheet = xlPackage.Workbook.Worksheets.Add(REPORT_NAME);

                    int x = 1;
                    using (var r = worksheet.Cells["A1:S1"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //18
                    var i = 0;

                    worksheet.Cells[x, ++i].Value = "Username";
                    worksheet.Cells[x, ++i].Value = "Groups";
                    worksheet.Cells[x, ++i].Value = "Title";
                    worksheet.Cells[x, ++i].Value = "Forename";
                    worksheet.Cells[x, ++i].Value = "Surname";
                    worksheet.Cells[x, ++i].Value = "Email";
                    worksheet.Cells[x, ++i].Value = "SecretaryEmail";
                    worksheet.Cells[x, ++i].Value = "OtherEmail";
                    worksheet.Cells[x, ++i].Value = "PhoneWork";
                    worksheet.Cells[x, ++i].Value = "PhoneMobile";
                    worksheet.Cells[x, ++i].Value = "PhoneOther";
                    worksheet.Cells[x, ++i].Value = "Trained";
                    worksheet.Cells[x, ++i].Value = "Enabled";

                    if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                    {
                        worksheet.Cells[x, ++i].Value = "Notes";
                    }

                    x++;

                    foreach (var user in users)
                    {
                        if (!user.Enabled)
                        {
                            using (var r = worksheet.Cells[string.Concat("A", x, ":S", x)])
                            {
                                r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 203, 203));
                            }
                        }

                        //   var specialty = "Unknown";
                        // groupsDict.TryGetValue(user.MainSpecialty, out specialty);

                        i = 0;
                        worksheet.Cells[x, ++i].Value = user.Username;
                        worksheet.Cells[x, ++i].Value = user.Groups;
                        worksheet.Cells[x, ++i].Value = user.Title;
                        worksheet.Cells[x, ++i].Value = user.Forename;
                        worksheet.Cells[x, ++i].Value = user.Surname;
                        worksheet.Cells[x, ++i].Value = user.Email;
                        worksheet.Cells[x, ++i].Value = user.SecretaryEmail;
                        worksheet.Cells[x, ++i].Value = user.OtherEmail;
                        worksheet.Cells[x, ++i].Value = user.PhoneWork;
                        worksheet.Cells[x, ++i].Value = user.PhoneMobile;
                        worksheet.Cells[x, ++i].Value = user.PhoneOther;
                        worksheet.Cells[x, ++i].Value = user.Trained ? "Yes" : "No";
                        worksheet.Cells[x, ++i].Value = user.Enabled ? "Yes" : "No";

                        if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                        {
                            worksheet.Cells[x, ++i].Value = user.Notes;
                        }

                        x++;
                    }

                    return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE) { FileDownloadName = string.Format("{0}.xlsx", REPORT_NAME) };

                }
            }

            return null;
        }


        public FileStreamResult SCEsNotSignedUp()
        {
            var loggedInUser = userManager.Fetch(User.Identity.Name);
            var users = reportService.FetchSCEsNotSignedUp();

            if (!users.HasContent()) return null;


            using (var xlPackage = new ExcelPackage())
            {
                const string reportName = "SCEs not signed up Report";
                var worksheet = xlPackage.Workbook.Worksheets.Add(reportName);

                var password = Guid.NewGuid().ToString("N");
                xlPackage.Encryption.IsEncrypted = true;
                xlPackage.Encryption.Password = password;

                SendPassword(password, reportName);

                worksheet.Cells[1, 1].Value = "This report contains personal details. Please ensure that it is kept confidential.";
                using (var r = worksheet.Cells["A1:X1"])
                {
                    r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(122, 255, 94, 0));
                    r.Style.Font.Bold = true;
                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                int x = 2;
                using (var r = worksheet.Cells["A2:X2"])
                {
                    r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                    r.Style.Font.Bold = true;
                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                var i = 0;
                var trustDict = formManager.FetchFormList("Trusts").Collection.ToDictionary(k => k.Value, v => v.Text);
                var gradeDict = formManager.FetchFormList("Grade").Collection.ToDictionary(k => k.Value, v => v.Text);
                // var siteDict = formManager.FetchFormList("Sites").Collection.ToDictionary(k => k.Value, v => v.Text);
                var groupDict = groupManager.FetchGroups().ToDictionary(k => k.ID, v => v.Name);

                worksheet.Cells[x, ++i].Value = "Username";
                worksheet.Cells[x, ++i].Value = "Title";
                worksheet.Cells[x, ++i].Value = "Forename";
                worksheet.Cells[x, ++i].Value = "Surname";
                worksheet.Cells[x, ++i].Value = "GMC Number";
                worksheet.Cells[x, ++i].Value = "Main Specialty";
                worksheet.Cells[x, ++i].Value = "Email";
                worksheet.Cells[x, ++i].Value = "SecretaryEmail";
                worksheet.Cells[x, ++i].Value = "OtherEmail";
                worksheet.Cells[x, ++i].Value = "Trust";
                worksheet.Cells[x, ++i].Value = "Grade";
                worksheet.Cells[x, ++i].Value = "Address 1";
                worksheet.Cells[x, ++i].Value = "Address 2";
                worksheet.Cells[x, ++i].Value = "City";
                worksheet.Cells[x, ++i].Value = "Region";
                worksheet.Cells[x, ++i].Value = "Postcode";
                worksheet.Cells[x, ++i].Value = "Phone Work";
                worksheet.Cells[x, ++i].Value = "Phone Mobile";
                worksheet.Cells[x, ++i].Value = "Phone Other";
                worksheet.Cells[x, ++i].Value = "Trained Date";
                worksheet.Cells[x, ++i].Value = "Trained Within 3 Years";
                worksheet.Cells[x, ++i].Value = "Enabled";

                if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                {
                    worksheet.Cells[x, ++i].Value = "Notes";
                }

                x++;

                foreach (var user in users)
                {
                    if (!user.Enabled)
                    {
                        using (var r = worksheet.Cells[string.Concat("A", x, ":X", x)])
                        {
                            r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 203, 203));
                        }
                    }

                    i = 0;
                    worksheet.Cells[x, ++i].Value = user.Username;
                    worksheet.Cells[x, ++i].Value = user.Title;
                    worksheet.Cells[x, ++i].Value = user.Forename;
                    worksheet.Cells[x, ++i].Value = user.Surname;
                    worksheet.Cells[x, ++i].Value = user.GMCNumber;
                    worksheet.Cells[x, ++i].Value = groupDict[user.MainSpecialty];
                    worksheet.Cells[x, ++i].Value = user.Email;
                    worksheet.Cells[x, ++i].Value = user.SecretaryEmail;
                    worksheet.Cells[x, ++i].Value = user.OtherEmail;
                    worksheet.Cells[x, ++i].Value = trustDict[user.Trust];
                    worksheet.Cells[x, ++i].Value = gradeDict[user.Grade];
                    worksheet.Cells[x, ++i].Value = user.Address1;
                    worksheet.Cells[x, ++i].Value = user.Address2;
                    worksheet.Cells[x, ++i].Value = user.City;
                    worksheet.Cells[x, ++i].Value = user.Region;
                    worksheet.Cells[x, ++i].Value = user.Postcode;
                    worksheet.Cells[x, ++i].Value = user.PhoneWork;
                    worksheet.Cells[x, ++i].Value = user.PhoneMobile;
                    worksheet.Cells[x, ++i].Value = user.PhoneOther;
                    
                    worksheet.Cells[x, ++i].Value = user.DateTrained.ToString("D");
                    worksheet.Cells[x, ++i].Value = user.Trained ? "Yes" : "No";
                    worksheet.Cells[x, ++i].Value = user.Enabled ? "Yes" : "No";

                    if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                    {
                        worksheet.Cells[x, ++i].Value = user.Notes;
                    }

                    x++;
                }

                return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE) { FileDownloadName = string.Format("{0}.xlsx", reportName) };

            }
        }

        public FileStreamResult ObserversNotSignedUp()
        {
            var loggedInUser = userManager.Fetch(User.Identity.Name);
            var users = reportService.FetchInterviewersNotSignedUp();

            if (!users.HasContent()) return null;


            using (var xlPackage = new ExcelPackage())
            {
                const string REPORT_NAME = "Observers not signed up Report";
                var worksheet = xlPackage.Workbook.Worksheets.Add(REPORT_NAME);

                int x = 1;
                using (var r = worksheet.Cells["A1:S1"])
                {
                    r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                    r.Style.Font.Bold = true;
                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                var i = 0;

                worksheet.Cells[x, ++i].Value = "Username";
                worksheet.Cells[x, ++i].Value = "Title";
                worksheet.Cells[x, ++i].Value = "Forename";
                worksheet.Cells[x, ++i].Value = "Surname";
                worksheet.Cells[x, ++i].Value = "Email";
                worksheet.Cells[x, ++i].Value = "SecretaryEmail";
                worksheet.Cells[x, ++i].Value = "OtherEmail";
                worksheet.Cells[x, ++i].Value = "PhoneWork";
                worksheet.Cells[x, ++i].Value = "PhoneMobile";
                worksheet.Cells[x, ++i].Value = "PhoneOther";
                worksheet.Cells[x, ++i].Value = "Trained";
                worksheet.Cells[x, ++i].Value = "Enabled";

                if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                {
                    worksheet.Cells[x, ++i].Value = "Notes";
                }

                x++;

                foreach (var user in users)
                {
                    if (!user.Enabled)
                    {
                        using (var r = worksheet.Cells[string.Concat("A", x, ":S", x)])
                        {
                            r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 203, 203));
                        }
                    }

                    i = 0;
                    worksheet.Cells[x, ++i].Value = user.Username;
                    worksheet.Cells[x, ++i].Value = user.Title;
                    worksheet.Cells[x, ++i].Value = user.Forename;
                    worksheet.Cells[x, ++i].Value = user.Surname;
                    worksheet.Cells[x, ++i].Value = user.Email;
                    worksheet.Cells[x, ++i].Value = user.SecretaryEmail;
                    worksheet.Cells[x, ++i].Value = user.OtherEmail;
                    worksheet.Cells[x, ++i].Value = user.PhoneWork;
                    worksheet.Cells[x, ++i].Value = user.PhoneMobile;
                    worksheet.Cells[x, ++i].Value = user.PhoneOther;
                    worksheet.Cells[x, 13].Value = user.Trained ? "Yes" : "No";
                    worksheet.Cells[x, ++i].Value = user.Enabled ? "Yes" : "No";

                    if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                    {
                        worksheet.Cells[x, 15].Value = user.Notes;
                    }

                    x++;
                }

                return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE) { FileDownloadName = string.Format("{0}.xlsx", REPORT_NAME) };

            }
        }

        public FileStreamResult InterviewersAndObserversNotSignedUp()
        {
            var loggedInUser = userManager.Fetch(User.Identity.Name);
            var users = reportService.FetchInterviewersAndObserversNotSignedUp();

            if (!users.HasContent()) return null;


            using (var xlPackage = new ExcelPackage())
            {
                const string REPORT_NAME = "Interviwers and Observers not signed up Report";
                var worksheet = xlPackage.Workbook.Worksheets.Add(REPORT_NAME);

                int x = 1;
                using (var r = worksheet.Cells["A1:S1"])
                {
                    r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                    r.Style.Font.Bold = true;
                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                var i = 0;

                worksheet.Cells[x, ++i].Value = "Username";
                worksheet.Cells[x, ++i].Value = "Groups";
                worksheet.Cells[x, ++i].Value = "Title";
                worksheet.Cells[x, ++i].Value = "Forename";
                worksheet.Cells[x, ++i].Value = "Surname";
                worksheet.Cells[x, ++i].Value = "Email";
                worksheet.Cells[x, ++i].Value = "SecretaryEmail";
                worksheet.Cells[x, ++i].Value = "OtherEmail";
                worksheet.Cells[x, ++i].Value = "PhoneWork";
                worksheet.Cells[x, ++i].Value = "PhoneMobile";
                worksheet.Cells[x, ++i].Value = "PhoneOther";
                worksheet.Cells[x, ++i].Value = "Trained";
                worksheet.Cells[x, ++i].Value = "Enabled";

                if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                {
                    worksheet.Cells[x, ++i].Value = "Notes";
                }

                x++;

                foreach (var user in users)
                {
                    if (!user.Enabled)
                    {
                        using (var r = worksheet.Cells[string.Concat("A", x, ":S", x)])
                        {
                            r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 203, 203));
                        }
                    }

                    i = 0;
                    worksheet.Cells[x, ++i].Value = user.Username;
                    worksheet.Cells[x, ++i].Value = user.Groups;
                    worksheet.Cells[x, ++i].Value = user.Title;
                    worksheet.Cells[x, ++i].Value = user.Forename;
                    worksheet.Cells[x, ++i].Value = user.Surname;
                    worksheet.Cells[x, ++i].Value = user.Email;
                    worksheet.Cells[x, ++i].Value = user.SecretaryEmail;
                    worksheet.Cells[x, ++i].Value = user.OtherEmail;
                    worksheet.Cells[x, ++i].Value = user.PhoneWork;
                    worksheet.Cells[x, ++i].Value = user.PhoneMobile;
                    worksheet.Cells[x, ++i].Value = user.PhoneOther;
                    worksheet.Cells[x, 13].Value = user.Trained ? "Yes" : "No";
                    worksheet.Cells[x, ++i].Value = user.Enabled ? "Yes" : "No";

                    if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                    {
                        worksheet.Cells[x, 15].Value = user.Notes;
                    }

                    x++;
                }

                return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE) { FileDownloadName = string.Format("{0}.xlsx", REPORT_NAME) };

            }
        }

        public FileStreamResult NotSignedUp()
        {
            var loggedInUser = userManager.Fetch(User.Identity.Name);
            var users = reportService.FetchUsersNotSignedUp();
            var groupsDict = groupManager.FetchGroups().ToDictionary(k => k.ID, v => v.Name);
            if (users.HasContent())
            {
                using (var xlPackage = new ExcelPackage())
                {
                    const string REPORT_NAME = "Inactive SCE Report";
                    var worksheet = xlPackage.Workbook.Worksheets.Add(REPORT_NAME);

                    int x = 1;
                    using (var r = worksheet.Cells["A1:S1"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //18
                    worksheet.Cells[x, 1].Value = "Username";
                    worksheet.Cells[x, 2].Value = "Title";
                    worksheet.Cells[x, 3].Value = "Forename";
                    worksheet.Cells[x, 4].Value = "Surname";
                    worksheet.Cells[x, 5].Value = "GMCNumber";
                    worksheet.Cells[x, 6].Value = "Specialty";
                    worksheet.Cells[x, 7].Value = "Email";
                    worksheet.Cells[x, 8].Value = "SecretaryEmail";
                    worksheet.Cells[x, 9].Value = "OtherEmail";
                    worksheet.Cells[x, 10].Value = "PhoneWork";
                    worksheet.Cells[x, 11].Value = "PhoneMobile";
                    worksheet.Cells[x, 12].Value = "PhoneOther";
                    worksheet.Cells[x, 13].Value = "Trained";
                    worksheet.Cells[x, 14].Value = "Enabled";

                    if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                    {
                        worksheet.Cells[x, 15].Value = "Notes";
                    }

                    x++;

                    foreach (var user in users)
                    {
                        if (!user.Enabled)
                        {
                            using (var r = worksheet.Cells[string.Concat("A", x, ":S", x)])
                            {
                                r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 203, 203));
                            }
                        }

                        var specialty = "Unknown";
                        groupsDict.TryGetValue(user.MainSpecialty, out specialty);

                        worksheet.Cells[x, 1].Value = user.Username;
                        worksheet.Cells[x, 2].Value = user.Title;
                        worksheet.Cells[x, 3].Value = user.Forename;
                        worksheet.Cells[x, 4].Value = user.Surname;
                        worksheet.Cells[x, 5].Value = user.GMCNumber;
                        worksheet.Cells[x, 6].Value = specialty;
                        worksheet.Cells[x, 7].Value = user.Email;
                        worksheet.Cells[x, 8].Value = user.SecretaryEmail;
                        worksheet.Cells[x, 9].Value = user.OtherEmail;
                        worksheet.Cells[x, 10].Value = user.PhoneWork;
                        worksheet.Cells[x, 11].Value = user.PhoneMobile;
                        worksheet.Cells[x, 12].Value = user.PhoneOther;
                        worksheet.Cells[x, 13].Value = user.Trained ? "Yes" : "No";
                        worksheet.Cells[x, 14].Value = user.Enabled ? "Yes" : "No";

                        if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                        {
                            worksheet.Cells[x, 15].Value = user.Notes;
                        }

                        x++;
                    }

                    return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE) { FileDownloadName = string.Format("{0}.xlsx", REPORT_NAME) };

                }
            }

            return null;
        }


        private void SendPassword(string password, string reportName)
        {
            var user = userManager.Fetch(User.Identity.Name);

            var replacements = new Dictionary<string, string>(4)
                    {
                        {"#forename#", user.Forename},
                        {"#surname#", user.Surname},
                        {"#username#", user.Username},
                        {"#reportname#", reportName},
                        {"#reportgenerated#", DateTime.Now.ToLongDateString()},
                        {"#password#", password},
                        {"#sitename#", configurationService.SiteName()},
                        {"#siteurl#", configurationService.SiteURL()},
                    };

            if (emailService.SendMail("SendReportPassword", user.Email, replacements))
            {
                emailService.SendEmailLog("SendReportPassword", user.Username);
            }
        }



        public FileStreamResult FullSessionReport()
        {//willingToChange.Any(n => n.SignupID == SignupID && n.UserID == UserID);

            var loggedInUser = userManager.Fetch(User.Identity.Name);
            var signups = signupManager.FetchAll();

            if (signups != null)
            {
                var willingToChange = reportService.FetchWillingToChangeSignups().ToArray();
                using (var xlPackage = new ExcelPackage())
                {


                    const string reportName = "Full Session Report";
                    var worksheet = xlPackage.Workbook.Worksheets.Add(reportName);

                    var password = Guid.NewGuid().ToString("N");
                    xlPackage.Encryption.IsEncrypted = true;
                    xlPackage.Encryption.Password = password;

                    SendPassword(password, reportName);
                    worksheet.Cells[1, 1].Value = "This report contains personal details. Please ensure that it is kept confidential.";
                    using (var r = worksheet.Cells["A1:W1"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(122, 255, 94, 0));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    int x = 2;
                    using (var r = worksheet.Cells["A2:W2"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    worksheet.Cells[x, 1].Value = "Date";
                    worksheet.Cells[x, 2].Value = "Specialty";
                    worksheet.Cells[x, 3].Value = "Location";
                    worksheet.Cells[x, 4].Value = "Slot";
                    worksheet.Cells[x, 5].Value = "Place";
                    worksheet.Cells[x, 6].Value = "Not Happy To Move";
                    worksheet.Cells[x, 7].Value = "Date Signed Up";
                    worksheet.Cells[x, 8].Value = "Username";
                    worksheet.Cells[x, 9].Value = "Title";
                    worksheet.Cells[x, 10].Value = "Forename";
                    worksheet.Cells[x, 11].Value = "Surname";
                    worksheet.Cells[x, 12].Value = "GMCNumber";
                    worksheet.Cells[x, 13].Value = "Home Trust";
                    worksheet.Cells[x, 14].Value = "Grade";
                    worksheet.Cells[x, 15].Value = "Email";
                    worksheet.Cells[x, 16].Value = "SecretaryEmail";
                    worksheet.Cells[x, 17].Value = "OtherEmail";
                    worksheet.Cells[x, 18].Value = "PhoneWork";
                    worksheet.Cells[x, 19].Value = "PhoneMobile";
                    worksheet.Cells[x, 20].Value = "PhoneOther";
                    worksheet.Cells[x, 21].Value = "Trained Within 3 Years";
                    worksheet.Cells[x, 22].Value = "Date Trained";

                    if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                    {
                        worksheet.Cells[x, 23].Value = "Notes";
                    }

                    x++;
                
                    var userIDs = signups.Where(o => o.Slots != null).SelectMany(n => n.Slots.Where(m => m.ApplicantsSignedUp != null).SelectMany(m => m.ApplicantsSignedUp).Select(o => o.User.ID));

                    IDictionary<string, SCEReportItem> userDict;

                    if (!userIDs.HasContent())
                        userDict = new Dictionary<string, SCEReportItem>();
                    else
                        userDict = reportService.FetchSCEData(userIDs).ToDictionary(k => k.Username, v => v);

                    var trustDict = formManager.FetchFormList("Trusts").Collection.ToDictionary(k => k.Value, v => v.Text);
                    var gradeDict = formManager.FetchFormList("Grade").Collection.ToDictionary(k => k.Value, v => v.Text);
                    // var siteDict = formManager.FetchFormList("Sites").Collection.ToDictionary(k => k.Value, v => v.Text);
               //     var groupDict = groupManager.FetchGroups().ToDictionary(k => k.ID, v => v.Name);


                    foreach (var signup in signups)
                    {
                        
                        foreach (var slot in signup.Slots.OrderBy(n => n.Time))
                        {
                            var users = slot.ApplicantsSignedUp.HasContent() ? slot.ApplicantsSignedUp.OrderBy(n => n.SignupDate).ToArray() : new UserSignup[] { };
                            var slotCount = 0;
                            for (int i = 1; i <= slot.TotalPlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces; i++)
                            {
                                string slotType = "Error";

                                if (slotCount + 1 <= slot.TotalPlacesAvailable) slotType = "Main";
                                if (slotCount + 1 > slot.TotalPlacesAvailable && slotCount + 1 <= slot.TotalPlacesAvailable + slot.ReservePlaces) slotType = "Reserve";
                                if (slotCount + 1 > slot.TotalPlacesAvailable + slot.ReservePlaces && slotCount + 1 <= slot.TotalPlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces) slotType = "Interested";

                                if (users.Length > slotCount)
                                {
                                    var sceData = userDict[users[slotCount].User.Username];

                                    worksheet.Cells[x, 1].Value = signup.Date.ToString("D");
                                    worksheet.Cells[x, 2].Value = signup.Group.Name;
                                    worksheet.Cells[x, 3].Value = signup.Description;
                                    worksheet.Cells[x, 4].Value = slot.Description;
                                    worksheet.Cells[x, 5].Value = slotType;
                                    worksheet.Cells[x, 6].Value = willingToChange.Any(n => n.SignupID == signup.ID && n.UserID == sceData.UserID) ? "Yes" : "No";
                                    worksheet.Cells[x, 7].Value = users[slotCount].SignupDate.ToString("f");
                                    worksheet.Cells[x, 8].Value = sceData.Username;
                                    worksheet.Cells[x, 9].Value = sceData.Title;
                                    worksheet.Cells[x, 10].Value = sceData.Forename;
                                    worksheet.Cells[x, 11].Value = sceData.Surname;
                                    worksheet.Cells[x, 12].Value = sceData.GMCNumber;
                                    worksheet.Cells[x, 13].Value = trustDict[sceData.Trust];
                                    worksheet.Cells[x, 14].Value = gradeDict[sceData.Grade];

                                    worksheet.Cells[x, 15].Value = sceData.Email;
                                    worksheet.Cells[x, 16].Value = sceData.SecretaryEmail;
                                    worksheet.Cells[x, 17].Value = sceData.OtherEmail;
                                    worksheet.Cells[x, 18].Value = sceData.PhoneWork;
                                    worksheet.Cells[x, 19].Value = sceData.PhoneMobile;
                                    worksheet.Cells[x, 20].Value = sceData.PhoneOther;
                                    worksheet.Cells[x, 21].Value = sceData.Trained ? "Yes" : "No";
                                    worksheet.Cells[x, 22].Value = sceData.DateTrained.ToString("D");

                                    if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                                    {
                                        worksheet.Cells[x, 23].Value = sceData.Notes;
                                    }

                                }
                                else
                                {
                                    worksheet.Cells[x, 1].Value = signup.Date.ToString("D");
                                    worksheet.Cells[x, 2].Value = signup.Group.Name;
                                    worksheet.Cells[x, 3].Value = signup.Description;
                                    worksheet.Cells[x, 4].Value = slot.Description;
                                    worksheet.Cells[x, 5].Value = slotType;
                                }
                                slotCount++;
                                x++;
                            }
                        }

                    }

                    worksheet.Cells.AutoFitColumns();

                    return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE)
                    {
                        FileDownloadName = string.Format("{0}.xlsx", reportName)
                    };
                }
            }

            throw new HttpException(404, "File not found.");
        }


        public FileStreamResult InterviewReport(int id)
        {
            var loggedInUser = userManager.Fetch(User.Identity.Name);
            var signup = signupManager.Fetch(id);
            if (signup != null)
            {
                var userIDs = signup.Slots.Where(o => o.ApplicantsSignedUp != null).SelectMany(n => n.ApplicantsSignedUp.Select(m => m.User.ID));

                IDictionary<string, SCEReportItem> userDict;

                if (!userIDs.HasContent())
                    userDict = new Dictionary<string, SCEReportItem>();
                else
                    userDict = reportService.FetchSCEData(userIDs).ToDictionary(k => k.Username, v => v);

                using (var xlPackage = new ExcelPackage())
                {
                    string REPORT_NAME = string.Format("{0} {1} Report", signup.Group.Name, signup.Date.ToString("F"));
                    var worksheet = xlPackage.Workbook.Worksheets.Add(REPORT_NAME);
                  
                    int x = 1;
                    using (var r = worksheet.Cells["A1:S1"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    var j = 0;

                    worksheet.Cells[x, ++j].Value = "Slot";
                    worksheet.Cells[x, ++j].Value = "Place";
                    worksheet.Cells[x, ++j].Value = "Date Signed Up";
                    worksheet.Cells[x, ++j].Value = "Username";
                    worksheet.Cells[x, ++j].Value = "Title";
                    worksheet.Cells[x, ++j].Value = "Forename";
                    worksheet.Cells[x, ++j].Value = "Surname";
                    worksheet.Cells[x, ++j].Value = "Email";
                    worksheet.Cells[x, ++j].Value = "SecretaryEmail";
                    worksheet.Cells[x, ++j].Value = "OtherEmail";
                    worksheet.Cells[x, ++j].Value = "PhoneWork";
                    worksheet.Cells[x, ++j].Value = "PhoneMobile";
                    worksheet.Cells[x, ++j].Value = "PhoneOther";
                    worksheet.Cells[x, ++j].Value = "Trained";
                    worksheet.Cells[x, ++j].Value = "Enabled";

                    if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                    {
                        worksheet.Cells[x, 18].Value = "Notes";
                    }

                    x++;

                    foreach (var slot in signup.Slots.OrderBy(n => n.Time))
                    {
                        var users = slot.ApplicantsSignedUp.HasContent() ? slot.ApplicantsSignedUp.OrderBy(n => n.SignupDate).ToArray() : new UserSignup[] { };
                        var slotCount = 0;
                        for (int i = 1; i <= slot.TotalPlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces; i++)
                        {
                            string slotType = "Error";

                            if (slotCount + 1 <= slot.TotalPlacesAvailable) slotType = "Main";
                            if (slotCount + 1 > slot.TotalPlacesAvailable && slotCount + 1 <= slot.TotalPlacesAvailable + slot.ReservePlaces) slotType = "Reserve";
                            if (slotCount + 1 > slot.TotalPlacesAvailable + slot.ReservePlaces && slotCount + 1 <= slot.TotalPlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces) slotType = "Interested";

                            if (users.Length > slotCount)
                            {
                               // PopulateWorksheetForSingleReport(ref worksheet, x, slot, userDict[users[slotCount].User.Username], slotType,
                                 //   users[slotCount]);
                                j = 0;
                                var sceData = userDict[users[slotCount].User.Username];
                                worksheet.Cells[x, ++j].Value = slot.Description;
                                worksheet.Cells[x, ++j].Value = slotType;
                                worksheet.Cells[x, ++j].Value = users[slotCount].SignupDate.ToString("f");
                                worksheet.Cells[x, ++j].Value = sceData.Username;
                                worksheet.Cells[x, ++j].Value = sceData.Title;
                                worksheet.Cells[x, ++j].Value = sceData.Forename;
                                worksheet.Cells[x, ++j].Value = sceData.Surname;
                                worksheet.Cells[x, ++j].Value = sceData.Email;
                                worksheet.Cells[x, ++j].Value = sceData.SecretaryEmail;
                                worksheet.Cells[x, ++j].Value = sceData.OtherEmail;
                                worksheet.Cells[x, ++j].Value = sceData.PhoneWork;
                                worksheet.Cells[x, ++j].Value = sceData.PhoneMobile;
                                worksheet.Cells[x, ++j].Value = sceData.PhoneOther;
                                worksheet.Cells[x, ++j].Value = sceData.Trained ? "Yes" : "No";
                                worksheet.Cells[x, ++j].Value = sceData.Enabled ? "Yes" : "No";

                                if (loggedInUser != null && loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin"))
                                {
                                    worksheet.Cells[x, 18].Value = sceData.Notes;
                                }

                            }
                            else
                            {
                                worksheet.Cells[x, 1].Value = slot.Description;
                                worksheet.Cells[x, 2].Value = slotType;
                            }
                            slotCount++;
                            x++;
                        }
                    }

                    worksheet.Cells.AutoFitColumns();

                    return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE)
                        {
                            FileDownloadName = string.Format("{0}.xlsx", REPORT_NAME)
                        };
                }
            }

            throw new HttpException(404, "File not found.");
        }
    }
}
