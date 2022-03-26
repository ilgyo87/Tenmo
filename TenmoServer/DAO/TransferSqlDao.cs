using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;
using System.Data.SqlClient;

namespace TenmoServer.DAO
{
    public class TransferSqlDao : ITransferDao
    {
        private readonly string connectionString;

        public TransferSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public IList<Transfer> AllTransferList()
        {
            IList<Transfer> transfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sqlTxt = "SELECT * " +
                                    "FROM transfer ";
                    SqlCommand cmd = new SqlCommand(sqlTxt, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Transfer transfer = CreateTransferFromReader(reader);
                        transfers.Add(transfer);
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }

            return transfers;
        }

        public Transfer GetTransferDetails(int transferId)
        {
            Transfer transfer = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sqlText = "SELECT transfer_id, transfer_type_id,transfer_status_id, account_from, account_to, amount" +
                                     " FROM transfer WHERE transfer_id = @transfer_id;";
                    SqlCommand cmd = new SqlCommand(sqlText, conn);
                    cmd.Parameters.AddWithValue("@transfer_id", transferId);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        transfer = CreateTransferFromReader(reader);
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return transfer;
        }

        public decimal GetBalance(int accountId)
        {
            decimal balance = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sqlTxt = "SELECT balance " +
                                    "FROM account WHERE account_id = @accountid";
                    SqlCommand cmd = new SqlCommand(sqlTxt, conn);
                    cmd.Parameters.AddWithValue("@accountid", accountId);

                    balance = Convert.ToDecimal(cmd.ExecuteScalar());

                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return balance;
        }

        public Transfer CreateNewTransfer(NewTransfer newTransfer)
        {
            int newTransferId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO transfer (transfer_type_id, transfer_status_id, account_from, account_to, amount ) " +
                                                    "OUTPUT INSERTED.transfer_id " +
                                                    "VALUES (@transfer_type_id, @transfer_status_id, @account_from, @account_to, @amount);", conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", newTransfer.TransferTypeId);
                    cmd.Parameters.AddWithValue("@transfer_status_id", newTransfer.TransferStatusId);
                    cmd.Parameters.AddWithValue("@account_from", newTransfer.AccountFrom);
                    cmd.Parameters.AddWithValue("@account_to", newTransfer.AccountTo);
                    cmd.Parameters.AddWithValue("@amount", newTransfer.Amount);

                    newTransferId = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return GetTransferDetails(newTransferId);
        }

        public Transfer UpdateTransfer(int transferId, int transferStatusId, int transferTypeId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sqlText = "UPDATE transfer SET transfer_status_id = @transfer_status_id, transfer_type_id = @transfer_type_id WHERE transfer_id = @transfer_id;";
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sqlText, conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", transferTypeId);
                    cmd.Parameters.AddWithValue("@transfer_status_id", transferStatusId);
                    cmd.Parameters.AddWithValue("@transfer_id", transferId);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return GetTransferDetails(transferId);
        }

