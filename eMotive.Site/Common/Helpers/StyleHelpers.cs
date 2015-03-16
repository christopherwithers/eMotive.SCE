using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eMotive.Models.Objects;

namespace eMotive.SCE.Common.Helpers
{
    public static class StyleHelpers
    {
        #region HomeView Styles
        public static MvcHtmlString HomeViewRowStyle(this HtmlHelper _helper, SlotType _type)
        {
            switch (_type)
            {
                case SlotType.Main:
                case SlotType.Reserve:
                    return MvcHtmlString.Create("class='success'");
                case SlotType.Interested:
                    return MvcHtmlString.Create("class='info'");
                default:
                    return MvcHtmlString.Create(string.Empty);
            } 
        }

       /* public static MvcHtmlString HomeViewRowBadge(this HtmlHelper _helper, SlotType _type)
        {
            switch (_type)
            {
                case SlotType.Main:
                case SlotType.Reserve:
                    return MvcHtmlString.Create("<span class='label label-success'>Signed Up</span>");
                case SlotType.Interested:
                    return MvcHtmlString.Create("<span class='label label-info'>Interested</span>");
                default:
                    return MvcHtmlString.Create(string.Empty);
            }
        }*/

        public static MvcHtmlString HomeViewRowBadge(this HtmlHelper _helper, SlotType _type)
        {
            switch (_type)
            {
                case SlotType.Main:
                    return MvcHtmlString.Create(string.Empty);// MvcHtmlString.Create("<span class='label label-success'>Signed Up</span> <span class='label label-success' style='clear: left;'>Main</span>");
                case SlotType.Reserve:
                    return MvcHtmlString.Create(string.Empty);// MvcHtmlString.Create("<span class='label label-success'>Signed Up</span> <span class='label label-success' style='clear: left;'>Reserve</span>");
                case SlotType.Interested:
                    return MvcHtmlString.Create("<span class='label label-success'>Signed Up</span> <span class='label label-info' style='clear: left;'>Interested</span>");
                default:
                    return MvcHtmlString.Create(string.Empty);
            }
        }

        public static MvcHtmlString HomeViewRowButton(this HtmlHelper _helper, SlotType _type)
        {
            switch (_type)
            {
                case SlotType.Main:
                case SlotType.Reserve:
                    return MvcHtmlString.Create("class='btn btn-success'");
                case SlotType.Interested:
                    return MvcHtmlString.Create("class='btn btn-info'");
                default:
                    return MvcHtmlString.Create(string.Empty);
            }
        }
        #endregion
    }
}