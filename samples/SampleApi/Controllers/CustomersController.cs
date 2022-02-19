using Microsoft.AspNetCore.Mvc;
using SampleApi.Models;

namespace SampleApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    [HttpGet]
    public CustomerDto GetExampleCustomer()
    {
        return CustomerBuilder.Init()
            .WithFirstName("Jarno")
            .WithLastName("IsTesting")
            .Build();
    }
}
