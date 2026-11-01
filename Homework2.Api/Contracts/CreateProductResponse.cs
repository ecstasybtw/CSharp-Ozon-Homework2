namespace Homework2.Api.Contracts;

public class CreateProductResponse
{
    public Guid Identifier { get; set; }

    public CreateProductResponse(Guid identifier)
    {
        Identifier = identifier;
    }
}
