using ClosedXML.Excel;
using InventoryManagement.Application.DTOs.Transaction;
using System.Globalization;
using System.Text;

namespace InventoryManagement.Application.Services
{
    public class ReportService
    {
        public string GenerateCsv(IEnumerable<TransactionResponseDTO> transactions)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ID,Item ID,From Location,To Location,User ID,Timestamp");

            foreach (var t in transactions)
            {
                csv.AppendLine($"{t.Id},{t.ItemId},{t.FromLocationId},{t.ToLocationId},{t.UserId},{t.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
            }

            return csv.ToString();
        }

        public byte[] GenerateExcel(IEnumerable<TransactionResponseDTO> transactions)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Transactions");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Item ID";
            worksheet.Cell(1, 3).Value = "From Location";
            worksheet.Cell(1, 4).Value = "To Location";
            worksheet.Cell(1, 5).Value = "User ID";
            worksheet.Cell(1, 6).Value = "Timestamp";

            int row = 2;
            foreach (var t in transactions)
            {
                worksheet.Cell(row, 1).Value = t.Id;
                worksheet.Cell(row, 2).Value = t.ItemId;
                worksheet.Cell(row, 3).Value = t.FromLocationId;
                worksheet.Cell(row, 4).Value = t.ToLocationId;
                worksheet.Cell(row, 5).Value = t.UserId;

                // Если Timestamp — DateTime, форматируем
                string timestampStr = (t.Timestamp != DateTime.MinValue)
                    ? t.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fffffff")
                    : "0001-01-01 00:00:00.0000000";

                worksheet.Cell(row, 6).SetValue(timestampStr);
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "@"; // Устанавливаем текстовый формат

                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
