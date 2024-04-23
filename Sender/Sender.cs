using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

internal class Sender
{
    public static void Main(string[] args)
    {
        createMessage();
    }

    public static void createMessage()
    {
        Console.WriteLine("---------------------------------------------------------\n");
        Console.Write("OrderId: ");
        string orderId = Console.ReadLine();

        Console.Write("Operation (Deposited/Refunded): ");
        string operation = Console.ReadLine();

        DateTime createDate = DateTime.Now;

        Message message = new Message()
        {
            OrderId = orderId,
            Operation = operation,
            CreateDate = createDate
        };

        string jsonMessage = JsonSerializer.Serialize(message);
        byte[] bodyMessage = Encoding.UTF8.GetBytes(jsonMessage);

        message.sendMessage(bodyMessage);
    }
}

public class RabbitConnection
{
    public IConnection getConnection()
    {
        var connectionFactory = new ConnectionFactory();
        connectionFactory.HostName = "localhost";
        connectionFactory.Port = 5672;
        connectionFactory.UserName = "guest";
        connectionFactory.Password = "guest";
        return connectionFactory.CreateConnection();
    }
}

public class Message
{
    private bool messageIsEmpty = false;

    private string orderId;
    required public string OrderId
    {
        get
        {
            return orderId;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine("OrderId can't be empty");
                messageIsEmpty = true;
            }
            else
                orderId = value;
        }
    }

    private string operation;
    required public string Operation
    {
        get
        {
            return operation;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Operation can't be empty\n");
                messageIsEmpty = true;
            }
            else if (value != "Deposited" && value != "Refunded")
            {
                Console.WriteLine("Invalid Operation\n");
                messageIsEmpty = true;
            }
            else
                operation = value;
        }
    }

    required public DateTime CreateDate { get; set; }

    public void sendMessage(byte[] bodyMessage)
    {
        if (!messageIsEmpty)
        {
            Console.WriteLine($"\nYour message:\nOrderId: {OrderId}\nOperation: {Operation}\nCreateDate: {CreateDate}\n");

            Console.WriteLine("Message sending\n");

            try { postMessage(bodyMessage, operation); }
            catch { Console.WriteLine("Connection refused\n"); }

            Sender.createMessage();
        }

        else
            Sender.createMessage();
    }

    public void postMessage(byte[] bodyMessage, string Operation)
    {
        var rabbitConnection = new RabbitConnection();

        using (var connection = rabbitConnection.getConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.BasicPublish(exchange: "testExchange",
                                     routingKey: Operation,
                                     basicProperties: null,
                                     body: bodyMessage);

                Console.WriteLine("Message sended\n");
            }
        }
    }
}
