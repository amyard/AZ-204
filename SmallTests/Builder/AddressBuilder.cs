public class AddressBuilder
{
    private string _street;
    private string _city;
    private string? _state;
    private string _country;
    private string _postalCode;
    
    public AddressBuilder() {}
    public static AddressBuilder Empty() => new();

    public AddressBuilder WithStreet(string street)
    {
        _street = street;
        return this;
    }

    public AddressBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public AddressBuilder WithState(string state)
    {
        _state = state;
        return this;
    }

    public AddressBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    public AddressBuilder WithPostalCode(string postalCode)
    {
        _postalCode = postalCode;
        return this;
    }

    public Address Build()
    {
        return new Address()
        {
            Street = _street,
            City = _city,
            State = _state ?? "N/A",
            Country = _country,
            PostalCode = _postalCode
        };
    }
}
