# Generated

## Generated.Builders

Creates builders for simple classes.

### Features

- Adds a WithProperty() funtion for each settable property and constructor argument.
- Injects constructor arguments on Build()
- Sets class properties (set or init) on Build()
- Collections can be fully replaced (.WithCollection()) or added one by one (.AddToCollection())

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
    public List<Address> Addresses { get; set; }
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
    .AddToAddresses( // Add one item to collection
        AddressBuilder.Init()
        .WithStreetAddress("221B Baker Street")
        .WithCity("London")
        .Build())
    .Build();
```

## Generated.Godot

Adds _Ready() method that gets all members with [GetNode] -attribute.

### Features

- Adds a _Ready() method that calls GetNode<T>() for all members with [GetNode] -attribute.

```c#
using System;
using Godot;

public partial class MyScene : Node2D
{
    [GetNode("MyLabel")]
    private Label _myLabel;

    // Optional
    private void Ready()
    {
        // My code
    }
}

```

Generates:

```c#
public partial class MyScene
{
    public override void _Ready()
    {
        _myLabel = GetNode<Label>("MyLabel");

        // Optional call to Ready() method.
        Ready();
    }
}
```
