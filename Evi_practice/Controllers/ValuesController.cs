using Evi_practice.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.Design;

namespace Evi_practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    { 
        private readonly ModelContext db;
        private readonly IWebHostEnvironment env;
        public ValuesController(ModelContext modelContext,IWebHostEnvironment webHostEnvironment)
        {
          this.db = modelContext;
            env = webHostEnvironment;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sales>>>GetOrders()
        {
            var orders=await db.Sales.Include(p=>p.Details).ThenInclude(a=>a.Product).ToListAsync();
            var orderdata = orders.Select(p => new Sales
            {
                Id = p.Id,
                CustomerName = p.CustomerName,
                Status = p.Status,
                OrderDate = p.OrderDate,
                Pictures=p.Pictures,
                Details = p.Details.Select(a => new Details
                {
                    Id = a.Id,
                    PId = a.PId,
                    Oid = a.Oid,
                    Price = a.Price
                }).ToList()
            }).ToList();
            return Ok(orderdata);
        }
        [HttpPost]
        public IActionResult Post()
        {
            var requst = HttpContext.Request.Form.Files[0];
            var p = HttpContext.Request.Form["order"];
            var entity = JsonConvert.DeserializeObject<Sales>(p);
            if (requst != null)
            {
                var ext=Path.GetExtension(requst.FileName);
                var filename=entity.CustomerName + ext;
                var path=Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","Pictures", filename);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    requst.CopyTo(stream);
                }
                var order = new Sales
                {
                    CustomerName = entity.CustomerName,
                    Status = entity.Status,
                    OrderDate = entity.OrderDate,
                    Pictures = "/Pictures/" + filename,
                    Details = entity.Details
                };
                db.Sales.Add(order);
                db.SaveChanges();
               
            }
            return Ok();
        }
        [HttpPut]
        public IActionResult Put()
        {
            
            var p = HttpContext.Request.Form["order"];
            var entity = JsonConvert.DeserializeObject<Sales>(p);
            var exist=db.Sales.Find(entity.Id);
            if (HttpContext.Request.Form.Count > 0)
            {
                var requst = HttpContext.Request.Form.Files[0];
                if (requst != null)
                {
                    var ext = Path.GetExtension(requst.FileName);
                    var filename = entity.CustomerName + ext;
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures", filename);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        requst.CopyTo(stream);
                    }
                    exist.Pictures = "/Pictures/" + filename;


                }

            }
            else
            {
                exist.Pictures=entity.Pictures;
            }
            using(var transaction=db.Database.BeginTransaction())
            {
                exist.CustomerName=entity.CustomerName;
                exist.Id=entity.Id;
                exist.Status=entity.Status;
                exist.OrderDate=entity.OrderDate;
                exist.Details=entity.Details;
                db.Entry(exist).State=EntityState.Modified;
                if (db.SaveChanges() > 0)
                {
                    transaction.Commit();
                    return Ok(exist);
                }
                else
                {
                    transaction.Rollback();
                    return Problem("Update failed");
                }
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Sales>> GetbyId(int id)  
        {
            var p = await db.Sales.Include(p => p.Details).ThenInclude(a => a.Product).FirstOrDefaultAsync(p=>p.Id==id);
            if(p == null)return NotFound();
            var orderdata = new Sales
            {
                Id = p.Id,
                CustomerName = p.CustomerName,
                Status = p.Status,
                OrderDate = p.OrderDate,
                Pictures = p.Pictures,
                Details = p.Details.Select(a => new Details
                {
                    Id = a.Id,
                    PId = a.PId,
                    Oid = a.Oid,
                    Price = a.Price
                }).ToList()
            };
            return Ok(orderdata);
        }
        [HttpDelete]
        public async Task<ActionResult<Sales>> Delete(int id)
        {
            var p = await db.Sales.Include(p => p.Details).ThenInclude(a => a.Product).FirstOrDefaultAsync(p => p.Id == id);
            if (p == null) return NotFound();
            db.Sales.Remove(p);
            if (db.SaveChanges() > 0)
            {
                return Ok();
            }
            else
            {
                return Problem("Delete failed");
            }
        }
    }
}
