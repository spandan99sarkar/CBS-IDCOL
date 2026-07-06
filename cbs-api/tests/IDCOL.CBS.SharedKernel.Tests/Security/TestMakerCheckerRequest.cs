using IDCOL.CBS.SharedKernel.Security;

namespace IDCOL.CBS.SharedKernel.Tests.Security;

/// <summary>Minimal concrete subclass so the abstract MakerCheckerRequest&lt;T&gt; can be tested.</summary>
public sealed class TestMakerCheckerRequest : MakerCheckerRequest<string>
{
    private TestMakerCheckerRequest(Guid id, string payload, string makerUserId)
        : base(id, "TEST_REQUEST", payload, makerUserId)
    {
    }

    public static TestMakerCheckerRequest Create(string payload, string makerUserId) =>
        new(Guid.NewGuid(), payload, makerUserId);
}
