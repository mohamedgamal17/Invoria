using FluentValidation;

namespace Invoria.BuildingBlocks.Application.Requests
{
    public class PagingParams
    {
        public int Skip { get; set; } = 0;
        public int Length { get; set; } = 10;
    }

    public class PagingParamasValidator<T> : AbstractValidator<PagingParams> where T : PagingParams
    {
        public PagingParamasValidator()
        {
            RuleFor(x => x.Skip)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Length)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

        }
    }
}
