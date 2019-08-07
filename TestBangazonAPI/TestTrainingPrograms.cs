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
    public class TestTrainingPrograms
    {
        [Fact]
        public async Task Test_Get_All_Training_Programs()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/trainingPrograms");


                string responseBody = await response.Content.ReadAsStringAsync();
                var trainingPrograms = JsonConvert.DeserializeObject<List<TrainingProgram>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(trainingPrograms.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_One_Training_Program()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/trainingPrograms/2");


                string responseBody = await response.Content.ReadAsStringAsync();
                var trainingProgram = JsonConvert.DeserializeObject<TrainingProgram>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(trainingProgram.Name != null);
                Assert.True(trainingProgram.Id == 2);
            }
        }
        [Fact]
        public async Task Test_Create_New_Training_Program()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                TrainingProgram newTrainingProgram = new TrainingProgram()
                {
                    Name = "Test Training Program",
                    StartDate = new DateTime(2019, 08, 26),
                    EndDate = new DateTime(2019, 09, 15),
                    MaxAttendees = 20
                };

                var jsonTrainingProgram = JsonConvert.SerializeObject(newTrainingProgram);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                    "/api/trainingPrograms",
                    new StringContent(jsonTrainingProgram, Encoding.UTF8, "application/json")
                    );


                string responseBody = await response.Content.ReadAsStringAsync();
                var trainingProgram = JsonConvert.DeserializeObject<TrainingProgram>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.True(trainingProgram.Id != 0);
                Assert.Equal(trainingProgram.Name, newTrainingProgram.Name);
                Assert.Equal(trainingProgram.MaxAttendees, newTrainingProgram.MaxAttendees);


            }
        }

        [Fact]
        public async Task Test_Update_Existing_Training_Program()
        {
            int testId = 3;
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                TrainingProgram testTrainingProgram = new TrainingProgram()
                {
                    Name = "Test Training Program 2",
                    StartDate = new DateTime(2020, 08, 26),
                    EndDate = new DateTime(2020, 09, 15),
                    MaxAttendees = 25
                };

                var jsonTrainingPrgram = JsonConvert.SerializeObject(testTrainingProgram);

                /*
                    ACT
                */
                var response = await client.PutAsync(
                    $"/api/trainingPrograms/{testId}",
                    new StringContent(jsonTrainingPrgram, Encoding.UTF8, "application/json")
                    );



                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*   
                 *   GET   
                */

                var getTrainingPrograms = await client.GetAsync($"/api/trainingPrograms/{testId}");
                getTrainingPrograms.EnsureSuccessStatusCode();

                string getResponse = await getTrainingPrograms.Content.ReadAsStringAsync();
                var updatedTrainingProgram = JsonConvert.DeserializeObject<TrainingProgram>(getResponse);

                Assert.Equal(HttpStatusCode.OK, getTrainingPrograms.StatusCode);
                Assert.Equal(testId, updatedTrainingProgram.Id);
                Assert.Equal(testTrainingProgram.Name, updatedTrainingProgram.Name);
                Assert.Equal(testTrainingProgram.MaxAttendees, updatedTrainingProgram.MaxAttendees);


            }
        }

        [Fact]
        public async Task Test_Update_Nonexisting_Training_Program()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                TrainingProgram testTrainingProgram = new TrainingProgram()
                {
                    Name = "Test Training Program 2",
                    StartDate = new DateTime(2020, 08, 26),
                    EndDate = new DateTime(2020, 09, 15),
                    MaxAttendees = 25
                };

                var jsonTrainingProgram = JsonConvert.SerializeObject(testTrainingProgram);

                /*
                    ACT
                */
                var response = await client.PutAsync(
                    "/api/trainingPrograms/9999999",
                    new StringContent(jsonTrainingProgram, Encoding.UTF8, "application/json")
                    );




                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


            }
        }

        [Fact]
        public async Task Test_Delete_Existing_Future_Training_Programs()
        {


            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                TrainingProgram newTrainingProgram = new TrainingProgram()
                {
                    Name = "Test Training Program 2",
                    StartDate = new DateTime(2020, 08, 26),
                    EndDate = new DateTime(2020, 09, 15),
                    MaxAttendees = 25
                };

                var jsonTrainingProgram = JsonConvert.SerializeObject(newTrainingProgram);


                var response = await client.PostAsync(
                    "/api/trainingPrograms",
                    new StringContent(jsonTrainingProgram, Encoding.UTF8, "application/json")
                    );


                string responseBody = await response.Content.ReadAsStringAsync();
                var trainingProgram = JsonConvert.DeserializeObject<TrainingProgram>(responseBody);


                /*
                    ACT
                */
                var deleteResponse = await client.DeleteAsync($"/api/trainingPrograms/{trainingProgram.Id}");


                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_Existing_Past_Training_Programs()
        {


            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
         
                /*
                    ACT
                */
                var deleteResponse = await client.DeleteAsync($"/api/trainingPrograms/2");


                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Delete_Nonexisting_Training_Programs()
        {


            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                /*
                    ACT
                */
                var deleteResponse = await client.DeleteAsync($"/api/traingingPrograms/135123233");


                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);





            }
        }
    }
}
