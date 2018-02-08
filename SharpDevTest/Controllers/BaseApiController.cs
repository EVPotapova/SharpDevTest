using Microsoft.AspNet.Identity.Owin;
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

        private ApplicationUserManager _appUserManager;
        private ApplicationDbContext _ApplicationDbContext;


        protected ApplicationDbContext ApplicationDbContext
            => _ApplicationDbContext ?? (_ApplicationDbContext = Request.GetOwinContext().Get<ApplicationDbContext>());
        protected ApplicationUserManager UserManager => _appUserManager ?? (_appUserManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>());

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