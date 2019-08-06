using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IConfiguration _config;

        public OrdersController(IConfiguration config)
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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT o.Id, o.CustomerId, o.PaymentTypeId, p.CustomerId AS PCId, p.[Description], p.Id AS ProductId, p.Price, p.ProductTypeId, p.Quantity, p.Title
                                        FROM [Order] o
                                        JOIN OrderProduct op ON op.OrderId = o.Id
                                        JOIN Product p ON p.Id = op.ProductId";
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Order> orders = new List<Order>();

                    while (reader.Read())
                    {
                        Order order = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                            Products = new List<Product>()
                        };

                        orders.Add(order);

                        //Product product = new Product
                        //{
                        //    Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                        //    ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                        //    CustomerId = reader.GetInt32(reader.GetOrdinal("PCId")),
                        //    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                        //    Title = reader.GetString(reader.GetOrdinal("Title")),
                        //    Description = reader.GetString(reader.GetOrdinal("Description")),                       
                        //};

                        //if (orders.Any(o => o.Id == order.Id))
                        //{
                        //    Order existingOrder = orders.Find(o => o.Id == order.Id);
                        //    existingOrder.Products.Add(product);
                        //}
                        //else
                        //{
                        //    order.Products.Add(product);
                        //    orders.Add(order);
                        //}
                    }

                    reader.Close();

                    return Ok(orders);
                }
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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
