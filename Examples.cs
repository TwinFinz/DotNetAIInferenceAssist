using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

    public class MyAIAPI
    {
        #region WebRequests
        public MyAIAPI()
        {
        }

        #region StableDiffusion
        private static string defaultNegativePrompt = "nsfw, nude, naked, face, worst quality, normal quality, low quality, low res, blurry, text, watermark, logo, banner, extra digits, cropped, jpeg artifacts, signature, username, error, sketch, duplicate, ugly, monochrome, horror, geometry, mutation, disgusting, bad anatomy, bad hands, three hands, three legs, bad arms, missing legs, missing arms, poorly drawn face, bad face, fused face, cloned face, worst face, three crus, extra crus, fused crus, worst feet, three feet, fused feet, fused thigh, three thigh, fused thigh, extra thigh, worst thigh, missing fingers, extra fingers, ugly fingers, long fingers, horn, realistic photo, extra eyes, huge eyes, 2girl, amputation, disconnected limbs";
        public static async Task<byte[]?> GenerateImageStableDiffusionAsync(string prompt, string negativePrompt = "", string model = "deliberate_v3", string samplerName = "DPM++ 2M Karras", int steps = 20, int cfgScale = 12, int seed = 214, int width = 711, int height = 400, bool enableHr = false, double denoisingStrength = 0.6, string[]? styles = null, int batchSize = 1, int nIter = 1, bool restoreFaces = false, bool tiling = false, double sNoise = 0.4, bool overrideSettingsRestoreAfterwards = true, string endpoint = "http://192.168.0.106:7860/sdapi/v1/txt2img")
        {
            prompt = prompt.Trim();
            if (!string.IsNullOrEmpty(negativePrompt.Trim()))
            {
                defaultNegativePrompt += $", {negativePrompt}";
            }
            StableDiffusionRequest requestPayload = new()
            {
                Prompt = prompt,
                Steps = steps,
                NegativePrompt = defaultNegativePrompt,
                SamplerName = samplerName,
                CfgScale = cfgScale,
                Seed = seed,
                Width = width,
                Height = height,
                EnableHr = enableHr,
                DenoisingStrength = denoisingStrength,
                Styles = styles,
                BatchSize = batchSize,
                NIter = nIter,
                RestoreFaces = restoreFaces,
                Tiling = tiling,
                SNoise = sNoise,
                OverrideSettingsRestoreAfterwards = overrideSettingsRestoreAfterwards,
                Model = model
            };
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(60);
            string jsonRequest = JsonSerializer.Serialize(requestPayload);
            HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            StableDiffusionResponse? jsonResponse = JsonSerializer.Deserialize<StableDiffusionResponse>(responseBody);
            if (jsonResponse != null && !string.IsNullOrEmpty(jsonResponse.Images![0]))
            {
                byte[] imageBytes = Convert.FromBase64String(jsonResponse.Images[0]);
                return imageBytes;
            }
            return null;
        }        
        public static async Task<StableDiffusionModel[]?> GetStableDiffusionModelsAsync(string endpoint = "http://192.168.0.106:7860/sdapi/v1/sd-models")
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(60);
            HttpResponseMessage response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            StableDiffusionModel[]? models = JsonSerializer.Deserialize<StableDiffusionModel[]>(responseBody);
            return models;
        }
        public static async Task<List<string>> GetStableDiffusionModelsListAsync(string endpoint = "http://192.168.0.106:7860/sdapi/v1/sd-models")
        {
            List<string> models = new();
            StableDiffusionModel[]? modelsResponse = await GetStableDiffusionModelsAsync(endpoint);
            if (modelsResponse != null)
            {
                Parallel.ForEach(modelsResponse, model =>
                {
                    lock (models)
                    {
                        models.Add(model.ModelName);
                    }
                });
            }
            return models;
        }
        public static async Task<ProgressResponse?> GetProgressAsync(string endpoint = "http://192.168.0.106:7860/sdapi/v1/progress")
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(15);
            HttpResponseMessage response = await client.PostAsync(endpoint, null);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            ProgressResponse? progressResponse = JsonSerializer.Deserialize<ProgressResponse>(responseBody);
            return progressResponse;
        }
        #region TypeClasses
        public class StableDiffusionRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = "";
            [JsonPropertyName("steps")]
            public int Steps { get; set; } = 20;
            [JsonPropertyName("negative_prompt")]
            public string NegativePrompt { get; set; } = "";
            [JsonPropertyName("sampler_name")]
            public string SamplerName { get; set; } = "DPM++ 2m karras";
            [JsonPropertyName("cfg_scale")]
            public int CfgScale { get; set; } = 12;
            [JsonPropertyName("seed")]
            public int Seed { get; set; } = -1;
            [JsonPropertyName("width")]
            public int Width { get; set; } = 512;
            [JsonPropertyName("height")]
            public int Height { get; set; } = 512;
            [JsonPropertyName("enable_hr")]
            public bool EnableHr { get; set; } = false;
            [JsonPropertyName("denoising_strength")]
            public double DenoisingStrength { get; set; } = 0;
            [JsonPropertyName("styles")]
            public string[]? Styles { get; set; } = null;
            [JsonPropertyName("batch_size")]
            public int BatchSize { get; set; } = 1;
            [JsonPropertyName("n_iter")]
            public int NIter { get; set; } = 1;
            [JsonPropertyName("restore_faces")]
            public bool RestoreFaces { get; set; } = false;
            [JsonPropertyName("tiling")]
            public bool Tiling { get; set; } = false;
            [JsonPropertyName("s_noise")]
            public double SNoise { get; set; } = 1;
            [JsonPropertyName("override_settings_restore_afterwards")]
            public bool OverrideSettingsRestoreAfterwards { get; set; } = true;
            [JsonPropertyName("model")]
            public string Model { get; set; } = "";
            /* Other availableOptions            
            [JsonPropertyName("override_settings")]
            public object? OverrideSettings { get; set; } = null;
            [JsonPropertyName("firstphase_width")]
            public int FirstPhaseWidth { get; set; } = 0;
            [JsonPropertyName("firstphase_height")]
            public int FirstPhaseHeight { get; set; } = 0;
            [JsonPropertyName("subseed")]
            public int Subseed { get; set; } = -1;
            [JsonPropertyName("subseed_strength")]
            public double SubseedStrength { get; set; } = 0;
            [JsonPropertyName("seed_resize_from_h")]
            public int SeedResizeFromH { get; set; } = -1;
            [JsonPropertyName("seed_resize_from_w")]
            public int SeedResizeFromW { get; set; } = -1;
            [JsonPropertyName("eta")]
            public double Eta { get; set; } = 0;
            [JsonPropertyName("s_churn")]
            public double SChurn { get; set; } = 0;
            [JsonPropertyName("s_tmax")]
            public double STmax { get; set; } = 0;
            [JsonPropertyName("s_tmin")]
            public double STmin { get; set; } = 0;
            */
        }
        public class StableDiffusionResponse
        {
            [JsonPropertyName("images")]
            public string[]? Images { get; set; }
        }
        public class StableDiffusionModel
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = "";
            [JsonPropertyName("model_name")]
            public string ModelName { get; set; } = "";
            [JsonPropertyName("hash")]
            public string? Hash { get; set; } = null;
            [JsonPropertyName("sha256")]
            public string? Sha256 { get; set; } = null;
            [JsonPropertyName("filename")]
            public string Filename { get; set; } = "";
            [JsonPropertyName("config")]
            public string? Config { get; set; } = null;
        }
        public class ProgressResponse
        {
            [JsonPropertyName("progress")]
            public int Progress { get; set; }
            [JsonPropertyName("eta_ralative")]
            public int EtaRelative { get; set; }
            [JsonPropertyName("state")]
            public object? State { get; set; }
            [JsonPropertyName("current_image")]
            public string CurrentImage { get; set; } = "";
            [JsonPropertyName("textinfo")]
            public string TextInfo { get; set; } = "";
        }
        public class PngInfoResponse
        {
            [JsonPropertyName("info")]
            public string Info { get; set; } = "";
        }
        #endregion
        #endregion

        #region GptNeo
        public static async Task<string> GenerateGptNeoAsyncHttp(string prompt, int maxTokens = 2000, string serverUrl = "http://192.168.0.106:8000")
        {
            prompt = prompt.Trim();
            var requestPayload = new GptNeoRequest
            {
                Text = prompt.Trim(),
                Temperature = 0.1,
                MinLength = 50, // Adjust as needed
                MaxLength = maxTokens, // Adjust as needed
                DoSample = true
            };
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(60);
            string endpoint = serverUrl + "/call"; // Adjust the endpoint URL
            string jsonRequest = JsonSerializer.Serialize(requestPayload);
            StringContent content = new(jsonRequest, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            GptNeoResponse? responsePayload = JsonSerializer.Deserialize<GptNeoResponse>(jsonResponse);
            return responsePayload?.GeneratedText ?? "";
        }
        #region TypeClasses
        public class GptNeoRequest
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = "";
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0.8;
            [JsonPropertyName("min_length")]
            public int MinLength { get; set; } = 50;
            [JsonPropertyName("max_length")]
            public int MaxLength { get; set; } = 8000;
            [JsonPropertyName("do_sample")]
            public bool DoSample { get; set; } = false;
        }
        public class GptNeoResponse
        {
            [JsonPropertyName("generated_text")]
            public string GeneratedText { get; set; } = "";
        }
        #endregion
        #endregion

        #region OpenAIAPI
        private static readonly string completionEndpoint = "https://api.openai.com/v1/completions";
        private static readonly string chatCompletionEndpoint = "https://api.openai.com/v1/chat/completions";
        private static readonly string editsEndpoint = "https://api.openai.com/v1/edits";
        private static readonly string modelEndpoint = "https://api.openai.com/v1/models";
        private static readonly string transcriptionEndpoint = "https://api.openai.com/v1/audio/transcriptions";
        private static readonly string translationEndpoint = "https://api.openai.com/v1/audio/translations";
        private static readonly string imageGenerationEndpoint = "https://api.openai.com/v1/images/generations";
        private static readonly string imageVariationsEndpoint = "https://api.openai.com/v1/images/variations";
        private static readonly string ModerationsEndpoint = "https://api.openai.com/v1/moderations";
        private static readonly string imageEditsEndpoint = "https://api.openai.com/v1/images/edits";
        private static readonly string fileUploadEndpoint = "https://api.openai.com/v1/files";

        public static double EstimateTokenCost(string input)
        {
            double tokenCount = Math.Ceiling((double)input.Length / 4);
            double costPerToken = 0.01; // Assuming a cost of 0.01 per token
            double cost = tokenCount * costPerToken;
            return cost;
        }
        public static async Task<bool> CheckTextForViolations(string prompt, string apiKey)
        {
            bool Violates = false;
            TextViolationCheckRequest payload = new()
            {
                Input = prompt,
                Model = "text-moderation-latest"
            };
            // create an HTTP client
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            string json = JsonSerializer.Serialize(payload);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(ModerationsEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
            }
            string responseJson = await response.Content.ReadAsStringAsync();
            TextModerationResponse responseData = JsonSerializer.Deserialize<TextModerationResponse>(responseJson)!;

            foreach (TextModerationResponse.ModResult modResults in responseData.Results)
            {
                if (modResults.Flagged)
                {
                    Violates = true;
                }
            }

            return Violates;
        }
        public static async Task<Message> GenerateChatCompletionAsync(List<Message> messages, string apiKey, string model = "gpt-3.5-turbo", double temperature = 1.0, double topP = 1.0, int n = 1, int maxTokens = 4000, double presencePenalty = 0.0, double frequencyPenalty = 0.0)
        {
            try
            {
                if (messages.Count < 1)
                {
                    throw new Exception("No messages found to create the completion.");
                }
                int inputCost = 0;
                foreach (Message message in messages)
                {
                    inputCost += (message.Content.Length / 4);
                }
                int curMsg = 0;
                while ((maxTokens - inputCost) < 500 && curMsg < messages.Count)
                {
                    if (messages[curMsg].Role != "system")
                    {
                        inputCost -= messages[curMsg].Content.Length / 4;
                        messages.RemoveAt(curMsg);
                    }
                    else
                    {
                        curMsg++;
                    }
                }
                ChatCompletionRequest payload = new()
                {
                    Model = model,
                    Messages = messages,
                    Temperature = temperature,
                    TopP = topP,
                    NumOfResponse = n,
                    MaxTokens = (maxTokens - inputCost),
                    PresencePenalty = presencePenalty,
                    FrequencyPenalty = frequencyPenalty
                };

                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                string json = JsonSerializer.Serialize(payload);
                StringContent content = new(json, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await client.PostAsync(chatCompletionEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    ChatCompletionResult result = JsonSerializer.Deserialize<ChatCompletionResult>(responseJson)!;
                    return result.Choices[0].Message;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<string> GenerateImageAsyncHttp(string prompt, string apiKey, string size = "1024x1024")
        {
            try
            {
                prompt = prompt.Trim();
                if (await CheckTextForViolations(prompt, apiKey))
                {
                    throw new Exception("This input violates OpenAI moderation checks.");
                }
                ImageGenerationRequest payload = new()
                {
                    Prompt = prompt,
                    Size = size,
                    User = apiKey
                };

                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                string json = JsonSerializer.Serialize(payload);
                StringContent content = new(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(imageGenerationEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                return responseData.Data.First().Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> ImageVariationsAsyncHttp(byte[] image, string apiKey, string size = "1024x1024")
        {
            try
            {
                using MultipartFormDataContent content = new();
                ByteArrayContent imageContent = new(image);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(imageContent, "image", "otter.png");
                content.Add(new StringContent(size), "size");
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                HttpResponseMessage response = await client.PostAsync(imageVariationsEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                return responseData.Data.First().Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> EditImageAsyncHttp(string prompt, byte[] image, byte[] mask, string apiKey, string size = "1024x1024")
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    using (MultipartFormDataContent content = new())
                    {
                        ByteArrayContent imageContent = new(image);
                        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        content.Add(imageContent, "image", "image.png");

                        if (mask != null)
                        {
                            ByteArrayContent maskContent = new(mask);
                            maskContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                            content.Add(maskContent, "mask", "mask.png");
                        }

                        content.Add(new StringContent(prompt), "prompt");
                        content.Add(new StringContent(1.ToString()), "n");
                        content.Add(new StringContent(size), "size");
                        HttpResponseMessage response = await client.PostAsync(imageEditsEndpoint, content);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to generate image edits: {response.ReasonPhrase}");
                        }
                        string responseJson = await response.Content.ReadAsStringAsync();
                        ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                        return responseData.Data.First().Url;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> EditImageAsyncHttp(string prompt, byte[] image, string apiKey, string size = "1024x1024")
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    using (MultipartFormDataContent content = new())
                    {
                        ByteArrayContent imageContent = new(image);
                        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        content.Add(imageContent, "image", "image.png");

                        content.Add(new StringContent(prompt), "prompt");
                        content.Add(new StringContent(1.ToString()), "n");
                        content.Add(new StringContent(size), "size");
                        HttpResponseMessage response = await client.PostAsync(imageEditsEndpoint, content);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to generate image edits: {response.ReasonPhrase}");
                        }
                        string responseJson = await response.Content.ReadAsStringAsync();
                        ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                        return responseData.Data.First().Url;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> TranslateAudioAsyncHttp(byte[] audio, string filename, string apiKey, string model = "whisper-1")
        {
            try
            {
                string audioB64 = Convert.ToBase64String(audio);
                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    using (MultipartFormDataContent content = new())
                    {
                        ByteArrayContent audioContent = new(audio);
                        audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        content.Add(audioContent, "file", filename);

                        content.Add(new StringContent(model), "model");

                        HttpResponseMessage response = await client.PostAsync(translationEndpoint, content);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to transcribe audio: {response.ReasonPhrase}");
                        }

                        string responseJson = await response.Content.ReadAsStringAsync();
                        TranscriptionResponse responseData = JsonSerializer.Deserialize<TranscriptionResponse>(responseJson)!;
                        return responseData.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> TranscribeAudioAsyncHttp(byte[] audio, string filename, string apiKey, string model = "whisper-1")
        {
            try
            {
                string audioB64 = Convert.ToBase64String(audio);
                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    using (MultipartFormDataContent content = new())
                    {
                        ByteArrayContent audioContent = new(audio);
                        audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/mpeg");
                        content.Add(audioContent, "file", filename);

                        content.Add(new StringContent(model), "model");

                        HttpResponseMessage response = await client.PostAsync(transcriptionEndpoint, content);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to transcribe audio: {response.ReasonPhrase}");
                        }

                        string responseJson = await response.Content.ReadAsStringAsync();
                        TranscriptionResponse responseData = JsonSerializer.Deserialize<TranscriptionResponse>(responseJson)!;
                        return responseData.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> UploadFileToOpenAIAsync(byte[] file, string purpose, string apiKey)
        {
            try
            {
                string jsonLines = Convert.ToBase64String(file);
                FileUploadRequest payload = new()
                {
                    Purpose = purpose,
                    File = jsonLines
                };
                /*
                 *  public class FileUploadRequest
                 *  {
                 *      [JsonPropertyName("purpose")]
                 *      public string Purpose { get; set; }
                 *      [JsonPropertyName("file")]
                 *      public string File { get; set; }
                 *  } 
                */
                // create an HTTP client
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                string json = JsonSerializer.Serialize(payload);
                StringContent content = new(json, Encoding.UTF8);
                HttpResponseMessage response = await client.PostAsync(fileUploadEndpoint, content); // fileUploadEndpoint = "https://api.openai.com/v1/files";
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                FileInformation responseData = JsonSerializer.Deserialize<FileInformation>(responseJson)!;
                return $"{fileUploadEndpoint}/{responseData.Filename}";
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<byte[]> RetrieveFileDetailsAsyncHttp(string fileId, string apiKey)
        {
            try
            {
                // create an HTTP client
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                HttpResponseMessage response = await client.GetAsync($"{fileUploadEndpoint}/{fileId}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                byte[] file = JsonSerializer.Deserialize<byte[]>(await response.Content.ReadAsStringAsync())!;
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<byte[]> RetrieveFileContentsAsyncHttp(string fileId, string apiKey)
        {
            try
            {
                // create an HTTP client
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                HttpResponseMessage response = await client.GetAsync($"{fileUploadEndpoint}/{fileId}/content");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                byte[] file = JsonSerializer.Deserialize<byte[]>(await response.Content.ReadAsStringAsync())!;
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task DeleteFileAsync(string fileUrl, string apiKey)
        {
            try
            {
                // create an HTTP client
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                string deleteEndpoint = $"{fileUrl}";
                HttpResponseMessage response = await client.DeleteAsync(deleteEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                Console.WriteLine("File deleted successfully");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<Model> RetrieveModelDetailsAsync(string modelId, string apiKey)
        {
            try
            {
                // create an HTTP client
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                HttpResponseMessage response = await client.GetAsync($"{modelEndpoint}/{modelId}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                Model responseData = JsonSerializer.Deserialize<Model>(responseJson)!;
                return responseData;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<List<Model>> GetModelsAsync(string apiKey)
        {
            try
            {
                List<Model> retList = new();
                // create an HTTP client
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                HttpResponseMessage response = await client.GetAsync(modelEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                ModelListResponse responseData = JsonSerializer.Deserialize<ModelListResponse>(responseJson)!;
                retList = responseData.Data.ToList<Model>();
                return retList;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<List<Model>> GetBotList(string apiKey)
        {
            try
            {
                List<Model> models = await GetModelsAsync(apiKey);
                ConcurrentBag<Model> resultList = new();
                if (models != null)
                {
                    IEnumerable<Task<Model>> tasks = models.Select(model => RetrieveModelDetailsAsync(model.Id, apiKey));
                    Model[] details = await Task.WhenAll(tasks);

                    foreach (Model modelDetails in details)
                    {
                        if (modelDetails.Permissions.Any(p => p.AllowView))
                        {
                            resultList.Add(modelDetails);
                        }
                    }
                }

                return resultList.ToList();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur while trying to retrieve the models
                throw new Exception(ex.Message);
            }
        }
        public static async Task<List<string>> GetBotNameList(string apiKey)
        {
            List<string> models = new();
            try
            {
                List<Model> result = await GetBotList(apiKey);
                if (result != null)
                {
                    await Task.WhenAll(result.Select(async model =>
                    {
                        if (!models.Contains(model.Id))
                        {
                            models.Add(model.Id);
                            await Task.Delay(0);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return models ?? new();
        }
        public static async Task<string> GenerateTextAsyncHttp(string prompt, string apiKey, string model = "text-davinci-003", int maxTokens = 2000)
        {
            prompt = prompt.Trim();
            if (await CheckTextForViolations(prompt, apiKey))
            {
                throw new Exception("This input violates OpenAI moderation checks.");
            }
            //string stopSequence = @"\?\?\?";
            string returnString = string.Empty;
            switch (model)
            {
                case "text-devinci-003":
                    maxTokens = 4000;
                    break;
                case "code-devinci-002":
                    maxTokens = 8000;
                    break;
                default:
                    break;
            }
            int promptCost = (int)EstimateTokenCost(prompt);
            int remainingTokens = maxTokens - 150 - promptCost;
            if (remainingTokens <= 10)
            {
                throw new Exception("API request too long.\nOpenAI API would fail to respond.\n Please shorten and try again.");
            }

            CompletionRequest payload = new()
            {
                Prompt = prompt.Trim(),
                Model = model,
                Temperature = 0.1,
                MaxTokens = maxTokens,
                TopP = 1.0,
                Frequency_Penalty = 0.6,
                Presence_Penalty = 0.0,
                User = apiKey
            };
            // create an HTTP client
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            string json = JsonSerializer.Serialize(payload);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(completionEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string responseJson = await response.Content.ReadAsStringAsync();
            CompletionResult responseData = JsonSerializer.Deserialize<CompletionResult>(responseJson)!;
            if (await CheckTextForViolations(responseData!.Choices.First().Text, apiKey))
            {
                throw new Exception("This response Violates OpenAI Moderation checks.");
            }
            returnString += $"Using Model: {responseData.Model}\n";
            returnString += $"Took: {responseData.Usage.Total_Tokens} Total Tokens and {responseData.Usage.Completion_Tokens} Tokens were used for the completion.\n";
            returnString += $"Stop Reason: {responseData.Choices.First().Finish_Reason}\n";
            returnString += $"{responseData.Choices.First().Text}\n\n";
            return returnString;
        } // Depreciated 
        public static async Task<string> EditTextAsyncHttp(string model, string apiKey, string instruction, string input = "", int maxTokens = 2000)
        {
            try
            {
                input = input.Trim();
                string returnString = string.Empty;
                if (await CheckTextForViolations(input, apiKey))
                {
                    throw new Exception("This input violates OpenAI moderation checks.");
                }
                int inputCost = (int)EstimateTokenCost(input);
                int remainingTokens = maxTokens - 110 - inputCost;
                if (remainingTokens < (maxTokens / 2))
                {
                    throw new Exception("API request too long.\nOpenAI API would fail to respond.\n Please shorten and try again.");
                }
                TextEditRequest payload = new()
                {
                    Model = model,
                    Input = input,
                    Instruction = instruction
                };
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                string json = JsonSerializer.Serialize(payload);
                StringContent content = new(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(editsEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("API request failed with status code: " + (int)response.StatusCode);
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                TextEditResponse responseData = JsonSerializer.Deserialize<TextEditResponse>(responseJson)!;
                double tokenPricePer1k = 0.02;
                double totalCost = responseData.Usages.Total_Tokens * (tokenPricePer1k / 1000);
                returnString += $"Took: {responseData.Usages.Total_Tokens} Total Tokens and {responseData.Usages.Completion_Tokens} Tokens were used for the completion.\n";
                returnString += $"Total Translation Cost: ${totalCost} at ${tokenPricePer1k} per 1000 tokens\n";
                returnString += responseData.Choices.First().Text;
                return returnString;
            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }
        } // Depreciated 
        #region TypeClasses
        public class Message
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;
            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }
        public class ChatCompletionRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("messages")]
            public List<Message> Messages { get; set; } = new();
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0.7;
            [JsonPropertyName("top_p")]
            public double TopP { get; set; } = 1.0;
            [JsonPropertyName("n")]
            public int NumOfResponse { get; set; } = 1;
            [JsonPropertyName("stream")]
            public bool Stream { get; set; } = false;
            [JsonPropertyName("stop")]
            public string? Stop { get; set; } = null;
            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; } = 4000;
            [JsonPropertyName("presence_penalty")]
            public double PresencePenalty { get; set; } = 0.0;
            [JsonPropertyName("frequency_penalty")]
            public double FrequencyPenalty { get; set; } = 0.0;
        }
        public class ChatCompletionChoice
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }
            [JsonPropertyName("message")]
            public Message Message { get; set; } = new();
            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; } = string.Empty;
        }
        public class ChatCompletionResult
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public string Object { get; set; } = string.Empty;
            [JsonPropertyName("created")]
            public long Created { get; set; }
            [JsonPropertyName("choices")]
            public List<ChatCompletionChoice> Choices { get; set; } = new();
            [JsonPropertyName("usage")]
            public Usage Usage { get; set; } = new();
        }
        public class Usage
        {
            [JsonPropertyName("prompt_tokens")]
            public int Prompt_Tokens { get; set; }
            [JsonPropertyName("completion_tokens")]
            public int Completion_Tokens { get; set; }
            [JsonPropertyName("total_tokens")]
            public int Total_Tokens { get; set; }
        }

        public class TextEditRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;
            [JsonPropertyName("instruction")]
            public string Instruction { get; set; } = string.Empty;
            [JsonPropertyName("n")]
            public int N { get; set; } = 1;
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0;
            [JsonPropertyName("top_p")]
            public double TopP { get; set; } = 1;
        }
        public class TextEditResponse
        {
            [JsonPropertyName("object")]
            public object Object { get; set; } = new();
            [JsonPropertyName("created")]
            public long Created { get; set; }
            [JsonPropertyName("choices")]
            public List<TextEditChoice> Choices { get; set; } = new();
            [JsonPropertyName("usage")]
            public Usage Usages { get; set; } = new();
        }
        public class TextEditChoice
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
            [JsonPropertyName("index")]
            public int Index { get; set; }
        }


        public class Choice
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
            [JsonPropertyName("index")]
            public int Index { get; set; }
            [JsonPropertyName("logprobs")]
            public object Logprobs { get; set; } = new();
            [JsonPropertyName("finish_reason")]
            public string Finish_Reason { get; set; } = string.Empty;
        }
        public class ObjectsInImg
        {
            [JsonPropertyName("bbox")]
            public List<int> Bbox { get; set; } = new();
            [JsonPropertyName("object_id")]
            public object ObjectId { get; set; } = new();
        }
        public class TextViolationCheckRequest
        {
            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            public TextViolationCheckRequest() { }
        }
        public class TextModerationResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("results")]
            public List<ModResult> Results { get; set; } = new();
            public class ModResult
            {
                [JsonPropertyName("categories")]
                public Categories CategoriesBool { get; set; } = new();
                [JsonPropertyName("category_scores")]
                public CategoryScores Category_Scores { get; set; } = new();
                [JsonPropertyName("flagged")]
                public bool Flagged { get; set; }
                public class Categories
                {
                    [JsonPropertyName("hate")]
                    public bool Hate { get; set; }
                    [JsonPropertyName("hate/threatening")]
                    public bool Hate_Threatening { get; set; }
                    [JsonPropertyName("self-harm")]
                    public bool Self_Harm { get; set; }
                    [JsonPropertyName("sexual")]
                    public bool Sexual { get; set; }
                    [JsonPropertyName("sexual/minors")]
                    public bool Sexual_Minors { get; set; }
                    [JsonPropertyName("violence")]
                    public bool Violence { get; set; }
                    [JsonPropertyName("violence/graphic")]
                    public bool Violence_Graphic { get; set; }
                }
                public class CategoryScores
                {
                    [JsonPropertyName("hate")]
                    public double Hate { get; set; }
                    [JsonPropertyName("hate/threatening")]
                    public double Hate_Threatening { get; set; }
                    [JsonPropertyName("self_harm")]
                    public double Self_Harm { get; set; }
                    [JsonPropertyName("sexual")]
                    public double Sexual { get; set; }
                    [JsonPropertyName("sexual_minors")]
                    public double Sexual_Minors { get; set; }
                    [JsonPropertyName("violence")]
                    public double Violence { get; set; }
                    [JsonPropertyName("violence_graphic")]
                    public double Violence_Graphic { get; set; }
                }
            }
        }
        public class ImageGenerationRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;
            [JsonPropertyName("n")]
            public int N { get; set; } = 1;
            [JsonPropertyName("size")]
            public string Size { get; set; } = "1024x1024";
            [JsonPropertyName("response_format")]
            public string Response_Format { get; set; } = "url"; // "b64_json"
            [JsonPropertyName("user")]
            public string User { get; set; } = string.Empty;
            public ImageGenerationRequest() { }
            public ImageGenerationRequest(string prompt, int n, string size)
            {
                Prompt = prompt;
                N = n;
                Size = size;
            }
        }
        public class ImageEditRequest
        {
            [JsonPropertyName("image_url")]
            public string Image { get; set; } = string.Empty;
            [JsonPropertyName("mask")]
            public string Mask { get; set; } = string.Empty;
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;
            [JsonPropertyName("n")]
            public int N { get; set; } = 1;
            [JsonPropertyName("size")]
            public string Size { get; set; } = "1024x1024";
            [JsonPropertyName("response_format")]
            public string Response_Format { get; set; } = "url"; // "b64_json"
            [JsonPropertyName("user")]
            public string User { get; set; } = string.Empty;
        }
        public class ImageVariationRequest
        {
            [JsonPropertyName("image")]
            public string Image { get; set; } = string.Empty;
            [JsonPropertyName("n")]
            public int N { get; set; } = 1;
            [JsonPropertyName("size")]
            public string Size { get; set; } = "1024x1024";
            public ImageVariationRequest()
            {
            }
            public ImageVariationRequest(byte[] image)
            {
                Image = image.ToString()!;
            }
        }
        public class ImageUrlResponse
        {
            [JsonPropertyName("created")]
            public long Created { get; set; }
            [JsonPropertyName("data")] 
            public List<ImageUrl> Data { get; set; } = new();
        }
        public class ImageJsonResponse
        {
            [JsonPropertyName("created")]
            public long Created { get; set; }
            [JsonPropertyName("data")]
            public List<B64Json> Data { get; set; } = new();
        }
        public class ImageUrl
        {
            public string Url { get; set; } = string.Empty;
        }
        public class B64Json
        {
            [JsonPropertyName("b64_json")]
            public string B64_Json { get; set; } = string.Empty;
        }
        public class ModelListResponse
        {
            [JsonPropertyName("list")]
            public string List { get; set; } = string.Empty;
            [JsonPropertyName("data")]
            public List<Model> Data { get; set; } = new();
        }
        public class Model
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public object Object { get; set; } = string.Empty;
            [JsonPropertyName("owned_by")]
            public string OwnedBy { get; set; } = string.Empty;
            [JsonPropertyName("permission")]
            public List<Permission> Permissions { get; set; } = new();
        }
        public class Permission
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public object Object { get; set; } = string.Empty;
            [JsonPropertyName("created")]
            public int Created { get; set; }
            [JsonPropertyName("allow_create_engine")]
            public bool AllowCreateEngine { get; set; }
            [JsonPropertyName("allow_sampling")]
            public bool AllowSampling { get; set; }
            [JsonPropertyName("allow_logprobs")]
            public bool AllowLogprobs { get; set; }
            [JsonPropertyName("allow_search_indices")]
            public bool AllowSearchIndices { get; set; }
            [JsonPropertyName("allow_view")]
            public bool AllowView { get; set; }
            [JsonPropertyName("allow_fine_tuning")]
            public bool AllowFineTuning { get; set; }
            [JsonPropertyName("organization")]
            public string Organization { get; set; } = string.Empty;
            [JsonPropertyName("group")]
            public object Group { get; set; } = string.Empty;
            [JsonPropertyName("is_blocking")]
            public bool IsBlocking { get; set; }
        }
        public class SpeechGenerationRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("format")]
            public string Format { get; set; } = string.Empty;
            [JsonPropertyName("max_tokens")]
            public int Max_Tokens { get; set; }
            [JsonPropertyName("stop")]
            public string Stop { get; set; } = string.Empty;

            public SpeechGenerationRequest()
            {
                Stop = "\"\"\"";
            }
        }
        public class FileUploadRequest
        {
            [JsonPropertyName("purpose")]
            public string Purpose { get; set; } = string.Empty;
            [JsonPropertyName("file")]
            public string File { get; set; } = string.Empty;
        }
        public class FileInformation
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public object Object { get; set; } = string.Empty;
            [JsonPropertyName("bytes")]
            public int Bytes { get; set; }
            [JsonPropertyName("created_at")]
            public int CreatedAt { get; set; }
            [JsonPropertyName("filename")]
            public string Filename { get; set; } = string.Empty;
            [JsonPropertyName("purpose")]
            public string Purpose { get; set; } = string.Empty;
        }
        public class FileDeleteResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public string File { get; set; } = string.Empty;
            [JsonPropertyName("deleted")]
            public bool Deleted { get; set; }
        }
        public class FileObject
        {
            [JsonPropertyName("file")]
            public byte[] File { get; set; }
            public FileObject(byte[] file)
            {
                File = file;
            }
        }
        public class TranscriptionRequest
        {
            [JsonPropertyName("file")]
            public string File { get; set; } = string.Empty;
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            public TranscriptionRequest()
            {
            }
        }
        public class TranscriptionResponse
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
            public TranscriptionResponse()
            {
            }
        }
        public class CompletionRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;
            [JsonPropertyName("suffix")]
            public string Suffix { get; set; } = string.Empty;
            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0;
            [JsonPropertyName("top_p")]
            public double TopP { get; set; } = 1;
            [JsonPropertyName("n")]
            public int N { get; set; } = 1;
            [JsonPropertyName("stream")]
            public bool Stream { get; set; } = false;
            [JsonPropertyName("logprobs")]
            public object Logprobs { get; set; } = new();
            [JsonPropertyName("echo")]
            public bool Echo { get; set; } = false;
            [JsonPropertyName("stop")]
            public object Stop { get; set; } = $"`?`?";
            [JsonPropertyName("presence_penalty")]
            public double Presence_Penalty { get; set; } = 0;
            [JsonPropertyName("frequency_penalty")]
            public double Frequency_Penalty { get; set; } = 0;
            [JsonPropertyName("best_of")]
            public int Best_Of { get; set; } = 1;
            //[JsonPropertyName("logit_bias")]
            //public object Logit_Bias { get; set; } = null;
            [JsonPropertyName("user")]
            public string User { get; set; } = string.Empty;
            public CompletionRequest()
            {
            }
        }
        public class CompletionResult
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public object Object { get; set; } = new();
            [JsonPropertyName("created")]
            public long Created { get; set; }
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();
            [JsonPropertyName("usage")]
            public Usage Usage { get; set; } = new();
            public CompletionResult()
            {
            }
        }
        #endregion
        #endregion
        #endregion
    }
