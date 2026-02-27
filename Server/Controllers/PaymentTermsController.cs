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
    [Route("odata/InvoicingSystem/PaymentTerms")]
    public partial class PaymentTermsController : ODataController
    {
        private readonly InvoicingSystemDbContext context;

        // Inyectamos el DbContext 
        public PaymentTermsController(InvoicingSystemDbContext context)
        {
            this.context = context;
        }

        #region GET(todos)
        // --- 1. LISTAR TODOS (GET) - Soporta filtros de OData como $filter, $orderby, etc.
        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IEnumerable<PaymentTerms> GetPaymentTerms()
        {
            // AsNoTracking() hace que EF no "vigile" los objetos para que el listado vaya más rápido
            var items = this.context.PaymentTerms.AsNoTracking().AsQueryable();
            return items;
        }
        #endregion

        #region GET(1)
        // --- 2. OBTENER UNO POR ID (Para editar con doble click en este caso)
        [HttpGet("/odata/InvoicingSystem/PaymentTerms({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public SingleResult<PaymentTerms> GetPaymentTerms(Guid key)
        {
            var items = this.context.PaymentTerms.AsNoTracking().Where(i => i.PaymentTermsId == key);
            var result = SingleResult.Create(items);

            return result;
        }
        #endregion

        #region DELETE(1)
        // --- 3. BORRAR UNO POR ID
        [HttpDelete("/odata/InvoicingSystem/PaymentTerms({key})")]
        public IActionResult DeletePaymentTerms(Guid key)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState); // Si el modelo llega mal formado, se termina

                var item = this.context.PaymentTerms.Where(i => i.PaymentTermsId == key).FirstOrDefault();

                if (item == null) return NotFound("El término de pago no existe");  //Si no existe el cliente, fuera

                this.context.PaymentTerms.Remove(item);
                this.context.SaveChanges();

                return new NoContentResult(); // 204 No Content (Todo ok pero no devuelvo nada)
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 547 || sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                // Error 547: Restricción de clave foránea
                return Conflict(new { message = "No se puede eliminar este término de pago porque está siendo utilizado en una o más facturas." });
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
        [HttpPut("/odata/InvoicingSystem/PaymentTerms({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PutPaymentTerms(Guid key, [FromBody]PaymentTermsDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Compruebo que el ID de la URL coincida con el del objeto
                if (itemDto == null || (itemDto.PaymentTermsId != key)) return BadRequest();

                var entityToUpdate = this.context.PaymentTerms.FirstOrDefault(i => i.PaymentTermsId == key);
                if (entityToUpdate == null) return NotFound();

                // Mapeo Manual: Volcar datos del DTO a la Entidad
                entityToUpdate.Description = itemDto.Description;
                entityToUpdate.PaymentDays = itemDto.PaymentDays;


                this.context.PaymentTerms.Update(entityToUpdate);
                this.context.SaveChanges();

                var itemToReturn = this.context.PaymentTerms.Where(i => i.PaymentTermsId == key);
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
        [HttpPatch("/odata/InvoicingSystem/PaymentTerms({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PatchPaymentTerms(Guid key, [FromBody]Delta<PaymentTerms> patch)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var entityToUpdate = this.context.PaymentTerms.Where(i => i.PaymentTermsId == key).FirstOrDefault();

                if (entityToUpdate == null) return BadRequest();
                 
                patch.Patch(entityToUpdate);
                this.context.PaymentTerms.Update(entityToUpdate);
                this.context.SaveChanges();

                var itemToReturn = this.context.PaymentTerms.Where(i => i.PaymentTermsId == key);
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
        public IActionResult Post([FromBody] PaymentTermsDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (itemDto == null) return BadRequest();

                // MAPEO MANUAL: Creo una entidad nueva a partir de los datos del DTO
                var newEntity = new PaymentTerms
                {
                    PaymentTermsId = itemDto.PaymentTermsId,
                    Description = itemDto.Description,
                    PaymentDays = itemDto.PaymentDays
                };


                this.context.PaymentTerms.Add(newEntity);
                this.context.SaveChanges();

                var itemToReturn = this.context.PaymentTerms.Where(i => i.PaymentTermsId == newEntity.PaymentTermsId);

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
    }
}
