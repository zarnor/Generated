
## Generated.Builders

Creates builders for simple classes.

```c#
using System;

public class Address
{
    public string StreetAddress { get; set; }
    public string City { get; set; }
}

public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<string> Titles { get; set; }
    public List<Address> Titles { get; set; }
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder { }

[FlexibleBuilder(typeof(Address))]
public partial class AddressBuilder { }

```

Creates a builder that can set individual properties and add elements to collections:

```c#
var customer = CustomerBuilder.Init()
    .WithFirstName("Test")
    .WithLastName("User")
    .WithTitles(new [] { "Dr", "Sgt" }) // Replace whole collection
    .AddAddress( // Add one item to collection
        AddressBuilder.Init()
        .WithStreetAddress("221B Baker Street")
        .WithCity("London")
        .Build())
    .Build();
```