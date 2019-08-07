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
            // Ensures "" or "budget"
            _filter = CheckFilter(_filter);

            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = MakeSqlGetCommand(_include);
                    if (_filter == "budget")
                    {
                        cmd.CommandText += " WHERE Budget > @gt";

                        cmd.Parameters.Add(new SqlParameter("@gt", _gt));
                    }
                    

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Department> departments = new List<Department>();
                    while (await reader.ReadAsync())
                    {
                        int Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        int foundIndex = departments.FindIndex(dept => dept.Id == Id);
                        Department department;
                        if (foundIndex == -1)
                        {
                            department = CreateDepartment(reader);
                            departments.Add(department);
                        }
                        else department = departments[foundIndex];


                        if (_include == "employees")
                        {
                            if (foundIndex == -1) department.Employees = new List<Employee>();
                            var newEmployee = CreateEmployee(reader);
                            if (newEmployee != null) department.Employees.Add(newEmployee);
                        }
                       

                    }

                    reader.Close();

                    return Ok(departments);
                }
            }
        }


        // GET api/values/5
        [HttpGet("{id}", Name = "GetDepartment")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
         [FromQuery] string _include
         )
        {
            // Ensures "" or "employees"
            _include = CheckInclude(_include);

            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = MakeSqlGetCommand(_include);
                    cmd.CommandText += " WHERE d.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                   


                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Department department = null;
                    while (await reader.ReadAsync())
                    {
                        if (department == null)
                        {
                            department = CreateDepartment(reader);
                        }


                        if (_include == "employees")
                        {
                            if (department.Employees == null) department.Employees = new List<Employee>();
                            var newEmployee = CreateEmployee(reader);
                            if (newEmployee != null) department.Employees.Add(newEmployee);
                        }


                    }

                    reader.Close();

                    if (department == null) return NotFound();

                    return Ok(department);
                }
            }
        }



        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = @"
                        INSERT INTO Department (Name, Budget)
                        OUTPUT INSERTED.Id
                        VALUES (@name, @budget)
                    ";
                    cmd.Parameters.Add(new SqlParameter("@name", department.Name));
                    cmd.Parameters.Add(new SqlParameter("@budget", department.Budget));


                    department.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetDepartment", new { id = department.Id }, department);
                }
            }
        }

        // PUT api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(
            [FromRoute] int id,
            [FromBody] Department department
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
                            UPDATE Department
                            SET Name = @name,
                                Budget = @budget
                            WHERE Id = @id
                        ";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@name", department.Name));
                        cmd.Parameters.Add(new SqlParameter("@budget", department.Budget));


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
                if (await DepartmentExists(id) == false)
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
                    Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
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


        private async Task<bool> DepartmentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = "SELECT Id FROM Department WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return await reader.ReadAsync();
                }
            }
        }
    }
}
