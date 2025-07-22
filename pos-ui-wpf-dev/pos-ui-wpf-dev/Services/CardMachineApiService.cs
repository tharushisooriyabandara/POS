using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace POS_UI.Services
{
    public class CardMachineApiService
    {
        private static readonly HttpClient _httpClient;
        static CardMachineApiService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _httpClient = new HttpClient(handler);
        }

        public class PairingRequest
        {
            [JsonPropertyName("deviceId")]
            public string DeviceId { get; set; }
            [JsonPropertyName("pairingCode")]
            public string PairingCode { get; set; }
        }

        public class PairingResponse
        {
            [JsonPropertyName("authToken")]
            public string AuthToken { get; set; }
        }

        public async Task<string> PairDeviceAsync(string ip, string port, string apiEndpoint, string deviceId, string pairingCode)
        {
            string url = $"https://{ip}:{port}{apiEndpoint}/pair?tid={deviceId}&pairingCode={pairingCode}";
            
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Pairing failed: {response.StatusCode}\n{error}");
                }
                var responseJson = await response.Content.ReadAsStringAsync();
                var pairingResponse = JsonSerializer.Deserialize<PairingResponse>(responseJson);
                if (pairingResponse == null || string.IsNullOrEmpty(pairingResponse.AuthToken))
                    throw new Exception("Pairing succeeded but no auth token returned.");
                return pairingResponse.AuthToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error pairing with card machine: {ex.Message}");
            }
        }

        public async Task<bool> CreateUserAsync(string ip, string port, string apiEndpoint, string terminalId, string userId, string userName, string password, bool supervisor, string authToken = null)
        {
            string url = $"https://{ip}:{port}{apiEndpoint}/user?userId={userId}&userName={userName}&password={password}&supervisor={supervisor}&tid={terminalId}";
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"Creating user: {url}");
                
                // Create a new request message to add headers
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                
                // Add Authorization header if auth token is provided
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                    System.Diagnostics.Debug.WriteLine($"Adding Authorization header: Bearer {authToken}");
                }
                
                var response = await _httpClient.SendAsync(request);
                System.Diagnostics.Debug.WriteLine($"Create user response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Create user error response: {error}");
                    return false;
                }
                
                var responseJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Create user success response: {responseJson}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create user exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string ip, string port, string apiEndpoint, string terminalId, string userId, string authToken = null)
        {
            string url = $"https://{ip}:{port}{apiEndpoint}/user?userId={userId}&tid={terminalId}";
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"Deleting user: {url}");
                
                // Create a new request message to add headers
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                
                // Add Authorization header if auth token is provided
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                    System.Diagnostics.Debug.WriteLine($"Adding Authorization header: Bearer {authToken}");
                }
                
                var response = await _httpClient.SendAsync(request);
                System.Diagnostics.Debug.WriteLine($"Delete user response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Delete user error response: {error}");
                    return false;
                }
                
                var responseJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Delete user success response: {responseJson}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete user exception: {ex.Message}");
                return false;
            }
        }
    }
} 