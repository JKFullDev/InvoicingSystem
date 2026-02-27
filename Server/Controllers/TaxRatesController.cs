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
    [Route("odata/InvoicingSystem/TaxRates")]
    public partial class TaxRatesController : ODataController
    {
        private readonly InvoicingSystemDbContext context;

        // Inyectamos el DbContext 
        public TaxRatesController(InvoicingSystemDbContext context)
        {
            this.context = context;
        }

        #region GET(todos)
        // --- 1. LISTAR TODOS (GET) - Soporta filtros de OData como $filter, $orderby, etc.
        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IEnumerable<TaxRates> GetTaxRates()
        {
            // AsNoTracking() hace que EF no "vigile" los objetos para que el listado vaya más rápido
            var items = this.context.TaxRates.AsNoTracking().AsQueryable();
            return items;
        }
        #endregion

        #region GET(1)
        // --- 2. OBTENER UNO POR ID (Para editar con doble click en este caso)
        [HttpGet("/odata/InvoicingSystem/TaxRates({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public SingleResult<TaxRates> GetTaxRates(Guid key)
        {
            var items = this.context.TaxRates.AsNoTracking().Where(i => i.TaxRateId == key);
            var result = SingleResult.Create(items);

            return result;
        }
        #endregion

        #region DELETE(1)
        // --- 3. BORRAR UNO POR ID
        [HttpDelete("/odata/InvoicingSystem/TaxRates({key})")]
        public IActionResult DeleteTaxRates(Guid key)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState); // Si el modelo llega mal formado, se termina

                var item = this.context.TaxRates.Where(i => i.TaxRateId == key).FirstOrDefault();

                if (item == null) return NotFound("El impuesto no existe");  //Si no existe, fuera

                this.context.TaxRates.Remove(item);
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
        // Permito editar productos facturados porque CurrentPrice es independiente de UnitPrice en facturas      [HttpPut("/odata/InvoicingSystem/TaxRates({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PutTaxRates(Guid key, [FromBody]TaxRatesDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Compruebo que el ID de la URL coincida con el del objeto
                if (itemDto == null || (itemDto.TaxRateId != key)) return BadRequest();

                var entityToUpdate = this.context.TaxRates.FirstOrDefault(i => i.TaxRateId == key);
                if (entityToUpdate == null) return NotFound();

                // Mapeo Manual: Volcar datos del DTO a la Entidad
                entityToUpdate.Name = itemDto.Name;
                entityToUpdate.Percentage = itemDto.Percentage;


                this.context.TaxRates.Update(entityToUpdate);
                this.context.SaveChanges();

                var itemToReturn = this.context.TaxRates.Where(i => i.TaxRateId == key);
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
        [HttpPatch("/odata/InvoicingSystem/TaxRates({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PatchTaxRates(Guid key, [FromBody]Delta<TaxRates> patch)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var entityToUpdate = this.context.TaxRates.Where(i => i.TaxRateId == key).FirstOrDefault();

                if (entityToUpdate == null) return BadRequest();
                 
                patch.Patch(entityToUpdate);
                this.context.TaxRates.Update(entityToUpdate);
                this.context.SaveChanges();

                var itemToReturn = this.context.TaxRates.Where(i => i.TaxRateId == key);
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
        public IActionResult Post([FromBody] TaxRatesDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (itemDto == null) return BadRequest();

                // MAPEO MANUAL: Creo una entidad nueva a partir de los datos del DTO
                var newEntity = new TaxRates
                {
                    TaxRateId = itemDto.TaxRateId,
                    Name = itemDto.Name,
                    Percentage = itemDto.Percentage,
                };


                this.context.TaxRates.Add(newEntity);
                this.context.SaveChanges();

                var itemToReturn = this.context.TaxRates.Where(i => i.TaxRateId == newEntity.TaxRateId);

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
        [HttpGet("/odata/InvoicingSystem/TaxRates({key})/IsInvoiced")]
        public IActionResult IsTaxRateInvoiced(Guid key)
        {
            try
            {
                // Cuento cuántas líneas de factura usan esta entidad
                var invoiceCount = this.context.SalesInvoiceLines
                    .Count(line => line.TaxRateId == key);

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
