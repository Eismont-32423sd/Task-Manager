using Domain.Entities.DbEntities;

namespace Application.Services.Interfaces
{
    public interface IJwtGenerator
    {
        string CreateJwtToken(User user);
    }
}
