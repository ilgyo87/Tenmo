using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient
{
    public class Transfer
    {
        public int TransferId { get; set; }

        public int TransferTypeId { get; set; }

        public int TransferStatusId { get; set; }

        public int AccountFrom { get; set; }

        public int AccountTo { get; set; }

        public decimal Amount { get; set; }
    }

    public class TransferUser
    {
        public int AccountFrom { get; set; }

        public int AccountTo { get; set; }

        public decimal Amount { get; set; }
    }

    public class TransferString
    {
        public int TransferId { get; set; }

        public string TransferTypeId { get; set; }

        public string TransferStatusId { get; set; }

        public string AccountFrom { get; set; }

        public string AccountTo { get; set; }

        public string Amount { get; set; }
    }
}
