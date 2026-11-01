using Homework2.Domain.Entities;

namespace Homework2.Api.Contracts;

public record GetProductListRequest
{
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public ProductType? ProductType { get; set; }
    public int? WarehouseNumber { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
