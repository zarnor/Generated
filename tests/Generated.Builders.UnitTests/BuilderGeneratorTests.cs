using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Xunit;

// https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators
using VerifyCS = Generated.Builders.UnitTests.CSharpSourceGeneratorVerifier<Generated.Builders.BuilderSourceGenerator>;

namespace Generated.Builders.UnitTests;

public class BuilderGeneratorTests
{
    [Fact]
    public async Task Builds_Simple_Dto()
    {
        var code = @"using System;
using Generated.Builders;

namespace TestProject.Models;

public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private string _firstName;
        private string _lastName;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithFirstName(string value)
        {
            _firstName = value;
            return this;
        }

        public CustomerBuilder WithLastName(string value)
        {
            _lastName = value;
            return this;
        }

        public Customer Build()
        {
            return new Customer()
            {
                FirstName = _firstName,
                LastName = _lastName
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Builds_Simple_Dto_With_Full_Attribute_Name()
    {
        var code = @"using System;
namespace TestProject.Models;

public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

[Generated.Builders.FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private string _firstName;
        private string _lastName;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithFirstName(string value)
        {
            _firstName = value;
            return this;
        }

        public CustomerBuilder WithLastName(string value)
        {
            _lastName = value;
            return this;
        }

        public Customer Build()
        {
            return new Customer()
            {
                FirstName = _firstName,
                LastName = _lastName
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Builds_Simple_Dto_With_Init_Accessors()
    {
        var code = @"using System;
using Generated.Builders;

// Needed for init accessors in .net before 5.0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}

namespace TestProject.Models
{
    public class Customer
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
    }

    [FlexibleBuilder(typeof(Customer))]
    public partial class CustomerBuilder
    {
    }
}
";
        var expected = @"using System;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private string _firstName;
        private string _lastName;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithFirstName(string value)
        {
            _firstName = value;
            return this;
        }

        public CustomerBuilder WithLastName(string value)
        {
            _lastName = value;
            return this;
        }

        public Customer Build()
        {
            return new Customer()
            {
                FirstName = _firstName,
                LastName = _lastName
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Ignores_Readonly_Property()
    {
        var code = @"using System;
using Generated.Builders;

namespace TestProject.Models;

public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => FirstName + LastName;
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private string _firstName;
        private string _lastName;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithFirstName(string value)
        {
            _firstName = value;
            return this;
        }

        public CustomerBuilder WithLastName(string value)
        {
            _lastName = value;
            return this;
        }

        public Customer Build()
        {
            return new Customer()
            {
                FirstName = _firstName,
                LastName = _lastName
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Builds_Collections()
    {
        var code = @"using System;
using System.Collections.Generic;
using Generated.Builders;

namespace TestProject.Models;

public class Customer
{
    public List<string> Names { get; set; }
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;
using System.Collections.Generic;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private List<string> _names;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithNames(IEnumerable<string> values)
        {
            _names = new List<string>(values);
            return this;
        }

        public CustomerBuilder AddToNames(string value)
        {
            if (_names == null)
            {
                _names = new List<string>();
            }

            _names.Add(value);

            return this;
        }

        public Customer Build()
        {
            return new Customer()
            {
                Names = _names
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Builds_ReadOnly_Collections()
    {
        var code = @"using System;
using System.Collections.Generic;
using Generated.Builders;

namespace TestProject.Models;

public class Customer
{
    public List<string> Names { get; }
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;
using System.Collections.Generic;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private List<string> _names;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithNames(IEnumerable<string> values)
        {
            _names = new List<string>(values);
            return this;
        }

        public CustomerBuilder AddToNames(string value)
        {
            if (_names == null)
            {
                _names = new List<string>();
            }

            _names.Add(value);

            return this;
        }

        public Customer Build()
        {
            var ret = new Customer();

            if (_names != default)
            {
                foreach (var element in _names)
                {
                    ret.Names.Add(element);
                }
            }

            return ret;
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Uses_Constructor_Args_For_Initialization()
    {
        var code = @"using System;
using Generated.Builders;

namespace TestProject.Models;

public class Customer
{
    public Customer(string firstName)
    {
        FirstName = firstName;
    }

    public string FirstName { get; }
    public string LastName { get; set; }
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private string _firstName;
        private string _lastName;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithFirstName(string value)
        {
            _firstName = value;
            return this;
        }

        public CustomerBuilder WithLastName(string value)
        {
            _lastName = value;
            return this;
        }

        public Customer Build()
        {
            return new Customer(_firstName)
            {
                LastName = _lastName
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Uses_Constructor_Args_Only_When_Setter_Defined_Also()
    {
        var code = @"using System;
using Generated.Builders;

namespace TestProject.Models;

public class Customer
{
    public Customer(string firstName)
    {
        FirstName = firstName;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private string _firstName;
        private string _lastName;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithFirstName(string value)
        {
            _firstName = value;
            return this;
        }

        public CustomerBuilder WithLastName(string value)
        {
            _lastName = value;
            return this;
        }

        public Customer Build()
        {
            return new Customer(_firstName)
            {
                LastName = _lastName
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Uses_Constructor_Args_For_Array()
    {
        var code = @"using System;
using System.Collections.Generic;
using System.Linq;
using Generated.Builders;

namespace TestProject.Models;

public class Customer
{
    public Customer(IEnumerable<string> names)
    {
        Names = names.ToArray();
    }

    public string[] Names { get; }
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;
using System.Collections.Generic;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private List<string> _names;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithNames(IEnumerable<string> values)
        {
            _names = new List<string>(values);
            return this;
        }

        public CustomerBuilder AddToNames(string value)
        {
            if (_names == null)
            {
                _names = new List<string>();
            }

            _names.Add(value);

            return this;
        }

        public Customer Build()
        {
            return new Customer(_names);
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Builds_Arrays()
    {
        var code = @"using System;
using System.Collections.Generic;
using Generated.Builders;

namespace TestProject.Models;

public class Customer
{
    public string[] Names { get; set; }
}

[FlexibleBuilder(typeof(Customer))]
public partial class CustomerBuilder
{
}
";
        var expected = @"using System;
using System.Collections.Generic;
using System.Linq;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private List<string> _names;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithNames(IEnumerable<string> values)
        {
            _names = new List<string>(values);
            return this;
        }

        public CustomerBuilder AddToNames(string value)
        {
            if (_names == null)
            {
                _names = new List<string>();
            }

            _names.Add(value);

            return this;
        }

        public Customer Build()
        {
            return new Customer()
            {
                Names = _names.ToArray()
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Adds_Namespace_For_Member_From_Different_Namespace()
    {
        var code = @"using System;
using System.Collections.Generic;
using Generated.Builders;

namespace TestProject.DataModels
{
    public class ProductData { }
}

namespace TestProject.Models
{
    using TestProject.DataModels;

    public class Product
    {
        public ProductData Data { get; set; }
    }

    [FlexibleBuilder(typeof(Product))]
    public partial class ProductBuilder
    {
    }
}
";
        var expected = @"using TestProject.DataModels;

namespace TestProject.Models
{
    public partial class ProductBuilder
    {
        private ProductData _data;

        private ProductBuilder()
        {
        }

        public static ProductBuilder Init()
        {
            return new ProductBuilder();
        }

        public ProductBuilder WithData(ProductData value)
        {
            _data = value;
            return this;
        }

        public Product Build()
        {
            return new Product()
            {
                Data = _data
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "ProductBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }


    [Fact]
    public async Task Adds_Namespace_For_Element_From_Different_Namespace()
    {
        var code = @"using System;
using System.Collections.Generic;
using Generated.Builders;

namespace TestProject.DataModels
{
    public class ProductData { }
}

namespace TestProject.Models
{
    using TestProject.DataModels;

    public class Product
    {
        public List<ProductData> Datas { get; set; }
    }

    [FlexibleBuilder(typeof(Product))]
    public partial class ProductBuilder
    {
    }
}
";
        var expected = @"using System.Collections.Generic;
using TestProject.DataModels;

namespace TestProject.Models
{
    public partial class ProductBuilder
    {
        private List<ProductData> _datas;

        private ProductBuilder()
        {
        }

        public static ProductBuilder Init()
        {
            return new ProductBuilder();
        }

        public ProductBuilder WithDatas(IEnumerable<ProductData> values)
        {
            _datas = new List<ProductData>(values);
            return this;
        }

        public ProductBuilder AddToDatas(ProductData value)
        {
            if (_datas == null)
            {
                _datas = new List<ProductData>();
            }

            _datas.Add(value);

            return this;
        }

        public Product Build()
        {
            return new Product()
            {
                Datas = _datas
            };
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "ProductBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }

    [Fact]
    public async Task Does_Not_Initialize_Member_With_Default_Value()
    {
        var code = @"using System;
using Generated.Builders;

// Needed for init accessors in .net before 5.0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}

namespace TestProject.Models
{
    public class Customer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; } = ""NO_LAST_NAME"";
    }

    [FlexibleBuilder(typeof(Customer))]
    public partial class CustomerBuilder
    {
    }
}
";
        var expected = @"using System;

namespace TestProject.Models
{
    public partial class CustomerBuilder
    {
        private string _firstName;
        private string _lastName;

        private CustomerBuilder()
        {
        }

        public static CustomerBuilder Init()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithFirstName(string value)
        {
            _firstName = value;
            return this;
        }

        public CustomerBuilder WithLastName(string value)
        {
            _lastName = value;
            return this;
        }

        public Customer Build()
        {
            var ret = new Customer()
            {
                FirstName = _firstName
            };

            if (_lastName != default)
            {
                ret.LastName = _lastName;
            }

            return ret;
        }
    }
}
";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(BuilderSourceGenerator), "FlexibleBuilderAttribute.g.cs", FlexibleBuilderAttribute.GetSourceCode()),
                    (typeof(BuilderSourceGenerator), "CustomerBuilder.g.cs", SourceText.From(expected, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                },
            },
        }.RunAsync();
    }
}
