using System.Text;

public class Store
{
    public List<Product> Products { get; set; }
    public List<Order> Orders { get; set; }

    /// <summary>
    /// Формирует строку со статистикой продаж продуктов
    /// Сортировка - по убыванию кол-ва проданных продуктов
    /// </summary>
    /// <param name="year">Год, за который подсчитывается статистика</param>
    public string GetProductStatistics(int year)
    {
        var productsStatisticCounter = new ProductsStatisticCounter();
        foreach (var order in Orders.Where(obj => obj.OrderDate.Year == year))
        {
            foreach (var item in order.Items)
            {
                productsStatisticCounter.IncreaseProductQuantity(item);
            }
        }

        var productsStatistics = productsStatisticCounter.GetStatisticQuantityFiltered();
        var resultStringBuilder = new StringBuilder();
        var productCounter = 1;
        foreach (var product in productsStatistics)
        {
            var foundedProduct = Products.Find(i => i.Id == product.ProductId);
            resultStringBuilder.Append($"{productCounter++}) {foundedProduct.Name} {product.Quantity} item(s)\r\n");
        }

        return resultStringBuilder.ToString();
    }

    /// <summary>
    /// Формирует строку со статистикой продаж продуктов по годам
    /// Сортировка - по убыванию годов.
    /// Выводятся все года, в которых были продажи продуктов
    /// </summary>
    public string GetYearsStatistics()
    {
        var yearsList = new List<int>();
        foreach (var order in Orders)
        {
            if (!yearsList.Contains(order.OrderDate.Year))
            {
                yearsList.Add(order.OrderDate.Year);
            }
        }

        yearsList.Sort((x, y) => y.CompareTo(x));

        var resultStringBuilder = new StringBuilder();

        foreach (var year in yearsList)
        {
            var productsStatisticCounter = new ProductsStatisticCounter();

            foreach (var order in Orders.Where(obj => obj.OrderDate.Year == year))
            {
                foreach (var item in order.Items)
                {
                    productsStatisticCounter.IncreaseProductQuantity(item);
                }
            }

            var productsStatistics = productsStatisticCounter.GetStatisticQuantityFiltered();
            double totalPrice = 0;
            foreach (var product in productsStatistics)
            {
                var foundProduct = Products.Find(i => i.Id == product.ProductId);
                totalPrice += foundProduct.Price * product.Quantity;
            }

            resultStringBuilder.Append($"{year} - {String.Format("{0:0.00}", totalPrice)} руб.\r\n");
            var bestProduct = Products.Find(i => i.Id == productsStatistics[0].ProductId);
            resultStringBuilder.Append(
                $"Most selling: {bestProduct.Name} ({productsStatistics[0].Quantity} item(s))\r\n\n");
        }

        return resultStringBuilder.ToString();
    }

    private class ProductsStatisticCounter
    {
        private readonly List<ProductStatisticItem> _productsQuantities = new List<ProductStatisticItem>();

        public void IncreaseProductQuantity(Order.OrderItem orderItem)
        {
            var foundedProduct = _productsQuantities.Find(i => i.ProductId == orderItem.ProductId);
            if (foundedProduct != null)
            {
                foundedProduct.Quantity += orderItem.Quantity;
            }
            else
            {
                _productsQuantities.Add(new ProductStatisticItem(orderItem.ProductId, orderItem.Quantity));
            }
        }

        public List<ProductStatisticItem> GetStatisticQuantityFiltered()
        {
            var sortedProductsList = new List<ProductStatisticItem>(_productsQuantities);
            sortedProductsList.Sort((x, y) => y.Quantity.CompareTo(x.Quantity));
            return sortedProductsList;
        }
    }

    private class ProductStatisticItem
    {
        public int ProductId { get; }
        public int Quantity { get; set; }


        public ProductStatisticItem(int productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
}

public class Order
{
    public int UserId { get; set; }
    public List<OrderItem> Items { get; set; }
    public DateTime OrderDate { get; set; }

    public class OrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

public class Program
{
    static void Main(string[] args)
    {
        var store = new Store
        {
            Products = new List<Product>
            {
                new() { Id = 1, Name = "Product 1", Price = 1000d },
                new() { Id = 2, Name = "Product 2", Price = 3000d },
                new() { Id = 3, Name = "Product 3", Price = 10000d }
            },
            Orders = new List<Order>
            {
                new()
                {
                    UserId = 1,
                    OrderDate = DateTime.UtcNow,
                    Items = new List<Order.OrderItem>
                    {
                        new() { ProductId = 1, Quantity = 2 }
                    }
                },
                new()
                {
                    UserId = 1,
                    OrderDate = DateTime.UtcNow,
                    Items = new List<Order.OrderItem>
                    {
                        new() { ProductId = 1, Quantity = 1 },
                        new() { ProductId = 2, Quantity = 1 },
                        new() { ProductId = 3, Quantity = 1 }
                    }
                },
                new()
                {
                    UserId = 1,
                    OrderDate = new DateTime(2015, 7, 20, 18, 30, 25),
                    Items = new List<Order.OrderItem>
                    {
                        new() { ProductId = 1, Quantity = 8 }
                    }
                },
                new()
                {
                    UserId = 1,
                    OrderDate = new DateTime(2015, 7, 20, 18, 30, 25),
                    Items = new List<Order.OrderItem>
                    {
                        new() { ProductId = 1, Quantity = 1 },
                        new() { ProductId = 2, Quantity = 1 },
                        new() { ProductId = 3, Quantity = 1 }
                    }
                }
            }
        };

        Console.WriteLine(store.GetProductStatistics(2022));
        Console.WriteLine(store.GetProductStatistics(2015));
        Console.WriteLine(store.GetYearsStatistics());
    }
}