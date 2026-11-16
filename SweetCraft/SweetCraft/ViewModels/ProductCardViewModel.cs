using SweetCraft.Data;
using SweetCraft.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace SweetCraft.ViewModels
{
    public sealed class ProductCardViewModel
    {
        public ProductCardViewModel(Products product)
        {
            Product = product;
        }

        public Products Product { get; }
        public string Name => Product.Name;
        public string Description => Product.Description;
        public string CategoryName => Product.Categories == null ? string.Empty : Product.Categories.Name;
        public string ImagePath => ImageService.GetAbsoluteProductImagePath(Product.ImagePath);
        public decimal CurrentPrice => PricingService.GetProductPrice(Product);
        public decimal? PreviousPrice => Product.OldPrice.HasValue && Product.DiscountPercent.HasValue ? Product.OldPrice : (decimal?)null;
        public bool IsPremium => CurrentPrice > 1000m;
        public Brush CardBackground => IsPremium ? new SolidColorBrush(Color.FromRgb(255, 239, 218)) : Brushes.White;
        public FontWeight PriceWeight => IsPremium ? FontWeights.Bold : FontWeights.Normal;
        public string PremiumLabel => IsPremium ? "ПРЕМИУМ" : string.Empty;
    }
}
