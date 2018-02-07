using SharpDevTest.Models.Filters;
using SharpDevTest.Models.Response;
using SharpDevTest.Models.Request;
using System;
using System.Threading.Tasks;

namespace SharpDevTest.Services.Interfaces
{
    public interface IPwAccountService
    {
        Task<UserGetModel> GetUserByUsername(string userName);
        Task<decimal> GetUserTotalAsync(string userName);
        Task<TransactionGetModel> GetTransactionByIdAsync(Guid id);
        Task<TransactionGetModel> PostNewTransaction(TransactionPostModel transaction);
        Task<TransactionGetListModel> GetTransactionsListAsync(TransactionFilter filter); //TODO: Add pagination via ListOptions
        Task<UserGetListModel> GetUsersByFilter(UserFilter filter);
    }
}
