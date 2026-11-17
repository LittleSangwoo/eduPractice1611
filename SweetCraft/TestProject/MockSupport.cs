namespace TestProject;

public class MockProduct
{
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public bool IsInOrders { get; set; }

    public bool IsPremium => Price > 1000m;

    public string GetSafeImagePath()
    {
        return string.IsNullOrWhiteSpace(ImagePath)
            ? "images/products/no-image.png"
            : ImagePath;
    }
}

public static class MockProductValidator
{
    public static bool CanDeleteProduct(MockProduct product)
    {
        return product is not null && !product.IsInOrders;
    }
}

public class MockUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public int RoleId { get; set; }

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}".Trim()
        : $"{LastName} {FirstName} {MiddleName}".Trim();
}

public static class MockUserValidator
{
    public static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var normalized = phone.Trim();
        if (normalized.StartsWith("+"))
            normalized = normalized[1..];

        return normalized.Length == 11
            && normalized.All(char.IsDigit)
            && (normalized.StartsWith("7") || normalized.StartsWith("8") || normalized.StartsWith("9"));
    }

    public static bool IsEmailUnique(string email, List<string> existingEmails)
    {
        return !existingEmails.Any(existing => string.Equals(existing, email, StringComparison.OrdinalIgnoreCase));
    }
}

public static class MockCartService
{
    public static bool CanAddToCart(MockUser user)
    {
        return user is not null && user.RoleId != 1;
    }
}

public class MockCartItem
{
    public decimal BasePrice { get; set; }
    public decimal FillingExtraPrice { get; set; }
    public decimal DecorationExtraPrice { get; set; }
    public int Quantity { get; set; } = 1;

    public decimal ItemTotalPrice => (BasePrice + FillingExtraPrice + DecorationExtraPrice) * Quantity;
}

public class MockOrderItem
{
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;

    public decimal TotalPrice => UnitPrice * Quantity;
}

public class MockOrder
{
    public List<MockOrderItem> Items { get; } = new();
    public decimal DeliveryCost { get; set; }

    public decimal CalculateTotalAmount()
    {
        return Items.Sum(item => item.TotalPrice) + DeliveryCost;
    }
}

public static class MockOrderValidator
{
    public static bool IsValidReadyDate(DateTime createdAt, DateTime readyAt, int leadTimeHours)
    {
        return readyAt >= createdAt.AddHours(leadTimeHours);
    }
}
