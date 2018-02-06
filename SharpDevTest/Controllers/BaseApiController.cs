using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SharpDevTest.Controllers
{
    public abstract class BaseApiController : ApiController
    {


        protected HttpResponseException CreateThrow4Xx(HttpStatusCode code, string message = null)
        {
            var response = new HttpResponseMessage(code);

            if (message != null)
            {
                response.Content = new StringContent(message);
            }

            return new HttpResponseException(response);
        }
        protected void ValidateModel()
        {
            if (!ModelState.IsValid)
            {
                var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                throw new HttpResponseException(response);
            }
        }



        protected HttpResponseException CreateThrow500(string message = null)
        {
            return CreateThrow4Xx(HttpStatusCode.InternalServerError, message ?? "Unexpected error on server. Please, try later.");
        }
        
    }
}