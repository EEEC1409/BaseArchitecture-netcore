using Company.NameProject.Application.Common.Interfaces;

namespace Company.NameProject.Infrastructure.Services
{
    public class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
