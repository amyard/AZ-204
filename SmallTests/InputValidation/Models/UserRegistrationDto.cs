namespace InputValidation.Models;

public class UserRegistrationDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public PersonalInfo? PersonalInfo { get; set; }
    public AddressInfo? Address { get; set; }
    public List<string> Interests { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; }
    public bool AcceptTerms { get; set; }
}

public class PersonalInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? MiddleName { get; set; }
    public string PreferredName { get; set; }
}

public class AddressInfo
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}
