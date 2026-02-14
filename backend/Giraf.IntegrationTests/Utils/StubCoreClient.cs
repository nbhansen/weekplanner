using GirafAPI.Clients;

namespace Giraf.IntegrationTests.Utils;

/// <summary>
/// Stub CoreClient for integration tests.
/// Returns true for IDs 1-100, false for anything else.
/// </summary>
public class StubCoreClient : ICoreClient
{
    private static bool IsKnownId(int id) => id >= 1 && id <= 100;

    public Task<bool> ValidateCitizenAsync(int id, string accessToken) =>
        Task.FromResult(IsKnownId(id));

    public Task<bool> ValidateGradeAsync(int id, string accessToken) =>
        Task.FromResult(IsKnownId(id));

    public Task<bool> ValidatePictogramAsync(int id, string accessToken) =>
        Task.FromResult(IsKnownId(id));
}
