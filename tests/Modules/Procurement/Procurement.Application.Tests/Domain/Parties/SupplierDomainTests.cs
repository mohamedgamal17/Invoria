using Invoria.Procurement.Domain.Parties;

namespace Invoria.Procurement.Application.Tests.Domain.Parties;

[TestFixture]
public class SupplierDomainTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_rejects_null_empty_or_whitespace_name(string? name)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Supplier.Create("sup-id", "SUP-001", name!, null, null, null));
        Assert.That(ex!.ParamName, Is.EqualTo("name"));
    }

    [Test]
    public void Create_rejects_empty_id()
    {
        Assert.Throws<ArgumentException>(() =>
            Supplier.Create("", "SUP-001", "Acme", null, null, null));
    }

    [Test]
    public void Create_rejects_empty_supplier_code()
    {
        Assert.Throws<ArgumentException>(() =>
            Supplier.Create("sup-id", " ", "Acme", null, null, null));
    }

    [Test]
    public void Create_sets_identity_audit_and_contact_fields()
    {
        var s = Supplier.Create("id-1", "SUP-99", "Acme Corp", "a@b.com", "+1", "creator");

        Assert.Multiple(() =>
        {
            Assert.That(s.Id, Is.EqualTo("id-1"));
            Assert.That(s.SupplierCode, Is.EqualTo("SUP-99"));
            Assert.That(s.Name, Is.EqualTo("Acme Corp"));
            Assert.That(s.ContactEmail, Is.EqualTo("a@b.com"));
            Assert.That(s.Phone, Is.EqualTo("+1"));
            Assert.That(s.CreatedBy, Is.EqualTo("creator"));
            Assert.That(s.CreatedAt, Is.Not.EqualTo(default(DateTimeOffset)));
        });
    }
}
