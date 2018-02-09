using SharpDevTest.Models;
using SharpDevTest.Models.DbModel;
using SharpDevTest.Models.Filters;
using SharpDevTest.Models.Request;
using SharpDevTest.Models.Response;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace SharpDevTest.Controllers
{
    [Authorize]
    [RoutePrefix("transaction")]
    public class TransactionController : BaseApiController
    {
        [HttpGet]
        public async Task<TransactionGetListModel> GetListAsync([FromUri]TransactionFilter filter)
        {
            try
            {
                return await GetTransactionsListAsync(filter);
            }
            catch (Exception ex)
            {
                throw CreateThrow500("Unexpected error. " + ex.Message);
            }
        }

        [HttpGet]
        [Route("{id}")]
        //Для реализации презаполнения полей при создании новой транзакции на основе старой
        public async Task<TransactionGetModel> GetAsync(Guid id)
        {
            TransactionGetModel res = null;
            try
            {
                res = await GetTransactionByIdAsync(id);
                if (!res.Sender.UserName.Equals(User.Identity.Name))//only current user transaction
                    throw CreateThrow4Xx(HttpStatusCode.Forbidden);
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
                res = await NewTransaction(newTransaction);
            }
            catch (Exception ex)
            {
                throw CreateThrow500("Unexpected error. " + ex.Message);
            }
            return res;
        }

        //TODO: Использовать Autofac и вынести это в сервисы
        #region Services

        private async Task<TransactionGetModel> GetTransactionByIdAsync(Guid id)
        {
            var dbModel = await ApplicationDbContext.Transactions.Include(t => t.Sender).Include(t => t.Recipient).FirstOrDefaultAsync(t => t.Id == id);


            return dbModel == null ? null : new TransactionGetModel
            {
                Id = dbModel.Id,
                PwAmount = dbModel.PwAmount,
                Sender = new UserGetModel { FullName = dbModel.Sender.FullName, Id = dbModel.Sender.Id, PwCoins = dbModel.Sender.PwCoins },
                Recipient = new UserGetModel { FullName = dbModel.Recipient.FullName, Id = dbModel.Recipient.Id, PwCoins = dbModel.Recipient.PwCoins },
                TransactionDate = dbModel.TransactionDate

            };//TODO: Здесь можно в большем проекте использовать Automapper (как и при других конвертах)
        }
        private async Task<TransactionGetListModel> GetTransactionsListAsync(TransactionFilter filter)
        {
            if (filter == null)
                filter = new TransactionFilter();

            var currentUser = ApplicationDbContext.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase));

            var query = ApplicationDbContext.Transactions.Include(t => t.Sender).Include(t => t.Recipient);

            if (filter.RecipientId != null)
            {
                query = query.Where(t => t.RecipientId.Equals(filter.RecipientId.ToString(), StringComparison.InvariantCultureIgnoreCase));
            }
            if (filter.TransactionDate != null)
            {
                query = query.Where(t => t.TransactionDate == filter.TransactionDate);
            }
            if (filter.PwAmount != null)
            {
                query = query.Where(t => t.PwAmount == filter.PwAmount);
            }

            var items = await query.OrderByDescending(t => t.TransactionDate).Select(dbModel => new TransactionGetModel
            {
                Id = dbModel.Id,
                PwAmount = dbModel.PwAmount,
                Sender = new UserGetModel { FullName = dbModel.Sender.FullName, Id = dbModel.Sender.Id, PwCoins = dbModel.Sender.PwCoins, UserName = dbModel.Sender.UserName },
                Recipient = new UserGetModel { FullName = dbModel.Recipient.FullName, Id = dbModel.Recipient.Id, PwCoins = dbModel.Recipient.PwCoins, UserName = dbModel.Recipient.UserName },
                TransactionDate = dbModel.TransactionDate
            }).Where(t => t.Sender.Id == currentUser.Id).ToListAsync();//only current user transactions

            if (items != null && items.Any())
            {
                return new TransactionGetListModel
                {
                    TotalItemsCount = items.Count,
                    Items = items
                };
            }
            return new TransactionGetListModel { TotalItemsCount = 0, Items = new List<TransactionGetModel>() };//Empty result

        }
        private async Task<TransactionGetModel> NewTransaction(TransactionPostModel transaction)
        {

            using (var dbContextTransaction = ApplicationDbContext.Database.BeginTransaction())
            {
                try
                {
                    var sender = ApplicationDbContext.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase));
                    var recipient = ApplicationDbContext.Users.FirstOrDefault(u => u.FullName.Equals(transaction.RecipientName, StringComparison.InvariantCultureIgnoreCase));

                    if (sender == null || recipient == null)
                    {
                        throw new Exception("Incorrect sender/recipient.");
                    }
                    var dbTransaction = new TransactionDbModel
                    {
                        Id = Guid.NewGuid(),
                        SenderId = sender.Id,
                        RecipientId = recipient.Id,
                        PwAmount = transaction.PwAmount
                    };


                    if (sender.PwCoins < dbTransaction.PwAmount)
                    {
                        throw new Exception("Not enough PW!");
                    }

                    sender.PwCoins = sender.PwCoins - dbTransaction.PwAmount;
                    recipient.PwCoins = recipient.PwCoins + dbTransaction.PwAmount;


                    ApplicationDbContext.Transactions.Add(dbTransaction);


                    await ApplicationDbContext.SaveChangesAsync();

                    dbContextTransaction.Commit();

                    return await GetTransactionByIdAsync(dbTransaction.Id);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    throw ex;
                }
            }
        }
        private async Task<decimal> GetUserTotalAsync(string userName)
        {
            return await ApplicationDbContext.Users.Where(u => u.Email.Equals(userName, StringComparison.InvariantCultureIgnoreCase)).Select(u => u.PwCoins).FirstOrDefaultAsync();
        }

        #endregion

    }
}
