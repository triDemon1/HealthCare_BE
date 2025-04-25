using HaNoiTravel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ChatBotController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {

            var openRouterRequest = new
            {
                model = "openai/gpt-3.5-turbo", // hoặc "openai/gpt-4", hoặc model khác nếu muốn
                messages = new[]
                {
                    new { role = "system", content = "Bạn là trợ lý hỗ trợ chăm sóc người già, dinh dưỡng, sức khỏe." },
                    new { role = "user", content = request.Message }
                }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
            req.Headers.Add("Authorization", "Bearer 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"); // thay bằng API key từ OpenRouter
            req.Headers.Add("HTTP-Referer", "http://localhost:7064"); // hoặc tên miền thật nếu đã deploy

            req.Content = new StringContent(JsonConvert.SerializeObject(openRouterRequest), Encoding.UTF8, "application/json");

            var res = await _httpClient.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            var obj = JsonConvert.DeserializeObject<dynamic>(json);
            var reply = obj?.choices?[0]?.message?.content?.ToString();

            return Ok(new { reply });
        }
    }
}
