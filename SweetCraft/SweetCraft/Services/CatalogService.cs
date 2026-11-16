using SweetCraft.Data;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SweetCraft.Services
{
    public sealed class CatalogService
    {
        public IList<Products> GetProducts()
        {
            using (var context = new SweetCraftDBEntities())
            {
                return context.Products.Include("Categories").Include("Fillings").Where(product => product.IsAvailable).OrderBy(product => product.Name).ToList();
            }
        }

        public IList<Categories> GetCategories()
        {
            using (var context = new SweetCraftDBEntities()) return context.Categories.OrderBy(category => category.SortOrder).ToList();
        }
    }
}
