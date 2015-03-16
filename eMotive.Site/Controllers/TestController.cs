using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.SCE.Common.Providers;
using eMotive.Models.Objects.Certificates;
using eMotive.Models.Objects.Uploads;
using eMotive.Models.Objects.Users;
using eMotive.SCE.Infrastructure;
using eMotive.Services.Interfaces;
using Extensions;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using Rotativa;
using ServiceStack.Mvc;

namespace eMotive.SCE.Controllers
{
    public class TestController : ServiceStackController
    {
        private readonly IUserManager userManager;
        private readonly IRoleManager rolemanager;
        private readonly IAccountManager accountManager;
        private readonly IGroupManager groupManager;
        private readonly IDocumentManagerService documentManager;
        private readonly INotificationService notificationManager;
        
        //
        // GET: /Test/
        public TestController(IUserManager _userManager, IRoleManager _rolemanager, IAccountManager _accountManager, IDocumentManagerService _documentManager, IGroupManager _groupManager, INotificationService _notificationManager)
        {
            userManager = _userManager;
            documentManager = _documentManager;
            groupManager = _groupManager;
            notificationManager = _notificationManager;
            rolemanager = _rolemanager;
            accountManager = _accountManager;
        }

        public ActionResult Index()
        {
            return View(userManager.New());
        }

        [HttpPost]
        public ActionResult Index(User user)
        {
            int id;
            if (!userManager.Create(user, out id))
            {
                var issues = notificationManager.FetchIssues();

                if(issues.HasContent())
                foreach (var message in issues)
                {
                    Response.Write(message + "<br/>");
                }
            }

            return View(user);
        }

        public ActionResult Index2()
        {
            return View(/*userManager.DoSearch(new BasicSearch {PageSize = 90})*/);
        }

        public ActionResult ModalTest()
        {
            return View();
        }

        public ActionResult UploadTest()
        {
            return View();
        }

        public ActionResult SignalRTest()
        {
            return View();
        }

        public class DictTester
        {
            public DictTester()
            {
            //    SomeStuff1 = new List<KeyValuePair<string, string>>();
                SomeStuff = new List<Field>();
                DropDowns = new Dictionary<string, List<KeyValuePair<string, string>>>();
            }

            public List<KeyValuePair<string, string>> SomeStuff1 { get; set; }
            public List<Field> SomeStuff { get; set; }

            public Dictionary<string, List<KeyValuePair<string, string>>> DropDowns { get; set; }

        }

        public enum FieldType { Text, DropDownList }

