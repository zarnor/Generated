
# Generated projects

## Generated.Builders

Creates builders for simple classes.

```c#
public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

[FlexibleBuilder(typeof(CustomerDto))]
public partial class CustomerBuilder
{
}
```

Creates a builder that can be called like:

```c#
var customer = CustomerBuilder.Init()
    .WithFirstName("Test")
    .WithLastName("User")
    .Build();
```