using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using eMotive.Models.Objects;
using Extensions;

namespace eMotive.SCE.Common.Helpers
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString BuildCrumbs(this HtmlHelper HtmlHelper)
        {
            return new MvcHtmlString(string.Empty);
        }

        public static MvcHtmlString GetSignupStatusName(this HtmlHelper _helper, SlotStatus _slotStatus)
        {
            string name = string.Empty;

            switch (_slotStatus)
            {
                case SlotStatus.AlreadySignedUp:
                    name = "Cancel My Session";
                    break;
                case SlotStatus.Clash:
                    name = "Unavailable";
                    break;
                case SlotStatus.Closed:
                    name = "Closed";
                    break;
                /*    case SlotStatus.SignupClosed:
                        name = "Closed";
                        break;
                    case SlotStatus.AlreadySignedUpClosed:
                        name = "Closed";
                        break;*/
                case SlotStatus.Full:
                    name = "Full";
                    break;
                case SlotStatus.Signup:
                    name = "Sign Up";
                    break;
                case SlotStatus.Interested:
                    name = "Interested";
                    break;
                case SlotStatus.Reserve:
                    name = "Reserve";
                    break;
                default:
                    name = "Error";
                    break;
            }

            return MvcHtmlString.Create(name);
        }


        public static MvcHtmlString AssignStatusFunctionality(this HtmlHelper _helper, SlotStatus _slotStatus, int _signupId, int _slotId)
        {
            switch (_slotStatus)
            {
                case SlotStatus.AlreadySignedUp:
                    return MvcHtmlString.Create(string.Format(" onclick='DoCancelSignup({0},{1});' ", _signupId, _slotId));
                case SlotStatus.Clash:
                    return MvcHtmlString.Create("  ");
                /*       case SlotStatus.AlreadySignedUpClosed:
                           return MvcHtmlString.Create(" onclick='ShowClosedSignedupModal();' ");
                       case SlotStatus.Closed:
                           return MvcHtmlString.Create(" onclick='ShowClosedModal();' ");*/
                case SlotStatus.Closed:
                    return MvcHtmlString.Create("  ");
                case SlotStatus.Full:
                    return MvcHtmlString.Create("  ");
                case SlotStatus.Signup:
                case SlotStatus.Reserve:
                case SlotStatus.Interested:
                    return MvcHtmlString.Create(string.Format(" onclick='DoSignup({0},{1});' ", _signupId, _slotId));
                default:
                    return MvcHtmlString.Create(" onclick='alert(\"error!\"); return false;' ");

            }
        }

        public static MvcHtmlString AssignStatusFunctionality(this HtmlHelper _helper, SlotStatus _slotStatus, int _signupId, int _slotId, string _username)
        {
            switch (_slotStatus)
            {
                case SlotStatus.AlreadySignedUp:
                    return MvcHtmlString.Create(string.Format(" onclick='DoCancelSignup({0},{1},\"{2}\");' ", _signupId, _slotId, _username));
                case SlotStatus.Clash:
                    return MvcHtmlString.Create(" ");
                /*       case SlotStatus.AlreadySignedUpClosed:
                           return MvcHtmlString.Create(" onclick='ShowClosedSignedupModal();' ");
                       case SlotStatus.Closed:
                           return MvcHtmlString.Create(" onclick='ShowClosedModal();' ");*/
                case SlotStatus.Closed:
                    return MvcHtmlString.Create("  ");
                case SlotStatus.Full:
                    return MvcHtmlString.Create(" ");
                case SlotStatus.Signup:
                case SlotStatus.Reserve:
                case SlotStatus.Interested:
                    return MvcHtmlString.Create(string.Format(" onclick='DoSignup({0},{1},\"{2}\");' ", _signupId, _slotId, _username));
                default:
                    return MvcHtmlString.Create(" onclick='alert(\"error!\"); return false;' ");

            }
        }


        public static MvcHtmlString SetSlotStatusButton(this HtmlHelper _helper, int _totalplaces, int _remainingPlaces, bool _signedUp)
        {
            if (_signedUp)
                return MvcHtmlString.Create("class='btn btn-danger'");

            var placesLeft = (100 * _remainingPlaces) / _totalplaces;

            string warningLevel = string.Empty;
            if (placesLeft <= 10)
            {
                warningLevel = "btn btn-danger";
            }
            else if (placesLeft <= 30)
            {
                warningLevel = "btn btn-warning";
            }
            else if (placesLeft <= 60)
            {
                warningLevel = "btn btn-info";
            }
            else
            {
                return MvcHtmlString.Create("class='btn'");
            }

            return MvcHtmlString.Create(string.Format("class='{0}'", warningLevel));
        }

        public static MvcHtmlString SetStatusButton(this HtmlHelper _helper, int _totalplaces, int _remainingPlaces, bool _signedUp)
        {
            if (_signedUp)
                return MvcHtmlString.Create("class='btn btn-success'");

            var placesLeft = (100 * _remainingPlaces) / _totalplaces;

            string warningLevel = string.Empty;
            if (placesLeft <= 10)
            {
                warningLevel = "btn btn-danger";
            }
            else if (placesLeft <= 30)
            {
                warningLevel = "btn btn-warning";
            }
            else if (placesLeft <= 60)
            {
                warningLevel = "btn btn-info";
            }
            else
            {
                return MvcHtmlString.Create("class='btn'");
            }

            return MvcHtmlString.Create(string.Format("class='{0}'", warningLevel));
        }

        public static MvcHtmlString SetStatusStyle(this HtmlHelper _helper, int _totalplaces, int _remainingPlaces, bool _signedUp)
        {
            if (_signedUp)
                return MvcHtmlString.Create("class='success'");

            var placesLeft = (100 * _remainingPlaces) / _totalplaces;

            string warningLevel = string.Empty;

            if (placesLeft <= 10)
            {
                warningLevel = "danger";
            }
            else if (placesLeft <= 30)
            {
                warningLevel = "warning";
            }
            else if (placesLeft <= 60)
            {
                warningLevel = "info";
            }
            else
            {
                return MvcHtmlString.Create(string.Empty);
            }

            return MvcHtmlString.Create(string.Format("class='{0}'", warningLevel));
        }


        public static MvcHtmlString RadioButtonListFor<TModel, TProperty>(this HtmlHelper<TModel> _helper, Expression<Func<TModel, TProperty>> _expression, IDictionary<string, string> _options, string _name, int _columns)
        {
            if (!_columns.IsNumeric() || _columns <= 0)
                throw new Exception("RadioButtonListFor: Invalid number of columns");

            var RadioButtonList = new System.Text.StringBuilder();

            var metadata = ModelMetadata.FromLambdaExpression(_expression, _helper.ViewData);

            int selectedItem;

            Int32.TryParse(metadata.Model.ToString(), out selectedItem);


            string radioButtonListName = metadata.PropertyName;

            if (_helper.ViewContext.HttpContext.Request.Form[radioButtonListName] != null)
                Int32.TryParse(_helper.ViewContext.HttpContext.Request.Form[radioButtonListName], out selectedItem);

            RadioButtonList.Append("<div id=\""); RadioButtonList.Append(_name); RadioButtonList.Append("\">");
            RadioButtonList.Append("<table>");
            int i = 0;
            foreach (var item in _options)
            {
                if (i == 0)
                    RadioButtonList.Append("<tr>");

                RadioButtonList.Append("<td>");

                // RadioButtonList.Append("<div>");
                var builder = new TagBuilder("input");
                builder.GenerateId(item.Value);
                builder.MergeAttribute("name", radioButtonListName);
                builder.MergeAttribute("type", "radio");
                builder.MergeAttribute("value", item.Key);
                builder.MergeAttribute("class", string.Concat("radioButtonList_", _name));

                if (selectedItem > 0)
                {
                    if (string.Compare(item.Key, selectedItem.ToString()) == 0)
                    {
                        builder.MergeAttribute("checked", "checked");
                    }
                }

                RadioButtonList.Append(builder.ToString(TagRenderMode.Normal));
                RadioButtonList.Append("<span class=\"radioOption\">");
                RadioButtonList.Append(item.Value);
                RadioButtonList.Append("</span>");

                RadioButtonList.Append("</td>");

                i++;

                if (i >= _columns)
                {
                    i = 0;
                    RadioButtonList.Append("</tr>");
                }
                //     RadioButtonList.Append("</div>");
            }
            RadioButtonList.Append("</table>");
            RadioButtonList.Append("</div>");

            return MvcHtmlString.Create(RadioButtonList.ToString());
        }

        public static MvcHtmlString RadioButtonListFor<TModel, TProperty>(this HtmlHelper<TModel> _helper, Expression<Func<TModel, TProperty>> _expression, IDictionary<string, string> _options, string _name)
        {
            var RadioButtonList = new System.Text.StringBuilder();

            var metadata = ModelMetadata.FromLambdaExpression(_expression, _helper.ViewData);

            int selectedItem;

            Int32.TryParse(metadata.Model.ToString(), out selectedItem);


            string radioButtonListName = metadata.PropertyName;

            if (_helper.ViewContext.HttpContext.Request.Form[radioButtonListName] != null)
                Int32.TryParse(_helper.ViewContext.HttpContext.Request.Form[radioButtonListName], out selectedItem);

            RadioButtonList.Append("<div id=\""); RadioButtonList.Append(_name); RadioButtonList.Append("\">");
            foreach (var item in _options)
            {
                // RadioButtonList.Append("<div>");
                var builder = new TagBuilder("input");
                builder.GenerateId(item.Value);
                builder.MergeAttribute("name", radioButtonListName);
                builder.MergeAttribute("type", "radio");
                builder.MergeAttribute("value", item.Key);
                builder.MergeAttribute("class", string.Concat("radioButtonList_", _name));
                if (selectedItem > 0)
                {
                    if (string.Compare(item.Key, selectedItem.ToString()) == 0)
                    {
                        builder.MergeAttribute("checked", "checked");
                    }
                }

                RadioButtonList.Append(builder.ToString(TagRenderMode.Normal));
                RadioButtonList.Append("<span class=\"radioOption\">");
                RadioButtonList.Append(item.Value);
                RadioButtonList.Append("</span>");
                RadioButtonList.Append("&nbsp"); RadioButtonList.Append("&nbsp");

                //     RadioButtonList.Append("</div>");
            }
            RadioButtonList.Append("</div>");

            return MvcHtmlString.Create(RadioButtonList.ToString());
        }

        public static MvcHtmlString RawRadioButtonListFor<TModel, TProperty>(this HtmlHelper<TModel> _helper, Expression<Func<TModel, TProperty>> _expression, IDictionary<string, string> _options, string _name)
        {
            var RadioButtonList = new System.Text.StringBuilder();

            var metadata = ModelMetadata.FromLambdaExpression(_expression, _helper.ViewData);

            int selectedItem;

            Int32.TryParse(metadata.Model.ToString(), out selectedItem);


            string radioButtonListName = metadata.PropertyName;

            if (_helper.ViewContext.HttpContext.Request.Form[radioButtonListName] != null)
                Int32.TryParse(_helper.ViewContext.HttpContext.Request.Form[radioButtonListName], out selectedItem);


            foreach (var item in _options)
            {
                var builder = new TagBuilder("input");
                builder.MergeAttribute("id", string.Format("{0}{1}", radioButtonListName, item.Key));
                builder.MergeAttribute("name", radioButtonListName);
                builder.MergeAttribute("type", "radio");
                builder.MergeAttribute("value", item.Key);
                if (selectedItem > 0)
                {
                    if (string.CompareOrdinal(item.Key, selectedItem.ToString()) == 0)
                    {
                        builder.MergeAttribute("checked", "checked");
                    }
                }

                RadioButtonList.Append(builder.ToString(TagRenderMode.Normal));
                RadioButtonList.AppendFormat("<label for=\"{0}{1}\">{2}</label>", radioButtonListName, item.Key, item.Value);
            }


            return MvcHtmlString.Create(RadioButtonList.ToString());
        }

        public static MvcHtmlString CheckBoxListForGroups<TModel, TProperty>(this HtmlHelper<TModel> _helper, Expression<Func<TModel, TProperty>> _expression, IEnumerable<KeyValuePair<string, string>> _options)
        {
            var checkBoxList = new System.Text.StringBuilder();

            var metadata = ModelMetadata.FromLambdaExpression(_expression, _helper.ViewData);

            var selectedItems = metadata.Model as string[];//new string[0];

            var checkBoxListName = metadata.PropertyName;

            if (_helper.ViewContext.HttpContext.Request.Form[checkBoxListName] != null)
                selectedItems = _helper.ViewContext.HttpContext.Request.Form[checkBoxListName].Split(',');


            foreach (var item in _options)
            {
                var isChecked = string.Empty;

                if (selectedItems != null && (selectedItems.Length > 0 && selectedItems.Any(n => n == item.Key)))
                {
                    isChecked = "checked=\"checked\"";
                }

                checkBoxList.AppendFormat("<label class=\"checkbox span3\"><input type=\"checkbox\" {0} name=\"{1}\" value=\"{2}\"> {3}</label>", isChecked, checkBoxListName, item.Key, item.Value);
            }


            return MvcHtmlString.Create(checkBoxList.ToString());
        }
    }
}