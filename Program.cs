using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace reembolso_teste
{
    public class Program
    {
        private static string _token;
        private static string _username;
        private static string _tela;
        private readonly static string apiErro = "https://script.google.com/macros/s/AKfycbwpfP3sULPg1NBrWyHRS0nP3Cq398Axz5ViXGFQyH0sXIR5TM3M8JKYIoXVqzqITyt3/exec";
        private readonly static string apiExistente = "https://script.google.com/macros/s/AKfycbz6rt50p6NoKUnAPhRTZJVVsiRmTLu2Uka-u5EN4r1QOLKUbJV4JqYXvxmTqQr6ZXUf/exec";
        private readonly static string urlConcluida = "https://script.google.com/macros/s/AKfycbxZcwa99f0wpRaR6gLDND_9Ri8DlJMOTWAaRHQFjzA3PhmiLSSeDvwoVn45mvDpvMOk/exec";
        private readonly static string urlPostTelas = "https://script.google.com/macros/s/AKfycbyQ_QDfnPXtTJUYsatlEKsAz0N2LEawqYmh2w0cWKO1_rOdiJqCLWluQfhFTwnDHucb/exec";
        private readonly static string urlTelaSim = "https://script.google.com/macros/s/AKfycbxJBISprFqnngfqeCc2ZvLIU70dsCCSNr1OIZx09IYKMrh70ChPlx85KtQaRGb65RmR/exec";
        private readonly static string apiUrl = "https://script.google.com/macros/s/AKfycbxtiKxz3TYsZKVJBDr0j7Mv_PIfwsDALeIq0K0s3EAy_ONpRSBTWH1v4VIQQSb9szPK/exec";
        static HttpClient client = new HttpClient();


        public static async Task Main()
        {
         
            while (true)
            {
                try
                {

                   
                    Console.WriteLine("Buscando clientes inativados!");
                    await Task.Delay(1000);
                    await PostValidarCompra();
                    Console.WriteLine("Cliente ativado!");
                    Console.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro inesperado: {ex.Message}");

                }
                await Task.Delay(10000);
            }
            
        }


        public static async Task PostValidarCompra()
        {
            string[] array2Telas = new string[13] {"pla4kl6v","pla6grjw","pladekre","plaemxex","plazvzno","205074",
             "260858","262168","262172","13189","86332","37831","12009"};

            string[] array3Telas = new string[9] {"pla648k1", "plazd6zm", "plazdl9l", "205075", "262173", "274443",
            "269390","42519","58436"};

            string[] array4Telas = new string[7] { "plarwq1n", "plavgv1n", "plazvdzj", "264909", "264914", "21873", "21873" };

            string url = "https://zyon.sigma-billing.com/api/customers";
            string bearerToken = _token;

            List<string> valores = await GetValoresFromApi(apiUrl);


            if (valores.Count == 0)
            {
                Console.WriteLine(" Nenhuma cliente encontrado!...");
                await Task.Delay(1000);
                Console.Clear();
                 
                return; // Sai do método caso não haja valores
            }
            Console.WriteLine("Iniciando processo de ativação!");
            await Task.Delay(1000);
            string urlLogin = "https://zyon.sigma-billing.com/api/auth/login";

            var requestBody = new
            {
                username = "Goldcard",
                password = "equipegold"
            };
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            client.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.6");
            client.DefaultRequestHeaders.Add("Locale", "pt");
            client.DefaultRequestHeaders.Add("Origin", "https://zyon.sigma-billing.com");
            client.DefaultRequestHeaders.Referrer = new Uri("https://zyon.sigma-billing.com/");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");

            try
            {
                HttpResponseMessage response = await client.PostAsync(urlLogin, content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseBody;
                    if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        using (var responseStream = await response.Content.ReadAsStreamAsync())
                        using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        using (var streamReader = new StreamReader(decompressedStream))
                        {
                            responseBody = await streamReader.ReadToEndAsync();
                        }
                    }
                    else
                    {
                        responseBody = await response.Content.ReadAsStringAsync();
                    }

                    var responseData = JsonDocument.Parse(responseBody);
                    _token = responseData.RootElement.GetProperty("token").GetString();
                }
                // Console.WriteLine("Token Bearer: " + _token);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Erro ao fazer a requisição: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Erro ao parsear JSON: {e.Message}");
            }

            foreach (var valor in valores)
            {
                string _tela = await EnviarPostTelasAsync(urlPostTelas, valor);
                int.TryParse(_tela, out int telaInt);
                await Task.Delay(1000);

                bool existeEmArray1 = array2Telas.Contains(_tela);
                bool existeEmArray2 = array3Telas.Contains(_tela);
                bool existeEmArray3 = array4Telas.Contains(_tela);

                if (existeEmArray1)
                {
                    telaInt = 2;
                    await EnviarPostSimTelaAsync(urlTelaSim, valor);
                    await Task.Delay(1000);
                    
                }
                else if (existeEmArray2)
                {
                    telaInt = 3;
                    await EnviarPostSimTelaAsync(urlTelaSim, valor);
                    await Task.Delay(1000);
                    
                }
                else if (existeEmArray3)
                {
                    telaInt = 4;
                    await EnviarPostSimTelaAsync(urlTelaSim, valor);
                    await Task.Delay(1000);
                    
                }

                else
                {
                    telaInt = 1;

                }




                var data = new
                {
                    bouquets = "",
                    connections = telaInt,
                    package_id = "Yen129WPEa",
                    password = "gold81500",
                    server_id = "BV4D3rLaqZ",
                    trial_hours = 1,
                    username = valor
                };

                var jsonContentt = JsonSerializer.Serialize(data);
                var contentt = new StringContent(jsonContentt, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                client.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.6");
                client.DefaultRequestHeaders.Add("Locale", "pt");
                client.DefaultRequestHeaders.Add("Origin", "https://zyon.sigma-billing.com");
                client.DefaultRequestHeaders.Referrer = new Uri("https://zyon.sigma-billing.com/");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");

                try
                {

                    HttpResponseMessage response = await client.PostAsync(url, contentt);

                    if (response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        await Task.Delay(1000);
                        await EnviarPostAsync(urlConcluida, valor);
                        


                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                    {
                        await EnviarPostAsync(urlConcluida, valor);
                        Console.WriteLine("Cliente já existente!");

                    }
                    else
                    {
                        
                        Console.WriteLine($"Código de status não tratado: {response.StatusCode}");
                        await EnviarPostAsync(apiErro, valor);
                    }

                }
                catch (HttpRequestException e)
                {
                    if (e.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        Console.WriteLine("Acesso negado. Verifique se o token Bearer está correto e tem as permissões necessárias.");
                        await EnviarPostAsync(apiErro, valor);
                    }
                    else if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Lógica para tratar o código 404 Not Found
                        Console.WriteLine("Recurso não encontrado (404 Not Found).");
                        await EnviarPostAsync(apiErro, valor);
                    }
                    
                    else
                    {
                        Console.WriteLine($"Erro ao fazer a requisição: {e.Message}");
                        await EnviarPostAsync(apiErro, valor);
                    }
                }
            }
            Console.WriteLine("Concluído!");
            Console.Clear();
        }

       

        private static async Task<List<string>> GetValoresFromApi(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Resultado>(responseBody);
                    return result.valoresD_vazia;
                }
                else
                {
                    Console.WriteLine("Erro ao acessar a API: " + response.StatusCode);
                    return new List<string>(); // Retorna uma lista vazia em caso de erro na requisição
                }
            }
        }

        private static async Task<HttpResponseMessage> EnviarPostAsync(string url, string valorColunaD)
        {
            using (HttpClient client = new HttpClient())
            {
                string parametros = $"valor={Uri.EscapeDataString(valorColunaD)}";
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


        public static async Task<string> EnviarPostTelasAsync(string url, string valorColunaD)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Construindo os parâmetros no formato application/x-www-form-urlencoded
                    var parametros = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("valorD", valorColunaD)
                    };
                    var content = new FormUrlEncodedContent(parametros);

                    // Fazendo a requisição POST e recebendo a resposta
                    var resposta = await client.PostAsync(url, content).ConfigureAwait(false);

                    // Verificando se a requisição foi bem-sucedida
                    if (resposta.IsSuccessStatusCode)
                    {
                        var respostaString = await resposta.Content.ReadAsStringAsync().ConfigureAwait(false);

                        // Desserializar para o tipo apropriado (RespostaApi neste caso)
                        RespostaApi respostaApi = JsonSerializer.Deserialize<RespostaApi>(respostaString);

                        // Retornar o valor de valorK 
                        return respostaApi.valorK;
                        
                    }
                    else
                    {
                        // Se não for bem-sucedida, ler a mensagem de erro da resposta
                        var respostaErro = await resposta.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new HttpRequestException($"Erro ao enviar POST: {resposta.StatusCode}, Detalhes: {respostaErro}");
                    }
                }
                catch (Exception ex)
                {
                    // Capturando e tratando exceções gerais
                    throw new HttpRequestException($"Erro durante o envio do POST: {ex.Message}");
                }
            }
        }

        public class Resultado
        {
            public List<string> valoresD_vazia { get; set; }
        }

        public class RespostaApi
        {
            public string valorK { get; set; }
        }


    }
}
