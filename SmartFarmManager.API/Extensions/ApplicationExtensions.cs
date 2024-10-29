namespace SmartFarmManager.API.Extensions
{
    public static class ApplicationExtensions
    {
        public static void UseInfrastructure(this WebApplication app)
        {


            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");

            });



            app.UseCors("CORS");

            app.UseHttpsRedirection();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseAuthorization();



            app.MapControllers();



        }
    }
}
