using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IAccountDao
    {
        Account GetAccount(int accountId);
        Account MyAccount(string username);
        IList<Account> AllAccountList();
        IList<Account> AccountListButMe(string username);
        public void AddMoney(int accountId, decimal amount);
        public void SubtractMoney(int accountId, decimal amount);
    }
}
