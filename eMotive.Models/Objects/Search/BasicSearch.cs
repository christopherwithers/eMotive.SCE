using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Extensions;

namespace eMotive.Models.Objects.Search
{
    public enum SortDirection { ASC, DESC }
    public class BasicSearch
    {

        private int? page;

        public BasicSearch()
        {
            page = 1;
            PageSize = 10;
        }

        public int NumberOfResults { get; set; }
        public int PageSize { get; set; }
        public int? Page
        {
            get
            {
                return page;
            }
            set
            {
                if (!value.HasValue || !value.Value.IsNumeric())
                {
                    page = 1;
                    return;
                }

                if (value <= 0)
                {
                    page = 1;

                    return;
                }

                /*       if (NumberOfResults > 0 && value > TotalPages)
                {
                    page = TotalPages;
                    return;
                }*/
         

                page = value;
            }
        }

        public string[] Type { get; set; }

        public string Query { get; set; }

        public string Error { get; set; }

        public string ItemType { get; set; } //todo: get rid of this!

        public Dictionary<string, string> Filter { get; set; } 

        //public string Filter { get; set; }



        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)NumberOfResults / PageSize); }
        }

        public string SortBy { get; set; }
        public SortDirection OrderBy { get; set; }

        public virtual string BuildSearchQueryString(bool _append, HashSet<string> _omitParams = null)
        {
            var sb = new StringBuilder();
            var isFirst = true;

            sb.Append(_append ? "&" : "?");

            if (_omitParams == null || !_omitParams.Contains("query"))
                if (!string.IsNullOrEmpty(Query))
                {
                    if (!isFirst)
                        sb.Append("&");

                    sb.Append("query=");
                    sb.Append(Query);
                    isFirst = false;
                }

            if (_omitParams == null || !_omitParams.Contains("page"))
                if (Page.HasValue)
                {
                    if (!isFirst)
                        sb.Append("&");

                    sb.Append("page=");
                    sb.Append(Page.Value);
                    isFirst = false;
                }

            if (_omitParams == null || !_omitParams.Contains("sortby"))
                if (!string.IsNullOrEmpty(SortBy))
                {
                    if (!isFirst)
                        sb.Append("&");

                    sb.Append("sortby=");
                    sb.Append(SortBy);
                    isFirst = false;
                }

            if (_omitParams == null || !_omitParams.Contains("pagesize"))
            {
                if (!isFirst)
                {
                    sb.Append("&");
                    isFirst = false;
                }
                sb.Append("pagesize=");
                sb.Append(PageSize);
            }

            if (_omitParams == null || !_omitParams.Contains("orderby"))
            {
                if (!isFirst)
                {
                    sb.Append("&");
                    isFirst = false;
                }
                sb.Append("orderby=");
                sb.Append(OrderBy);
            }
            return sb.ToString();
        }

        public string BuildSearchQueryString(string _field, SortDirection _direction, HashSet<string> _omitParams = null)//todo: how to inject params into this??
        {
            var sb = new StringBuilder();
            var isFirst = true;

          //  SortBy = _field;
        //    OrderBy = _direction;

            sb.Append("?");
            if (_omitParams == null || !_omitParams.Contains("query"))
                if (!string.IsNullOrEmpty(Query))
                {
                    if (!isFirst)
                        sb.Append("&");

                    sb.Append("query=");
                    sb.Append(Query);
                    isFirst = false;
                }

            if (_omitParams == null || !_omitParams.Contains("page"))
                if (Page.HasValue)
                {
                    if (!isFirst)
                        sb.Append("&");

                    sb.Append("page=");
                    sb.Append(Page.Value);
                    isFirst = false;
                }

            if (_omitParams == null || !_omitParams.Contains("sortby"))
                if (!string.IsNullOrEmpty(SortBy))
                {
                    if (!isFirst)
                        sb.Append("&");

                    sb.Append("sortby=");
                    sb.Append(_field);
                    isFirst = false;
                }


            if (_omitParams == null || !_omitParams.Contains("pagesize"))
            {
                if (!isFirst)
                {
                    sb.Append("&");
                    isFirst = false;
                }
                sb.Append("pagesize=");
                sb.Append(PageSize);
            }

            if (_omitParams == null || !_omitParams.Contains("orderby"))
            {
                if (!isFirst)
                {
                    sb.Append("&");
                    isFirst = false;
                }
                sb.Append("orderby=");
                sb.Append(_direction);
            }
            return sb.ToString();
        }
    
    }
}
