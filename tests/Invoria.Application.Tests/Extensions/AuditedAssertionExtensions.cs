using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Application.Tests.Extensions
{
    public static class AuditedAssertionExtensions
    {
        public static void AssertAudited(this AuditedEntityDto dto , IAuditedEntity entity)
        {
            dto.CreatedAt.Should().Be(entity.CreatedAt);
            dto.CreatedBy.Should().Be(entity.CreatedBy);
            dto.LastModifiedBy.Should().Be(entity.LastModifiedBy);
            dto.LastModifiedAt.Should().Be(entity.LastModifiedAt);
        }
    }
}
