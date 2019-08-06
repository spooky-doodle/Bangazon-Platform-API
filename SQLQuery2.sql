SELECT o.Id, o.CustomerId, o.PaymentTypeId, p.CustomerId AS PCId, p.[Description], p.Id AS ProductId, p.Price, p.ProductTypeId, p.Quantity, p.Title
                                        FROM [Order] o
                                        LEFT JOIN OrderProduct op ON op.OrderId = o.Id
                                        LEFT JOIN Product p ON p.Id = op.ProductId
										WHERE o.Id = 1