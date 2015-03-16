using System;
using System.Web.Mvc;
using Extensions;

namespace eMotive.SCE.Common
{
    public class CustomJsonResult : JsonResult
    {
        public CustomJsonResult()
        {
            //  JsonRequestBehavior = JsonRequestBehavior.DenyGet;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

          /*  if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
                String.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("JsonRequest_GetNotAllowed");
            }*/

            var response = context.HttpContext.Response;

            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data != null)
            {

                response.Write(Data.ToJson());
            }
        }
    } 
}