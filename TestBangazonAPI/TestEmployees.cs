using System;
using System.Net;
using Newtonsoft.Json;
using Xunit;
using BangazonAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace TestBangazonAPI
{
    public class TestEmployee
    {
        [Fact]
        public async Task Test_Get_All_Employees()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                 * ARRANGE
                 */

                /*
                * ACT
                */
                var response = await client.GetAsync("/api/Employees");

                string responseBody = await response.Content.ReadAsStringAsync();
                var employees = JsonConvert.DeserializeObject<List<Employee>>(responseBody);

                /*
                * ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(employees.Count > 0);
                Assert.NotNull(employees[1].Department.Name);
                Assert.NotNull(employees[1].Computers[1]);
                Assert.NotNull(employees[1].Computers[1].Manufacturer);
            }
        }

        [Fact]
        public async Task Test_Get_Specific_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                 Arrange
                 */

                /*
                ACT
                 */
                var response = await client.GetAsync("/api/Employees/1");

                string responseBody = await response.Content.ReadAsStringAsync();
                var employee = JsonConvert.DeserializeObject<Employee>(responseBody);

                /*
                * ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(employee.FirstName != null);
                Assert.True(employee.Department.Id > 0);
                Assert.NotNull(employee.Computers[0]);
            }
        }

        [Fact]
        public async Task Test_Add_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                 * ARRANGE
                 */
                var newEmployee = new Employee()
                {
                    FirstName = "Michael",
                    LastName = "Scott",
                    DepartmentId = 3,
                    IsSupervisor = false
                };
                var jsonNewEmployee = JsonConvert.SerializeObject(newEmployee);

                /*
                * ACT
                */
                var response = await client.PostAsync("/api/Employees",
                    new StringContent(jsonNewEmployee,
                    Encoding.UTF8, "application/json"));

                string responseBody = await response.Content.ReadAsStringAsync();
                var employee = JsonConvert.DeserializeObject<Employee>(responseBody);

                /*
                * ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.True(employee.Id != 0);
                Assert.Equal(newEmployee.FirstName, employee.FirstName);
                Assert.Equal(newEmployee.DepartmentId, newEmployee.DepartmentId);
                Assert.Equal(newEmployee.IsSupervisor, newEmployee.IsSupervisor);
            }
        }

        [Fact]
        public async Task Test_Update_Existing_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                 * ARRANGE
                 */
                var testNum = 1;

                var testEmployee = new Employee()
                {
                    FirstName = "Dwight",
                    LastName = "Schrute",
                    DepartmentId = 3,
                    IsSupervisor = true
                };
                var jsonTestEmployee = JsonConvert.SerializeObject(testEmployee);

                /*
                    ACT
                */
                var response = await client.PutAsync(
                    $"/api/Employees/{testNum}",
                    new StringContent(jsonTestEmployee, Encoding.UTF8, "application/json")
                    );

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*   
                 *   GET   
                */

                var getEmployee = await client.GetAsync($"/api/Employees/{testNum}");
                getEmployee.EnsureSuccessStatusCode();

                string getResponse = await getEmployee.Content.ReadAsStringAsync();
                Employee updatedEmployee = JsonConvert.DeserializeObject<Employee>(getResponse);

                Assert.Equal(HttpStatusCode.OK, getEmployee.StatusCode);
                Assert.Equal(testNum, updatedEmployee.Id);
                Assert.Equal(testEmployee.FirstName, updatedEmployee.FirstName);
                Assert.Equal(testEmployee.DepartmentId, updatedEmployee.DepartmentId);
            }
        }

    }
}
