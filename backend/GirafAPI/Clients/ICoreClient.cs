namespace GirafAPI.Clients;

public interface ICoreClient
{
    Task<bool> ValidateCitizenAsync(int id, string accessToken);
    Task<bool> ValidateGradeAsync(int id, string accessToken);
    Task<bool> ValidatePictogramAsync(int id, string accessToken);
}
