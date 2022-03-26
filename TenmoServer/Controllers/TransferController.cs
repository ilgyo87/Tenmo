using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private ITransferDao transferDao;
        private IAccountDao accountDao;

        public TransferController(ITransferDao transferDao, IAccountDao accountDao)
        {
            this.transferDao = transferDao;
            this.accountDao = accountDao;
        }

        [HttpGet()]
        public ActionResult<Transfer> GetAllTransferList()
        {
            IList<Transfer> transfers = transferDao.AllTransferList();
            if (transfers == null)
            {
                return NoContent();
            }
            return Ok(transfers);
        }

        [HttpGet("{id}")]
        public ActionResult<Transfer> GetTransfer(int id)
        {
            Transfer transfer = transferDao.GetTransferDetails(id);
            if (transfer != null)
            {
                return transfer;
            }
                return NotFound();
        }

        [HttpGet("account_id={id}/completed")]
        public ActionResult<Transfer> GetTransfersFromUser(int id)
        {
            Account account = accountDao.GetAccount(id);
            if (account == null)
            {
                return NotFound();
            }
            IList<Transfer> transfers = transferDao.GetListOfCompletedTransfersFromAccount(id);
            if (transfers == null)
            {
                return NoContent();
            }
            return Ok(transfers);
        }

        [HttpGet("account_id={id}/pending")]
        public ActionResult<Transfer> GetPendingTransfersFromUser(int id)
        {
            Account account = accountDao.GetAccount(id);
            if (account == null)
            {
                return NotFound();
            }
            IList<Transfer> transfers = transferDao.GetListOfPendingRequestsFromUser(id);
            if (transfers == null)
            {
                return NoContent();
            }
            return Ok(transfers);
        }

        [HttpPost("send")]
        public ActionResult<Transfer> SendMoney(TransferUser transferParam)
        {
            Account myAccount = accountDao.GetAccount(transferParam.AccountFrom);
            Account requestAccount = accountDao.GetAccount(transferParam.AccountTo);
            if (myAccount != null && requestAccount != null)
            {
                Transfer transfer = transferDao.SendMoney(myAccount.AccountId, requestAccount.AccountId, transferParam.Amount);
                if (transfer == null)
                {
                    return BadRequest(new { message = "An error occurred and transfer was not created(1)." });
                }
                accountDao.SubtractMoney(myAccount.AccountId, transferParam.Amount);
                accountDao.AddMoney(requestAccount.AccountId, transferParam.Amount);
                return Created($"/transfer/{transfer.TransferId}", transfer);
            }
            return BadRequest(new { message = "An error occurred and transfer was not created(2)." });
        }

        [HttpPost("request")]
        public ActionResult<Transfer> RequestMoney(TransferUser transferParam)
        {
            Account myAccount = accountDao.GetAccount(transferParam.AccountFrom);
            Account requestAccount = accountDao.GetAccount(transferParam.AccountTo);
            if (myAccount != null && requestAccount != null)
            {
                Transfer transfer = transferDao.RequestMoney(myAccount.AccountId, requestAccount.AccountId, transferParam.Amount);
                try
                {
                    return Created($"/transfer/{transfer.TransferId}", transfer);
                }
                catch (NullReferenceException)
                {
                    return BadRequest(new { message = "You can not request $0 or a negative value." });
                }
            }
            return BadRequest(new { message = "An error occurred and transfer was not created." });
        }

        [HttpPut("{transferId}/approve")]
        public ActionResult<Transfer> AprroveRequest(int transferId)
        {
            Transfer existingTransfer = transferDao.GetTransferDetails(transferId);
            if (existingTransfer == null)
            {
                return NotFound();
            }
            Transfer approve = transferDao.ApproveRequest(transferId);
            if (approve == null)
            {
                return BadRequest(new { message = "An error occurred and request can not be approved." });
            }
            accountDao.SubtractMoney(approve.AccountFrom, approve.Amount);
            accountDao.AddMoney(approve.AccountTo, approve.Amount);
            return Ok(approve);
        }

        [HttpPut("{transferId}/reject")]
        public ActionResult<Transfer> RejectRequest(int transferId)
        {
            Transfer existingTransfer = transferDao.GetTransferDetails(transferId);
            if (existingTransfer == null)
            {
                return NotFound();
            }
            Transfer reject = transferDao.RejectRequest(transferId);
            if (reject == null)
            {
                return BadRequest(new { message = "An error occurred and request can not be rejected." });
            }
            return Ok(reject);
        }

        //[HttpGet("myhistory/{accountId}")]
        //public IList<Transfer> TransferHistory(int accountId)
        //{
        //    return transferDao.TransferHistory(accountId);
        //}

        //[HttpGet("{id}")]
        //public ActionResult<Transfer> GetTransferById(int id)
        //{
        //    return transferDao.GetTranferDetail(id);
        //}

        //[HttpPost()]
        //public ActionResult<Transfer> TransferTo(Transfer transfer)
        //{
        //    Transfer newTransfer = transferDao.CreateNewTransfer(transfer);
        //    return Created($"/transfer/{newTransfer.TransferId}", newTransfer);
        //}
    }
}
