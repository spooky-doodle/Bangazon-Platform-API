using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
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
        public async Task<IActionResult> Get([FromQuery] string _include)

        {
            using (SqlConnection conn = Connection)
            {
                string sqlCommandTxt;

                if (_include == "products")
                {
                    sqlCommandTxt = @"SELECT o.Id, o.CustomerId, o.PaymentTypeId, p.CustomerId AS PCId, p.[Description], p.Id AS ProductId, p.Price, p.ProductTypeId, p.Quantity, p.Title
                                        FROM [Order] o
                                        LEFT JOIN OrderProduct op ON op.OrderId = o.Id
                                        LEFT JOIN Product p ON p.Id = op.ProductId";
                } else if (_include == "customers")
                {
                    sqlCommandTxt = @"SELECT o.Id, o.CustomerId, o.PaymentTypeId, 
                                        c.Id AS CId, c.FirstName, c.LastName
                                        FROM [Order] o
                                        LEFT JOIN Customer c ON c.Id = o.CustomerId";
                } else
                {
                    sqlCommandTxt = @"SELECT Id, CustomerId, PaymentTypeId
                                         FROM [Order]";
                }


                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sqlCommandTxt;

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Order> orders = new List<Order>();

                    while (reader.Read())
                    {

                            Order order = new Order
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            };
                            try
                            {
                                order.PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"));

                            }
                            catch (SqlNullValueException)
                            {
                                // Don't do anything
                            }

                        if (_include == "products")
                        {

                            Product product = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("PCId")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                            };
                            order.Products = new List<Product>();

                            if (orders.Any(o => o.Id == order.Id))
                            {
                                Order existingOrder = orders.Find(o => o.Id == order.Id);
                                existingOrder.Products.Add(product);
                            }
                            else
                            {
                                order.Products.Add(product);
                                orders.Add(order);
                            }
                        } else if (_include == "customers")
                        {
                            Customer customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName"))
                            };

                            order.Customer = customer;
                            orders.Add(order);
                        } else
                        {
                            orders.Add(order);
                        }
                       
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
