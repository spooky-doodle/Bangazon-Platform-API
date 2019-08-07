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
    public class TestDeparments
    {
        [Fact]
        public async Task Test_Get_All_Departments()
        {

            /*
                ARRANGE
            */


            /*
                ACT
            */

            // Fetch()
            var response = await GetResponse("/api/departments");
            // Json.Parse()
            var departments = await ParseDepartmentList(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(departments.Count > 0);
        }


        [Fact]
        public async Task Test_Get_Departments_Filter_By_Budget()
        {


            /* Arrange */
            Department departmentMatchingQuery = new Department()
            {
                Name = "Acctg",
                Budget = 300000
            };

            Department departmentNotMatchingQuery = new Department()
            {
                Name = "BrassTacks",
                Budget = 100000
            };

            var postResponse = await PostDepartment(departmentMatchingQuery);
            Department createdDepartment = await ParseOneDepartment(postResponse);


            var notMatchingResponse = await PostDepartment(departmentNotMatchingQuery);
            Department createdNotMatchingDepartment = await ParseOneDepartment(notMatchingResponse);



            /* Act */
            var queryResult = await GetResponse("/api/departments?_filter=budget&_gt=200000");
            var queriedDepartments = await ParseDepartmentList(queryResult);



            var foundDepartment = queriedDepartments.Find(dept => dept.Id == createdDepartment.Id);
            var notFoundDepartment = queriedDepartments.Find(dept => dept.Id == createdNotMatchingDepartment.Id);

            /* Assert */


            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Created, notMatchingResponse.StatusCode);

            Assert.Equal(HttpStatusCode.OK, queryResult.StatusCode);


            Assert.NotNull(foundDepartment);
            Assert.Null(notFoundDepartment);



        }



        [Fact]
        public async Task Test_Get_All_Departments_Include_Employees()
        {
            /*
                ARRANGE  
                TODO:  Create a new product and assign to a Department.
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/departments?_include=employees");
            var Departments = await ParseDepartmentList(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(Departments);
            Assert.NotEmpty(Departments[0].Employees);

        }

     

        [Fact]
        public async Task Test_Get_One_Department()
        {
            /*
                ARRANGE
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/departments/1");


            var Department = await ParseOneDepartment(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(Department.Name);
            Assert.True(Department.Id == 1);
        }

        [Fact]
        public async Task Test_Get_One_Department_Include_Employees()
        {
            int testId = 2;
            /*
                ARRANGE
            */


            /*
                ACT
            */
            var response = await GetResponse($"/api/Departments/{testId}?_include=employees");


            var Department = await ParseOneDepartment(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(Department.Name);
            Assert.True(Department.Id == testId);
            Assert.NotEmpty(Department.Employees);
        }

        [Fact]
        public async Task Test_Get_One_Department_Nonexistant()
        {
            /*
                ARRANGE
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/departments/99999999");

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task Test_Create_New_Department()
        {
            /*
                ARRANGE
            */
            Department newDepartment = new Department()
            {
                Name = "Bob",
                Budget = 200000
            };


            /*
                ACT
            */
            var response = await PostDepartment(newDepartment);

            var createdDepartment = await ParseOneDepartment(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(createdDepartment.Id != 0);
            Assert.Equal(createdDepartment.Name, newDepartment.Name);
            Assert.Equal(createdDepartment.Budget, newDepartment.Budget);


        }

        [Fact]
        public async Task Test_Update_Existing_Department()
        {
            /*
                ARRANGE
            */

            Department newDepartment = new Department()
            {
                Name = "Apples",
                Budget = 200000
            };

            var createdResponse = await PostDepartment(newDepartment);
            var parsedDepartment = await ParseOneDepartment(createdResponse);

            var departmentToUpdate = new Department()
            {
                Name = "Bananas",
                Budget = 500000
            };
            /*
                ACT
            */

            //  Put the updated Department
            var updateResponse = await PutDepartment(departmentToUpdate, parsedDepartment.Id);


            // Then fetch.
            var getDepartment = await GetResponse($"/api/Departments/{parsedDepartment.Id}");
            Department updatedDepartment = await ParseOneDepartment(getDepartment);


            /*
                ASSERT
            */

            //  Make sure create was successful
            Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
            //  Make sure update was successful
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
            //  Make sure get was successful
            Assert.Equal(HttpStatusCode.OK, getDepartment.StatusCode);

            Assert.Equal(parsedDepartment.Id, updatedDepartment.Id);
            Assert.Equal(departmentToUpdate.Name, updatedDepartment.Name);
            Assert.Equal(departmentToUpdate.Budget, updatedDepartment.Budget);


        }

        [Fact]
        public async Task Test_Update_Nonexisting_Department()
        {
            /*
                ARRANGE
            */

            Department testDepartment = new Department()
            {
                Name = "Billy",
                Budget = 55000
            };


            /*
                ACT
            */
            var response = await PutDepartment(testDepartment, 91231274);




            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


        }

        [Fact]
        public async Task Test_Delete_Department_Should_Fail()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */



                /*
                    ACT
                */
                var response = await client.DeleteAsync("/api/departments/1");




                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


            }
        }





        //  Begin helper methods

        private async Task<HttpResponseMessage> GetResponse(string url)
        {
            using (var client = new APIClientProvider().Client)
            {
                return await client.GetAsync(url);
            }
        }


        private async Task<HttpResponseMessage> PostDepartment(Department newDepartment)
        {
            using (var client = new APIClientProvider().Client)
            {

                var jsonDepartment = JsonConvert.SerializeObject(newDepartment);
                return await client.PostAsync(
                    "/api/departments",
                    new StringContent(jsonDepartment, Encoding.UTF8, "application/json")
                    );
            }
        }

        private async Task<Department> ParseOneDepartment(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var parsedDepartment = JsonConvert.DeserializeObject<Department>(responseBody);
            return parsedDepartment;
        }

        private async Task<List<Department>> ParseDepartmentList(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var parsedDepartment = JsonConvert.DeserializeObject<List<Department>>(responseBody);
            return parsedDepartment;
        }

        private async Task<HttpResponseMessage> PutDepartment(Department updatedDepartment, int testId)
        {
            using (var client = new APIClientProvider().Client)
            {
                var jsonDepartment = JsonConvert.SerializeObject(updatedDepartment);

                return await client.PutAsync(
                    $"/api/departments/{testId}",
                    new StringContent(jsonDepartment, Encoding.UTF8, "application/json")
                    );
            }
        }

        //[Fact]
        //public async Task Test_Delete_Existing_Department()
        //{


        //    using (var client = new APIClientProvider().Client)
        //    {
        //        /*
        //            ARRANGE
        //        */
        //        Department newDepartment = new Department()
        //        {
        //            FirstName = "Adam",
        //            LastName = "Driver"
        //        };

        //        var jsonDepartment = JsonConvert.SerializeObject(newDepartment);


        //        var response = await client.PostAsync(
        //            "/api/Departments",
        //            new StringContent(jsonDepartment, Encoding.UTF8, "application/json")
        //            );


        //        string responseBody = await response.Content.ReadAsStringAsync();
        //        var Department = JsonConvert.DeserializeObject<Department>(responseBody);


        //        /*
        //            ACT
        //        */
        //        var deleteResponse = await client.DeleteAsync($"/api/Departments/{Department.Id}");


        //        /*
        //            ASSERT
        //        */
        //        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);





        //    }
        //}


        //[Fact]
        //public async Task Test_Delete_Nonexisting_Department()
        //{


        //    using (var client = new APIClientProvider().Client)
        //    {
        //        /*
        //            ARRANGE
        //        */

        //        /*
        //            ACT
        //        */
        //        var deleteResponse = await client.DeleteAsync($"/api/Departments/135123233");


        //        /*
        //            ASSERT
        //        */
        //        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);





        //    }
        //}
    }
}


