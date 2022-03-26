using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDao
    {
        IList<Transfer> AllTransferList();
        Transfer GetTransferDetails(int transferId);
        decimal GetBalance(int accountId);
        Transfer CreateNewTransfer(NewTransfer newTransfer);
        Transfer UpdateTransfer(int transferId, int transferStatusId, int transferTypeId);
        IList<Transfer> GetListOfTransfersFromAccount(int accountId);
        IList<Transfer> GetListOfCompletedTransfersFromAccount(int accountId);
        IList<Transfer> GetListOfPendingRequestsFromUser(int accountId);
        Transfer SendMoney(int accFrom, int accTo, decimal amount);
        Transfer RequestMoney(int accFrom, int accTo, decimal amount);
        Transfer ApproveRequest(int transferId);
        Transfer RejectRequest(int transferId);






    }
}
