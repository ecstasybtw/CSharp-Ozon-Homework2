using Homework2.Domain.Entities;

namespace Homework2.Api.Contracts;

public record CreateProductRequest
{
    public string Name { get; set; }
    public long PriceInKopecks { get; set; }
    public float WeightInKg { get; set; }
    public ProductType ProductType { get; set; }
    public int WarehouseNumber { get; set; }
}
