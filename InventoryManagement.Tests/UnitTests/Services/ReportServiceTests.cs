using ClosedXML.Excel;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Tests.UnitTests.Services
{
    public class ReportServiceTests
    {
        private readonly IReportService _reportService;

        public ReportServiceTests()
        {
            _reportService = new ReportService();
        }

        [Fact]
        public void GenerateCsv_ShouldReturnCorrectCsvFormat()
        {
            // Arrange
            var transactions = new List<TransactionResponseDTO>
            {
                new TransactionResponseDTO
                {
                    Id = 1,
                    ItemId = 100,
                    FromLocationId = 200,
                    ToLocationId = 300,
                    UserId = "user123",
                    Timestamp = new DateTime(2024, 3, 16, 14, 30, 0)
                }
            };

            string expectedCsv =
                "ID,Item ID,From Location,To Location,User ID,Timestamp" + Environment.NewLine +
                "1,100,200,300,user123,2024-03-16 14:30:00" + Environment.NewLine;

            // Act
            string csvResult = _reportService.GenerateCsv(transactions);

            // Assert
            Assert.Equal(expectedCsv, csvResult);
        }


        [Fact]
        public void GenerateExcel_ShouldCreateValidExcelFile()
        {
            // Arrange
            var transactions = new List<TransactionResponseDTO>
        {
            new TransactionResponseDTO
            {
                Id = 1,
                ItemId = 100,
                FromLocationId = 200,
                ToLocationId = 300,
                UserId = "user123",
                Timestamp = new DateTime(2024, 3, 16, 14, 30, 0)
            }
        };

            // Act
            byte[] excelData = _reportService.GenerateExcel(transactions);

            // Assert
            Assert.NotNull(excelData);
            Assert.True(excelData.Length > 0);

            // Проверяем содержимое файла
            using var stream = new MemoryStream(excelData);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            Assert.Equal("ID", worksheet.Cell(1, 1).Value);
            Assert.Equal("Item ID", worksheet.Cell(1, 2).Value);
            Assert.Equal("From Location", worksheet.Cell(1, 3).Value);
            Assert.Equal("To Location", worksheet.Cell(1, 4).Value);
            Assert.Equal("User ID", worksheet.Cell(1, 5).Value);
            Assert.Equal("Timestamp", worksheet.Cell(1, 6).Value);

            Assert.Equal(1, worksheet.Cell(2, 1).GetValue<int>());
            Assert.Equal(100, worksheet.Cell(2, 2).GetValue<int>());
            Assert.Equal(200, worksheet.Cell(2, 3).GetValue<int>());
            Assert.Equal(300, worksheet.Cell(2, 4).GetValue<int>());
            Assert.Equal("user123", worksheet.Cell(2, 5).GetString());
            Assert.Equal("2024-03-16 14:30:00.0000000", worksheet.Cell(2, 6).GetString());
        }
    }
}
