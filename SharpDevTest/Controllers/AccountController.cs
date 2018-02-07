using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using SharpDevTest.Models;
using SharpDevTest.Services.Interfaces;
using SharpDevTest.Models.Request;
using SharpDevTest.Models.Response;
using System.Net;
using SharpDevTest.Models.Filters;

namespace SharpDevTest.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : BaseApiController
    {
        private ApplicationUserManager _userManager;
        private IPwAccountService PwAccountService;



        public AccountController(ApplicationUserManager userManager, IPwAccountService pwAccountService)
        {
            UserManager = userManager;
            PwAccountService = pwAccountService;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [Route("Users")]
        public async Task<UserGetListModel> GetUserList([FromUri]UserFilter filter)
        {
            try
            {
                return await PwAccountService.GetUsersByFilter(filter);
            }
            catch (Exception ex)
            {
                throw CreateThrow500(ex.Message);
            }

        }

        [Route("CurrentUser")]
        public async Task<UserGetModel> GetCurrentUser()
        {
            string userName = User.Identity.GetUserName();

            var res = await PwAccountService.GetUserByUsername(userName);
            if (res == null)
                throw CreateThrow4Xx(HttpStatusCode.NotFound, "User is not found.");

            return res;
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IHttpActionResult> AddNewUserAsync(UserPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Email, FullName = model.FullName, Email = model.Email, PwCoins = 500M }; //TODO: unhardcode start value

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        #endregion
    }
}
