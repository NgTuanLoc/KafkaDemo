using MassTransit;
using SharedLibrary;

namespace Consumer;

public class MessageConsumer : IConsumer<ProductEntity>
{
    public Task Consume(ConsumeContext<ProductEntity> context)
    {
        Console.WriteLine("========================");
        Console.WriteLine(context.Message.Id);
        Console.WriteLine(context.Message.Name);
        Console.WriteLine(context.Message.Description);

        return Task.CompletedTask;
    }
}
