using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using SupportManagement.Application.DTOs;

namespace SupportManagement.Infrastructure.Services;

public class ReportService
{
    public byte[] GenerateTicketExcel(List<TicketSummaryDto> tickets)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Tickets");

        var headers = new[] { "Ticket No", "Subject", "Category", "Priority", "Status", "Assigned To", "Team", "SLA Breached", "Due Date", "Created At" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        for (int i = 0; i < tickets.Count; i++)
        {
            var t = tickets[i];
            var row = i + 2;
            worksheet.Cell(row, 1).Value = t.TicketNo;
            worksheet.Cell(row, 2).Value = t.Subject;
            worksheet.Cell(row, 3).Value = t.CategoryName;
            worksheet.Cell(row, 4).Value = t.Priority.ToString();
            worksheet.Cell(row, 5).Value = t.Status.ToString();
            worksheet.Cell(row, 6).Value = t.AssignedUserName ?? "-";
            worksheet.Cell(row, 7).Value = t.AssignedTeamName ?? "-";
            worksheet.Cell(row, 8).Value = t.IsSlaBreached ? "Yes" : "No";
            worksheet.Cell(row, 9).Value = t.ResolutionDueAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
            worksheet.Cell(row, 10).Value = t.CreatedAt.ToString("yyyy-MM-dd HH:mm");
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateTicketPdf(List<TicketSummaryDto> tickets)
    {
        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        document.Add(new Paragraph("Support Tickets Report").SetFontSize(18).SetBold());
        document.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").SetFontSize(10));
        document.Add(new Paragraph(" "));

        var table = new Table(5).UseAllAvailableWidth();
        foreach (var header in new[] { "Ticket No", "Subject", "Priority", "Status", "Created At" })
        {
            table.AddHeaderCell(new Cell().Add(new Paragraph(header).SetBold()));
        }

        foreach (var t in tickets)
        {
            table.AddCell(t.TicketNo);
            table.AddCell(t.Subject.Length > 40 ? t.Subject[..40] + "..." : t.Subject);
            table.AddCell(t.Priority.ToString());
            table.AddCell(t.Status.ToString());
            table.AddCell(t.CreatedAt.ToString("yyyy-MM-dd"));
        }

        document.Add(table);
        document.Close();

        return stream.ToArray();
    }
}
