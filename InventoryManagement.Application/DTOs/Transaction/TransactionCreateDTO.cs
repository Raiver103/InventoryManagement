﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Transaction
{
    public class TransactionCreateDTO
    { 
        public int ItemId { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
