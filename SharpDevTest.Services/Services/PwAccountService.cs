using SharpDevTest.Services.Interfaces;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using SharpDevTest.Models.Filters;
using SharpDevTest.Models.Request;
using SharpDevTest.Models.Response;
using SharpDevTest.Models.DbModel;
using SharpDevTest.Models;
using System.Linq;

namespace SharpDevTest.Services.Services
{
    public class PwAccountService : IPwAccountService
    {

        private DbContext DbContext { get; } //TODO: В полном проекте стоит вынести слой Data в отдельный ClassLibrary
        public PwAccountService(DbContext context)
        {
            DbContext = context;
        }

        public async Task<TransactionGetModel> GetTransactionByIdAsync(Guid id)
        {
            var dbModel = await DbContext.Set<TransactionDbModel>().Include(t => t.Sender).Include(t => t.Recipient).FirstOrDefaultAsync(t => t.Id == id);

            return dbModel == null ? null : new TransactionGetModel
            {
                Id = dbModel.Id,
                PwAmount = dbModel.PwAmount,
                Sender = dbModel.Sender,
                Recipient = dbModel.Recipient,
                TransactionDate = dbModel.TransactionDate

            };//TODO: Здесь можно в большем проекте использовать Automapper
        }

        public async Task<TransactionGetListModel> GetTransactionsListAsync(TransactionFilter filter)
        {
            if (filter == null)
                filter = new TransactionFilter();

            var query = DbContext.Set<TransactionDbModel>().Include(t => t.Sender).Include(t => t.Recipient);

            if (filter.RecipientId != null)
            {
                query = query.Where(t => t.RecipientId.Equals(filter.RecipientId.ToString(),StringComparison.InvariantCultureIgnoreCase));
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
                Sender = dbModel.Sender,
                Recipient = dbModel.Recipient,
                TransactionDate = dbModel.TransactionDate
            }).ToListAsync();

            if (items != null && items.Any())
            {
                return new TransactionGetListModel
                {
                    TotalItemsCount = items.Count,
                    Items = items
                };
            }
            return new TransactionGetListModel();//Empty result

        }

        public async Task<TransactionGetModel> PostNewTransaction(TransactionPostModel transaction)
        {

            using (var dbContextTransaction = DbContext.Database.BeginTransaction())
            {
                try
                {
                    var dbTransaction = new TransactionDbModel
                    {
                        SenderId = transaction.SenderId.ToString(),
                        RecipientId = transaction.RecipientId.ToString(),
                        PwAmount = transaction.PwAmount
                    };

                    DbContext.Set<TransactionDbModel>().Add(dbTransaction);
                    await DbContext.SaveChangesAsync();

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

        public async Task<decimal> GetUserTotalAsync(string userName)
        {
            return await DbContext.Set<ApplicationUser>().Where(u => u.Email.Equals(userName, StringComparison.InvariantCultureIgnoreCase)).Select(u => u.PwCoins).FirstOrDefaultAsync();
        }
    }
}
