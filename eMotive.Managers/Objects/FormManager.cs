using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects;
using eMotive.Models.Objects.Forms;
using eMotive.Repository.Interfaces;
using eMotive.Services.Interfaces;
using Extensions;
using rForm = eMotive.Repository.Objects.Forms;

namespace eMotive.Managers.Objects
{
    public class FormManager : IFormManager
    {
        private readonly IFormRepository _formRepository;
         
        public FormManager(IFormRepository formRepository)
        {
            _formRepository = formRepository;
            AutoMapperManagerConfiguration.Configure();
        }

        public IMappingEngine Mapper { get; set; }
        public INotificationService NotificationService { get; set; }

        public Form NewForm()
        {
            return Mapper.Map<rForm.Form, Form>(_formRepository.NewForm());
        }

        public bool CreateForm(Form form, out int id)
        {
            id = -1;

            if (string.IsNullOrEmpty(form.Name))
            {
                    NotificationService.AddIssue("The form has no name.");
                    return false;
            }

            var formCheck = _formRepository.FetchForm(form.Name);

            if (formCheck != null)
            {
                if (String.Equals(form.Name, formCheck.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    NotificationService.AddIssue(string.Format("A form named '{0}' already exists.", form.Name));
                    return false;
                }
            }

            if (!form.Fields.HasContent())
            {
                NotificationService.AddIssue("The form has no fields.");
                return false;
            }

            var duplicatesText = form.Fields.GroupBy(n => n.Field).SelectMany(g => g.Skip(1));

            if (duplicatesText.HasContent())
            {
                NotificationService.AddIssue(
                    string.Format("Field item text must be unique. Duplications detected: '{0}'",
                        string.Join(",", duplicatesText.Select(n => n.Field))));
                return false;
            }


                if (form.Fields.Any(n => n.Type == FieldType.DropDownList && n.ListID == 0))
                {
                    NotificationService.AddIssue("Please select a list for fields specified as Drop Down Lists.");
                    return false;
                }



            form.Fields = form.Fields.GroupBy(x => x.Field).Select(y => y.First()); //todo: need to remove empty 

            if (_formRepository.CreateForm(Mapper.Map<Form, rForm.Form>(form), out id))
            {
                return true;
            }

            return false;
        }

        public bool UpdateForm(Form form)
        {
            var checkForm = _formRepository.FetchFormList(form.Name);

            if (string.IsNullOrEmpty(form.Name))
            {

                NotificationService.AddIssue("The form has no name.");
                return false;

            }

            if (checkForm != null)
            {
                if (String.Equals(form.Name, checkForm.Name, StringComparison.InvariantCultureIgnoreCase) && form.ID != checkForm.ID)
                {
                    NotificationService.AddIssue(string.Format("A form with the name '{0}' already exists.", form.Name));
                    return false;
                }
            }


            if (!form.Fields.HasContent())
            {
                NotificationService.AddIssue("The form has no fields.");
                return false;
            }

            var duplicatesText = form.Fields.GroupBy(n => n.Field).SelectMany(g => g.Skip(1));

            if (duplicatesText.HasContent())
            {
                NotificationService.AddIssue(
                    string.Format("Field item text must be unique. Duplications detected: '{0}'",
                        string.Join(",", duplicatesText.Select(n => n.Field))));
                return false;
            }

            if (form.Fields.Any(n => n.Type == FieldType.DropDownList && n.ListID == 0))
            {
                NotificationService.AddIssue("Please select a list for fields specified as Drop Down Lists.");
                return false;
            }

            form.Fields = form.Fields.GroupBy(x => x.Field).Select(y => y.First()); //todo: need to remove empty 

            var repList = Mapper.Map<Form, rForm.Form>(form);

            if (_formRepository.UpdateForm(repList))
            {
                //  var updated = FetchFormList(repList.ID);
                // aud

                return true;
            }

            NotificationService.AddIssue("An error occurred, the Drop Down List could not be saved.");
            return false;
        }

        public bool DeleteForm(Form form)
        {
            throw new NotImplementedException();
        }

        public Form FetchForm(int id)
        {
            var form = Mapper.Map<rForm.Form, Form>(_formRepository.FetchForm(id));

            return form;
        }

        public Form FetchForm(string name)
        {
            var form = Mapper.Map<rForm.Form, Form>(_formRepository.FetchForm(name));

            return form;
        }

        public IEnumerable<Form> FetchForm()
        {
            return Mapper.Map<IEnumerable<rForm.Form>, IEnumerable<Form>>(_formRepository.FetchForm());
        }

        public IEnumerable<Form> FetchForm(IEnumerable<int> ids)
        {
            return Mapper.Map<IEnumerable<rForm.Form>, IEnumerable<Form>>(_formRepository.FetchForm(ids));
        }

        public IEnumerable<FormType> FetchForTypes()
        {
            return Mapper.Map <IEnumerable<rForm.FormType>, IEnumerable<FormType>>(_formRepository.FetchFormTypes());
        }

        public FormList NewFormList()
        {
            return Mapper.Map<rForm.FormList, FormList>(_formRepository.NewFormList());
        }

        public bool CreateFormList(FormList formList, out int id)
        {
            id = -1;

            if (string.IsNullOrEmpty(formList.Name))
            {
                if (!formList.Collection.HasContent())
                {
                    NotificationService.AddIssue("The form list has no name.");
                    return false;
                }
            }

            var formCheck = _formRepository.FetchFormList(formList.Name);

            if (formCheck != null)
            {
                if (String.Equals(formList.Name, formCheck.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    NotificationService.AddIssue(string.Format("A drop down list named '{0}' already exists.", formList.Name));
                    return false;
                }
            }

            if (!formList.Collection.HasContent())
            {
                NotificationService.AddIssue("The form list has no items.");
                return false;
            }

            var duplicatesText = formList.Collection.GroupBy(n => n.Text).SelectMany(g => g.Skip(1));

            if (duplicatesText.HasContent())
            {
                NotificationService.AddIssue(
                    string.Format("Drop down list item text must be unique. Duplications detected: '{0}'",
                        string.Join(",", duplicatesText.Select(n => n.Text))));
                return false;
            }

            var duplicateValues = formList.Collection.GroupBy(n => n.Text).SelectMany(g => g.Skip(1));

            if (duplicateValues.HasContent())
            {
                NotificationService.AddIssue(
                    string.Format("Drop down list item values must be unique. Duplications detected: '{0}'",
                        string.Join(",", duplicateValues.Select(n => n.Value))));
                return false;
            }

            formList.Collection = formList.Collection.GroupBy(x => x.Text).Select(y => y.First()); //todo: need to remove empty 

            if (_formRepository.CreateFormList(Mapper.Map<FormList, rForm.FormList>(formList), out id))
            {
                return true;
            }

            return false;
        }

        public bool UpdateFormList(FormList formList)
        {
            if (string.IsNullOrEmpty(formList.Name))
            {

                    NotificationService.AddIssue("The form list has no name.");
                    return false;
                
            }

            var checkFormList = _formRepository.FetchFormList(formList.Name);

            if (checkFormList != null)
            {
                if (String.Equals(formList.Name, checkFormList.Name, StringComparison.InvariantCultureIgnoreCase) && formList.ID != checkFormList.ID)
                {
                    NotificationService.AddIssue(string.Format("A Drop Down List with the name '{0}' already exists.", formList.Name));
                    return false;
                }
            }

            var repList = Mapper.Map<FormList, rForm.FormList>(formList);


            if (!formList.Collection.HasContent())
            {
                NotificationService.AddIssue("The form list has no items.");
                return false;
            }

            var duplicatesText = formList.Collection.GroupBy(n => n.Text).SelectMany(g => g.Skip(1));

            if (duplicatesText.HasContent())
            {
                NotificationService.AddIssue(
                    string.Format("Drop down list item text must be unique. Duplications detected: '{0}'",
                        string.Join(",", duplicatesText.Select(n => n.Text))));
                return false;
            }

            var duplicateValues = formList.Collection.GroupBy(n => n.Text).SelectMany(g => g.Skip(1));

            if (duplicateValues.HasContent())
            {
                NotificationService.AddIssue(
                    string.Format("Drop down list item values must be unique. Duplications detected: '{0}'",
                        string.Join(",", duplicateValues.Select(n => n.Value))));
                return false;
            }

            formList.Collection = formList.Collection.GroupBy(x => x.Text).Select(y => y.First()); //todo: need to remove empty 

            if (_formRepository.UpdateFormList(repList))
            {
              //  var updated = FetchFormList(repList.ID);
              // aud

                return true;
            }

            NotificationService.AddIssue("An error occurred, the Drop Down List could not be saved.");
            return false;
        }

        public bool DeleteFormList(FormList formList)
        {
            throw new System.NotImplementedException();
        }

        public FormList FetchFormList(int id)
        {
            return Mapper.Map<rForm.FormList, FormList>(_formRepository.FetchFormList(id));
        }

        public FormList FetchFormList(string name)
        {
            return Mapper.Map<rForm.FormList, FormList>(_formRepository.FetchFormList(name));
        }

        public IEnumerable<FormList> FetchFormList()
        {
            return Mapper.Map<IEnumerable<rForm.FormList>, IEnumerable<FormList>>(_formRepository.FetchFormList());
        }

        public IEnumerable<FormList> FetchFormList(IEnumerable<int> ids)
        {
            return Mapper.Map<IEnumerable<rForm.FormList>, IEnumerable<FormList>>(_formRepository.FetchFormList(ids));
        }
    }
}
