using InvoicingSystem.Server.Data;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Radzen;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;

namespace InvoicingSystem.Server.Controllers
{
    // La ruta base de OData. Tiene que coincidir con lo que pongamos en Program.cs
    [Route("odata/InvoicingSystem/SalesInvoiceHeaders")]
    public partial class SalesInvoiceHeadersController : ODataController
    {
        private readonly InvoicingSystemDbContext context;
        private readonly ILogger<SalesInvoiceHeadersController> _logger;

        // Inyectamos el DbContext y el Logger
        public SalesInvoiceHeadersController(InvoicingSystemDbContext context, ILogger<SalesInvoiceHeadersController> logger)
        {
            this.context = context;
            _logger = logger;
        }

        #region GET(todos)
        // Listo todas las facturas incluyendo sus líneas
        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IEnumerable<SalesInvoiceHeaders> GetSalesInvoiceHeaders()
        {
            // Incluyo las líneas para que se carguen junto con la factura
            var items = this.context.SalesInvoiceHeaders
                .Include(h => h.Lines)
                .AsNoTracking()
                .AsQueryable();
            return items;
        }
        #endregion

        #region GET(1)
        // Obtengo una factura por ID incluyendo sus líneas
        [HttpGet("/odata/InvoicingSystem/SalesInvoiceHeaders({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public SingleResult<SalesInvoiceHeaders> GetSalesInvoiceHeaders(string key)
        {
            // Incluyo las líneas para que se carguen con la factura
            var items = this.context.SalesInvoiceHeaders
                .Include(h => h.Lines)
                .AsNoTracking()
                .Where(i => i.SalesInvoiceHeaderId == key);
            var result = SingleResult.Create(items);

            return result;
        }
        #endregion

        #region DELETE(1)
        // --- 3. BORRAR UNO POR ID
        [HttpDelete("/odata/InvoicingSystem/SalesInvoiceHeaders({key})")]
        public IActionResult DeleteSalesInvoiceHeaders(string key)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState); // Si el modelo llega mal formado, se termina

                var item = this.context.SalesInvoiceHeaders.Where(i => i.SalesInvoiceHeaderId == key).FirstOrDefault();

                if (item == null) return NotFound("El impuesto no existe");  //Si no existe, fuera

                this.context.SalesInvoiceHeaders.Remove(item);
                this.context.SaveChanges();

                return new NoContentResult(); // 204 No Content (Todo ok pero no devuelvo nada)
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 547 || sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                // Error 547: Restricción de clave foránea
                // Error 2601/2627: Violación de índice único
                return Conflict(new { message = "No se puede eliminar este impuesto porque está siendo utilizado en una o más facturas." });
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }
        #endregion

