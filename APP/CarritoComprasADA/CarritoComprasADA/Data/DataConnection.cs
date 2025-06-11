using CarritoComprasADA.Models;
using System.Collections.Generic;
using System.Transactions;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CarritoComprasADA_API.Data
{
    public class DataConnection
    {
        private readonly IConfiguration _configuration;

        public DataConnection(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
