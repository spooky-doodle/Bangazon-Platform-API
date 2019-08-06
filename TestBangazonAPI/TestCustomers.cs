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
    public class TestCustomers
    {
        [Fact]
        public async Task Test_Get_All_Customers()
        {

            /*
                ARRANGE
            */


            /*
                ACT
            */

            // Fetch()
            var response = await GetResponse("/api/customers");
            // Json.Parse()
            var customers = await ParseCustomerList(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(customers.Count > 0);
        }


        [Fact]
        public async Task Test_Get_Customers_Matching_Query()
        {


            /* Arrange */
            Customer newCustomer = new Customer()
            {
                FirstName = "Adam's",
                LastName = "Apple"
            };

            var postResponse = await PostCustomer(newCustomer);

            Customer createdCustomer = await ParseOneCustomer(postResponse);


            /* Act */
            var queryResult = await GetResponse("/api/customers?q=Apple");


            var queriedCustomers = await ParseCustomerList(queryResult);


            /* Assert */


            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, queryResult.StatusCode);


            Assert.Contains<Customer>(createdCustomer, queriedCustomers);



        }



        [Fact]
        public async Task Test_Get_All_Customers_Include_Products()
        {
            /*
                ARRANGE  
                TODO:  Create a new product and assign to a customer.
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/customers?_include=products");
            var customers = await ParseCustomerList(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(customers.Count > 0);
            Assert.NotNull(customers[1].Products[0]);

        }

        [Fact]
        public async Task Test_Get_All_Customers_Include_Payments()
        {
            /*
                ARRANGE  
                TODO:  Create a new product and assign to a customer.
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/customers?_include=payments");


            var customers = await ParseCustomerList(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(customers.Count > 0);
            Assert.NotNull(customers[1].PaymentTypes[0]);

        }

        [Fact]
        public async Task Test_Get_One_Customer()
        {
            /*
                ARRANGE
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/customers/1");


            var customer = await ParseOneCustomer(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(customer.FirstName != null);
            Assert.True(customer.Id == 1);
        }

        [Fact]
        public async Task Test_Get_One_Customer_Include_Products()
        {
            /*
                ARRANGE
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/customers/2?_include=products");


            var customer = await ParseOneCustomer(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(customer.FirstName != null);
            Assert.True(customer.Id == 2);
            Assert.True(customer.Products.Count > 0);
        }


        [Fact]
        public async Task Test_Get_One_Customer_Include_Payments()
        {
            /*
                ARRANGE
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/customers/2?_include=payments");


            var customer = await ParseOneCustomer(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(customer.FirstName != null);
            Assert.True(customer.Id == 2);
            Assert.True(customer.PaymentTypes.Count > 0);
        }
        [Fact]
        public async Task Test_Get_One_Customer_Nonexistant()
        {
            /*
                ARRANGE
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/customers/99999999");

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task Test_Create_New_Customer()
        {
            /*
                ARRANGE
            */
            Customer newCustomer = new Customer()
            {
                FirstName = "Bob",
                LastName = "Barker"
            };


            /*
                ACT
            */
            var response = await PostCustomer(newCustomer);

            var createdCustomer = await ParseOneCustomer(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(createdCustomer.Id != 0);
            Assert.Equal(createdCustomer.FirstName, newCustomer.FirstName);
            Assert.Equal(createdCustomer.LastName, newCustomer.LastName);


        }

        [Fact]
        public async Task Test_Update_Existing_Customer()
        {
            int testId = 2;
            /*
                ARRANGE
            */

            Customer testCustomer = new Customer()
            {
                Id = testId,
                FirstName = "Jason",
                LastName = "Server"
            };


            /*
                ACT
            */

            //  Put the updated customer
            var response = await PutCustomer(testCustomer, testId);


            // Then fetch.
            var getCustomer = await GetResponse($"/api/customers/{testId}");
            Customer updatedCustomer = await ParseOneCustomer(getCustomer);


            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            getCustomer.EnsureSuccessStatusCode();


            Assert.Equal(HttpStatusCode.OK, getCustomer.StatusCode);
            Assert.Equal(testId, updatedCustomer.Id);
            Assert.Equal(testCustomer.FirstName, updatedCustomer.FirstName);
            Assert.Equal(testCustomer.LastName, updatedCustomer.LastName);


        }

        [Fact]
        public async Task Test_Update_Nonexisting_Customer()
        {
            /*
                ARRANGE
            */

            Customer testCustomer = new Customer()
            {
                FirstName = "Billy",
                LastName = "Blanks"
            };


            /*
                ACT
            */
            var response = await PutCustomer(testCustomer, 91231274);




            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


        }

        [Fact]
        public async Task Test_Delete_Customer_Should_Fail()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */



                /*
                    ACT
                */
                var response = await client.DeleteAsync("/api/customers/1");




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


        private async Task<HttpResponseMessage> PostCustomer(Customer newCustomer)
        {
            using (var client = new APIClientProvider().Client)
            {

                var jsonCustomer = JsonConvert.SerializeObject(newCustomer);
                return await client.PostAsync(
                    "/api/customers",
                    new StringContent(jsonCustomer, Encoding.UTF8, "application/json")
                    );
            }
        }

        private async Task<Customer> ParseOneCustomer(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var parsedCustomer = JsonConvert.DeserializeObject<Customer>(responseBody);
            return parsedCustomer;
        }

        private async Task<List<Customer>> ParseCustomerList(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var parsedCustomers = JsonConvert.DeserializeObject<List<Customer>>(responseBody);
            return parsedCustomers;
        }

        private async Task<HttpResponseMessage> PutCustomer(Customer updatedCustomer, int testId)
        {
            using (var client = new APIClientProvider().Client)
            {
                var jsonCustomer = JsonConvert.SerializeObject(updatedCustomer);

                return await client.PutAsync(
                    $"/api/customers/{testId}",
                    new StringContent(jsonCustomer, Encoding.UTF8, "application/json")
                    );
            }
        }

        //[Fact]
        //public async Task Test_Delete_Existing_Customer()
        //{


        //    using (var client = new APIClientProvider().Client)
        //    {
        //        /*
        //            ARRANGE
        //        */
        //        Customer newCustomer = new Customer()
        //        {
        //            FirstName = "Adam",
        //            LastName = "Driver"
        //        };

        //        var jsonCustomer = JsonConvert.SerializeObject(newCustomer);


        //        var response = await client.PostAsync(
        //            "/api/customers",
        //            new StringContent(jsonCustomer, Encoding.UTF8, "application/json")
        //            );


        //        string responseBody = await response.Content.ReadAsStringAsync();
        //        var customer = JsonConvert.DeserializeObject<Customer>(responseBody);


        //        /*
        //            ACT
        //        */
        //        var deleteResponse = await client.DeleteAsync($"/api/customers/{customer.Id}");


        //        /*
        //            ASSERT
        //        */
        //        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);





        //    }
        //}


        //[Fact]
        //public async Task Test_Delete_Nonexisting_Customer()
        //{


        //    using (var client = new APIClientProvider().Client)
        //    {
        //        /*
        //            ARRANGE
        //        */

        //        /*
        //            ACT
        //        */
        //        var deleteResponse = await client.DeleteAsync($"/api/customers/135123233");


        //        /*
        //            ASSERT
        //        */
        //        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);





        //    }
        //}
    }
}


