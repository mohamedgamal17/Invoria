namespace Invoria.BuildingBlocks.Domain.Dtos
{
    public abstract class EntityDto<T>
    {
        public string Id { get; set; }
    }

    public abstract class EntityDto : EntityDto<string> { }
}
