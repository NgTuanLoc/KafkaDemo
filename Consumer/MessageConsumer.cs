using MassTransit;
using SharedLibrary;

namespace Consumer;

public class MessageConsumer : IConsumer<ProductEntity>
{
    public Task Consume(ConsumeContext<ProductEntity> context)
    {
        Console.WriteLine("========================");
        Console.WriteLine(context.Message.Name);
        return Task.CompletedTask;
    }
}
