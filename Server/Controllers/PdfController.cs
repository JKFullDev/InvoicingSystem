using InvoicingSystem.Server.Data;
using InvoicingSystem.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace InvoicingSystem.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly InvoicingSystemDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PdfController(InvoicingSystemDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet("invoice/{invoiceId}")]
        public async Task<IActionResult> GenerateInvoicePdf(string invoiceId)
        {
            try
            {
                // Cargo la factura con todas sus relaciones
                var invoice = await _context.SalesInvoiceHeaders
                    .Include(h => h.Customer)
                    .Include(h => h.PaymentTerms) 
                    .Include(h => h.Lines)
                        .ThenInclude(l => l.Product)
                    .Include(h => h.Lines)
                        .ThenInclude(l => l.TaxRate)
                    .FirstOrDefaultAsync(h => h.SalesInvoiceHeaderId == invoiceId);

                if (invoice == null)
                {
                    return NotFound(new { message = $"No se encontró la factura {invoiceId}" });
                }

                // Configuro QuestPDF (licencia Community para proyectos pequeños)
                QuestPDF.Settings.License = LicenseType.Community;

                // Genero el PDF
                var document = new InvoiceDocument(invoice, _environment.WebRootPath);
                byte[] pdfBytes = document.GeneratePdf();

                // Devuelvo el PDF
                return File(pdfBytes, "application/pdf", $"Factura_{invoiceId}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el PDF", error = ex.Message });
            }
        }
    }
}
