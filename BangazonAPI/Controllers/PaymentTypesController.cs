﻿using System;
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

        //POST: api/PaymentTypes
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentType paymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        INSERT INTO PaymentType(Name, AcctNumber, CustomerId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @acctNumber, @customerId)
                                      ";
                    cmd.Parameters.Add(new SqlParameter("@name", paymentType.Name));
                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@customerId", paymentType.CustomerId));

                    paymentType.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetPaymentType", new { id = paymentType.Id }, paymentType);
                }
            }
        }

        // PUT: api/PaymentTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] PaymentType paymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                       UPDATE PaymentType
                                       SET Name = @name,
                                           AcctNumber = @acctNumber,
                                           CustomerId = @customerId
                                       WHERE Id = @id
                                      ";
                        cmd.Parameters.Add(new SqlParameter("@id", paymentType.Id));
                        cmd.Parameters.Add(new SqlParameter("@name", paymentType.Name));
                        cmd.Parameters.Add(new SqlParameter("@acctNumber", paymentType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@customerId", paymentType.CustomerId));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }

                        throw new Exception("No rows affected");
                    }
                }
            }

            catch (Exception)
            {
                if (await PaymentTypeExists(id) == false)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM PaymentType WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (await PaymentTypeExists(id) == false)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<bool> PaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = "SELECT Id FROM PaymentType WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return await reader.ReadAsync();
                }
            }
        }
    }
}
