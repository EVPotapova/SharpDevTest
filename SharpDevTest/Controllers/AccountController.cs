using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using SharpDevTest.Models;
using SharpDevTest.Models.Request;
using SharpDevTest.Models.Response;
using System.Net;
using SharpDevTest.Models.Filters;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace SharpDevTest.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : BaseApiController
    {

        [Route("Users")]
        public async Task<UserGetListModel> GetUserList([FromUri]UserFilter filter)
        {
            try
            {
                return await GetUsersByFilter(filter);
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

            var res = await ApplicationDbContext.Users.FirstOrDefaultAsync(u => u.UserName.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase));
            if (res == null)
                throw CreateThrow4Xx(HttpStatusCode.NotFound, "User is not found.");

            return new UserGetModel
            {
                Id = res.Id,
                FullName = res.FullName,
                PwCoins = res.PwCoins,
                UserName = res.UserName
            };
        }
        
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }
        
        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IHttpActionResult> AddNewUserAsync(UserPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Email, FullName = model.FullName, Email = model.Email, PwCoins = 500M }; //TODO: unhardcode start value (to config)

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Helpers
        
        private async Task<UserGetModel> GetUserById(string id)
        {
            ApplicationUser res = await ApplicationDbContext.Users.FirstOrDefaultAsync(u => u.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            if (res != null)
            {
                return new UserGetModel
                {
                    UserName = res.UserName,
                    Id = res.Id,
                    FullName = res.FullName,
                    PwCoins = res.PwCoins
                };
            }
            return null;
        }
        private async Task<UserGetListModel> GetUsersByFilter(UserFilter filter)
        {
            if (filter == null)
                filter = new UserFilter();

            IQueryable<ApplicationUser> query = ApplicationDbContext.Users;

            if (filter.FullName != null)
            {
                query = query.Where(t => t.FullName.Contains(filter.FullName));
            }


            var items = await query.OrderByDescending(t => t.Id).Select(dbModel => new UserGetModel
            {
                UserName = dbModel.UserName,
                Id = dbModel.Id,
                PwCoins = dbModel.PwCoins,
                FullName = dbModel.FullName
            }).ToListAsync();

            if (items != null && items.Any())
            {
                return new UserGetListModel
                {
                    TotalItemsCount = items.Count,
                    Items = items
                };
            }
            return new UserGetListModel { TotalItemsCount = 0, Items = new List<UserGetModel>() };//Empty result
        }

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
