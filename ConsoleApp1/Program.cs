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
        
        var productsStatistic = productsStatisticCounter.productsStatistic;
        productsStatistic.Sort((x, y) => y.Quantity.CompareTo(x.Quantity));
        var resultStringBuilder = new StringBuilder();
        var productCounter = 1;
        foreach (var product in productsStatistic)
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
        var yearsProductsStatistics = new Dictionary<int, ProductsStatisticCounter>();

        var resultStringBuilder = new StringBuilder();
        foreach (var order in Orders)
        {
            if (!yearsProductsStatistics.ContainsKey(order.OrderDate.Year))
            {
                yearsProductsStatistics.Add(order.OrderDate.Year, new ProductsStatisticCounter());
            }
            foreach (var item in order.Items)
            {
                yearsProductsStatistics[order.OrderDate.Year].IncreaseProductQuantity(item);
            }
        }

        foreach (var year in yearsProductsStatistics.Keys.OrderByDescending(i => i))
        {
            double totalPrice = 0;
            foreach (var product in yearsProductsStatistics[year].productsStatistic)
            {
                var foundProduct = Products.Find(i => i.Id == product.ProductId);
                totalPrice += foundProduct.Price * product.Quantity;
            }
            
            var bestProductStatisticItem = yearsProductsStatistics[year].productsStatistic.MaxBy(i => i.Quantity);
            var bestProductName = Products.Find(i => i.Id == bestProductStatisticItem.ProductId).Name;
            
            resultStringBuilder.Append($"{year} - {String.Format("{0:0.00}", totalPrice)} руб.\r\n");
            resultStringBuilder.Append($"Most selling: {bestProductName} ({bestProductStatisticItem.Quantity} item(s))\r\n\n");
        }

        return resultStringBuilder.ToString();
    }

    private class ProductsStatisticCounter
    {
        public List<ProductStatisticItem> productsStatistic { get; }

        public ProductsStatisticCounter()
        {
            productsStatistic = new List<ProductStatisticItem>();
        }

        public void IncreaseProductQuantity(Order.OrderItem orderItem)
        {
            var foundedProduct = productsStatistic.Find(i => i.ProductId == orderItem.ProductId);
            if (foundedProduct != null)
            {
                foundedProduct.Quantity += orderItem.Quantity;
            }
            else
            {
                productsStatistic.Add(new ProductStatisticItem(orderItem.ProductId, orderItem.Quantity));
            }
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