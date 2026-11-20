namespace Startup_Proj.Midelwares
{
    public class MyCustomMiddleware
    {
        private readonly RequestDelegate _next;
        
        public MyCustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            await context.Response.WriteAsync("new MyCustomMiddleware started!!\n");
            await _next(context);
            await context.Response.WriteAsync("After MyCustomMiddleware responce\n");

        }

    }
}