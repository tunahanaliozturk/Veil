using FluentAssertions;
using Moongazing.Veil.Patterns;

namespace Moongazing.Veil.Tests.ObjectMasking;

public sealed class NestedObjectTests
{
    [Fact]
    public void MaskObject_NestedObject_MasksNestedProperties()
    {
        var order = new OrderDto
        {
            OrderId = "ORD-123",
            Customer = new CustomerDto
            {
                Name = "Jane Doe",
                Email = "jane@example.com"
            }
        };

        var masked = Veil.MaskObject(order);

        masked.OrderId.Should().Be("ORD-123");
        masked.Customer.Should().NotBeNull();
        masked.Customer.Email.Should().Contain("*");
        masked.Customer.Email.Should().Contain("@");
        masked.Customer.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public void MaskObject_NullNestedObject_HandledGracefully()
    {
        var order = new OrderDto
        {
            OrderId = "ORD-123",
            Customer = null!
        };

        var act = () => Veil.MaskObject(order);
        act.Should().NotThrow();

        var masked = Veil.MaskObject(order);
        masked.Customer.Should().BeNull();
    }

    [Fact]
    public void MaskObject_CollectionOfObjects_MasksEachItem()
    {
        var report = new ReportDto
        {
            Title = "Q1 Report",
            Contacts = new List<CustomerDto>
            {
                new() { Name = "Alice", Email = "alice@test.com" },
                new() { Name = "Bob", Email = "bob@test.com" }
            }
        };

        var masked = Veil.MaskObject(report);

        masked.Title.Should().Be("Q1 Report");
        masked.Contacts.Should().HaveCount(2);

        foreach (var contact in masked.Contacts)
        {
            contact.Email.Should().Contain("*");
            contact.Email.Should().Contain("@");
        }
    }

    [Fact]
    public void MaskObject_OriginalNestedObject_IsNotModified()
    {
        var order = new OrderDto
        {
            OrderId = "ORD-123",
            Customer = new CustomerDto
            {
                Name = "Jane Doe",
                Email = "jane@example.com"
            }
        };

        _ = Veil.MaskObject(order);

        order.Customer.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void MaskObject_EmptyCollection_HandledGracefully()
    {
        var report = new ReportDto
        {
            Title = "Empty",
            Contacts = new List<CustomerDto>()
        };

        var masked = Veil.MaskObject(report);
        masked.Contacts.Should().BeEmpty();
    }

    private sealed class CustomerDto
    {
        public string Name { get; set; } = string.Empty;

        [Veiled(VeilPattern.Email)]
        public string Email { get; set; } = string.Empty;
    }

    private sealed class OrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        public CustomerDto Customer { get; set; } = null!;
    }

    private sealed class ReportDto
    {
        public string Title { get; set; } = string.Empty;
        public List<CustomerDto> Contacts { get; set; } = [];
    }
}