        public IList<Transfer> GetListOfTransfersFromAccount(int accountId)
        {
            IList<Transfer> transfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sqlTxt = "SELECT * " +
                                    "FROM transfer " +
                                    "WHERE account_from = @account_from OR account_to = @account_to";
                    SqlCommand cmd = new SqlCommand(sqlTxt, conn);
                    cmd.Parameters.AddWithValue("@account_from", accountId);
                    cmd.Parameters.AddWithValue("@account_to", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Transfer transfer = CreateTransferFromReader(reader);
                        transfers.Add(transfer);
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return transfers;
        }

        public IList<Transfer> GetListOfCompletedTransfersFromAccount(int accountId)
        {
            IList<Transfer> transfers = GetListOfTransfersFromAccount(accountId);
            IList<Transfer> completedTransfers = new List<Transfer>();
            try
            {
                foreach (Transfer transfer in transfers)
                {
                    if (transfer.TransferStatusId == 2)
                    {
                        completedTransfers.Add(transfer);
                    }
                }
            }           
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return completedTransfers;
        }

        public IList<Transfer> GetListOfPendingRequestsFromUser(int accountId)
        {
            IList<Transfer> transfers = GetListOfTransfersFromAccount(accountId);
            IList<Transfer> pendingTransfers = new List<Transfer>();
            try
            {
                foreach (Transfer transfer in transfers)
                {
                    if (transfer.TransferStatusId == 1 && transfer.TransferTypeId == 1 && transfer.AccountTo != accountId)
                    {
                        pendingTransfers.Add(transfer);
                    }
                }
            }          
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return pendingTransfers;
        }

        public Transfer SendMoney(int accFrom, int accTo, decimal amount)
        {
            try
            {
                NewTransfer newTransfer = new NewTransfer
                {
                    TransferTypeId = 2,
                    TransferStatusId = 2,
                    AccountFrom = accFrom,
                    AccountTo = accTo,
                    Amount = amount
                };
                if (GetBalance(accFrom) >= amount)
                {
                    Transfer transfer = CreateNewTransfer(newTransfer);

                    return GetTransferDetails(transfer.TransferId);
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public Transfer RequestMoney(int accFrom, int accTo, decimal amount)
        {
            try
            {
                NewTransfer newTransfer = new NewTransfer
                {
                    TransferTypeId = 1,
                    TransferStatusId = 1,
                    AccountFrom = accFrom,
                    AccountTo = accTo,
                    Amount = amount
                };

                if (amount != 0)
                {
                    Transfer transfer = CreateNewTransfer(newTransfer);
                    return GetTransferDetails(transfer.TransferId);
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public Transfer ApproveRequest(int transferId)
        {
            try
            {
                Transfer newTransfer = GetTransferDetails(transferId);
                if (newTransfer != null && newTransfer.TransferStatusId == 1 && GetBalance(newTransfer.AccountFrom) >= newTransfer.Amount)
                {
                    UpdateTransfer(transferId, 2, 1);

                    return GetTransferDetails(newTransfer.TransferId);
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public Transfer RejectRequest(int transferId)
        {
            try
            {
                Transfer newTransfer = GetTransferDetails(transferId);
                if (newTransfer != null && newTransfer.TransferStatusId == 1)
                {
                    UpdateTransfer(transferId, 3, 1);

                    return GetTransferDetails(newTransfer.TransferId);
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        private Transfer CreateTransferFromReader(SqlDataReader reader)
        {
            Transfer transfer = new Transfer();
            transfer.TransferId = Convert.ToInt32(reader["transfer_id"]);
            transfer.TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]);
            transfer.TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]);
            transfer.AccountTo = Convert.ToInt32(reader["account_to"]);
            transfer.AccountFrom = Convert.ToInt32(reader["account_from"]);
            transfer.AccountTo = Convert.ToInt32(reader["account_to"]);
            transfer.Amount = Convert.ToDecimal(reader["amount"]);

            return transfer;
        }

        //public IList<Transfer> TransferHistory(int accountId)
        //{
        //    IList<Transfer> transferHistory = new List<Transfer>();
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(connectionString))
        //        {
        //            conn.Open();

        //            SqlCommand cmd = new SqlCommand("SELECT t.transfer_id, ts.transfer_status_desc, tp.transfer_type_desc, account_from, account_to, amount " +
        //                                            "FROM transfer t JOIN transfer_status ts ON t.transfer_type_id = ts.transfer_status_id " +
        //                                            "JOIN transfer_type tp ON t.transfer_type_id = tp.transfer_type_id " +
        //                                            "WHERE account_from = @account_id OR account_to = @account_id; ", conn);
        //            cmd.Parameters.AddWithValue("@account_id", accountId);
        //            SqlDataReader reader = cmd.ExecuteReader();
        //            while (reader.Read())
        //            {

        //                Transfer history = TransferHistoryFromReader(reader);
        //                if (history.AccountTo == accountId)
        //                {
        //                    history.Display = ($"+ ${history.Amount.ToString()}");
        //                    history.TransferTypeDesc = "Recevied";
        //                }
        //                else
        //                {
        //                    history.Display = ($"- ${history.Amount.ToString()}");
        //                    history.TransferTypeDesc = "Sent";
        //                }
        //                transferHistory.Add(history);
        //            }
        //        }
        //    }
        //    catch (SqlException e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //    return transferHistory;
        //}

        //private Transfer TransferHistoryFromReader(SqlDataReader reader)
        //{
        //    Transfer transfer = new Transfer();
        //    transfer.TransferId = Convert.ToInt32(reader["transfer_id"]);
        //    transfer.AccountTo = Convert.ToInt32(reader["account_to"]);
        //    transfer.AccountFrom = Convert.ToInt32(reader["account_from"]);
        //    transfer.AccountTo = Convert.ToInt32(reader["account_to"]);
        //    transfer.Amount = Convert.ToDecimal(reader["amount"]);
        //    transfer.TransferStatusDesc = Convert.ToString(reader["transfer_status_desc"]);
        //    transfer.TransferTypeDesc = Convert.ToString(reader["transfer_type_desc"]);

        //    return transfer;
        //}



    }
}
