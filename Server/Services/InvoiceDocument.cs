using InvoicingSystem.Server.Data.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvoicingSystem.Server.Services
{
    public class InvoiceDocument : IDocument
    {
        public SalesInvoiceHeaders Invoice { get; }
        private readonly string _logoPath;

        public InvoiceDocument(SalesInvoiceHeaders invoice, string webRootPath)
        {
            Invoice = invoice;
            _logoPath = Path.Combine(webRootPath, "images", "logo.png");
        }

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        #region Header
        void ComposeHeader(IContainer container)
        {
            var titleScale = 1f;
            var scale = 0.8f;
            var smallScale = 0.6f;

            var navyDark = "#001B48";
            var darkGray = "#878787";
            var lightGray = "#E6E6E6";

            container.PaddingBottom(10).Column(col =>
            {
                // PRIMERA FILA: Logo y Metadatos
                col.Item().Row(row =>
                {
                    // Logo
                    row.RelativeItem().Column(column =>
                    {
                        if (File.Exists(_logoPath))
                        {
                            column.Item().Height(2, Unit.Centimetre).Image(_logoPath);
                        }
                        else
                        {
                            column.Item().Text("LOGO").FontSize(20).Bold();
                        }
                    });

                    row.RelativeItem().PaddingTop(10);

                    // Metadatos Factura
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Scale(1.4f).Text($"Factura {Invoice.SalesInvoiceHeaderId}").Bold().FontColor(navyDark).AlignRight();
                        column.Spacing(2);
                        column.Item().Scale(scale).Text($"Ref. cliente: {Invoice.CustomerReference}").FontColor(navyDark).AlignRight();
                        column.Item().Scale(scale).Text($"Fecha facturación: {Invoice.InvoiceDate:dd/MM/yyyy}").FontColor(navyDark).AlignRight();
                        column.Item().Scale(scale).Text($"Fecha de vencimiento: {Invoice.DueDate:dd/MM/yyyy}").FontColor(navyDark).AlignRight();
                        column.Item().Scale(scale).Text($"Código cliente: {Invoice.CustomerId}").FontColor(navyDark).AlignRight();
                        column.Item().Scale(scale).Text($"Ref. presupuesto: {Invoice.QuoteReference}").FontColor(navyDark).AlignRight();
                    });
                });

                col.Item().PaddingTop(20);

                // SEGUNDA FILA: Emisor y Cliente
                col.Item().Row(row =>
                {
                    // Info Emisor (tu empresa - hardcodeado o desde configuración)
                    row.RelativeItem(1).Column(column =>
                    {
                        column.Item().Scale(smallScale).Text("Emisor");
                        column.Spacing(5);

                        column.Item().Height(110).Background(lightGray).Padding(10).Column(innerCol =>
                        {
                            innerCol.Item().Scale(titleScale).Text("HARD2BIT S.L.").Bold().FontColor(navyDark);
                            innerCol.Item().Scale(scale).Text("Avda Juan Caramuel, Nº 1, Planta 3, Puerta C\nParque Tecnológico de Leganés\n28918 Leganés\nMadrid").FontColor(navyDark);
                            innerCol.Item().Scale(scale).Text("");
                            innerCol.Item().Scale(scale).Text("Teléfono: +34 910 139 827").FontColor(navyDark);
                            innerCol.Item().Scale(scale).Text("Correo: admon@hard2bit.com").FontColor(navyDark);
                            innerCol.Item().Scale(scale).Text("Web: https://hard2bit.com").FontColor(navyDark);
                        });
                    });

                    row.Spacing(25);

                    // Info Cliente
                    row.RelativeItem(1.2f).Column(column =>
                    {
                        column.Item().Scale(smallScale).Text("Enviar a");
                        column.Spacing(5);

                        column.Item().Height(110).Border(0.6f).BorderColor(darkGray).Padding(10).Column(innerCol =>
                        {
                            innerCol.Item().Scale(titleScale).Text(Invoice.Customer?.Name ?? "Cliente desconocido").Bold();
                            innerCol.Item().Scale(scale).Text(Invoice.Customer?.Address ?? "");
                            innerCol.Item().Scale(scale).Text("");
                            innerCol.Item().Scale(scale).Text($"CIF/NIF: {Invoice.Customer?.Nif ?? ""}");
                            innerCol.Item().Scale(scale).Text("");
                        });
                    });

                    col.Item().PaddingTop(15).Scale(0.75f).Text("Importes visualizados en Euros").AlignRight();
                });
            });
        }
        #endregion

        #region Body
        void ComposeContent(IContainer container)
        {
            var scale = 0.9f;
            var darkGray = "#878787";
            var lightGray = "#E6E6E6";
            var extraLightGray = "#FAFAFA";
            var navyDark = "#001B48";

            container.Column(col =>
            {
                // Tabla de líneas
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Border(0.6f).BorderColor(darkGray).MaxHeight(350).Table(table =>
                        {
                            table.ExtendLastCellsToTableBottom();

                            // Definición de columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);    // Descripción
                                columns.ConstantColumn(50);   // IVA
                                columns.ConstantColumn(70);   // P.U.
                                columns.ConstantColumn(50);   // Cant.
                                columns.ConstantColumn(80);   // Total
                            });

                            // Cabecera
                            table.Header(header =>
                            {
                                header.Cell().Border(0.6f).BorderColor(darkGray).Padding(2).Text("Descripción").AlignLeft();
                                header.Cell().Border(0.6f).BorderColor(darkGray).Padding(2).Scale(scale).Text("IVA").AlignCenter();
                                header.Cell().Border(0.6f).BorderColor(darkGray).Padding(2).Scale(scale).Text("P.U.").AlignCenter();
                                header.Cell().Border(0.6f).BorderColor(darkGray).Padding(2).Scale(scale).Text("Cant.").AlignCenter();
                                header.Cell().Border(0.6f).BorderColor(darkGray).Padding(2).Scale(scale).Text("Total").AlignCenter();
                            });

                            // Filas de datos
                            foreach (var line in Invoice.Lines ?? Enumerable.Empty<SalesInvoiceLines>())
                            {
                                var productName = line.Product?.Name ?? "Producto";
                                var taxRate = line.TaxRate?.Percentage ?? 21;

                                table.Cell().Border(0.6f).BorderColor(darkGray).Padding(5).Column(desc =>
                                {
                                    desc.Item().Text(productName).Bold();
                                    if (!string.IsNullOrWhiteSpace(line.CustomDescription))
                                    {
                                        desc.Item().PaddingLeft(10).Text(line.CustomDescription).FontSize(9);
                                    }
                                });

                                table.Cell().Border(0.6f).BorderColor(darkGray).Padding(5).AlignRight().Text($"{taxRate}%");
                                table.Cell().Border(0.6f).BorderColor(darkGray).Padding(5).AlignRight().Text($"{line.UnitPrice:N2}");
                                table.Cell().Border(0.6f).BorderColor(darkGray).Padding(5).AlignRight().Text(line.Quantity.ToString());
                                table.Cell().Border(0.6f).BorderColor(darkGray).Padding(5).AlignRight().Text($"{line.TotalLine:N2}");
                            }

                            // Relleno para ocupar espacio restante
                            table.Cell().BorderRight(0.6f).BorderColor(darkGray).ExtendVertical().Text("");
                            table.Cell().BorderRight(0.6f).BorderColor(darkGray).ExtendVertical().Text("");
                            table.Cell().BorderRight(0.6f).BorderColor(darkGray).ExtendVertical().Text("");
                            table.Cell().BorderRight(0.6f).BorderColor(darkGray).ExtendVertical().Text("");
                            table.Cell().ExtendVertical().Text("");
                        });
                    });
                });

                // Condiciones de pago y totales
                col.Item().EnsureSpace().PaddingTop(5).Row(row =>
                {
                    // Columna Izquierda: Condiciones de pago
                    row.RelativeItem().Column(banco =>
                    {
                        banco.Item().Text(text =>
                        {
                            text.Span("Condiciones de pago: ").FontSize(10 * scale).Bold();
                            text.Span(Invoice.PaymentTerms?.Description ?? "").FontSize(9);
                        });

                        banco.Item().PaddingTop(10).Scale(0.75f).Text("Pago mediante transferencia a la cuenta bancaria siguiente:").Bold();
                        banco.Item().Scale(0.75f).Text("Banco: SANTANDER");
                        banco.Item().Scale(0.75f).Text("Nombre del titular: HARD2BIT SL");
                        banco.Item().Scale(0.75f).Text("IBAN: ES33 0049 6109 6520 1635 4905").Bold();
                        banco.Item().Scale(0.75f).Text("BIC/SWIFT: BSCHESMMXXX").Bold();
                    });

                    row.ConstantItem(50);

                    // Columna Derecha: Totales
                    row.RelativeItem().Column(totales =>
                    {
                        var totalBase = Invoice.Lines?.Sum(l => l.TotalLine) ?? 0;
                        var totalIva = totalBase * 0.21m; // IVA 21%
                        var totalFinal = totalBase + totalIva;

                        totales.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.ConstantColumn(80);
                            });

                            t.Cell().Scale(scale).Text("Total (Base imp).");
                            t.Cell().Scale(scale).AlignRight().Text($"{totalBase:N2}");

                            t.Cell().Scale(scale).Background(extraLightGray).Text("Total IVA 21%");
                            t.Cell().Scale(scale).Background(extraLightGray).AlignRight().Text($"{totalIva:N2}");

                            t.Cell().Scale(scale).Background(lightGray).Text("Total").FontColor(navyDark);
                            t.Cell().Scale(scale).Background(lightGray).AlignRight().Text($"{totalFinal:N2}").FontColor(navyDark);
                        });
                    });
                });
            });
        }
        #endregion

        #region Footer
        void ComposeFooter(IContainer container)
        {
            container.BorderTop(0.6f).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
            {
                row.RelativeItem();

                row.AutoItem().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Black));
                    text.Line("Sociedad Laboral - CIF/NIF: B86717147 - Núm. seguridad social: 28221010147. Datos Registrales: Hoja M-550809 Tomo 30604 Folio 98");
                    text.Line("CNAE: 6202 - Actividades de consultoría informática - CIF intra.: ESB86717147");
                });

                row.RelativeItem().AlignRight().AlignMiddle().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Black));
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }
        #endregion
    }
}
