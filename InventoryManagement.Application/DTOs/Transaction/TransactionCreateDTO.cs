namespace InventoryManagement.Application.DTOs.Transaction
{
    public class TransactionCreateDTO
    { 
        public int ItemId { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public string UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
