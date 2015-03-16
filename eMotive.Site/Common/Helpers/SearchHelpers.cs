using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using Extensions;
using eMotive.Models.Objects.Search;
using ServiceStack.Common;

namespace eMotive.SCE.Common.Helpers
{
    public static class SearchHelpers
    {
        public static MvcHtmlString PageLinks(this HtmlHelper _helper, BasicSearch _paging, Func<int, string> _pageUrl, params string[] _params)
        {
            var sb = new System.Text.StringBuilder();
            //   TagBuilder tag;
            if (!_paging.Page.IsNumeric())
                _paging.Page = 1;

            if (_paging.Page < 1)
                _paging.Page = 1;

            if (_paging.Page > _paging.TotalPages)
                _paging.Page = 1;// _paging.TotalPages;

            const int grace = 5;
            const int range = grace * 2;
            var totalPages = _paging.TotalPages;
            var start = (_paging.Page - grace) > 0 ? (_paging.Page - grace) : 1;
            var end = start + range;
            var search = string.Empty;

            //if (!string.IsNullOrEmpty(_paging.Query))
            search = _paging.BuildSearchQueryString(true, new HashSet<string> {"page"});//string.Concat("&Query=", _paging.Query);

            if (_params.HasContent())
            {
                if (!search.IsNullOrEmpty())
                {
                    sb.Append(search);
                    sb.Append("&");
                }
                else
                {
                    sb.Append("?");
                }

                foreach (var param in _params)
                {
                    sb.Append(param);
                    sb.Append("&");
                }

                //remove trailing &
                sb.Remove(sb.Length - 1, 1);

                search = sb.ToString();

                sb.Length = 0;
            }

            if (end > totalPages)
            {
                end = totalPages;
                start = (end - range) > 0 ? (end - range) : 1;
            }

            if (totalPages > 1)
            {
                if (_paging.Page <= 1)
                {
                    sb.Append("first previous ");
                }
                else
                {
                    sb.Append("<a href='"); sb.Append(_pageUrl(1)); sb.Append(search); sb.Append("'>first</a> ");
                    sb.Append("<a href='"); sb.Append(_pageUrl((int) (_paging.Page - 1))); sb.Append(search); sb.Append("'>previous</a> ");
                }
            }

            if (start > 1)
            {
              //  sb.Append("<a href='"); sb.Append(_pageUrl(1)); sb.Append(search); sb.Append("'>First</a>&nbsp;&nbsp;");
                sb.Append("<a href='"); sb.Append(_pageUrl(1)); sb.Append(search); sb.Append("'>1</a> ...");
            }

            for (var i = start; i <= end; i++)
            {
                if (i == _paging.Page)
                {
                    sb.Append("<span>"); sb.Append(i); sb.Append("</span>&nbsp;&nbsp;");
                }
                else
                {
                    sb.Append("<a href='"); sb.Append(_pageUrl(i.Value)); sb.Append(search); sb.Append("'>"); sb.Append(i); sb.Append("</a>&nbsp;&nbsp;");
                }
            }

            if (end < totalPages)
            {
                sb.Append("... <a href='"); sb.Append(_pageUrl(totalPages)); sb.Append(search); sb.Append("'>"); sb.Append(totalPages); sb.Append("</a>");
            }

            if (totalPages > 1)
            {
                if (_paging.Page >= totalPages)
                {
                    sb.Append(" next last");
                }
                else
                {
                    sb.Append(" <a href='"); sb.Append(_pageUrl((int)(_paging.Page +1))); sb.Append(search); sb.Append("'>next</a>");
                    sb.Append(" <a href='"); sb.Append(_pageUrl(totalPages)); sb.Append(search); sb.Append("'>last</a>");
                }
                
            }

            sb.Append("<div>"); sb.Append(_paging.NumberOfResults); sb.Append(" "); sb.Append(_paging.ItemType); sb.Append(" found. Displaying page ");
            sb.Append(_paging.Page); sb.Append(" of "); sb.Append(totalPages); sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString SearchString(this HtmlHelper _helper, string _query, string _page, string _sortby, string _orderby, params string[] _params)
        {
            var sb = new StringBuilder();
            var isFirst = true;

            sb.Append("?");

            if (!string.IsNullOrEmpty(_query))
            {
                if (!isFirst)
                    sb.Append("&");

                sb.Append("query=");
                sb.Append(_query);
                isFirst = false;
            }

            if (!string.IsNullOrEmpty(_page))
            {
                if (!isFirst)
                    sb.Append("&");

                sb.Append("page=");
                sb.Append(_page);
                isFirst = false;
            }

            if (!string.IsNullOrEmpty(_sortby))
            {
                if (!isFirst)
                    sb.Append("&");

                sb.Append("sortby=");
                sb.Append(_sortby);
                isFirst = false;
            }

            if (!string.IsNullOrEmpty(_orderby))
            {
                if (!isFirst)
                    sb.Append("&");

                sb.Append("orderby=");
                sb.Append(_orderby);
                isFirst = false;
            }

            if (_params.HasContent())
            {
                if (!isFirst)
                    sb.Append("&");

                foreach (var param in _params)
                {
                    sb.Append(param);
                    sb.Append("&");
                }

                //remove trailing &
                sb.Remove(sb.Length - 1, 1);
            }

            return MvcHtmlString.Create(string.Empty);
        }
    }
}