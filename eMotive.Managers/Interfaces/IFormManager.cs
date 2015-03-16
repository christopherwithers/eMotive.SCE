using System.Collections.Generic;
using eMotive.Models.Objects.Forms;

namespace eMotive.Managers.Interfaces
{
    public interface IFormManager
    {
        Form NewForm();
        bool CreateForm(Form form, out int id);
        bool UpdateForm(Form form);
        bool DeleteForm(Form form);

        Form FetchForm(int id);
        Form FetchForm(string name);

        IEnumerable<Form> FetchForm();
        IEnumerable<Form> FetchForm(IEnumerable<int> ids);

        IEnumerable<FormType> FetchForTypes();

        FormList NewFormList();
        bool CreateFormList(FormList formList, out int id);
        bool UpdateFormList(FormList formList);
        bool DeleteFormList(FormList formList);

        FormList FetchFormList(int id);
        FormList FetchFormList(string name);

        IEnumerable<FormList> FetchFormList();
        IEnumerable<FormList> FetchFormList(IEnumerable<int> ids);
    }
}
