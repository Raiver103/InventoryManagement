//using Microsoft.AspNetCore.Components;
//using Microsoft.JSInterop;
//using System.Net.Http.Headers;
//using System.Threading.Tasks;

//public class AuthService
//{
//    private readonly HttpClient _httpClient;
//    private readonly NavigationManager _navigationManager;
//    private readonly IJSRuntime _jsRuntime;
//    private IJSObjectReference? _jsModule;

//    public AuthService(HttpClient httpClient, NavigationManager navigationManager, IJSRuntime jsRuntime)
//    {
//        _httpClient = httpClient;
//        _navigationManager = navigationManager;
//        _jsRuntime = jsRuntime;
//    }

//    public async Task InitializeAsync()
//    {
//        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/auth.js");
//    }

//    public async Task Login(string returnUrl = "/")
//    {
//        _navigationManager.NavigateTo($"https://localhost:7098/auth/login?returnUrl={returnUrl}");
//    }

//    public async Task Logout()
//    {
//        if (_jsModule != null)
//        {
//            await _jsModule.InvokeVoidAsync("removeAuthToken");
//        }
//        _navigationManager.NavigateTo("https://localhost:7098/auth/logout");
//    }

//    public async Task<string> GetTokenAsync()
//    {
//        if (_jsModule == null)
//        {
//            return string.Empty; // Prevents interop call during prerendering
//        }

//        return await _jsModule.InvokeAsync<string>("getAuthToken");
//    }

//    public async Task SetTokenAsync(string token)
//    {
//        if (_jsModule != null)
//        {
//            await _jsModule.InvokeVoidAsync("setAuthToken", token);
//        }
//        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//    }
//}
