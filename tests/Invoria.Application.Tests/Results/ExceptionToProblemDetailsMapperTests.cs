using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Infrastructure.Results;

namespace Invoria.Application.Tests.Results;

[TestFixture]
public class ExceptionToProblemDetailsMapperTests
{
    [Test]
    public void Map_includes_all_messages_for_BusinessValidationException()
    {
        var exception = new BusinessValidationException(
            "First error.",
            "Second error.");

        var problem = ExceptionToProblemDetailsMapper.Map(exception);

        problem.Status.Should().Be(422);
        problem.ErrorCode.Should().Be(BusinessValidationException.DefaultCode);
        problem.Errors.Should().ContainKey("messages");
        problem.Errors["messages"].Should().Equal("First error.", "Second error.");
    }
}
