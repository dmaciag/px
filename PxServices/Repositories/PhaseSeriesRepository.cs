using MySql.Data.MySqlClient;
using PxServices.Interfaces;
using PxServices.Models;

namespace PxServices.Repositories
{
    public class PinBarRepository : IPinBarRepository
    {
        private readonly string _connectionString = "Data Source=localhost;Initial Catalog=px;User ID=root;Password=";
    }
}
