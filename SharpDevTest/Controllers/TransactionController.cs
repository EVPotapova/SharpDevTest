using SharpDevTest.Models.Filters;
using SharpDevTest.Models.Request;
using SharpDevTest.Models.Response;
using SharpDevTest.Services.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace SharpDevTest.Controllers
{
    [Authorize]
    [RoutePrefix("transaction")]
    public class TransactionController : BaseApiController
    {
        IPwAccountService PwService;

        public TransactionController(IPwAccountService pwService)
        {
            PwService = pwService;
        }

        [HttpGet]
        public async Task<TransactionGetListModel> GetListAsync([FromUri]TransactionFilter filter)
        {
            try
            {
                return await PwService.GetTransactionsListAsync(filter);
            }
            catch (Exception ex)
            {
                throw CreateThrow500("Unexpected error. " + ex.Message);
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<TransactionGetModel> GetAsync(Guid id)
        {
            TransactionGetModel res = null;
            try
            {
                res = await PwService.GetTransactionByIdAsync(id);
            }
            catch (Exception ex)
            {
                throw CreateThrow500("Unexpected error. " + ex.Message);
            }
            if (res == null)
                throw CreateThrow4Xx(HttpStatusCode.NotFound);
            return res;
        }

        [HttpPost]
        public async Task<TransactionGetModel> PostAsync([FromBody]TransactionPostModel newTransaction)
        {
            ValidateModel();
            TransactionGetModel res = null;
            try
            {
                res = await PwService.PostNewTransaction(newTransaction, User.Identity.Name);
            }
            catch (Exception ex)
            {
                throw CreateThrow500("Unexpected error. " + ex.Message);
            }
            return res;
        }

    }
}
