namespace Homework2.Domain.Entities;

public enum ProductType
{
    General = 0,
    HouseholdChemicals = 1,
    Appliance = 2,
    Groceries = 3
}

public class Product
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public long PriceInKopecks { get; init; }
    public float WeightInKg { get; init; }
    public ProductType ProductType { get; init; }
    public int WarehouseNumber { get; init; }
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;

}
