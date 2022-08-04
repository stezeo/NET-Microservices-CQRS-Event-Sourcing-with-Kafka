using Confluent.Kafka;
using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization;
using Post.Cmd.Api.Commands;
using Post.Cmd.Domain.Aggregates;
using Post.Cmd.Infrastructure.Config;
using Post.Cmd.Infrastructure.Dispatchers;
using Post.Cmd.Infrastructure.Handlers;
using Post.Cmd.Infrastructure.Producers;
using Post.Cmd.Infrastructure.Repositories;
using Post.Cmd.Infrastructure.Stores;
using Post.Common.Events;


namespace Post.Cmd.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MongoDbConfig>(Configuration.GetSection(nameof(MongoDbConfig)));
            services.Configure<ProducerConfig>(Configuration.GetSection(nameof(ProducerConfig)));
            services.AddScoped<IEventStoreRepository, EventStoreRepository>();
            services.AddScoped<IEventProducer, EventProducer>();
            services.AddScoped<IEventStore, EventStore>();
            services.AddScoped<IEventSourcingHandler<PostAggregate>, EventSourcingHandler>();
            services.AddScoped<ICommandHandler, CommandHandler>();
            services.AddControllers();
            services.AddSwaggerGen();

            var commandHandler = services.BuildServiceProvider().GetRequiredService<ICommandHandler>();
            var dispatcher = new CommandDispatcher();
            dispatcher.RegisterHandler<NewPostCommand>(commandHandler.HandleAsync);
            dispatcher.RegisterHandler<EditMessageCommand>(commandHandler.HandleAsync);
            dispatcher.RegisterHandler<LikePostCommand>(commandHandler.HandleAsync);
            dispatcher.RegisterHandler<AddCommentCommand>(commandHandler.HandleAsync);
            dispatcher.RegisterHandler<EditCommentCommand>(commandHandler.HandleAsync);
            dispatcher.RegisterHandler<RemoveCommentCommand>(commandHandler.HandleAsync);
            dispatcher.RegisterHandler<DeletePostCommand>(commandHandler.HandleAsync);
            services.AddSingleton<ICommandDispatcher>(_ => dispatcher);

            BsonClassMap.RegisterClassMap<BaseEvent>();
            BsonClassMap.RegisterClassMap<PostCreatedEvent>();
            BsonClassMap.RegisterClassMap<MessageUpdatedEvent>();
            BsonClassMap.RegisterClassMap<PostLikedEvent>();
            BsonClassMap.RegisterClassMap<CommentAddedEvent>();
            BsonClassMap.RegisterClassMap<CommentUpdatedEvent>();
            BsonClassMap.RegisterClassMap<CommentRemovedEvent>();
            BsonClassMap.RegisterClassMap<PostRemovedEvent>();
        }

        // Configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseRouting();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
