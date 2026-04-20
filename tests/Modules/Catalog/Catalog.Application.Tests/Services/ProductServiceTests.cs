using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Catalog.Application.Tests.Assertions;
using Invoria.Catalog.Application.Tests.Products;
using Invoria.Catalog.Contracts.Services;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Application.Tests.Services
{
    [TestFixture]
    public class ProductServiceTests : ProductTestFixture
    {
        private readonly IProductService _productService;
        private readonly ICatalogRepository<Product> _productRepository;
        private readonly IInventoryRepository<Batch> _batchRepository;

        public ProductServiceTests()
        {
            _productService = ServiceProvider.GetRequiredService<IProductService>();
            _productRepository = ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
            _batchRepository = ServiceProvider.GetRequiredService<IInventoryRepository<Batch>>();
        }

        [Test]
        public async Task GetProductByIdAsync_should_return_product_when_found()
        {
            var product = new Product("Test Product", "TEST-CODE", 10);
            await _productRepository.Add(product);
            await _batchRepository.Add(new Batch(product.Id, 8, 10m));

            var result = await _productService.GetProductByIdAsync(product.Id);

            result.ShouldBeSuccess();
            result.Value!.AssertProductDto(product, 8, 0);
        }

        [Test]
        public async Task GetProductByIdAsync_should_return_failure_when_product_not_found()
        {
            var nonExistentId = Guid.NewGuid().ToString();

            var result = await _productService.GetProductByIdAsync(nonExistentId);

            result.IsSuccess.Should().BeFalse();
            result.Exception.Should().NotBeNull();
            result.Exception.Should().BeOfType<NotFoundException>();
        }

        [Test]
        public async Task ListProductsByIdsAsync_should_return_all_matching_products()
        {
            var a = new Product("A", "A-CODE", 1);
            var b = new Product("B", "B-CODE", 2);
            await _productRepository.Add(a);
            await _productRepository.Add(b);
            await _batchRepository.Add(new Batch(a.Id, 3, 10m));
            await _batchRepository.Add(new Batch(b.Id, 7, 10m));

            var result = await _productService.ListProductsByIdsAsync(new[] { a.Id, b.Id });

            result.ShouldBeSuccess();
            result.Value.Should().HaveCount(2);
            var byId = result.Value!.ToDictionary(x => x.Id);
            byId[a.Id].AssertProductDto(a, 3, 0);
            byId[b.Id].AssertProductDto(b, 7, 0);
        }

        [Test]
        public async Task ListProductsByIdsAsync_should_omit_missing_ids()
        {
            var product = new Product("Only", "ONLY", 1);
            await _productRepository.Add(product);
            var missingId = Guid.NewGuid().ToString();

            var result = await _productService.ListProductsByIdsAsync(new[] { product.Id, missingId });

            result.ShouldBeSuccess();
            result.Value.Should().ContainSingle();
            result.Value![0].AssertProductDto(product);
        }

        [Test]
        public async Task ListProductsByIdsAsync_should_return_empty_when_no_ids()
        {
            var result = await _productService.ListProductsByIdsAsync(Array.Empty<string>());

            result.ShouldBeSuccess();
            result.Value.Should().BeEmpty();
        }

        [Test]
        public async Task ListProductsByIdsAsync_should_return_empty_when_all_ids_whitespace()
        {
            var result = await _productService.ListProductsByIdsAsync(new[] { "", "   ", "\t" });

            result.ShouldBeSuccess();
            result.Value.Should().BeEmpty();
        }

        [Test]
        public async Task ListProductsByIdsAsync_should_deduplicate_ids()
        {
            var product = new Product("One", "ONE", 1);
            await _productRepository.Add(product);

            var result = await _productService.ListProductsByIdsAsync(new[] { product.Id, product.Id });

            result.ShouldBeSuccess();
            result.Value.Should().ContainSingle();
            result.Value![0].AssertProductDto(product);
        }
    }
}
