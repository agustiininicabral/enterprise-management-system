using EnterpriseManagement.Api.Domain;
namespace EnterpriseManagement.Api.Services;
public interface ITokenService { (string Token, DateTime ExpiresAt) CreateToken(User user); }
