using System.Diagnostics;

[DebuggerDisplay("Order with number {Number} and date {CreatedOn}")]
public class Order
{
    public int Number { get; set; }
    public DateTime CreatedOn { get; set; }
    public Address ShippingAddress { get; set; }
}

[DebuggerDisplay("Address: {Street}, {City}, {State}, {Country}, {PostalCode}")]
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}
