using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DDay.iCal;
using eMotive.Managers.Interfaces;
using eMotive.SCE.Common;
using eMotive.SCE.Common.ActionFilters;
using eMotive.Models.Objects.Signups;
using eMotive.Models.Objects.SignupsMod;
using eMotive.Models.Objects.StatusPages;
using eMotive.Models.Objects.Uploads;
using eMotive.Models.Objects.Users;
using eMotive.SCE.Common;
using eMotive.SCE.Infrastructure;
using eMotive.Search.Objects;
using eMotive.Services.Interfaces;
using Extensions;
using Newtonsoft.Json.Linq;
//using Ninject;
using OfficeOpenXml;
using ServiceStack.Mvc;
using Group = eMotive.Models.Objects.Signups.Group;
using Signup = eMotive.Models.Objects.Signups.Signup;

namespace eMotive.SCE.Areas.Admin.Controllers
{
    public class SignupsController : ServiceStackController
    {
        private readonly ISessionManager signupManager;
        private readonly IGroupManager groupManager;
        private readonly IDocumentManagerService documentManager;
        private readonly IUserManager userManager;
        private readonly IFormManager formManager;

        private readonly Dictionary<string, string> searchFilter;

        public SignupsController(ISessionManager _signupManager, IGroupManager _groupManager, IDocumentManagerService _documentManager, IUserManager _userManager, IFormManager _formManager)
        {
            signupManager = _signupManager;
            groupManager = _groupManager;
            documentManager = _documentManager;
            userManager = _userManager;
            formManager = _formManager;

            searchFilter = new Dictionary<string, string> { { "Type", "Signup" } };
        }

       // [Inject]
        public INotificationService NotificationService { get; set; }

      //  [Inject]
        public IeMotiveConfigurationService ConfigurationService { get; set; }
        
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult Index(SignupSearch signupSearch)
        {
           /* var signupAdminView = new AdminSignupView
                {
                    Signups = signupManager.FetchAllM()
                };*/

            if (!string.IsNullOrEmpty(signupSearch.SelectedGroupFilter) && signupSearch.SelectedGroupFilter != "0")
                searchFilter.Add("GroupID", signupSearch.SelectedGroupFilter);

            if (!string.IsNullOrEmpty(signupSearch.SelectedLocationFilter) && signupSearch.SelectedLocationFilter != "0")
                searchFilter.Add("SiteID", signupSearch.SelectedLocationFilter);

            signupSearch.Filter = searchFilter;
        //    signupSearch.PageSize = 20;
            var searchItem = signupManager.DoSearch(signupSearch);

            if (searchItem.Items.HasContent())
            {
                signupSearch.Page = searchItem.CurrentPage;
                signupSearch.NumberOfResults = searchItem.NumberOfResults;
                signupSearch.Signups = signupManager.FetchRecordsFromSearch(searchItem); 
            }
            else
            {
              //  signupSearch.Signups = signupManager.FetchAllM();
                signupSearch = new SignupSearch();
            }

            var groups = groupManager.FetchGroups();

            if (groups.HasContent())
            {
                var groupFilter = new Collection<KeyValuePair<string, string>> { new KeyValuePair<string, string>("0", string.Empty) };

                groupFilter.AddRange(groups.Select(n => new KeyValuePair<string, string>(n.ID.ToString(CultureInfo.InvariantCulture), n.Name)));
                signupSearch.GroupFilter = groupFilter;
            }

            var sites = formManager.FetchFormList("Sites");

            if (sites != null && sites.Collection.HasContent())
            {
                var siteFilter = new Collection<KeyValuePair<string, string>> { new KeyValuePair<string, string>("0", string.Empty) };

                siteFilter.AddRange(sites.Collection.Select(n => new KeyValuePair<string, string>(n.Value, n.Text)));
                signupSearch.LocationFilter = siteFilter;
            }

            return View(signupSearch);
        }
        /*
        public ActionResult Details(int id)
        {
            var signup = signupManager.FetchM(id);

            return View(signup);
        }*/

