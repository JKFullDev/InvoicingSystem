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
    [Route("/odata/InvoicingSystem/Customers")]
    public partial class CustomersController : ODataController
    {
        private readonly InvoicingSystem.Server.Data.InvoicingSystemDbContext context;

        // Inyectamos el DbContext 
        public CustomersController(InvoicingSystemDbContext context)
        {
            this.context = context;
        }

        #region GET(todos)
        // --- 1. LISTAR TODOS (GET) - Soporta filtros de OData como $filter, $orderby, etc.
        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IEnumerable<InvoicingSystem.Server.Data.Models.Customers> GetCustomers()
        {
            // AsNoTracking() hace que EF no "vigile" los objetos para que el listado vaya más rápido
            var items = this.context.Customers.AsNoTracking().AsQueryable<InvoicingSystem.Server.Data.Models.Customers>();
            return items;
        }
        #endregion

        #region GET(1)
        // --- 2. OBTENER UNO POR ID (Para editar con doble click en este caso)
        [HttpGet("/odata/InvoicingSystem/Customers(CustomerId={CustomerId})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public SingleResult<InvoicingSystem.Server.Data.Models.Customers> GetCustomers(string key)
        {
            var items = this.context.Customers.AsNoTracking().Where(i => i.CustomerId == key);
            var result = SingleResult.Create(items);

            return result;
        }
        #endregion

        #region DELETE(1)
        // --- 3. BORRAR UNO POR ID
        [HttpDelete("/odata/InvoicingSystem/Customers(CustomerId={CustomerId})")]
        public IActionResult DeleteCustomers(string key)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState); // Si el modelo llega mal formado, se termina

                var item = this.context.Customers.Where(i => i.CustomerId == key).FirstOrDefault();

                if (item == null) return BadRequest();  //Si no existe el cliente, fuera

                this.context.Customers.Remove(item);
                this.context.SaveChanges();

                return new NoContentResult(); // 204 No Content (Todo ok pero no devuelvo nada)
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
        [HttpPut("/odata/InvoicingSystem/Customers(CustomerId={CustomerId})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PutCustomers(string key, [FromBody]CustomersDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Compruebo que el ID de la URL coincida con el del objeto
                if (itemDto == null || (itemDto.CustomerId != key)) return BadRequest();

                var entityToUpdate = this.context.Customers.FirstOrDefault(i => i.CustomerId == key);
                if (entityToUpdate == null) return NotFound();

                // Mapeo Manual: Volcar datos del DTO a la Entidad
                entityToUpdate.Name = itemDto.Name;
                entityToUpdate.Address = itemDto.Address;
                entityToUpdate.City = itemDto.City;
                entityToUpdate.Nif = itemDto.Nif;


                this.context.Customers.Update(entityToUpdate);
                this.context.SaveChanges();

                var itemToReturn = this.context.Customers.Where(i => i.CustomerId == key);
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
        [HttpPatch("/odata/InvoicingSystem/Customers(CustomerId={CustomerId})")]
        [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
        public IActionResult PatchCustomers(string key, [FromBody]Delta<Customers> patch)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var entityToUpdate = this.context.Customers.Where(i => i.CustomerId == key).FirstOrDefault();

                if (entityToUpdate == null) return BadRequest();
                 
                patch.Patch(entityToUpdate);
                this.context.Customers.Update(entityToUpdate);
                this.context.SaveChanges();

                var itemToReturn = this.context.Customers.Where(i => i.CustomerId == key);
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
        public IActionResult Post([FromBody] CustomersDTO itemDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (itemDto == null) return BadRequest();

                // MAPEO MANUAL: Creo una entidad nueva a partir de los datos del DTO
                var newEntity = new Customers
                {
                    CustomerId = itemDto.CustomerId,
                    Name = itemDto.Name,
                    Address = itemDto.Address,
                    City = itemDto.City,
                    Nif = itemDto.Nif
                };


                this.context.Customers.Add(newEntity);
                this.context.SaveChanges();

                var itemToReturn = this.context.Customers.Where(i => i.CustomerId == newEntity.CustomerId);

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
