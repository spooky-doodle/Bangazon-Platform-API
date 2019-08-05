using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public PaymentTypesController(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: api/PaymentTypes
        [HttpGet]
        public async Task<IActionResult> GetAllPaymentTypes()
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT p.Id, p.Name, p.AcctNumber, p.CustomerId FROM PaymentType p";
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<PaymentType> paymentTypes = new List<PaymentType>();
                    while(await reader.ReadAsync())
                    {
                        PaymentType paymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))
                        };
                        paymentTypes.Add(paymentType);
                    }

                    reader.Close();
                    return Ok(paymentTypes);
                }
            }
        }

        // GET: api/PaymentTypes/5
        [HttpGet("{id}", Name = "GetPaymentType")]
        //public string Get(int id)
        public async Task<IActionResult> GetSpecificPaymentTypes(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT p.Id, p.Name, p.AcctNumber, p.CustomerId 
                                        FROM PaymentType p
                                        WHERE p.Id = @id
                                      ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();


                    PaymentType paymentType = null;
                    if(await reader.ReadAsync())
                    {
                        paymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))
                        };
                    }

                    reader.Close();
                    return Ok(paymentType);
                }
            }
        }

        // POST: api/PaymentTypes
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // PUT: api/PaymentTypes/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
