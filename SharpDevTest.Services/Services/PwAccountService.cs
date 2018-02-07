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
using System.Collections.Generic;

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
                Sender = new UserGetModel { FullName = dbModel.Sender.FullName, Id = dbModel.Sender.Id, PwCoins = dbModel.Sender.PwCoins},
                Recipient = new UserGetModel { FullName = dbModel.Recipient.FullName, Id = dbModel.Recipient.Id, PwCoins = dbModel.Recipient.PwCoins },
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
                Sender = new UserGetModel { FullName = dbModel.Sender.FullName, Id = dbModel.Sender.Id, PwCoins = dbModel.Sender.PwCoins },
                Recipient = new UserGetModel { FullName = dbModel.Recipient.FullName, Id = dbModel.Recipient.Id, PwCoins = dbModel.Recipient.PwCoins },
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
            return new TransactionGetListModel { TotalItemsCount = 0, Items = new List<TransactionGetModel>() };//Empty result

        }
        public async Task<TransactionGetModel> PostNewTransaction(TransactionPostModel transaction, string userName)
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

                    var sender = DbContext.Set<TransactionDbModel>().FirstOrDefault(u => u.Id.ToString().Equals(dbTransaction.SenderId, StringComparison.InvariantCultureIgnoreCase));
                    var recipient = DbContext.Set<TransactionDbModel>().FirstOrDefault(u => u.Id.ToString().Equals(dbTransaction.RecipientId, StringComparison.InvariantCultureIgnoreCase));

                    if (sender == null || recipient == null)
                    {
                        throw new Exception("Incorrect sender/recipient.");
                    }

                    if (sender.PwAmount<dbTransaction.PwAmount)
                    {
                        throw new Exception("Not enough PW!");
                    }

                    sender.PwAmount = sender.PwAmount - dbTransaction.PwAmount;
                    recipient.PwAmount = sender.PwAmount + dbTransaction.PwAmount;

                    
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
        public async Task<UserGetModel> GetUserByUsername(string userName)
        {
            ApplicationUser res = await DbContext.Set<ApplicationUser>().FirstOrDefaultAsync(u => u.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase));
            if (res != null)
            {
                return new UserGetModel
                {
                    FullName = res.FullName,
                    PwCoins = res.PwCoins
                };
            }
            return null;
        }


        public async Task<UserGetModel> GetUserById(string id)
        {
            ApplicationUser res = await DbContext.Set<ApplicationUser>().FirstOrDefaultAsync(u => u.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            if (res != null)
            {
                return new UserGetModel
                {
                    FullName = res.FullName,
                    PwCoins = res.PwCoins
                };
            }
            return null;
        }
        public async Task<UserGetListModel> GetUsersByFilter(UserFilter filter)
        {
            if (filter == null)
                filter = new UserFilter();

            IQueryable<ApplicationUser> query = DbContext.Set<ApplicationUser>();

            if (filter.FullName != null)
            {
                query = query.Where(t => t.FullName.Contains(filter.FullName));
            }


            var items = await query.OrderByDescending(t => t.Id).Select(dbModel => new UserGetModel
            {
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
    }
}
