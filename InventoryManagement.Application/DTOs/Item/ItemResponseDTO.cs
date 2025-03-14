﻿namespace InventoryManagement.Application.DTOs.Item
{
    public class ItemResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public int LocationId { get; set; }
    }
}
