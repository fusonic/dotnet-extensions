// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Common.Entities;
using Fusonic.Extensions.EntityFrameworkCore.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.EntityFrameworkCore.Tests;

public class QueryableExtensionsTests : IDisposable
{
    private readonly TestDbContext testDbContext;

    public QueryableExtensionsTests()
    {
        var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
                              .UseInMemoryDatabase(Guid.NewGuid().ToString())
                              .Options;

        testDbContext = new TestDbContext(dbContextOptions);
        var _ = CreateSampleEntity().Result;
        testDbContext.SaveChanges();
    }

    [Fact]
    public async Task IsRequired_Entity_Succeeds()
    {
        // Arrange
        var sampleEntity = await CreateSampleEntity();

        // Act
        var result = testDbContext.SampleDomainEntities.SingleOrDefault(s => s.Id == sampleEntity.Id).IsRequired();

        // Assert
        result.Should().Be(sampleEntity);
    }

    [Fact]
    public void IsRequired_Entity_ThrowsException()
    {
        // Act
        var errorAction = () => testDbContext.SampleDomainEntities.SingleOrDefault(s => s.Id == Guid.NewGuid()).IsRequired();

        // Assert
        errorAction.Should()
                   .Throw<EntityNotFoundException>()
                   .WithMessage($"Could not find entity of type '{nameof(SampleDomainEntity)}'.");
    }

    [Fact]
    public async Task IsRequiredAsync_Task_Succeeds()
    {
        // Arrange
        var sampleEntity = await CreateSampleEntity();

        // Act
        var result = await testDbContext.SampleDomainEntities.SingleOrDefaultAsync(s => s.Id == sampleEntity.Id, TestContext.Current.CancellationToken).IsRequiredAsync();

        // Assert
        result.Should().Be(sampleEntity);
    }

    [Fact]
    public async Task IsRequiredAsync_Task_ThrowsException()
    {
        // Act
        var errorAction = () => testDbContext.SampleDomainEntities.SingleOrDefaultAsync(s => s.Id == Guid.Empty).IsRequiredAsync();

        // Assert
        await errorAction.Should()
                         .ThrowAsync<EntityNotFoundException>()
                         .WithMessage($"Could not find entity of type '{nameof(SampleDomainEntity)}'.");
    }

    [Fact]
    public async Task IsRequiredAsync_DbSet_Succeeds()
    {
        // Arrange
        var sampleEntity = await CreateSampleEntity();

        // Act
        await testDbContext.SampleDomainEntities.IsRequiredAsync(sampleEntity.Id, TestContext.Current.CancellationToken);

        // Assert, throws no exception
    }

    [Fact]
    public async Task IsRequiredAsync_DbSet_ThrowsException()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var errorAction = () => testDbContext.SampleDomainEntities.IsRequiredAsync(guid);

        // Assert
        (await errorAction.Should().ThrowAsync<EntityNotFoundException>())
           .WithMessage($"Could not find entity of type '{nameof(SampleDomainEntity)}' with id {guid}.");
    }

    [Fact]
    public async Task IsRequiredAsync_Queryable_Succeeds()
    {
        // Arrange
        var sampleEntity = await CreateSampleEntity();

        // Act
        await testDbContext.SampleDomainEntities.Where(e => e.Id == sampleEntity.Id).IsRequiredAsync(TestContext.Current.CancellationToken);

        // Assert, throws no exception
    }

    [Fact]
    public async Task IsRequiredAsync_Queryable_ThrowsException()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var errorAction = () => testDbContext.SampleDomainEntities.Where(e => e.Id == guid).IsRequiredAsync();

        // Assert
        (await errorAction.Should().ThrowAsync<EntityNotFoundException>())
           .WithMessage($"Could not find entity of type '{nameof(SampleDomainEntity)}'.");
    }

    [Fact]
    public async Task SingleRequiredAsync_Succeeds()
    {
        // Act
        var result = await testDbContext.SampleDomainEntities.SingleRequiredAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(await testDbContext.SampleDomainEntities.FirstOrDefaultAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SingleRequiredAsync_LessThanOne_ThrowsException()
    {
        // Arrange
        testDbContext.SampleDomainEntities.RemoveRange(testDbContext.SampleDomainEntities);
        await testDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var errorAction = () => testDbContext.SampleDomainEntities.SingleRequiredAsync();

        // Assert
        await errorAction.Should()
                         .ThrowAsync<EntityNotFoundException>()
                         .WithMessage($"Could not find entity of type '{nameof(SampleDomainEntity)}'.");
    }

    [Fact]
    public async Task SingleRequiredAsync_MoreThanOne_ThrowsException()
    {
        // Arrange
        await CreateSampleEntity();

        // Act
        var errorAction = () => testDbContext.SampleDomainEntities.SingleRequiredAsync();

        // Assert
        await errorAction.Should()
                         .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SingleRequiredAsync_ById_Succeeds()
    {
        // Arrange
        var sampleEntity = await CreateSampleEntity();

        // Act
        var result = await testDbContext.SampleDomainEntities.SingleRequiredAsync(sampleEntity.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(sampleEntity);
    }

    [Fact]
    public async Task SingleRequiredAsync_ById_ThrowsException()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var errorAction = () => testDbContext.SampleDomainEntities.SingleRequiredAsync(guid);

        // Assert
        await errorAction.Should()
                         .ThrowAsync<EntityNotFoundException>()
                         .WithMessage($"Could not find entity of type '{nameof(SampleDomainEntity)}' with id {guid}.");
    }

    [Fact]
    public async Task SingleRequiredAsync_ByPredicate_Succeeds()
    {
        // Arrange
        var sampleEntity = await CreateSampleEntity();

        // Act
        var result = await testDbContext.SampleDomainEntities.SingleRequiredAsync(s => s.Id == sampleEntity.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(sampleEntity);
    }

    [Fact]
    public async Task SingleRequiredAsync_ByPredicate_ThrowsException()
    {
        // Act
        var errorAction = () => testDbContext.SampleDomainEntities.SingleRequiredAsync(s => s.Id == Guid.NewGuid());

        // Assert
        await errorAction.Should()
                         .ThrowAsync<EntityNotFoundException>()
                         .WithMessage($"Could not find entity of type '{nameof(SampleDomainEntity)}'.");
    }

    [Fact]
    public async Task FindRequiredAsync_Succeeds()
    {
        // Arrange
        var sampleEntity = await CreateSampleEntity();

        // Act
        var result = await testDbContext.SampleDomainEntities.FindRequiredAsync(sampleEntity.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(sampleEntity);
    }

    [Fact]
    public async Task FindRequiredAsync_ThrowsException()
    {
        // Act
        Func<Task> errorAction = () => testDbContext.SampleDomainEntities.FindRequiredAsync(Guid.NewGuid());

        // Assert
        (await errorAction.Should().ThrowAsync<EntityNotFoundException>())
           .WithMessage($"Could not find entity of type '{nameof(SampleDomainEntity)}'.");
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue()
    {
        // Arrange
        var sampleEntity = await CreateSampleEntity();

        // Act
        var result = await testDbContext.SampleDomainEntities.ExistsAsync(sampleEntity.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse()
    {
        // Act
        var result = await testDbContext.SampleDomainEntities.ExistsAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    private async Task<SampleDomainEntity> CreateSampleEntity()
    {
        var guid = Guid.NewGuid();
        var sampleEntity = new SampleDomainEntity(guid);
        await testDbContext.SampleDomainEntities.AddAsync(sampleEntity);
        await testDbContext.SaveChangesAsync();
        return sampleEntity;
    }

    public void Dispose()
    {
        testDbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}