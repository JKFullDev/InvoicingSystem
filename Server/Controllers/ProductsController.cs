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
    [Route("odata/InvoicingSystem/Products")]
    public partial class ProductsController : ODataController
    {
        private readonly InvoicingSystemDbContext context;

        // Inyectamos el DbContext 
        public ProductsController(InvoicingSystemDbContext context)
        {
            this.context = context;
        }

        #region GET(todos)
        // --- 1. LISTAR TODOS (GET) - Soporta filtros de OData como $filter, $orderby, etc.
        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IEnumerable<Products> GetProducts()
        {
            // AsNoTracking() hace que EF no "vigile" los objetos para que el listado vaya más rápido
            var items = this.context.Products.AsNoTracking().AsQueryable();
            return items;
        }
        #endregion

        #region GET(1)
        // --- 2. OBTENER UNO POR ID (Para editar con doble click en este caso)
        [HttpGet("/odata/InvoicingSystem/Products({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public SingleResult<Products> GetProducts(Guid key)
        {
            var items = this.context.Products.AsNoTracking().Where(i => i.ProductId == key);
            var result = SingleResult.Create(items);

            return result;
        }
        #endregion

        #region DELETE(1)
        // --- 3. BORRAR UNO POR ID
        [HttpDelete("/odata/InvoicingSystem/Products({key})")]
        public IActionResult DeleteProducts(Guid key)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState); // Si el modelo llega mal formado, se termina

                var item = this.context.Products.Where(i => i.ProductId == key).FirstOrDefault();

                if (item == null) return NotFound("El producto no existe");  //Si no existe, fuera

                this.context.Products.Remove(item);
                this.context.SaveChanges();

                return new NoContentResult(); // 204 No Content (Todo ok pero no devuelvo nada)
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 547 || sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                // Error 547: Restricción de clave foránea
                // Error 2601/2627: Violación de índice único
                return Conflict(new { message = "No se puede eliminar este producto porque está siendo utilizado en una o más facturas." });
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
        // Permito editar productos facturados porque CurrentPrice es independiente de UnitPrice en facturas
        [HttpPut("/odata/InvoicingSystem/Products({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PutProducts(Guid key, [FromBody]ProductsDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Compruebo que el ID de la URL coincida con el del objeto
                if (itemDto == null || (itemDto.ProductId != key)) return BadRequest();

                var entityToUpdate = this.context.Products.FirstOrDefault(i => i.ProductId == key);
                if (entityToUpdate == null) return NotFound();

                // Mapeo Manual: Volcar datos del DTO a la Entidad
                entityToUpdate.Name = itemDto.Name;
                entityToUpdate.Description = itemDto.Description;
                entityToUpdate.CurrentPrice = itemDto.CurrentPrice;


                this.context.Products.Update(entityToUpdate);
                this.context.SaveChanges();

                var itemToReturn = this.context.Products.Where(i => i.ProductId == key);
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
        [HttpPatch("/odata/InvoicingSystem/Products({key})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PatchProducts(Guid key, [FromBody]Delta<Products> patch)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var entityToUpdate = this.context.Products.Where(i => i.ProductId == key).FirstOrDefault();

                if (entityToUpdate == null) return BadRequest();
                 
                patch.Patch(entityToUpdate);
                this.context.Products.Update(entityToUpdate);
                this.context.SaveChanges();

                var itemToReturn = this.context.Products.Where(i => i.ProductId == key);
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
        public IActionResult Post([FromBody] ProductsDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (itemDto == null) return BadRequest();

                // MAPEO MANUAL: Creo una entidad nueva a partir de los datos del DTO
                var newEntity = new Products
                {
                    ProductId = itemDto.ProductId,
                    Name = itemDto.Name,
                    Description = itemDto.Description,
                    CurrentPrice = itemDto.CurrentPrice
                };


                this.context.Products.Add(newEntity);
                this.context.SaveChanges();

                var itemToReturn = this.context.Products.Where(i => i.ProductId == newEntity.ProductId);

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

        #region VERIFICACIÓN DE PRODUCTO EN FACTURAS
        // Compruebo si un producto ya está en alguna factura
        [HttpGet("/odata/InvoicingSystem/Products({key})/IsInvoiced")]
        public IActionResult IsProductInvoiced(Guid key)
        {
            try
            {
                // Cuento cuántas líneas de factura usan este producto
                var invoiceCount = this.context.SalesInvoiceLines
                    .Count(line => line.ProductId == key);

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