        public ActionResult Edit(int id)
        {
            var signup = signupManager.Fetch(id);

            return View(signup);
        }



        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult SignupDetails(int? id)
        {
            var signup = signupManager.FetchM(id.Value);

            return View(signup);
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

        [HttpGet]
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult BoxiUpload()
        {
            var lastUpload = documentManager.FetchLastUploaded(UploadReference.BOXI);

            return View(lastUpload);
        }


        public ActionResult UserSessions(string username)
        {
            var user = userManager.Fetch(username);
            var signups = signupManager.FetchSignupInformation(username);

            var sessionView = new ThirdPartySignupSessions {Signup = signups, User = user};

            return View(sessionView);
        }

        public ActionResult UserSlots(string username, int? id)
        {
            var user = userManager.Fetch(username);
            var slots = signupManager.FetchSlotInformation(id.HasValue ? id.Value : -1, username);

            var slotView = new ThirdPartySignupSlots {Slots = slots, User = user};

            return View(slotView);
        }


        [HttpGet]
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult A100ApplicantUpload()
        {
            var applicantUploadView = new ApplicantUploadView
            {
                Groups = null,
                LastUploadedDocument = documentManager.FetchLastUploaded(UploadReference.A100Applicants)
            };


            return View(applicantUploadView);
        }



        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult Groups()
        {
            var groups = groupManager.FetchGroups();

            return View(groups);
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        [HttpGet]
        public ActionResult GroupEdit(int? id)
        {
            var group = groupManager.FetchGroup(id.HasValue ? id.Value : -1);

            return View(group);
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        [HttpPost]
        public ActionResult GroupEdit(Group group)
        {
            if (ModelState.IsValid)
            {
                if (groupManager.UpdateGroup(group))
                {
                    var successView = new SuccessView
                    {
                        Message = "The Group was successfully updated.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = string.Format("Edit {0}", group.Name), URL = @Url.Action("GroupEdit", "Signups", new {id = group.ID})},
                                new SuccessView.Link {Text = "Return to Group Home", URL = @Url.Action("Groups")}
                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "Admin" });
                }

                var errors = NotificationService.FetchIssues();
                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }

            return View(group);
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult EditSignup(int id)
        {
            return View();
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult OpenClose()
        {
            var signups = signupManager.FetchAllBrief();

            var signupDict = signups.GroupBy(
        m =>
        string.Format("{1} {0}", m.Date.Year,
                      System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(
                          m.Date.Month))).ToDictionary(k => k.Key, g => g.OrderBy(n => n.Date).ToList());

            return View(signupDict);
        }


    //    [AjaxOnly]
        public JsonResult FetchSignup(int idSignup)
        {
            var signup = signupManager.Fetch(idSignup);

            var issues = NotificationService.FetchIssues();

          //  return new CustomJsonResult
          //  {
            return new JsonResult {Data =  new {success = signup != null, message = issues, result = signup}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
            // };

        }

      //  [AjaxOnly]
        public CustomJsonResult FetchAllGroups()
        {
            //  var signup = signupManager.sa
            var groups = signupManager.FetchAllGroups();

            var issues = NotificationService.FetchIssues();

            return new CustomJsonResult
            {
                Data = new { success = false, message = issues, result = groups }
            };

        }

        [AjaxOnly]
        public CustomJsonResult SaveSignup(Signup signup)
        {
          //  var signup = signupManager.sa

            var issues = NotificationService.FetchIssues();

            return new CustomJsonResult
            {
                Data = new { success = false, message = issues }
            };

        }


        [AjaxOnly]
        public CustomJsonResult SignupToSlot(int idSignup, int idSlot, string username)
        {
            if (signupManager.SignupToSlot(idSignup, idSlot, username))
            {
                var signup = signupManager.Fetch(idSignup);
                var slot = signup.Slots.Single(n => n.ID == idSlot);

                ApplicantSignupPush(signup.ID, signup.Slots.Sum(n => n.TotalPlacesAvailable),
                    signup.Slots.Sum(n => n.ApplicantsSignedUp.HasContent() ? n.TotalPlacesAvailable - n.ApplicantsSignedUp.Count() : n.TotalPlacesAvailable));

                ApplicantSlotPush(slot.ID, slot.TotalPlacesAvailable,
                    slot.ApplicantsSignedUp.HasContent() ? slot.TotalPlacesAvailable - slot.ApplicantsSignedUp.Count() : slot.TotalPlacesAvailable);

                return new CustomJsonResult
                {
                    Data = new { success = true, message = "successfully signed up." }
                };
            }

            var issues = NotificationService.FetchIssues();


            return new CustomJsonResult
            {
                Data = new { success = false, message = issues }
            };

        }

        [AjaxOnly]
        public CustomJsonResult CancelSignupToSlot(int idSignup, int idSlot, string username)
        {
            if (signupManager.CancelSignupToSlot(idSignup, idSlot, username))
            {
                var signup = signupManager.Fetch(idSignup);
                var slot = signup.Slots.Single(n => n.ID == idSlot);

                ApplicantSignupPush(signup.ID, signup.Slots.Sum(n => n.TotalPlacesAvailable),
                    signup.Slots.Sum(n => n.ApplicantsSignedUp.HasContent() ? n.TotalPlacesAvailable - n.ApplicantsSignedUp.Count() : n.TotalPlacesAvailable));

                ApplicantSlotPush(slot.ID, slot.TotalPlacesAvailable,
                    slot.ApplicantsSignedUp.HasContent() ? slot.TotalPlacesAvailable - slot.ApplicantsSignedUp.Count() : slot.TotalPlacesAvailable);

                return new CustomJsonResult
                {
                    Data = new { success = true, message = "successfully cancelled appointment." }
                };
            }

            return new CustomJsonResult
            {
                Data = new { success = false, message = "An error occurred. The signup could not be cancelled." }
            };
        }

        private void ApplicantSignupPush(int _signupID, int _totalPlaces, int _remainingPlaces)
        {
         /*   var pusher = new PusherServer.Pusher(ConfigurationService.PusherID(), ConfigurationService.PusherKey(), ConfigurationService.PusherSecret());

            var result = pusher.Trigger("SignupSelection", "PlacesChanged",
                                                    new
                                                    {
                                                        SignUpId = _signupID,
                                                        TotalPlaces = _totalPlaces,
                                                        PlacesAvailable = _remainingPlaces
                                                    });*/
        }

        private void ApplicantSlotPush(int _slotID, int _totalPlaces, int _remainingPlaces)
        {
            /*var pusher = new PusherServer.Pusher(ConfigurationService.PusherID(), ConfigurationService.PusherKey(), ConfigurationService.PusherSecret());

            var result = pusher.Trigger("SignupSelection", "SlotChanged",
                                                    new
                                                    {
                                                        SlotId = _slotID,
                                                        TotalPlaces = _totalPlaces,
                                                        PlacesAvailable = _remainingPlaces
                                                    });*/
        }

    }
}
