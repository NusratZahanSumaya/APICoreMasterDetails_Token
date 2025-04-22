using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Evi_practice.Models
{
    public class ModelContext:IdentityDbContext<ApplicationUser>
    {
        public ModelContext(DbContextOptions<ModelContext>op):base(op) 
        {
            
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sales>Sales {  get; set; }
        public DbSet<Details> Details { get; set; }
    }
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Sales
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public bool Status { get; set; }
        public string Pictures { get; set; }
        public List<Details> Details { get; set; }
    }
    public class Details
    {
        public int Id { get; set; }
        [ForeignKey(nameof(Product))]
        public int PId { get; set; }
        [ForeignKey(nameof(Sales))]
        public int Oid { get; set; }
        public double Price { get; set; }
        public Product Product { get; set; }
        public Sales Sales { get; set; }
    }

}
