namespace TestProject;

[TestClass]
public class UserValidationTests
{
    [TestMethod]
    public void User_FullName_CombinesNamesWithoutMiddleNameCorrectly()
    {
        var user = new MockUser
        {
            LastName = "Кузнецова",
            FirstName = "Анна",
            MiddleName = null
        };
        string expected = "Кузнецова Анна";
        string actual = user.FullName;
        Assert.AreEqual(expected, actual, "ФИО при отсутствии отчества сформировано неверно!");
    }

    [TestMethod]
    public void UserValidation_CorrectPhoneNumber_ReturnsTrue()
    {
        string phone = "+79001110004";
        bool isValid = MockUserValidator.IsValidPhone(phone);
        Assert.IsTrue(isValid, "Корректный номер телефона был отклонен валидатором!");
    }

    [TestMethod]
    public void CartService_WhenUserIsGuest_BlocksAddToCart()
    {
        var currentUser = new MockUser { RoleId = 1 }; // 1 - Гость
        bool canAdd = MockCartService.CanAddToCart(currentUser);
        Assert.IsFalse(canAdd, "Гостю разрешено добавление в корзину без авторизации!");
    }

    [TestMethod]
    public void UserValidation_DuplicateEmail_ReturnsFalse()
    {
        var existingEmails = new List<string> { "admin@sweetcraft.ru", "maria.m@sweetcraft.ru" };
        bool isUnique = MockUserValidator.IsEmailUnique("maria.m@sweetcraft.ru", existingEmails);
        Assert.IsFalse(isUnique, "Дубликат Email не был заблокирован при регистрации!");
    }
}
