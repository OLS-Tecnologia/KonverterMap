using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonverterMap.Test
{
    [TestClass]
    public class ReverseMapTests
    {
        [TestInitialize]
        public void Setup()
        {
            Konverter.Instance.CreateMap<User, UserDto>()
                .ForMember(dest => dest.WelcomeMessage, (src, map) =>
                    $"Hello {src.Name} from {src.Country}")
                .When(dest => dest.Name, src => !string.IsNullOrWhiteSpace(src.Name))
                .BeforeMap((src, dest) => { if (src.Country == null) src.Country = "Brazil"; })
                .AfterMap((src, dest) => dest.Age += 1)
                .ReverseMap();

            Konverter.Instance.CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Label, (src, map) =>
                    src.IsAvailable ? $"{src.Name} - In Stock" : $"{src.Name} - Out of Stock")
                .When(dest => dest.Price, src => src.Price > 0)
                .ReverseMap();

            Konverter.Instance.CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.TotalOrders, (src, map) =>
                    src.Orders?.Count ?? 0)
                .AfterMap((src, dest) => dest.LoyaltyPoints += 10)
                .ReverseMap();

            Konverter.Instance.CreateMap<Item, ItemDto>().ReverseMap();

            Konverter.Instance.CreateMap<Invoice, InvoiceDto>()
                .Ignore(dest => dest.Items) // intentionally skipping Items
                .ReverseMap();
        }

        [TestMethod]
        public void Should_Map_Forward_And_Reverse_With_Same_Config()
        {
            var user = new User
            {
                Name = "Fábio",
                Age = 30,
                Country = null
            };

            var dto = Konverter.Instance.Map<User, UserDto>(user);

            Assert.AreEqual("Hello Fábio from Brazil", dto.WelcomeMessage);
            Assert.AreEqual(31, dto.Age); // AfterMap

            var user2 = Konverter.Instance.Map<UserDto, User>(dto);
            Assert.AreEqual("Fábio", user2.Name);
            Assert.AreEqual(31, user2.Age); // stays the same
            Assert.AreEqual("Brazil", user2.Country); // was defaulted in BeforeMap
        }

        [TestMethod]
        public void Should_Map_Product_With_Condition_And_Custom_Label()
        {
            var product = new Product
            {
                Name = "Mouse",
                Price = 99.90m,
                IsAvailable = true,
                Category = "Peripherals"
            };

            var dto = Konverter.Instance.Map<Product, ProductDto>(product);
            Assert.AreEqual("Mouse - In Stock", dto.Label);
            Assert.AreEqual(99.90m, dto.Price);
            Assert.AreEqual("Peripherals", dto.Category);

            var reversed = Konverter.Instance.Map<ProductDto, Product>(dto);
            Assert.AreEqual("Mouse", reversed.Name); // restored
            Assert.AreEqual("Peripherals", reversed.Category); // maintained
        }

        [TestMethod]
        public void Should_Map_Customer_And_Apply_AfterMap()
        {
            var customer = new Customer
            {
                Name = "Joana",
                LoyaltyPoints = 50,
                Orders = new List<Order> { new(), new() }
            };

            var dto = Konverter.Instance.Map<Customer, CustomerDto>(customer);
            Assert.AreEqual("Joana", dto.Name);
            Assert.AreEqual(60, dto.LoyaltyPoints); // AfterMap added +10
            Assert.AreEqual(2, dto.TotalOrders);

            var reversed = Konverter.Instance.Map<CustomerDto, Customer>(dto);
            Assert.AreEqual("Joana", reversed.Name);
            Assert.AreEqual(60, reversed.LoyaltyPoints); // maintained
            Assert.AreEqual(0, reversed.Orders.Count); // not mapped back, expected
        }

        [TestMethod]
        public void Should_Ignore_List_Mapping_And_Not_Fail()
        {
            var invoice = new Invoice
            {
                Id = 123,
                Items = new List<Item>
                {
                    new() { Name = "A", Value = 100 },
                    new() { Name = "B", Value = 50 }
                }
            };

            var dto = Konverter.Instance.Map<Invoice, InvoiceDto>(invoice);

            Assert.AreEqual(123, dto.Id);
            Assert.AreEqual(0, dto.Items.Count); // ignored

            var reversed = Konverter.Instance.Map<InvoiceDto, Invoice>(dto);

            Assert.AreEqual(123, reversed.Id);
            Assert.AreEqual(0, reversed.Items.Count); // not mapped back
        }

    }

    #region Models
    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string? Country { get; set; }
    }

    public class UserDto
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string? Country { get; set; }
        public string? WelcomeMessage { get; set; }
    }
    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? Category { get; set; }
    }

    public class ProductDto
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public int LoyaltyPoints { get; set; }
        public List<Order> Orders { get; set; } = new();
    }

    public class CustomerDto
    {
        public string Name { get; set; }
        public int LoyaltyPoints { get; set; }
        public int TotalOrders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
    }

    public class Invoice
    {
        public int Id { get; set; }
        public List<Item> Items { get; set; } = new();
        public decimal Total => Items?.Sum(i => i.Value) ?? 0;
    }

    public class InvoiceDto
    {
        public int Id { get; set; }
        public List<ItemDto> Items { get; set; } = new();
    }

    public class Item
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }

    public class ItemDto
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }
    #endregion
}
