using System.Collections.Generic;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects;
using eMotive.Models.Objects.Forms;
using eMotive.Services.Interfaces;
using Extensions;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;


namespace eMotive.Api
{
    [Route("/Forms/FormList/New", "GET")]
    public class NewFormList
    {
    }

    [Route("/Forms/New", "GET")]
    public class NewForm
    {
    }

    [Route("/Forms/FormList")]
    [Route("/Forms/FormList/{Ids}")]
    public class GetFormLists
    {
        public int[] Ids { get; set; }
    }

    [Route("/Forms")]
    [Route("/Forms/{Ids}")]
    public class GetForms
    {
        public int[] Ids { get; set; }
    }

    [Route("/Forms/Types")]
    public class GetFields
    { 
    }

    [Route("/Forms/FormList", "DELETE")]
    public class DeleteFormList
    {
        public int Id { get; set; }
    }

    [Route("/Forms", "POST")]
    [Route("/Forms", "PUT")]
    public class SaveForm
    {
        public Form form { get; set; }
    }

    [Route("/Forms/FormList", "POST")]
    [Route("/Forms/FormList", "PUT")]
    public class SaveFormList
    {
        public FormList formList { get; set; }
    }

    public class FormService : Service
    {
        private readonly IFormManager _formManager;
        private readonly INotificationService _notificationService;

        public FormService(IFormManager formManager, INotificationService notificationService)
        {
            _formManager = formManager;
            _notificationService = notificationService;
        }

        public INotificationService NotificationService { get; set; }

        public object Get(NewFormList request)
        {
            return new ServiceResult<FormList>
            {
                Success = true,
                Result = _formManager.NewFormList(),
                Errors = new string[] { }
            };
        }

        public object Get(NewForm request)
        {//todo: should check for errors here? THey will possibly never occur, but there might be fringe cases
            return new ServiceResult<Form>
            {
                Success = true,
                Result = _formManager.NewForm(),
                Errors = new string[] { }
            };
        }

        public object Get(GetFields request)
        {//todo: should check for errors here? THey will possibly never occur, but there might be fringe cases
            return new ServiceResult<IEnumerable<FormType>>
            {
                Success = true,
                Result = _formManager.FetchForTypes(),
                Errors = new string[] { }
            };
        }

        public object Get(GetFormLists request)
        {
            var result = request.Ids.IsEmpty()
                ? _formManager.FetchFormList()
                : _formManager.FetchFormList(request.Ids);

            var issues = _notificationService.FetchIssues(); //TODO: how to deal with errors when going directly into the api?? perhaps organise messages better?
            var success = result.HasContent() || !issues.HasContent();

            

            return new ServiceResult<IEnumerable<FormList>>
            {
                Success = success,
                Result = result,
                Errors = issues
            };

        }

        public object Get(GetForms request)
        {
            var result = request.Ids.IsEmpty()
                ? _formManager.FetchForm()
                : _formManager.FetchForm(request.Ids);

            var issues = _notificationService.FetchIssues(); //TODO: how to deal with errors when going directly into the api?? perhaps organise messages better?
            var success = result.HasContent() || !issues.HasContent();



            return new ServiceResult<IEnumerable<Form>>
            {
                Success = success,
                Result = result,
                Errors = issues
            };

        }

        public object Post(SaveForm request)
        {
            int id;
            var success = _formManager.CreateForm(request.form, out id);

            if (success)
                request.form.ID = id;

            var issues = _notificationService.FetchIssues();

            return new ServiceResult<Form>
            {
                Success = success,
                Result = request.form,
                Errors = issues
            };
        }

        public object Put(SaveForm request)
        {

            var success = _formManager.UpdateForm(request.form);

            var issues = _notificationService.FetchIssues();

            return new ServiceResult<Form>
            {
                Success = success,
                Result = request.form,
                Errors = issues
            };
        }
        
        public object Post(SaveFormList request)
        {
               int id;
               var success = _formManager.CreateFormList(request.formList, out id);

                if (success)
                    request.formList.ID = id;

                var issues = _notificationService.FetchIssues();

                return new ServiceResult<FormList>
                {
                    Success = success,
                    Result = request.formList,
                    Errors = issues
                };
        }

        public object Put(SaveFormList request)
        {

            var success = _formManager.UpdateFormList(request.formList);

            var issues = _notificationService.FetchIssues();

            return new ServiceResult<FormList>
            {
                Success = success,
                Result = request.formList,
                Errors = issues
            };
        }

    }
}
