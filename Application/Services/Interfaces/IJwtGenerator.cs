using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IJwtGenerator
    {
        string CreateJwtToken(User user);
    }
}
