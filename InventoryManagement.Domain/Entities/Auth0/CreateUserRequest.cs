namespace InventoryManagement.Domain.Entities.Auth0
{
    public class CreateUserRequest
    {
        public string FirstName { get; set; } = "firstName";
        public string LastName { get; set; } = "lastName";
        public string Role { get; set; } = "Employee";
        public string Email { get; set; } = "test@gmail.com";
        public string Password { get; set; } = "aaaAAA123";
    }
}