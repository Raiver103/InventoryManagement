namespace InventoryManagement.Domain.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<Item> ManagedItems { get; set; } = new List<Item>();
    }
}
