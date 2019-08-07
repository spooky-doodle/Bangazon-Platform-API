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
    DELETE
    User should be able to GET a list, and GET a single item.

    Given A computer is assigned to an employee,
    When a user attempts to delete the computer,
    Then they should not be allowed to delete the computer.

    A computer is considered to be assigned to an employee when there is a 
    ComputerEmployee record with ComputerId matching the Computer's Id, 
    that has a null UnassignDate.

*/

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputersController(IConfiguration config)
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

        // GET api/computers
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = MakeSqlGetCommand();

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Computer> computers = new List<Computer>();
                    while (await reader.ReadAsync())
                    {
                        Computer newComputer = CreateComputer(reader);
                        computers.Add(newComputer);


                    }

                    reader.Close();

                    return Ok(computers);
                }
            }
        }


        // GET api/computers/5
        [HttpGet("{id}", Name = "GetComputer")]
        public async Task<IActionResult> Get(
            [FromRoute] int id
         )
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = MakeSqlGetCommand();
                    cmd.CommandText += " WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));



                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Computer computer = null;
                    while (await reader.ReadAsync())
                    {
                        if (computer == null)
                        {
                            computer = CreateComputer(reader);
                        }


                        //if (_include == "employees")
                        //{
                        //    if (computer.Employees == null) computer.Employees = new List<Employee>();
                        //    var newEmployee = CreateEmployee(reader);
                        //    if (newEmployee != null) computer.Employees.Add(newEmployee);
                        //}


                    }

                    reader.Close();

                    if (computer == null) return NotFound();

                    return Ok(computer);
                }
            }
        }



        // POST api/computers
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer computer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = @"
                        INSERT INTO Computer (Make, Manufacturer, PurchaseDate)
                        OUTPUT INSERTED.Id
                        VALUES (@make, @manufacturer, @purchaseDate)
                    ";
                    cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                    cmd.Parameters.Add(new SqlParameter("@manufacturer", computer.Manufacturer));
                    cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));



                    computer.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetDepartment", new { id = computer.Id }, computer);
                }
            }
        }

        // PUT api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(
            [FromRoute] int id,
            [FromBody] Computer computer
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
                            UPDATE Computer
                            SET Make = @make,
                                Manufacturer = @manufacturer,
                                DecommissionDate = @decommissionDate,
                                PurchaseDate = @purchaseDate
                            WHERE Id = @id
                        ";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@manufacturer", computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@decommissionDate", computer.DecommissionDate));
                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));




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
                if (await ComputerExists(id) == false)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/computers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id )
        {
            if (await CheckActiveAssignment(id)) return NotFound();

            try
            {

                using (SqlConnection conn = Connection)
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Computer WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
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
                if (await ComputerExists(id) == false)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }



        static private Computer CreateComputer(SqlDataReader reader)
        {
            Computer newComputer = new Computer()
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                Make = reader.GetString(reader.GetOrdinal("Make")),
                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
            };

            try
            {
                newComputer.DecommissionDate = reader.GetDateTime(reader.GetOrdinal("DecommissionDate"));
            }
            catch (SqlNullValueException)
            { }  //  DecommissionDate defaults to null.

            return newComputer;
        }


        //    I might want this later!!


        //static private Employee CreateEmployee(SqlDataReader reader)
        //{
        //    try
        //    {
        //        return new Employee()
        //        {
        //            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
        //            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        //            LastName = reader.GetString(reader.GetOrdinal("LastName")),
        //            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
        //        };
        //    }
        //    catch (SqlNullValueException)
        //    {
        //        return null;
        //    }

        //}

        static private string MakeSqlGetCommand()
        {
            var outputString = "SELECT c.Id, c.Make, c.Manufacturer, c.PurchaseDate, c.DecommissionDate";
            //var employeeFields = "e.FirstName, e.LastName, e.IsSupervisor, e.Id AS EmployeeId";

            outputString += " FROM Computer c";

            return outputString;
        }

        private async Task<bool> CheckActiveAssignment(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, ComputerId, UnassignDate FROM ComputerEmployee " +
                        "WHERE ComputerId = @id AND UnassignDate IS NULL";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    return await reader.ReadAsync();
                }
            }
        }


        private async Task<bool> ComputerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = "SELECT Id FROM Computer WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return await reader.ReadAsync();
                }
            }
        }
    }
}