        #region PUT
        // Actualizo una factura completa incluyendo sus líneas
        [HttpPut("/odata/InvoicingSystem/SalesInvoiceHeaders({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PutSalesInvoiceHeaders(string key, [FromBody]SalesInvoiceHeadersDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (itemDto == null || (itemDto.SalesInvoiceHeaderId != key)) return BadRequest();

                // Debug: Log de entrada
                _logger.LogWarning($"[SERVER] ===== PUT FACTURA INICIADO =====");
                _logger.LogWarning($"[SERVER] Factura: {key}");
                _logger.LogWarning($"[SERVER] Líneas recibidas en DTO: {itemDto.Lines?.Count ?? 0}");

                // Cargo la entidad existente SIN las líneas primero
                var entityToUpdate = this.context.SalesInvoiceHeaders
                    .FirstOrDefault(i => i.SalesInvoiceHeaderId == key);

                if (entityToUpdate == null)
                {
                    _logger.LogError($"[SERVER] Factura no encontrada: {key}");
                    return NotFound();
                }

                // Actualizo los campos de la cabecera
                entityToUpdate.CustomerReference = itemDto.CustomerReference;
                entityToUpdate.InvoiceDate = itemDto.InvoiceDate;
                entityToUpdate.DueDate = itemDto.DueDate;
                entityToUpdate.QuoteReference = itemDto.QuoteReference;
                entityToUpdate.CustomerId = itemDto.CustomerId;
                entityToUpdate.PaymentTermsId = itemDto.PaymentTermsId;

                // Estrategia simple: Elimino TODAS las líneas viejas y añado TODAS las nuevas
                // Primero elimino todas las líneas existentes de esta factura
                var existingLines = this.context.SalesInvoiceLines
                    .Where(l => l.SalesInvoiceHeaderId == key)
                    .ToList();

                _logger.LogWarning($"[SERVER] Líneas existentes a eliminar: {existingLines.Count}");

                if (existingLines.Any())
                {
                    this.context.SalesInvoiceLines.RemoveRange(existingLines);
                    _logger.LogWarning($"[SERVER] RemoveRange ejecutado");
                }

                // Ahora añado todas las líneas del DTO como nuevas
                if (itemDto.Lines != null && itemDto.Lines.Any())
                {
                    _logger.LogWarning($"[SERVER] Añadiendo {itemDto.Lines.Count} líneas nuevas");

                    foreach (var lineDto in itemDto.Lines)
                    {
                        var newGuid = Guid.NewGuid();
                        var newLine = new SalesInvoiceLines
                        {
                            SalesInvoiceLineId = newGuid,
                            SalesInvoiceHeaderId = key,
                            ProductId = lineDto.ProductId,
                            TaxRateId = lineDto.TaxRateId,
                            UnitPrice = lineDto.UnitPrice,
                            Quantity = lineDto.Quantity,
                            CustomDescription = lineDto.CustomDescription
                        };

                        _logger.LogWarning($"[SERVER] Creando línea: GUID={newGuid}, Producto={newLine.ProductId}, Cantidad={newLine.Quantity}");
                        this.context.SalesInvoiceLines.Add(newLine);
                        _logger.LogWarning($"[SERVER] Línea añadida al contexto");
                    }
                }
                else
                {
                    _logger.LogWarning($"[SERVER] DTO no tiene líneas");
                }

                _logger.LogWarning($"[SERVER] Ejecutando SaveChanges...");
                var changesCount = this.context.SaveChanges();
                _logger.LogWarning($"[SERVER] SaveChanges completado. Cambios guardados: {changesCount}");

                // Cargo la factura actualizada con las líneas para devolverla
                var itemToReturn = this.context.SalesInvoiceHeaders
                    .AsNoTracking()
                    .Include(h => h.Lines)
                    .Where(i => i.SalesInvoiceHeaderId == key)
                    .FirstOrDefault();

                _logger.LogWarning($"[SERVER] Líneas en respuesta: {itemToReturn?.Lines.Count ?? 0}");
                _logger.LogWarning($"[SERVER] ===== PUT FACTURA COMPLETADO =====");

                return new ObjectResult(SingleResult.Create(new[] { itemToReturn }.AsQueryable()));
            }
            catch(Exception ex)
            {
                _logger.LogError($"[SERVER] ERROR: {ex.Message}");
                _logger.LogError($"[SERVER] STACK TRACE: {ex.StackTrace}");
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }

        #endregion

        #region PATCH

        // --- 5. ACTUALIZAR PARCIAL
        [HttpPatch("/odata/InvoicingSystem/SalesInvoiceHeaders({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PatchSalesInvoiceHeaders(string key, [FromBody]Delta<SalesInvoiceHeaders> patch)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var entityToUpdate = this.context.SalesInvoiceHeaders.Where(i => i.SalesInvoiceHeaderId == key).FirstOrDefault();

                if (entityToUpdate == null) return BadRequest();
                 
                patch.Patch(entityToUpdate);
                this.context.SalesInvoiceHeaders.Update(entityToUpdate);
                this.context.SaveChanges();

                var itemToReturn = this.context.SalesInvoiceHeaders.Where(i => i.SalesInvoiceHeaderId == key);
                return new ObjectResult(SingleResult.Create(itemToReturn));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }

        #endregion

        #region POST
        // Creo una factura nueva con sus líneas
        [HttpPost]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult Post([FromBody] SalesInvoiceHeadersDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (itemDto == null) return BadRequest();

                // Creo la entidad de cabecera
                var newEntity = new SalesInvoiceHeaders
                {
                    SalesInvoiceHeaderId = itemDto.SalesInvoiceHeaderId,
                    CustomerReference = itemDto.CustomerReference,
                    InvoiceDate = itemDto.InvoiceDate,
                    DueDate = itemDto.DueDate,
                    QuoteReference = itemDto.QuoteReference,
                    CustomerId = itemDto.CustomerId,
                    PaymentTermsId = itemDto.PaymentTermsId,
                    Lines = new List<SalesInvoiceLines>()
                };

                // Añado las líneas si existen
                if (itemDto.Lines != null && itemDto.Lines.Any())
                {
                    foreach (var lineDto in itemDto.Lines)
                    {
                        newEntity.Lines.Add(new SalesInvoiceLines
                        {
                            SalesInvoiceLineId = lineDto.SalesInvoiceLineId == Guid.Empty ? Guid.NewGuid() : lineDto.SalesInvoiceLineId,
                            SalesInvoiceHeaderId = itemDto.SalesInvoiceHeaderId,
                            ProductId = lineDto.ProductId,
                            TaxRateId = lineDto.TaxRateId,
                            UnitPrice = lineDto.UnitPrice,
                            Quantity = lineDto.Quantity,
                            CustomDescription = lineDto.CustomDescription
                        });
                    }
                }

                this.context.SalesInvoiceHeaders.Add(newEntity);
                this.context.SaveChanges();

                var itemToReturn = this.context.SalesInvoiceHeaders
                    .Include(h => h.Lines)
                    .Where(i => i.SalesInvoiceHeaderId == newEntity.SalesInvoiceHeaderId);

                return new ObjectResult(SingleResult.Create(itemToReturn))
                {
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }
        #endregion

        #region VERIFICACIÓN DE ENTIDAD EN FACTURAS
        // Compruebo si ya está en alguna factura
        [HttpGet("/odata/InvoicingSystem/SalesInvoiceHeaders({key})/IsInvoiced")]
        public IActionResult IsSalesInvoiceHeaderInvoiced(string key)
        {
            try
            {
                // Cuento cuántas líneas de factura usan esta entidad
                var invoiceCount = this.context.SalesInvoiceHeaders
                    .Count(line => line.SalesInvoiceHeaderId == key);

                return Ok(new { isInvoiced = invoiceCount > 0, invoiceCount = invoiceCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
