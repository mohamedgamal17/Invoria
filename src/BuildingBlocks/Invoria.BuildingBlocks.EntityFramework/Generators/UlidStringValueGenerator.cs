
using Invoria.BuildingBlocks.EntityFramework.Primitives;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Invoria.BuildingBlocks.EntityFramework.Generators
{
    public class UlidStringValueGenerator : ValueGenerator<string>
    {
        public override bool GeneratesTemporaryValues => false;

        public override string Next(EntityEntry entry)
            => UlidGenerator.NewUlid().ToString();
    }
}
