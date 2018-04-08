using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace RoutingExamples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            //The order of these things are important. 

            app.Use(async (context, next) =>
            {
                var path1 = "/page/10";
                RouteTemplate template = TemplateParser.Parse("page/{id}");
                var templateMatcher = new TemplateMatcher(template, new RouteValueDictionary());

                var routeData = new RouteValueDictionary();//This dictionary will be populated by the parameter template part (in this case "title")
                var isMatch1 = templateMatcher.TryMatch(path1, routeData);
                await context.Response.WriteAsync($"{path1} is match? {isMatch1} => route data value for 'id' is {routeData["id"]} \n");
                await next.Invoke();
            });

            app.Use(async (context, next) =>
            {
                var path = "/page/gone/a";
                RouteTemplate template = TemplateParser.Parse("page/gone/{id}");
                var templateMatcher = new TemplateMatcher(template, new RouteValueDictionary());

                var routeData = new RouteValueDictionary();//This dictionary will be populated by the parameter template part (in this case "title")
                var isMatch1 = templateMatcher.TryMatch(path, routeData);
                await context.Response.WriteAsync($"{path} is match? {isMatch1} => route data value for 'id' is {routeData["id"]} \n");
            });

            var routes = new RouteBuilder(app);
            routes.MapGet("", (context) => {
                return context.Response.WriteAsync($"Home Page. Try /good or /good/morning.");
            });

            routes.MapGet("{*path}", (context) => {
                var routeData = context.GetRouteData();
                var path = routeData.Values;
                return context.Response.WriteAsync($"Path: {string.Join(",", path)}");
            });

            app.UseRouter(routes.Build());

            var defaultHandler = new RouteHandler(context =>
            {
                var routeValues = context.GetRouteData().Values;
                context.Response.Headers.Add("Content-Type", "text/html");
                return context.Response.WriteAsync(
                    $@"
                    <html><head><body>
                    <h1>Routing</h1>
                    Click on the following links to see the changes
                    <ul>
                        <li><a href=""/try"">/try</a></li>
                        <li><a href=""/do/33"">/do/33</a></li>
                        <li><a href=""/values/11/again"">/values/11/again</a></li>
                    </ul> 
                    <br/>
                    Path: {context.Request.Path}. 
                    <br/>
                    <br/>
                    Route values from context.GetRouteData().Values: {string.Join(", ", routeValues)}
                    <br/>
                    Note:
                    <br/>
                    context.GetRouteData() returns nothing regardless what kind of path you are requesting, e.g. '/hello-x'
                    </body></html>
                    ");
            });
            app.UseRouter(defaultHandler);

        app.Use(async (context, next) =>
            {
                context.Items["Greeting"] = "Hello World";
                await next.Invoke();
                await context.Response.WriteAsync($"{context.Items["Goodbye"]}\n");
            });

            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync($"{context.Items["Greeting"]}\n");
                context.Items["Goodbye"] = "Goodbye for now";
            });

            app.UseStaticFiles();


        }
    }
}
