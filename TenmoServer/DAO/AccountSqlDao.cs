using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;
using System.Data.SqlClient;

namespace TenmoServer.DAO
{
    public class AccountSqlDao : IAccountDao
    {
        private readonly string connectionString;

        public AccountSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public Account GetAccount(int accountId)
        {
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT a.account_id, a.balance, a.user_id, tu.username AS username FROM " +
                                                "account a JOIN tenmo_user tu ON a.user_id = tu.user_id " +
                                                "WHERE account_id= @account_id;", conn);
                cmd.Parameters.AddWithValue("@account_id", accountId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    account = AccountFromReader(reader);
                }
            }

            return account;
        }

        public Account MyAccount(string username)
        {
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT a.account_id, a.balance, a.user_id, username AS username FROM " +
                                                "account a JOIN tenmo_user tu ON a.user_id = tu.user_id " +
                                                "WHERE username = @username;", conn);
                cmd.Parameters.AddWithValue("@username", username);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    account = AccountFromReader(reader);
                }
            }

            return account;
        }

        public IList<Account> AllAccountList()
        {
            IList<Account> accounts = new List<Account>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT a.account_id, a.balance, a.user_id, tu.username AS username " +
                                                    "FROM account a JOIN tenmo_user tu ON a.user_id = tu.user_id ", conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Account account = AccountFromReader(reader);
                        accounts.Add(account);
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return accounts;
        }

        public IList<Account> AccountListButMe(string username)
        {
            IList<Account> accounts = new List<Account>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT a.account_id, a.balance, a.user_id, tu.username AS username FROM " +
                                                    "account a JOIN tenmo_user tu ON a.user_id = tu.user_id " +
                                                    "WHERE tu.username <> @username", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Account account = AccountFromReader(reader);
                        accounts.Add(account);
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return accounts;
        }

        public void AddMoney(int accountId, decimal amount)
        {
            try
            {
                Account account = GetAccount(accountId);
                account.Balance += amount;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("UPDATE account " +
                                                    "SET balance = @balance " +
                                                    "WHERE account_id = @account_id; ", conn);
                    cmd.Parameters.AddWithValue("@balance", account.Balance);
                    cmd.Parameters.AddWithValue("@account_id", accountId);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
        }

        public void SubtractMoney(int accountId, decimal amount)
        {
            try
            {
                Account account = GetAccount(accountId);
                account.Balance -= amount;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("UPDATE account " +
                                                    "SET balance = @balance " +
                                                    "WHERE account_id = @account_id; ", conn);
                    cmd.Parameters.AddWithValue("@balance", account.Balance);
                    cmd.Parameters.AddWithValue("@account_id", accountId);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
        }

        private Account AccountFromReader(SqlDataReader reader)
        {
            Account account = new Account();
            account.AccountId = Convert.ToInt32(reader["account_id"]);
            account.Balance = Convert.ToDecimal(reader["balance"]);
            account.UserId = Convert.ToInt32(reader["user_id"]);
            account.Username = Convert.ToString(reader["username"]);

            return account;
        }

        //public Account UpdateTransferAccount(Account account)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(connectionString))
        //        {
        //            conn.Open();

        //            SqlCommand cmd = new SqlCommand("UPDATE account SET balance = @balance " +
        //                       "WHERE account_id = @account_id; ", conn);
        //            cmd.Parameters.AddWithValue("@balance", account.Balance);
        //            cmd.Parameters.AddWithValue("@account_id", account.AccountId);

        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //    catch (SqlException e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //    return account;
        //}


    }
}

