using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
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
    public class CustomersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomersController(IConfiguration config)
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

        // GET api/values
        [HttpGet]

        public async Task<IActionResult> Get([FromQuery] string _include)
        {
            // Ensures "", "products", or "payments"
            _include = CheckInclude(_include);

            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = MakeSqlGetCommand(_include);

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Customer> customers = new List<Customer>();
                    while (await reader.ReadAsync())
                    {
                        int Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        int foundIndex = customers.FindIndex(cust => cust.Id == Id);
                        Customer customer;
                        if (foundIndex == -1)
                        {
                            customer = CreateCustomer(reader);
                            customers.Add(customer);
                        }
                        else customer = customers[foundIndex];


                        if (_include == "products")
                        {
                            if (foundIndex == -1) customer.Products = new List<Product>();
                            var newProduct = CreateProduct(reader);
                            if (newProduct != null) customer.Products.Add(newProduct);
                        }
                        else if (_include == "payments")
                        {
                            if (foundIndex == -1) customer.PaymentTypes = new List<PaymentType>();
                            var newPayment = CreatePaymentType(reader);
                            if (newPayment != null) customer.PaymentTypes.Add(newPayment);

                        }

                    }

                    reader.Close();

                    return Ok(customers);
                }
            }
        }



        // GET api/values/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromQuery] string _include = ""
            )
        {
            _include = CheckInclude(_include);
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = MakeSqlGetCommand(_include);
                    cmd.CommandText += " WHERE c.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Customer cust = null;
                    while (await reader.ReadAsync())
                    {
                        int Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        if (cust == null) cust = CreateCustomer(reader);
                        if (_include == "products")
                        {
                            if (cust.Products == null) cust.Products = new List<Product>();
                            var newProduct = CreateProduct(reader);
                            if (newProduct != null) cust.Products.Add(newProduct);
                        }
                        else if (_include == "payments")
                        {
                            if (cust.PaymentTypes == null) cust.PaymentTypes = new List<PaymentType>();
                            var newPayment = CreatePaymentType(reader);
                            if (newPayment != null) cust.PaymentTypes.Add(newPayment);

                        }

                    }
                    reader.Close();

                    if (cust == null) return NotFound();

                    return Ok(cust);
                }
            }
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = @"
                        INSERT INTO Customer (FirstName, LastName)
                        OUTPUT INSERTED.Id
                        VALUES (@firstName, @lastName)
                    ";
                    cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", customer.LastName));


                    customer.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetCustomer", new { id = customer.Id }, customer);
                }
            }
        }

        // PUT api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(
            [FromRoute] int id,
            [FromBody] Customer customer
            )
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            UPDATE Customer
                            SET FirstName = @firstName,
                                LastName = @lastName
                            WHERE Id = @id
                        ";
                        cmd.Parameters.Add(new SqlParameter("@id", customer.Id));
                        cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", customer.LastName));


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
                if (await CustomerExists(id) == false)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/values/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {

        //        using (SqlConnection conn = Connection)
        //        {
        //            await conn.OpenAsync();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = "DELETE FROM Customer WHERE Id = @id";
        //                cmd.Parameters.Add(new SqlParameter("@id", id));
        //                int rowsAffected = await cmd.ExecuteNonQueryAsync();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (await CustomerExists(id) == false)
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        static private string CheckInclude(string include)
        {
            switch (include)
            {
                case "products":
                    return "products";
                case "payments":
                    return "payments";
                default:
                    return "";
            }
        }


        private Customer CreateCustomer(SqlDataReader reader)
        {
            return new Customer()
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
            };
        }
        static private Product CreateProduct(SqlDataReader reader)
        {
            try
            {
                return new Product()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price"))
                };
            }
            catch (SqlNullValueException)
            {
                return null;
            }
            
        }

        static private string MakeSqlGetCommand(string include)
        {
            var outputString = "SELECT c.Id, c.FirstName, c.LastName";
            var productFields = "p.Title, p.Description, p.Quantity, p.Price, p.Id AS ProductId";
            var paymentFields = "p.AcctNumber, p.[Name], p.Id AS PaymentId";

            if (include == "products") outputString += $", {productFields}";
            if (include == "payments") outputString += $", {paymentFields}";

            outputString += " FROM Customer c";
            if (include == "products") outputString += " LEFT JOIN Product p on c.Id = p.CustomerId";
            if (include == "payments") outputString += " LEFT JOIN PaymentType p on c.Id = p.CustomerId";




            return outputString;
        }

        static private PaymentType CreatePaymentType(SqlDataReader reader)
        {

            try
            {
                return new PaymentType()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                    AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                    Name = reader.GetString(reader.GetOrdinal("Name"))
                };
            }
            catch (SqlNullValueException)
            {
                return null;
            }
            

        }
        private async Task<bool> CustomerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = "SELECT Id FROM Customer WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return await reader.ReadAsync();
                }
            }
        }
    }
}
