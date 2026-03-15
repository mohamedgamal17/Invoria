namespace Invoria.BuildingBlocks.Domain.Dtos
{
    public class AuditedEntityDto<T> : EntityDto<T>
    {
        public DateTimeOffset CreatedAt { get;  set; }
        public string? CreatedBy { get;  set; }
        public DateTimeOffset? LastModifiedAt { get;  set; }
        public string? LastModifiedBy { get;  set; }
    }

    public class AuditedEntityDto : AuditedEntityDto<string>
    {

    }

}
