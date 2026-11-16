using System.Collections.Concurrent;
using Dapper;
using Homework2.Application.Interfaces;
using Homework2.Domain.Entities;
using Homework2.Infrastructure.Helpers;

namespace Homework2.Infrastructure.Repositories;

public class ProductRepository(IPostgresConnectionFactory postgresConnectionFactory) : IProductRepository
{

    public async Task<Product?> GetProductByIdAsync(Guid identifier)
    {
        await using var connection = postgresConnectionFactory.GetConnection();

        var sqlQuery = "SELECT * FROM Product " +
                       "WHERE id = @identifier";

        var result = await connection.QueryFirstOrDefaultAsync<Product>(sqlQuery, new { identifier });

        return result;
    }

    public async Task<IEnumerable<Product>> GetProductListAsync(
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        ProductType? productType = null,
        int? warehouseNumber = null,
        int page = 1,
        int pageSize = 20
    )
    {
        await using var connection = postgresConnectionFactory.GetConnection();

        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        if (createdFrom.HasValue && createdTo.HasValue && createdFrom.Value >= createdTo.Value)
        {
            throw new ArgumentException("createdFrom must be before createdTo.", nameof(createdTo));
        }

        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (createdFrom.HasValue)
        {
            conditions.Add("datecreated >= @createdFrom");
            parameters.Add("createdFrom", createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            conditions.Add("datecreated < @createdTo");
            parameters.Add("createdTo", createdTo.Value);
        }

        if (productType != null)
        {
            conditions.Add("producttype = @productType");
            parameters.Add("productType", (int)productType.Value);
        }

        if (warehouseNumber != null)
        {
            conditions.Add("warehousenumber = @warehouseNumber");
            parameters.Add("warehouseNumber", warehouseNumber.Value);
        }

        var sqlQuery = "SELECT id, name, priceinkopecks, producttype, weightinkg, warehousenumber, datecreated FROM product";

        if (conditions.Count > 0)
        {
            sqlQuery += " WHERE " + string.Join(" AND ", conditions);
        }

        sqlQuery += " ORDER BY datecreated, id LIMIT @pageSize OFFSET @offset";
        parameters.Add("pageSize", pageSize);
        parameters.Add("offset", ((long)page - 1) * pageSize);

        return await connection.QueryAsync<Product>(sqlQuery, parameters);
    }


    public async Task<Guid> CreateProductAsync(
        Product product
    )

    {
        await using var connection = postgresConnectionFactory.GetConnection();

        var sqlQuery = "INSERT INTO Product (id, name, priceinkopecks, producttype, weightinkg, warehousenumber, datecreated) " +
                        "VALUES (@id, @name, @priceinkopecks, @producttype, @weightinkg, @warehousenumber, @datecreated)";

        await connection.ExecuteAsync(sqlQuery, param: new
        {
            id = product.Id,
            name = product.Name,
            priceinkopecks = product.PriceInKopecks,
            producttype = product.ProductType,
            weightinkg = product.WeightInKg,
            warehousenumber = product.WarehouseNumber,
            datecreated = product.DateCreated
        });

        return product.Id;

    }


    public async Task<Product?> UpdateProductAsync(
        Guid identifier,
        string? name = null,
        long? priceInKopecks = null,
        float? weightInKg = null,
        ProductType? productType = null,
        int? warehouseNumber = null
    )
    {
        var assignments = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("identifier", identifier);

        if (name != null)
        {
            assignments.Add("name = @name");
            parameters.Add("name", name);
        }

        if (priceInKopecks.HasValue)
        {
            assignments.Add("priceinkopecks = @priceInKopecks");
            parameters.Add("priceInKopecks", priceInKopecks.Value);
        }

        if (weightInKg.HasValue)
        {
            assignments.Add("weightinkg = @weightInKg");
            parameters.Add("weightInKg", weightInKg.Value);
        }

        if (productType.HasValue)
        {
            assignments.Add("producttype = @productType");
            parameters.Add("productType", (int)productType.Value);
        }

        if (warehouseNumber.HasValue)
        {
            assignments.Add("warehousenumber = @warehouseNumber");
            parameters.Add("warehouseNumber", warehouseNumber.Value);
        }

        if (assignments.Count == 0)
        {
            throw new ArgumentException("At least one field must be provided for update.");
        }

        var sqlQuery = "UPDATE product SET " + string.Join(", ", assignments)
            + " WHERE id = @identifier"
            + " RETURNING id, name, priceinkopecks, producttype, weightinkg, warehousenumber, datecreated";

        await using var connection = postgresConnectionFactory.GetConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(sqlQuery, parameters);
    }

}
