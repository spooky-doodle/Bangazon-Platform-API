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

/*
        Verbs to be supported

            GET
            POST
            PUT

        User should be able to GET a list, and GET a single item.

        If the query string parameter of ?_include=employees is provided, 
        then all employees in the department(s) should be included in the response.

        If the query string parameters of ?_filter=budget&_gt=300000 is provided on a 
        request for the list of departments, then any department whose budget is $300,000, 
        or greater, should be in the response.

*/

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentsController(IConfiguration config)
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

        public async Task<IActionResult> Get(
            [FromQuery] string _include, 
            [FromQuery] string _filter = "",
            [FromQuery] int _gt = 0
            )
        {
            // Ensures "" or "employees"
            _include = CheckInclude(_include);
            _filter = CheckFilter(_filter);

            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = MakeSqlGetCommand(_include);
                    cmd.CommandText += " WHERE FirstName LIKE '%' + @q + '%'" +
                        " OR LastName LIKE '%' + @q + '%'";
                    cmd.Parameters.Add(new SqlParameter("@q", q));

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

        // DELETE api/customers/5
        //[HttpDelete("{id}")]
        //public ActionResult Delete(/* int id */)
        //{

        //    return Forbid();
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
            if (include == "employees") return include;
            return "";
        }
        static private string CheckFilter(string filter)
        {
            if (filter == "budget") return filter;
            return "";
        }


        private Department CreateDepartment(SqlDataReader reader)
        {
            return new Department()
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
            };
        }
        static private Employee CreateEmployee(SqlDataReader reader)
        {
            try
            {
                return new Employee()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                };
            }
            catch (SqlNullValueException)
            {
                return null;
            }
            
        }

        static private string MakeSqlGetCommand(string include)
        {
            var outputString = "SELECT d.Id, d.[Name], d.Budget";
            var employeeFields = "e.FirstName, e.LastName, e.IsSupervisor, e.Id AS EmployeeId";

            if (include == "employees") outputString += $", {employeeFields}";

            outputString += " FROM Department d";
            if (include == "employees") outputString += " LEFT JOIN Employee e on d.Id = e.DepartmentId";




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
