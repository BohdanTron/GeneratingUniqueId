using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SqlServerImplementation;

await using var context = new AppDbContext();

const string text = "SELECT @result =  (NEXT VALUE FOR dbo.MySequence)";

var result = new SqlParameter("@result", System.Data.SqlDbType.BigInt)
{
    Direction = System.Data.ParameterDirection.Output
};

for (var i = 0; i < 10; i++)
{
    await context.Database.ExecuteSqlRawAsync(text, result);

    Console.WriteLine((long)result.Value);
}
