using FluentMigrator;
using Homework2.Domain.Entities;

namespace Homework2.Infrastructure.Migrations;

[Migration(202611152211, "Create Product Table")]
public class CreateProductTableMigration : Migration
{
    public override void Up()
    {
        Create.Table("Product")
            .WithColumn(nameof(Product.Id)).AsGuid().PrimaryKey()
            .WithColumn(nameof(Product.Name)).AsString().NotNullable()
            .WithColumn(nameof(Product.PriceInKopecks)).AsInt64().NotNullable()
            .WithColumn(nameof(Product.ProductType)).AsInt32().NotNullable()
            .WithColumn(nameof(Product.WeightInKg)).AsFloat().NotNullable()
            .WithColumn(nameof(Product.WarehouseNumber)).AsInt32().NotNullable()
            .WithColumn(nameof(Product.DateCreated)).AsDateTimeOffset().NotNullable();
    }

    public override void Down()
    {
        if (Schema.Table("Product").Exists())
        {
            Delete.Table("Product");
        }
    }
}
