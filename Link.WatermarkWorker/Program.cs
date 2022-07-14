using Link.WatermarkWorker;
using MassTransit;



IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {

            x.AddConsumer<WaterMarkService>();

            x.UsingRabbitMq((content, cfg) =>
            {
                cfg.Host("localhost", "/", host =>
                {
                    host.Username("guest");
                    host.Password("guest");
                });
                cfg.ReceiveEndpoint("addwatermark", e =>
                {

                    e.ConfigureConsumer<WaterMarkService>(content);
                });
            });

        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
