namespace TestProject;

[TestClass]
public class OrderAndCartTests
{
    [TestMethod]
    public void CartItem_WithExtras_CalculatesCorrectTotalPrice()
    {
        var cartItem = new MockCartItem
        {
            BasePrice = 1500m,
            FillingExtraPrice = 150m,
            DecorationExtraPrice = 300m,
            Quantity = 1
        };
        decimal expected = 1950m;
        decimal actual = cartItem.ItemTotalPrice;
        Assert.AreEqual(expected, actual, "Стоимость позиции с добавками рассчитана неверно!");
    }

    [TestMethod]
    public void Order_WithItemsAndDelivery_CalculatesCorrectTotalAmount()
    {
        var order = new MockOrder { DeliveryCost = 350m };
        order.Items.Add(new MockOrderItem { UnitPrice = 950m, Quantity = 2 });

        decimal expected = 2250m;
        decimal actual = order.CalculateTotalAmount();
        Assert.AreEqual(expected, actual, "Итоговая сумма заказа с учетом доставки посчитана неверно!");
    }

    [TestMethod]
    public void OrderValidation_ReadyAtBeforeLeadTime_ReturnsFalse()
    {
        DateTime createdAt = new DateTime(2026, 10, 10, 12, 0, 0);
        DateTime readyAt = new DateTime(2026, 10, 10, 18, 0, 0);
        int leadTimeHours = 24;

        bool isValid = MockOrderValidator.IsValidReadyDate(createdAt, readyAt, leadTimeHours);
        Assert.IsFalse(isValid, "Валидация пропустила дату получения меньше срока изготовления!");
    }

    [TestMethod]
    public void ProductDeletion_WhenProductInOrders_ReturnsFalse()
    {
        var productToDelete = new MockProduct { ProductId = 5, IsInOrders = true };
        bool canDelete = MockProductValidator.CanDeleteProduct(productToDelete);
        Assert.IsFalse(canDelete, "Система разрешила удалить товар, связанный с заказами!");
    }

}
