﻿namespace InventoryManagement.Domain.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
