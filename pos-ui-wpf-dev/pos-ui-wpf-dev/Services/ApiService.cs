using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace POS_UI.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://pos-go-api-dev.delivergate.com";
        private readonly SettingsService _settingsService;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            // receive responses in JSON format
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            _settingsService = new SettingsService();
            var (tenantCode, outletCode) = _settingsService.LoadSettings();
            _httpClient.DefaultRequestHeaders.Add("x-tenant-code", tenantCode);
            _httpClient.DefaultRequestHeaders.Add("x-outlet-code", outletCode);
        }

        public async Task<(string accessToken, string refreshToken, DateTime accessTokenExpiry, DateTime refreshTokenExpiry)> LoginAsync(string email, string pin)
        {
            var form = new[]
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("pin", pin)
            };
            var content = new FormUrlEncodedContent(form);

            var response = await _httpClient.PostAsync("/api/v1/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Status: {response.StatusCode}\n{error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");
            string accessToken = data.GetProperty("accessToken").GetString();
            string refreshToken = data.GetProperty("refreshToken").GetString();
            DateTime accessTokenExpiry = DateTime.MinValue;
            DateTime refreshTokenExpiry = DateTime.MinValue;
            if (data.TryGetProperty("accessTokenExpiry", out var accessExpProp))
            {
                if (accessExpProp.ValueKind == JsonValueKind.Number)
                    accessTokenExpiry = DateTimeOffset.FromUnixTimeSeconds(accessExpProp.GetInt64()).UtcDateTime;
                else if (accessExpProp.ValueKind == JsonValueKind.String)
                    accessTokenExpiry = DateTime.Parse(accessExpProp.GetString()).ToUniversalTime();
            }
            if (data.TryGetProperty("refreshTokenExpiry", out var refreshExpProp))
            {
                if (refreshExpProp.ValueKind == JsonValueKind.Number)
                    refreshTokenExpiry = DateTimeOffset.FromUnixTimeSeconds(refreshExpProp.GetInt64()).UtcDateTime;
                else if (refreshExpProp.ValueKind == JsonValueKind.String)
                    refreshTokenExpiry = DateTime.Parse(refreshExpProp.GetString()).ToUniversalTime();
            }
            return (accessToken, refreshToken, accessTokenExpiry, refreshTokenExpiry);
        }

        public async Task<(string accessToken, string refreshToken, DateTime accessTokenExpiry, DateTime refreshTokenExpiry)> RefreshTokenAsync(string refreshToken)
        {
            try
            {  
                var form = new[]
                {
                    new KeyValuePair<string, string>("refreshToken", refreshToken)
                };
                var content = new FormUrlEncodedContent(form);
                

                //System.Windows.MessageBox.Show("Making API call to /api/v1/auth/refresh...");
                //Console.WriteLine("Making API call to /api/v1/auth/refresh...");
                //Console.WriteLine($"Full URL: {_httpClient.BaseAddress}/api/v1/auth/refresh");
                HttpResponseMessage response;
                try
                {
                    // Add a timeout to prevent hanging
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                    //Console.WriteLine("Starting HTTP request with 5-second timeout...");
                    //Console.WriteLine("About to call PostAsync...");
                    
                    // Test if we can reach the base URL first
                    try
                    {
                        //Console.WriteLine("Testing base URL connectivity...");
                       // var testResponse = await _httpClient.GetAsync("", cts.Token);
                        //Console.WriteLine($"Base URL test response: {testResponse.StatusCode}");
                    }
                    catch (Exception testEx)
                    {
                        //Console.WriteLine($"Base URL test failed: {testEx.Message}");
                        // Continue anyway, the main request might still work
                    }
                    
                    response = await _httpClient.PostAsync("/api/v1/auth/refresh", content, cts.Token);
                    //Console.WriteLine($"API response status: {response.StatusCode}");
                    //System.Windows.MessageBox.Show($"API response status: {response.StatusCode}");
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    //Console.WriteLine("API call timed out after 5 seconds");
                    //System.Windows.MessageBox.Show("API call timed out after 5 seconds");
                    throw new Exception("API call timed out");
                }
                catch (System.Net.Http.HttpRequestException httpEx)
                {
                    //Console.WriteLine($"HTTP request failed: {httpEx.Message}");
                    //System.Windows.MessageBox.Show($"HTTP request failed: {httpEx.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Unexpected error during API call: {ex.Message}");
                    //Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    //System.Windows.MessageBox.Show($"Unexpected error during API call: {ex.Message}");
                    throw;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine($"API error: {error}");
                    //System.Windows.MessageBox.Show($"API error: {error}");
                    throw new Exception($"Status: {response.StatusCode}\n{error}");
                }

                //System.Windows.MessageBox.Show("Reading response content...");
                //Console.WriteLine("Reading response content...");
                var json = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"Response JSON: {json}");
                //System.Windows.MessageBox.Show($"Response JSON: {json}");
                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                System.Windows.MessageBox.Show("Parsing token data...");
                Console.WriteLine("Parsing token data...");
                string accessToken = data.GetProperty("accessToken").GetString();
                string newRefreshToken = data.GetProperty("refreshToken").GetString();
                DateTime accessTokenExpiry = DateTime.MinValue;
                DateTime refreshTokenExpiry = DateTime.MinValue;
                if (data.TryGetProperty("accessTokenExpiry", out var accessExpProp))
                {
                    if (accessExpProp.ValueKind == JsonValueKind.Number)
                        accessTokenExpiry = DateTimeOffset.FromUnixTimeSeconds(accessExpProp.GetInt64()).UtcDateTime;
                    else if (accessExpProp.ValueKind == JsonValueKind.String)
                        accessTokenExpiry = DateTime.Parse(accessExpProp.GetString()).ToUniversalTime();
                }
                if (data.TryGetProperty("refreshTokenExpiry", out var refreshExpProp))
                {
                    if (refreshExpProp.ValueKind == JsonValueKind.Number)
                        refreshTokenExpiry = DateTimeOffset.FromUnixTimeSeconds(refreshExpProp.GetInt64()).UtcDateTime;
                    else if (refreshExpProp.ValueKind == JsonValueKind.String)
                        refreshTokenExpiry = DateTime.Parse(refreshExpProp.GetString()).ToUniversalTime();
                }
                System.Windows.MessageBox.Show("RefreshTokenAsync completed successfully");
                Console.WriteLine("RefreshTokenAsync completed successfully");
                return (accessToken, newRefreshToken, accessTokenExpiry, refreshTokenExpiry);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RefreshTokenAsync error: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
                System.Windows.MessageBox.Show("RefreshTokenAsync error: " + ex.Message + "\n\nStackTrace: " + ex.StackTrace);
                throw; // Re-throw to be caught by the calling method
            }
        }

        public void SetBearerToken(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }
        
        public async Task<string> PlaceOrderAsync(object orderRequest)
        {
            var json = JsonSerializer.Serialize(orderRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/v1/orders", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Status: {response.StatusCode}\n{responseBody}");
            }
            return responseBody;
        }
       /*public void UpdateTenantCode()
        {
            var (tenantCode, outletCode) = _settingsService.LoadSettings();
            _httpClient.DefaultRequestHeaders.Remove("x-tenant-code");
            _httpClient.DefaultRequestHeaders.Add("x-tenant-code", tenantCode);
            _httpClient.DefaultRequestHeaders.Remove("x-outlet-code");
            _httpClient.DefaultRequestHeaders.Add("x-outlet-code", outletCode);
        }*/

        public async Task<List<POS_UI.Models.UserModel>> GetUsersAsync()
        {
            // Set bearer token from settings
            var accessToken = POS_UI.Properties.Settings.Default.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                SetBearerToken(accessToken);
            }

            var response = await _httpClient.GetAsync("/api/v1/users/");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Status: {response.StatusCode}\n{error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            //System.Windows.MessageBox.Show("User API JSON:\n" + json);
            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");
            var users = new List<POS_UI.Models.UserModel>();
            foreach (var userElem in data.EnumerateArray())
            {
                users.Add(new POS_UI.Models.UserModel
                {
                    FirstName = userElem.GetProperty("first_name").GetString(),
                    LastName = userElem.GetProperty("last_name").GetString(),
                    Email = userElem.GetProperty("email").GetString(),
                    Role = userElem.GetProperty("role").GetString(),
                    // Pin is not returned by API for security, so leave it null
                });
            }
            return users;
        }

        /*public async Task<POS_UI.Models.UserModel> GetUserByEmailAsync(string email)
        {
            var users = await GetUsersAsync();
            return users.FirstOrDefault(u => u.Email != null && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }*/

        public async Task<POS_UI.Models.UserModel> GetUserByIdAsync(string id)
        {
            var accessToken = POS_UI.Properties.Settings.Default.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                SetBearerToken(accessToken);
            }
            var response = await _httpClient.GetAsync($"/api/v1/users/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Status: {response.StatusCode}\n{error}");
            }
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            string GetString(JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.String)
                    return element.GetString();
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("String", out var strProp))
                    return strProp.GetString();
                return "";
            }

            return new POS_UI.Models.UserModel
            {
                FirstName = data.TryGetProperty("first_name", out var nameProp) ? GetString(nameProp) : "",
                LastName = data.TryGetProperty("last_name", out var lastNameProp) ? GetString(lastNameProp) : "",
                Email = data.TryGetProperty("email", out var emailProp) ? GetString(emailProp) : "",
                Role = data.TryGetProperty("role", out var roleProp) ? GetString(roleProp) : ""
            };
        }

        public async Task<(List<string> Categories, List<Models.ProductItemModel> Products)> GetProductsAndCategoriesAsync()
        {
            try
            {
                //Retrieves the stored access token from application settings.
                var accessToken = POS_UI.Properties.Settings.Default.AccessToken;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    SetBearerToken(accessToken);
                }
                //Makes an HTTP GET request to the API endpoint to retrieve the products and categories.
                var response = await _httpClient.GetAsync("/api/v1/main-menu/65/categories/webshop-brand/1/shop/2");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Status: {response.StatusCode}\n{error}");
                }
                //Reads the response content as a string.
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                //Initializes two lists to store the categories and products.
                var categories = new List<string>();
                var products = new List<Models.ProductItemModel>();

                //Iterates through each category in the data.
                foreach (var categoryProperty in data.EnumerateObject())
                {
                    var categoryName = categoryProperty.Name;
                    categories.Add(categoryName);

                    if (categoryProperty.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var productElement in categoryProperty.Value.EnumerateArray())
                        {
                            var product = new Models.ProductItemModel
                            {
                                Id = productElement.TryGetProperty("id", out var idElement) ? idElement.GetInt32() : 0,
                                ItemName = productElement.TryGetProperty("title", out var titleElement) ? titleElement.GetString() : "No Name",
                                Category = categoryName,
                                //ImageUrl = productElement.TryGetProperty("image_url", out var imageElement) && imageElement.ValueKind == JsonValueKind.String ? imageElement.GetString() : null,
                                Size = "Default" 
                            };

                            if (productElement.TryGetProperty("price", out var priceElement) && decimal.TryParse(priceElement.GetString(), out decimal price))
                            {
                                product.Price = price;
                            }
                            else
                            {
                                product.Price = 0.0m;
                            }

                            // Parse modifiers if they exist
                            if (productElement.TryGetProperty("modifiers", out var modifiersElement) && modifiersElement.ValueKind == JsonValueKind.Array)
                            {
                                var modifiers = new List<Models.ModifierModel>();
                                foreach (var modifierElement in modifiersElement.EnumerateArray())
                                {
                                    // Get the modifier object
                                    if (modifierElement.TryGetProperty("modifier", out var modifierObj))
                                    {
                                        var modifier = new Models.ModifierModel
                                        {
                                            Id = modifierObj.TryGetProperty("id", out var modIdElement) ? modIdElement.GetInt32() : 0,
                                            Title = modifierObj.TryGetProperty("title", out var modTitleElement) ? modTitleElement.GetString() : "#####",
                                            MinPermitted = modifierObj.TryGetProperty("min_permitted", out var minPermittedElement) ? minPermittedElement.GetInt32() : 0,
                                            MaxPermitted = modifierObj.TryGetProperty("max_permitted", out var maxPermittedElement) ? maxPermittedElement.GetInt32() : 0
                                        };
                                        //MessageBox.Show($"Modifier: {modifier.Title}, MinPermitted: {modifier.MinPermitted}, MaxPermitted: {modifier.MaxPermitted}");

                                        // Parse modifier items if they exist
                                        if (modifierElement.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                                        {
                                            var modifierItems = new List<Models.ModifierItemModel>();
                                            foreach (var itemElement in itemsElement.EnumerateArray())
                                            {
                                                decimal priceValue = 0.0m;
                                                if (itemElement.TryGetProperty("price", out var itemPriceElement))
                                                {
                                                    if (itemPriceElement.ValueKind == JsonValueKind.Number)
                                                        priceValue = itemPriceElement.GetDecimal();
                                                    else if (itemPriceElement.ValueKind == JsonValueKind.String && decimal.TryParse(itemPriceElement.GetString(), out decimal parsed))
                                                        priceValue = parsed;
                                                }
                                                var modifierItem = new Models.ModifierItemModel
                                                {
                                                    Id = itemElement.TryGetProperty("id", out var itemIdElement) ? itemIdElement.GetInt32() : 0,
                                                    ItemName = itemElement.TryGetProperty("item_name", out var itemNameElement) ? itemNameElement.GetString() : "#####",
                                                    ItemPrice = priceValue
                                                };

                                                // Parse nested modifiers if they exist
                                                if (itemElement.TryGetProperty("modifier_list", out var nestedModifiersElement) && nestedModifiersElement.ValueKind == JsonValueKind.Array)
                                                {
                                                    var nestedModifiers = new List<Models.ModifierModel>();
                                                    foreach (var nestedModifierElement in nestedModifiersElement.EnumerateArray())
                                                    {
                                                        if (nestedModifierElement.TryGetProperty("modifier", out var nestedModifierObj))
                                                        {
                                                            var nestedModifier = new Models.ModifierModel
                                                            {
                                                                Id = nestedModifierObj.TryGetProperty("id", out var nestedModIdElement) ? nestedModIdElement.GetInt32() : 0,
                                                                Title = nestedModifierObj.TryGetProperty("title", out var nestedModTitleElement) ? nestedModTitleElement.GetString() : "#####",
                                                                MinPermitted = nestedModifierObj.TryGetProperty("min_permitted", out var nestedMinPermittedElement) ? nestedMinPermittedElement.GetInt32() : 0,
                                                                MaxPermitted = nestedModifierObj.TryGetProperty("max_permitted", out var nestedMaxPermittedElement) ? nestedMaxPermittedElement.GetInt32() : 0
                                                            };

                                                            // Parse nested modifier items if they exist
                                                            if (nestedModifierElement.TryGetProperty("items", out var nestedItemsElement) && nestedItemsElement.ValueKind == JsonValueKind.Array)
                                                            {
                                                                var nestedModifierItems = new List<Models.ModifierItemModel>();
                                                                foreach (var nestedItemElement in nestedItemsElement.EnumerateArray())
                                                                {
                                                                    decimal nestedPriceValue = 0.0m;
                                                                    if (nestedItemElement.TryGetProperty("price", out var nestedItemPriceElement))
                                                                    {
                                                                        if (nestedItemPriceElement.ValueKind == JsonValueKind.Number)
                                                                            nestedPriceValue = nestedItemPriceElement.GetDecimal();
                                                                        else if (nestedItemPriceElement.ValueKind == JsonValueKind.String && decimal.TryParse(nestedItemPriceElement.GetString(), out decimal parsed))
                                                                            nestedPriceValue = parsed;
                                                                    }
                                                                    var nestedModifierItem = new Models.ModifierItemModel
                                                                    {
                                                                        Id = nestedItemElement.TryGetProperty("id", out var nestedItemIdElement) ? nestedItemIdElement.GetInt32() : 0,
                                                                        ItemName = nestedItemElement.TryGetProperty("item_name", out var nestedItemNameElement) ? nestedItemNameElement.GetString() : "#####",
                                                                        ItemPrice = nestedPriceValue
                                                                    };
                                                                    nestedModifierItems.Add(nestedModifierItem);
                                                                }
                                                                nestedModifier.ModifierItems = nestedModifierItems;
                                                            }
                                                            else
                                                            {
                                                                // If no nested items, create a default item with "#####"
                                                                nestedModifier.ModifierItems = new List<Models.ModifierItemModel>
                                                                {
                                                                    new Models.ModifierItemModel
                                                                    {
                                                                        Id = 0,
                                                                        ItemName = "#####",
                                                                        ItemPrice = 0.0m
                                                                    }
                                                                };
                                                            }
                                                            nestedModifiers.Add(nestedModifier);
                                                        }
                                                    }
                                                    modifierItem.NestedModifiers = nestedModifiers;
                                                }
                                                modifierItems.Add(modifierItem);
                                            }
                                            modifier.ModifierItems = modifierItems;
                                        }
                                        else
                                        {
                                            // If no items, create a default item with "#####"
                                            modifier.ModifierItems = new List<Models.ModifierItemModel>
                                            {
                                                new Models.ModifierItemModel
                                                {
                                                    Id = 0,
                                                    ItemName = "#####",
                                                    ItemPrice = 0.0m
                                                }
                                            };
                                        }
                                        modifiers.Add(modifier);
                                    }
                                }
                                product.Modifiers = modifiers;
                            }
                            else
                            {
                                // If no modifiers, create a default modifier with "#####"
                                product.Modifiers = new List<Models.ModifierModel>
                                {
                                    new Models.ModifierModel
                                    {
                                        Id = 0,
                                        Title = "#####",
                                        ModifierItems = new List<Models.ModifierItemModel>
                                        {
                                            new Models.ModifierItemModel
                                            {
                                                Id = 0,
                                                ItemName = "#####",
                                                ItemPrice = 0.0m
                                            }
                                        }
                                    }
                                };
                            }

                            products.Add(product);
                        }
                    }
                }

                return (categories, products);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProductsAndCategoriesAsync: {ex.Message}");
                throw;
            }
        }

       /* public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                // Set bearer token from settings
                var accessToken = POS_UI.Properties.Settings.Default.AccessToken;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    SetBearerToken(accessToken);
                }

                System.Diagnostics.Debug.WriteLine("Making category API request...");
                var response = await _httpClient.GetAsync("/api/v1/main-menu/65/categories/webshop-brand/1/shop/3");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Category API error: {response.StatusCode} - {error}");
                    throw new Exception($"Status: {response.StatusCode}\n{error}");
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Category API response received, length: {json.Length}");
                
                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                var categories = new List<string>();
                
                // The API returns categories as object keys, not as array elements
                // Each key is a category name and the value is an array of products
                foreach (var property in data.EnumerateObject())
                {
                    var categoryName = property.Name;
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        categories.Add(categoryName);
                        System.Diagnostics.Debug.WriteLine($"Found category: {categoryName}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Total categories found: {categories.Count}");
                return categories;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCategoriesAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to be handled by the calling method
            }
        }*/

    
    }
} 