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

        // Inyectamos el DbContext 
        public SalesInvoiceHeadersController(InvoicingSystemDbContext context)
        {
            this.context = context;
        }

        #region GET(todos)
        // --- 1. LISTAR TODOS (GET) - Soporta filtros de OData como $filter, $orderby, etc.
        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IEnumerable<SalesInvoiceHeaders> GetSalesInvoiceHeaders()
        {
            // AsNoTracking() hace que EF no "vigile" los objetos para que el listado vaya más rápido
            var items = this.context.SalesInvoiceHeaders.AsNoTracking().AsQueryable();
            return items;
        }
        #endregion

        #region GET(1)
        // --- 2. OBTENER UNO POR ID (Para editar con doble click en este caso)
        [HttpGet("/odata/InvoicingSystem/SalesInvoiceHeaders({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public SingleResult<SalesInvoiceHeaders> GetSalesInvoiceHeaders(string key)
        {
            var items = this.context.SalesInvoiceHeaders.AsNoTracking().Where(i => i.SalesInvoiceHeaderId == key);
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
        // --- 4. ACTUALIZAR TODO (PUT)
        // Permito editar productos facturados porque CurrentPrice es independiente de UnitPrice en facturas      [HttpPut("/odata/InvoicingSystem/SalesInvoiceHeaders({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PutSalesInvoiceHeaders(string key, [FromBody]SalesInvoiceHeadersDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Compruebo que el ID de la URL coincida con el del objeto
                if (itemDto == null || (itemDto.SalesInvoiceHeaderId != key)) return BadRequest();

                var entityToUpdate = this.context.SalesInvoiceHeaders.FirstOrDefault(i => i.SalesInvoiceHeaderId == key);
                if (entityToUpdate == null) return NotFound();

                // Mapeo Manual: Volcar datos del DTO a la Entidad
                entityToUpdate.SalesInvoiceHeaderId = itemDto.SalesInvoiceHeaderId;
                entityToUpdate.CustomerReference = itemDto.CustomerReference;
                entityToUpdate.InvoiceDate = itemDto.InvoiceDate;
                entityToUpdate.DueDate = itemDto.DueDate;
                entityToUpdate.QuoteReference = itemDto.QuoteReference;
                entityToUpdate.CustomerId = itemDto.CustomerId;
                entityToUpdate.PaymentTermsId = itemDto.PaymentTermsId;


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
        // --- 6. CREAR (POST)
        [HttpPost]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult Post([FromBody] SalesInvoiceHeadersDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (itemDto == null) return BadRequest();

                // MAPEO MANUAL: Creo una entidad nueva a partir de los datos del DTO
                var newEntity = new SalesInvoiceHeaders
                {

                    SalesInvoiceHeaderId = itemDto.SalesInvoiceHeaderId,
                    CustomerReference = itemDto.CustomerReference,
                    InvoiceDate = itemDto.InvoiceDate,
                    DueDate = itemDto.DueDate,
                    QuoteReference = itemDto.QuoteReference,
                    Customer = null!,
                    CustomerId = itemDto.CustomerId,
                    PaymentTermsId = itemDto.PaymentTermsId,
                    PaymentTerms = null!

                };


                this.context.SalesInvoiceHeaders.Add(newEntity);
                this.context.SaveChanges();

                var itemToReturn = this.context.SalesInvoiceHeaders.Where(i => i.SalesInvoiceHeaderId == newEntity.SalesInvoiceHeaderId);

                // Devuelvo 201 Created con el objeto nuevo
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
