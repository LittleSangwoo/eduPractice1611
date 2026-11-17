namespace TestProject;

[TestClass]
public class ProductTests
{
    [TestMethod]
    public void Product_WithPriceOver1000_ReturnsIsPremiumTrue()
    {
        var product = new MockProduct { Price = 1250m };
        bool expected = true;
        bool actual = product.IsPremium;
        Assert.AreEqual(expected, actual, "Флаг премиум-товара установлен неверно!");
    }

    [TestMethod]
    public void Product_WithEmptyImage_ReturnsDefaultPicturePath()
    {
        var product = new MockProduct { ImagePath = "" };
        string expected = "images/products/no-image.png";
        string actual = product.GetSafeImagePath();
        Assert.AreEqual(expected, actual, "Путь к заглушке изображения товара неверный!");
    }
}
