using System;
using System.Threading.Tasks;
using TestAppAlex.Services;
using TestAppAlex.Repositories;
using TestAppAlex.Models;

public class UserServiceTests
{
    public static async Task Run()
    {
        Console.WriteLine("=== Starting Manual Unit Test ===");

        var mockRepo = new InMemoryUserRepository();
        var service = new UserService(mockRepo);

        var success = await service.RegisterAsync("Alex", "alex@test.com", "pass123");

        Console.WriteLine("Register success: " + success);
        Console.WriteLine(success == true ? " PASS" : " FAIL");
    }
}
