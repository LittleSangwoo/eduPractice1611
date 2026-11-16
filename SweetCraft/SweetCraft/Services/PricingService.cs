using SweetCraft.Data;

namespace SweetCraft.Services
{
    public static class PricingService
    {
        public static decimal GetProductPrice(Products product)
        {
            if (product == null) return 0m;
            return product.OldPrice.HasValue && product.DiscountPercent.HasValue
                ? decimal.Round(product.OldPrice.Value * (100m - product.DiscountPercent.Value) / 100m, 2)
                : product.Price;
        }

        public static decimal GetPriceWithOptions(Products product, Fillings filling, Decorations decoration)
        {
            return GetProductPrice(product) + (filling == null ? 0m : filling.ExtraPrice) + (decoration == null ? 0m : decoration.ExtraPrice);
        }
    }
}
