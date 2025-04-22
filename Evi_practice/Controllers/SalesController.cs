using Evi_practice.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

namespace Evi_practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ModelContext db;
        private readonly IWebHostEnvironment env;
        public SalesController(ModelContext modelContext, IWebHostEnvironment webHostEnvironment)
        {
            this.db = modelContext;
            env = webHostEnvironment;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sales>>> GetOrders()
        {
            var ors = await db.Sales.Include(p => p.Details)
                .ThenInclude(p => p.Product).ToListAsync();
            var orderdata = ors.Select(p => new Sales
            {
                Id = p.Id,
                CustomerName = p.CustomerName,
                Status = p.Status,
                Pictures = p.Pictures,
                OrderDate = p.OrderDate,
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
            var rqst = HttpContext.Request.Form.Files[0];
            var p = HttpContext.Request.Form["order"];
            var entity = JsonConvert.DeserializeObject<Sales>(p);
            if (rqst != null)
            {
                var ext = Path.GetExtension(rqst.FileName);
                var filname = entity.CustomerName + ext;
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures", filname);
                using (var stram = new FileStream(filepath, FileMode.Create))
                {
                    rqst.CopyTo(stram);
                }
                var order = new Sales
                {
                    CustomerName = entity.CustomerName,
                    OrderDate = entity.OrderDate,
                    Status = entity.Status,
                    Pictures = "/Pictures/" + filname,
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
            var exist = db.Sales.Find(entity.Id);
            if (HttpContext.Request.Form.Count > 0)
            {
                var requestedfile = HttpContext.Request.Form.Files[0];
                if (requestedfile != null)
                {
                    var ext = Path.GetExtension(requestedfile.FileName);
                    var filename = entity.CustomerName + ext;
                    var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures", filename);
                    using (var stream = new FileStream(filepath, FileMode.Create))
                    {
                        requestedfile.CopyTo(stream);
                    }
                    exist.Pictures = "/Pictures/" + filename;
                }
            }
            else
            {
                exist.Pictures = entity.Pictures;
            }
            db.Details.RemoveRange(db.Details.Where(p => p.Oid == exist.Id));
            db.SaveChanges();
            using (var transaction = db.Database.BeginTransaction())
            {
                exist.CustomerName = entity.CustomerName;
                exist.OrderDate = entity.OrderDate;
                exist.Status = entity.Status;
                exist.Id = entity.Id;
                exist.Details = entity.Details;
                db.Entry(exist).State = EntityState.Modified;
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
            var p = await db.Sales.Include(p => p.Details).ThenInclude(p => p.Product).FirstOrDefaultAsync(p => p.Id == id);
            if (p == null) return NotFound();
            var order = new Sales
            {
                Id = p.Id,
                CustomerName = p.CustomerName,
                OrderDate = p.OrderDate,
                Status = p.Status,
                Pictures = p.Pictures,
                Details = p.Details.Select(a => new Details
                {
                    Id = a.Id,
                    PId = a.PId,
                    Oid = a.Oid,
                    Price = a.Price,
                }).ToList()
            };
            return Ok(order);


        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<Sales>> Delete(int id)
        {
            var p = await db.Sales.Include(p => p.Details).ThenInclude(p => p.Product).FirstOrDefaultAsync(p => p.Id == id);
            if (p == null) return NotFound();
            db.Sales.Remove(p);
            if (db.SaveChanges() > 0)
            {
                return Ok();
            }
            return Problem("delete failed");

        }


    }
}
