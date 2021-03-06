using System.Collections.Generic;
using System.Threading.Tasks;
using HelloWorld.DbItem;

namespace HelloWorld.Interfaces
{
    /// <summary>
    /// Defines the expected CRUD methods of database handlers
    /// </summary>
    public interface IDbHandler
    {
        Task<List<Person>> GetPeopleAsync();
        Task AddPersonAsync(string id, string name);
        Task<Person> DeletePersonAsync(string id);
        Task UpdatePersonAsync(string id, string newName);
    }
}