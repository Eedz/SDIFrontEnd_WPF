using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
namespace SDIFrontEnd_WPF;
public abstract class ApiServiceBase
{
    protected readonly HttpClient _http;

    protected ApiServiceBase(HttpClient httpClient)
    {
        _http = httpClient;
    }

    // --------------------------------------------------
    // Health Check
    // --------------------------------------------------
    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var response = await _http.GetAsync("https://localhost:7137/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // --------------------------------------------------
    // Centralized Execution Wrapper
    // --------------------------------------------------
    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> apiCall)
    {
        try
        {
            return await apiCall();
        }
        catch (HttpRequestException)
        {
            HandleConnectionLost();
            throw;
        }
        catch (TaskCanceledException)
        {
            HandleConnectionLost();
            throw;
        }
    }

    protected async Task ExecuteAsync(Func<Task> apiCall)
    {
        try
        {
            await apiCall();
        }
        catch (HttpRequestException)
        {
            HandleConnectionLost();
            throw;
        }
        catch (TaskCanceledException)
        {
            HandleConnectionLost();
            throw;
        }
    }

    private void HandleConnectionLost()
    {
        MessageBox.Show(
            "Connection to the server was lost. The application will now close.",
            "Server Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        Application.Current.Shutdown();
    }
}