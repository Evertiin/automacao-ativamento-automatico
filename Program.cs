using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReembolsoTeste
{
    public class Program
    {
        private static string _token;
        private static string valorCompra;
        private readonly static HttpClient client = new HttpClient();

        private static readonly Dictionary<string, string> urls = new()
        {
            { "ApiErro", "https://script.google.com/macros/s/AKfycbxdpZ_aiRhdgjUawpkzLizmo-VRgWu5ByiqSDh3ZexPWr6qCiZowqzZQ0dSAx_yAQ/exec" },
            { "UrlConcluida", "https://script.google.com/macros/s/AKfycby2HcMzkRJJq12FEHyPlHyPZaQwtCpGu90HD44KWpEffwwRKKUiGeLzDccUsY22tfw/exec" },
            { "UrlPostTelas", "https://script.google.com/macros/s/AKfycbw1VsGdO1IBki4eT1WAg2sQTTnT6MgKIB10YEBMZc291KnJLHlCKC6FAghFPOBADys/exec" },
            { "UrlTelaSim", "https://script.google.com/macros/s/AKfycbyF8jvE_8KBGRsJRp-PBXKwmAaFZEtBLW27_dEBMdez8LKIyGdtlqikg_86_hJNbjY/exec" },
            { "ApiUrl", "https://script.google.com/macros/s/AKfycbybNIYK9IuekFN_WniKCA3BiY0T8arJyh382zh8tpmttjs4Ol2DPG60OYlXAoEe_Jk/exec" }
        };

        public static async Task Main()
        {
            ConfigurarHttpClient();
            Console.WriteLine("Buscando clientes inativados!");

            try
            {
                await PostValidarCompra();
                Console.WriteLine("Processo concluído!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado: {ex.Message}");
            }
        }

        private static void ConfigurarHttpClient()
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Locale", "pt");
            client.DefaultRequestHeaders.Add("Origin", "https://tp.sigma-billing.com");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        public static async Task PostValidarCompra()
        {
            var valores = await GetValoresFromApi(urls["ApiUrl"]);
            if (valores.Count == 0)
            {
                Console.WriteLine("Nenhum cliente encontrado.");
                return;
            }

            Console.WriteLine("Iniciando processo de ativação...");
            _token = await ObterTokenAsync();
            if (string.IsNullOrEmpty(_token)) return;

            foreach (var valor in valores)
            {
                valorCompra = valor;
                string _tela = await EnviarPostTelasAsync(urls["UrlPostTelas"], valor);
                int telaInt = await DefinirTela(_tela);
                await EnviarDadosClienteAsync(valor, telaInt);
                await EnviarPostAsync(urls["UrlConcluida"], valor);
            }
            Console.WriteLine("Processo de ativação concluído!");
        }

        private static async Task<HttpResponseMessage> EnviarPostAsync(string url, string valorColunaD)
        {
            using (HttpClient client = new HttpClient())
            {
                string parametros = $"valorD={Uri.EscapeDataString(valorColunaD)}";
                var content = new StringContent(parametros, Encoding.UTF8, "application/x-www-form-urlencoded");
                return await client.PostAsync(url, content);
            }
        }
        private static async Task<HttpResponseMessage> EnviarPostSimTelaAsync(string url, string valorColunaD)
        {
            using (HttpClient client = new HttpClient())
            {
                string parametros = $"valorD={Uri.EscapeDataString(valorColunaD)}";
                var content = new StringContent(parametros, Encoding.UTF8, "application/x-www-form-urlencoded");
                return await client.PostAsync(url, content);
            }
        }

        private static async Task<int> DefinirTela(string tela)
        {
            var array2Telas = new[] { "pla4kl6v", "pla6grjw", "pladekre", "plaemxex", "plazvzno", "205074", "260858", "262168", "262172", "13189", "86332", "37831", "12009" };
            var array3Telas = new[] { "pla648k1", "plazd6zm", "plazdl9l", "205075", "262173", "274443", "269390", "42519", "58436" };
            var array4Telas = new[] { "plarwq1n", "plavgv1n", "plazvdzj", "264909", "264914", "21873", "21873" };

            if (array2Telas.Contains(tela))
            {
                await EnviarPostSimTelaAsync(urls["UrlTelaSim"], valorCompra);
                return 2;
            }
            if (array3Telas.Contains(tela))
            {
                await EnviarPostSimTelaAsync(urls["UrlTelaSim"], valorCompra);
                return 3;
            }
            if (array4Telas.Contains(tela))
            {
                await EnviarPostSimTelaAsync(urls["UrlTelaSim"], valorCompra);
                return 4;
            }

            return 1; 
        }


        private static async Task<string> ObterTokenAsync()
        {
            var loginData = new { username = "goldcard", password = "Card3399" };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://tp.sigma-billing.com/api/auth/login", content);
                return await ExtrairToken(response);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Erro ao fazer login: {e.Message}");
                return null;
            }
        }

        private static async Task<string> ExtrairToken(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var responseBody = response.Content.Headers.ContentEncoding.Contains("gzip") ?
                    await DescomprimirResponse(response) : await response.Content.ReadAsStringAsync();

                return JsonDocument.Parse(responseBody).RootElement.GetProperty("token").GetString();
            }
            Console.WriteLine("Erro ao autenticar.");
            return null;
        }

        private static async Task<string> DescomprimirResponse(HttpResponseMessage response)
        {
            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(decompressedStream);
            return await streamReader.ReadToEndAsync();
        }

        private static async Task<List<string>> GetValoresFromApi(string apiUrl)
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Resultado>(await response.Content.ReadAsStringAsync());
                return result?.valoresD_vazia ?? new List<string>();
            }
            Console.WriteLine("Erro ao acessar a API.");
            return new List<string>();
        }

        private static async Task EnviarDadosClienteAsync(string valor, int telaInt)
        {
            var data = new { server_id = "BV4D3rLaqZ", package_id = "VpKDaJWRAa", trial_hours = 1, connections = telaInt, password = "gold81500", username = valor };
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await client.PostAsync("https://tp.sigma-billing.com/api/customers", content);
            Console.WriteLine(response.IsSuccessStatusCode ? "Cliente ativado!" : "Erro na ativação do cliente.");
        }

        private static async Task<string> EnviarPostTelasAsync(string url, string valor)
        {
            var parametros = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("valorD", valor) });
            HttpResponseMessage resposta = await client.PostAsync(url, parametros);
            if (resposta.IsSuccessStatusCode)
            {
                var respostaString = await resposta.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<RespostaApi>(respostaString)?.valorK;
            }
            throw new HttpRequestException($"Erro ao enviar POST para {url}");
        }

        private class Resultado
        {
            public List<string> valoresD_vazia { get; set; }
        }

        private class RespostaApi
        {
            public string valorK { get; set; }
        }
    }
}
