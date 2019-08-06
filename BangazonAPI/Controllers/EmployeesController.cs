using System;
using System.Collections.Generic;
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
    public class EmployeesController : Controller
    {
        private readonly IConfiguration _config;
        public EmployeesController(IConfiguration config)
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
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS EmployeeId, 
                                        e.FirstName, 
                                        e.LastName, 
                                        e.DepartmentId AS DeptId, 
                                        e.IsSupervisor, 
                                        d.Id AS DepartmentId, 
                                        d.Name, 
                                        d.Budget, 
                                        ce.AssignDate, 
                                        ce.UnassignDate, 
                                        c.Id AS ComputerId, 
                                        c.Make, 
                                        c.Manufacturer, 
                                        c.PurchaseDate, 
                                        c.DecomissionDate
                                        FROM Employee e
                                        JOIN Department d
                                        ON e.DepartmentId = d.Id
                                        JOIN ComputerEmployee ce
                                        ON e.Id = ce.EmployeeId
                                        JOIN Computer c
                                        ON ce.ComputerId = c.Id
                                        ORDER BY e.Id
                                      ";
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Employee> employees = new List<Employee>();
                    while (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("EmployeeId"));
                        Employee employee = employees.Find(e => id == e.Id);

                        if (employee == null)
                        {
                            employee = new Employee
                            {
                                Id = id,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DeptId")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))

                            };
                            employees.Add(employee);
                        }

                        try
                        {
                            Department department = new Department()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            };
                            employee.Department = department;
                        }
                        
                        catch(SqlNullValueException)
                        {
                            // nada
                        };

                        try
                        {
                            Computer computer = new Computer()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))
                            };
                            try
                            {
                               computer.DecommissionDate = reader.GetDateTime(reader.GetOrdinal("DecommissionDate"));
                            }
                            catch (IndexOutOfRangeException)
                            {
                                computer.DecommissionDate = null;
                            }
                            employee.Computers.Add(computer);
                        }
                        catch
                        {
                            // nada
                        }
                    }

                    reader.Close();
                    return Ok(employees);
                }
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}", Name = "GetEmployee")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS EmployeeId, 
                                        e.FirstName, 
                                        e.LastName, 
                                        e.DepartmentId AS DeptId, 
                                        e.IsSupervisor, 
                                        d.Id AS DepartmentId, 
                                        d.Name, 
                                        d.Budget, 
                                        ce.AssignDate, 
                                        ce.UnassignDate, 
                                        c.Id AS ComputerId, 
                                        c.Make, 
                                        c.Manufacturer, 
                                        c.PurchaseDate, 
                                        c.DecomissionDate
                                        FROM Employee e
                                        JOIN Department d
                                        ON e.DepartmentId = d.Id
                                        JOIN ComputerEmployee ce
                                        ON e.Id = ce.EmployeeId
                                        JOIN Computer c
                                        ON ce.ComputerId = c.Id
                                        WHERE e.Id = @id
                                      ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Employee employee = null;
                    if (reader.Read())
                    {
                        if (employee == null)
                        {
                            employee = new Employee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DeptId")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                            };
                        }

                        try
                        {
                            Department department = new Department()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            };
                            employee.Department = department;
                        }

                        catch (SqlNullValueException)
                        {
                            // nada
                        };

                        try
                        {
                            Computer computer = new Computer()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))
                            };
                            try
                            {
                                computer.DecommissionDate = reader.GetDateTime(reader.GetOrdinal("DecommissionDate"));
                            }
                            catch (IndexOutOfRangeException)
                            {
                                computer.DecommissionDate = null;
                            }
                            employee.Computers.Add(computer);
                        }
                        catch
                        {
                            // nada
                        }
                    }

                    reader.Close();
                    return Ok(employee);
                }
            }
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
