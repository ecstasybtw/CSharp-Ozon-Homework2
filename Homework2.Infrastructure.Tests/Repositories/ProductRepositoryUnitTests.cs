using Homework2.Infrastructure.Helpers;
using Homework2.Infrastructure.Repositories;
using Xunit;

namespace Homework2.Infrastructure.Tests.Repositories;

public class ProductRepositoryUnitTests
{
    // These checks must fail before any database request is made.
    private readonly ProductRepository _repository = new(
        new PostgresConnectionFactory("Host=localhost;Port=1;Database=test;Username=test;Password=test"));

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(-1, 20)]
    public async Task InvalidPageThrowsException(int page, int pageSize)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _repository.GetProductListAsync(page: page, pageSize: pageSize));
    }

    [Fact]
    public async Task InvalidDatesThrowException()
    {
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetProductListAsync(createdFrom: date, createdTo: date.AddDays(-1)));
    }

    [Fact]
    public async Task EmptyUpdateThrowsException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.UpdateProductAsync(Guid.NewGuid()));
    }
}
