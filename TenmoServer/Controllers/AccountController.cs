using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController:ControllerBase
    {
        private readonly IAccountDao accountDao;

        public AccountController(IAccountDao _accountDao)
        {
            accountDao = _accountDao;
        }

        [HttpGet()]
        public ActionResult<Account> AllAccountList()
        {
            IList<Account> accounts = accountDao.AllAccountList();
            if(accounts == null)
            {
                return NoContent();
            }
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public ActionResult<Account> GetAccount(int id)
        {
            Account account = accountDao.GetAccount(id);
            if (account != null)
            {
                return account;
            }
            return NotFound();
        }

        [HttpGet("otheraccounts")]
        public ActionResult<Account> List()
        {
            string username = User.Identity.Name;
            return Ok(accountDao.AccountListButMe(username));
        }

        [HttpGet("mybalance")]
        public ActionResult<Account> MyBalance()
        {
            string username = User.Identity.Name;
            return Ok(accountDao.MyAccount(username));
        }

        //[HttpGet("sendtransfer/{Id}")]
        //public ActionResult<Account> GetAccount(int id)
        //{
        //    return accountDao.GetAccount(id);
        //}

        //[HttpPut("transfer")]
        //public ActionResult<Account> UpdateTransferToAccount(Account account)
        //{
        //    return accountDao.UpdateTransferAccount(account);
        //}
    }
}