        public class Field
        {
            public FieldType Type { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        [HttpGet]
        public ActionResult DictTest()
        {
            var test = new DictTester();

            test.SomeStuff.Add(new Field { Type = FieldType.Text, Name = "Forename", Value ="Chris" });
            test.SomeStuff.Add(new Field { Type = FieldType.Text, Name = "Surname", Value = "Withers" });
            test.SomeStuff.Add(new Field { Type = FieldType.DropDownList, Name = "1", Value = "3" });
            test.SomeStuff.Add(new Field { Type = FieldType.DropDownList, Name = "2", Value = "2" });

            test.DropDowns.Add("1", new List<KeyValuePair<string, string>>());
            test.DropDowns["1"].Add(new KeyValuePair<string, string>("1", "Item 1"));
            test.DropDowns["1"].Add(new KeyValuePair<string, string>("2", "Item 2"));
            test.DropDowns["1"].Add(new KeyValuePair<string, string>("3", "Item 3"));
            test.DropDowns["1"].Add(new KeyValuePair<string, string>("4", "Item 4"));

            test.DropDowns.Add("2", new List<KeyValuePair<string, string>>());
            test.DropDowns["2"].Add(new KeyValuePair<string, string>("1", "Item 11"));
            test.DropDowns["2"].Add(new KeyValuePair<string, string>("2", "Item 22"));
            test.DropDowns["2"].Add(new KeyValuePair<string, string>("3", "Item 33"));
            test.DropDowns["2"].Add(new KeyValuePair<string, string>("4", "Item 44"));

            return View(test);
        }

        [HttpPost]
        public ActionResult DictTest(DictTester test)
        {
            test.DropDowns.Add("1", new List<KeyValuePair<string, string>>());
            test.DropDowns["1"].Add(new KeyValuePair<string, string>("1", "Item 1"));
            test.DropDowns["1"].Add(new KeyValuePair<string, string>("2", "Item 2"));
            test.DropDowns["1"].Add(new KeyValuePair<string, string>("3", "Item 3"));
            test.DropDowns["1"].Add(new KeyValuePair<string, string>("4", "Item 4"));

            test.DropDowns.Add("2", new List<KeyValuePair<string, string>>());
            test.DropDowns["2"].Add(new KeyValuePair<string, string>("1", "Item 11"));
            test.DropDowns["2"].Add(new KeyValuePair<string, string>("2", "Item 22"));
            test.DropDowns["2"].Add(new KeyValuePair<string, string>("3", "Item 33"));
            test.DropDowns["2"].Add(new KeyValuePair<string, string>("4", "Item 44"));

            return View(test);
        }



        public ActionResult BoxiUploadTest()
        {
            var applicantUploadView = new ApplicantUploadView
                {
                    Groups = groupManager.FetchGroups(),
                    LastUploadedDocument = documentManager.FetchLastUploaded(UploadReference.A100Applicants)
                };


            return View(applicantUploadView);
        }



        public class FineUploaderResult : ActionResult
        {
            public const string ResponseContentType = "text/plain";

            private readonly bool _success;
            private readonly string _error;
            private readonly bool? _preventRetry;
            private readonly JObject _otherData;

            public FineUploaderResult(bool success, object otherData = null, string error = null, bool? preventRetry = null)
            {
                _success = success;
                _error = error;
                _preventRetry = preventRetry;

                if (otherData != null)
                    _otherData = JObject.FromObject(otherData);
            }

            public override void ExecuteResult(ControllerContext context)
            {
                var response = context.HttpContext.Response;
                response.ContentType = ResponseContentType;

                response.Write(BuildResponse());
            }

            public string BuildResponse()
            {
                var response = _otherData ?? new JObject();
                response["success"] = _success;

                if (!string.IsNullOrWhiteSpace(_error))
                    response["error"] = _error;

                if (_preventRetry.HasValue)
                    response["preventRetry"] = _preventRetry.Value;

                return response.ToString();
            }
        }


        //
        // GET: /Test/Details/5
/*
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Test/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Test/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Test/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Test/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Test/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Test/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }*/

        public ActionResult SendSCEEmails()
        {
          /*  var users = userManager.FetchAll();
            var sces = users.Where(n => n.Roles.Any(r => r.Name == "SCE"));

            foreach (var p in sces)
            {
             //   Response.Write(sces.Count());
                
                accountManager.CreateNewAccountPassword(p);
            }*/
            return View();
        }



        public ActionResult PDF()
        {
            return new ActionAsPdf(
               "PDFView") { FileName = "Certificate.pdf" };
        }

        public ActionResult PDFView()
        {
            var certificate = new SCECertificate
            {
                Title = "Mr",
                Forename = "Christopher",
                Surname = "Withers",
                DateOfCourse = string.Format(new MyCustomDateProvider(), "{0}", DateTime.Now),
                Grade = "Test Grade",
                RCPNumber = "000001",
                Trust = "Some Trust",
                DateSigned = string.Format(new MyCustomDateProvider(), "{0}", DateTime.Now)
            };

            return View(certificate);
        }


        public ActionResult AddSCEs()
        {throw new Exception("No here");
            var newUsers = new Collection<UserWithRoles>();
            var roles = rolemanager.FetchAll();
         //   var groups = groupManager.FetchGroups();
            var sceRole = roles.First(n => n.Name == "SCE");
            var sces = userManager.FetchAllSceData();

            foreach (var sce in sces)
            {
                var newUser = new UserWithRoles();
                newUser.Roles = new[] {sceRole};
                newUser.Username = sce.Username;
                newUser.Forename = sce.Forename;
                newUser.Surname = sce.Surname;
                newUser.Email = sce.Email;
                newUser.Enabled = sce.Enabled;
                newUser.Archived = false;

                var usersGroups = GetSCEGroups(sce.MainSpecialty);
             //   newUsers.Add(newUser);

              //  userManager.Create(newUser, usersGroups);
            }

          //  throw new Exception(newUsers.Count() + " " +sces.Count());

            
            return View(newUsers);
        }

        private ICollection<int> GetSCEGroups(int _id)
        {
            var sceGroups = new Collection<int>();
            switch (_id)
            {
                case 1:
                    sceGroups.Add(1);
                    sceGroups.Add(11);
                    return sceGroups;
                case 2:
                    sceGroups.Add(2);
                    sceGroups.Add(1);
                    sceGroups.Add(11);
                    return sceGroups;
                case 3:
                    sceGroups.Add(3);
                    sceGroups.Add(11);
                    return sceGroups;
                case 4:
                    sceGroups.Add(4);
                    sceGroups.Add(11);
                    return sceGroups;
                case 5:
                    sceGroups.Add(5);
                    sceGroups.Add(11);
                    return sceGroups;
                case 6:
                    sceGroups.Add(6);
                    sceGroups.Add(11);
                    return sceGroups;
                case 7:
                    sceGroups.Add(7);
                    sceGroups.Add(1);
                    sceGroups.Add(11);
                    return sceGroups;
                case 8:
                    sceGroups.Add(8);
                    sceGroups.Add(11);
                    return sceGroups;
                case 9:
                    sceGroups.Add(9);
                    sceGroups.Add(11);
                    return sceGroups;
                case 10:
                    sceGroups.Add(10);
                    sceGroups.Add(11);
                    return sceGroups;
                default:
                    sceGroups.Add(11);
                    return sceGroups;

            }
        }
    }
}
