namespace InventoryManagement.Domain.Entities
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
    }
}
