Console.WriteLine("Hello, World!");

var order = OrderBuilder.Empty()
    .WithNumber(10)
    .WithCreatedOn(DateTime.UtcNow)
    .WithShippingAddress(x => x
        .WithStreet("Baker Street")
        .WithCity("London")
        .WithCountry("United States")
        .WithPostalCode("12345"))
    .Build();

Console.WriteLine(order);
