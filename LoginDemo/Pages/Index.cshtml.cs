using LoginDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceReferences;
using System.Text.Json;

public class IndexModel : PageModel
{
    [BindProperty] public string Login { get; set; }
    [BindProperty] public string Password { get; set; }
    public Entity EntityData { get; set; }
    public string Message { get; set; }
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var client = new ICUTechClient(ICUTechClient.EndpointConfiguration.IICUTechPort);
            var response = await client.LoginAsync(Login, Password,"");

            if (response != null && !string.IsNullOrEmpty(response.@return))
            {
                try
                {
                    using var doc = JsonDocument.Parse(response.@return);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("ResultCode", out var resultCode))
                    {
                        var message = root.GetProperty("ResultMessage").GetString();
                        Message = $"Login failed: {message}";
                        IsSuccess = false;
                    }
                    else if (root.TryGetProperty("EntityId", out _))
                    {
                        EntityData = JsonSerializer.Deserialize<Entity>(response.@return);
                        Message = "Login successful";
                        IsSuccess = true;
                    }
                    else
                    {
                        Message = "Unknown response format.";
                        IsSuccess = false;
                    }
                }
                catch
                {
                    Message = "Error parsing server response.";
                    IsSuccess = false;
                }
            }
            else
            {
                Message = "Empty response from server.";
                IsSuccess = false;
            }

            await client.CloseAsync();
        }
        catch (Exception ex)
        {
            Message = $"Error: {ex.Message}";
            IsSuccess = false;
        }

        return Page();
    }
}
