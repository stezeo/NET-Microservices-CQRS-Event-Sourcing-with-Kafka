using Confluent.Kafka;
using CQRS.Core.Consumers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Post.Query.Api.Queries;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.Consumers;
using Post.Query.Infrastructure.Handlers;
using Post.Query.Infrastructure.Repositories;
using Microsoft.Extensions.Hosting;
using Post.Query.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using CQRS.Core.Infrastructure;
using Post.Query.Domain.Entities;
using Post.Query.Infrastructure.Dispatchers;

namespace Post.Query.Api
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
            Action<DbContextOptionsBuilder> configureDbContext = o => o.UseLazyLoadingProxies().UseSqlServer(Configuration.GetConnectionString("SqlServer"));

            services.AddDbContext<DatabaseContext>(configureDbContext);
            services.AddSingleton<DatabaseContextFactory>(new DatabaseContextFactory(configureDbContext));
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IQueryHandler, QueryHandler>();
            services.AddScoped<IEventHandler, Post.Query.Infrastructure.Handlers.EventHandler>();
            services.Configure<ConsumerConfig>(Configuration.GetSection(nameof(ConsumerConfig)));
            services.AddScoped<IEventConsumer, EventConsumer>();

            var queryHandler = services.BuildServiceProvider().GetRequiredService<IQueryHandler>();
            var dispatcher = new QueryDispatcher();
            dispatcher.RegisterHandler<FindAllPostsQuery>(queryHandler.HandleAsync);
            dispatcher.RegisterHandler<FindPostByIdQuery>(queryHandler.HandleAsync);
            dispatcher.RegisterHandler<FindPostsByAuthorQuery>(queryHandler.HandleAsync);
            dispatcher.RegisterHandler<FindPostsWithCommentsQuery>(queryHandler.HandleAsync);
            dispatcher.RegisterHandler<FindPostsWithLikesQuery>(queryHandler.HandleAsync);
            services.AddScoped<IQueryDispatcher<PostEntity>>(_ => dispatcher);

            services.AddControllers();
            services.AddHostedService<ConsumerHostedService>();
        }

        // Configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
