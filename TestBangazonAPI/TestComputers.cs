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
    public class TestComputers
    {
        [Fact]
        public async Task Test_Get_All_Computers()
        {

            /*
                ARRANGE
            */


            /*
                ACT
            */

            // Fetch()
            var response = await GetResponse("/api/computers");
            // Json.Parse()
            var computers = await ParseComputerList(response);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(computers);
        }






        [Fact]
        public async Task Test_Get_One_Computer()
        {
            /*
                ARRANGE
            */
            var newComputerResponse = await PostComputer(new Computer()
            {
                Make = "Computer",
                Manufacturer = "Apple",
                PurchaseDate = DateTime.Now
            });
            var newComputer = await ParseOneComputer(newComputerResponse);








            /*
                ACT
            */
            var response = await GetResponse($"/api/computers/{newComputer.Id}");
            var foundComputer = await ParseOneComputer(response);

            var deleteResponse = await DeleteComputer(newComputer.Id);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(foundComputer.Make, newComputer.Make);
            Assert.Equal(foundComputer.Manufacturer, newComputer.Manufacturer);
            Assert.Equal(foundComputer.Id, newComputer.Id);

        }

        [Fact]
        public async Task Test_Get_One_Computer_Nonexistant()
        {
            /*
                ARRANGE
            */


            /*
                ACT
            */
            var response = await GetResponse("/api/computers/99999999");

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task Test_Create_And_Delete_Computer()
        {
            /*
                ARRANGE
            */
            var computerToAdd = new Computer()
            {
                Make = "Bawglk",
                Manufacturer = "Adgasg",
                PurchaseDate = DateTime.Now
            };


            /*
                ACT
            */
            var newComputerResponse = await PostComputer(computerToAdd);
            var newComputer = await ParseOneComputer(newComputerResponse);

            var deleteResponse = await DeleteComputer(newComputer.Id);

            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.Created, newComputerResponse.StatusCode);
            Assert.True(newComputer.Id > 0);
            Assert.Equal(computerToAdd.Make, newComputer.Make);
            Assert.Equal(computerToAdd.Manufacturer, newComputer.Manufacturer);


            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);


        }

        [Fact]
        public async Task Test_Update_Existing_Computer()
        {
            /*
                ARRANGE
            */
            var computerToAdd = new Computer()
            {
                Make = "asdfasdf",
                Manufacturer = "aswqtw",
                PurchaseDate = DateTime.Now
            };
            var newComputerResponse = await PostComputer(computerToAdd);
            var newComputer = await ParseOneComputer(newComputerResponse);

            var computerUpdates = new Computer()
            {
                Make = "sbwerer",
                Manufacturer = "rgwetdk",
                PurchaseDate = DateTime.Now,
                DecommissionDate = DateTime.Now
            };

            /*
                ACT
            */

            //  Put the updated computer
            var updateResponse = await PutComputer(computerUpdates, newComputer.Id);



            // Then fetch.
            var updatedComputerResponse = await GetResponse($"/api/computers/{newComputer.Id}");
            Computer updatedComputer = await ParseOneComputer(updatedComputerResponse);


            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.Created, newComputerResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);



            Assert.Equal(HttpStatusCode.OK, updatedComputerResponse.StatusCode);
            Assert.Equal(newComputer.Id, updatedComputer.Id);
            Assert.Equal(computerUpdates.Make, updatedComputer.Make);
            Assert.Equal(computerUpdates.Manufacturer, updatedComputer.Manufacturer);
            Assert.NotNull(updatedComputer.DecommissionDate);



        }

        [Fact]
        public async Task Test_Update_Nonexisting_Computer()
        {
            /*
                ARRANGE
            */
            var computerToAdd = new Computer()
            {
                Make = "asdfasdf",
                Manufacturer = "aswqtw",
                PurchaseDate = DateTime.Now
            };

            /*
                ACT
            */
            var response = await PutComputer(computerToAdd, 91231274);




            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


        }



        [Fact]
        public async Task Test_Delete_Nonexisting_Computer()
        {


            /*
                ARRANGE
            */

            /*
                ACT
            */
            var deleteResponse = await DeleteComputer(135123233);


            /*
                ASSERT
            */
            Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);






        }





        //  Begin helper methods

        private async Task<HttpResponseMessage> GetResponse(string url)
        {
            using (var client = new APIClientProvider().Client)
            {
                return await client.GetAsync(url);
            }
        }


        private async Task<HttpResponseMessage> PostComputer(Computer newComputer)
        {
            using (var client = new APIClientProvider().Client)
            {

                var jsonComputer = JsonConvert.SerializeObject(newComputer);
                return await client.PostAsync(
                    "/api/computers",
                    new StringContent(jsonComputer, Encoding.UTF8, "application/json")
                    );
            }
        }

        private async Task<Computer> ParseOneComputer(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var parsedComputer = JsonConvert.DeserializeObject<Computer>(responseBody);
            return parsedComputer;
        }

        private async Task<List<Computer>> ParseComputerList(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var parsedComputers = JsonConvert.DeserializeObject<List<Computer>>(responseBody);
            return parsedComputers;
        }

        private async Task<HttpResponseMessage> PutComputer(Computer updatedComputer, int testId)
        {
            using (var client = new APIClientProvider().Client)
            {
                var jsonComputer = JsonConvert.SerializeObject(updatedComputer);

                return await client.PutAsync(
                    $"/api/computers/{testId}",
                    new StringContent(jsonComputer, Encoding.UTF8, "application/json")
                    );
            }
        }

        private async Task<HttpResponseMessage> DeleteComputer(int id)
        {
            using (var client = new APIClientProvider().Client)
            {

                return await client.DeleteAsync(
                    $"/api/computers/{id}");
            }
        }

    }
}


