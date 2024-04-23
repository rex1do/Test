using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

internal class depositedReceiver
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Deposited messages:");
        try { getMessage(); }
        catch { Console.WriteLine("Connection refused"); }
        Console.ReadLine();
    }

    public static void getMessage()
    {
        var rabbitConnection = new RabbitConnection();
        var channel = rabbitConnection.getConnection().CreateModel();

        var consumer = new EventingBasicConsumer(channel);

        channel.BasicConsume("Deposited", autoAck: false, consumer);

        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(message);
            channel.BasicAck(e.DeliveryTag, false);
        };
    }
}

public class RabbitConnection
{
    public IConnection getConnection()
    {
        var connectionFactory = new ConnectionFactory(); 
        connectionFactory.HostName = "localhost";
        connectionFactory.Port = 5672;
        connectionFactory.UserName = "depositedReceiver";
        connectionFactory.Password = "depositedReceiver";
        return connectionFactory.CreateConnection();
    }
}