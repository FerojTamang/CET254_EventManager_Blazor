using CET254_EventManager.Data;
using CET254_EventManager.Models;
using CET254_EventManager.Repositories;

namespace CET254_EventManager.Tests.TestHelpers;

public static class TestRepositoryFactory
{
    public static Repository<T> Create<T>(ApplicationDbContext context)
        where T : BaseEntity
    {
        return new Repository<T>(context);
    }
}
