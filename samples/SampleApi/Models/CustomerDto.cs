using Generated.Builders;

namespace SampleApi.Models;

public class CustomerDto
{
    public string FirstName { get; set; }

    public string LastName { get; set; }
}

[FlexibleBuilder(typeof(CustomerDto))]
public partial class CustomerBuilder
{
}
