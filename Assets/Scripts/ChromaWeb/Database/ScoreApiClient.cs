using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ChromaWeb.Database
{
    
    /// <summary>
    /// Struct qui contient les informations d'un score tel que stocké dans la base de données.
    /// </summary>
    
    [Serializable]
    public struct ScoreInfo
    {
        [JsonProperty("score_id")]
        public int ScoreId { get; set; }
        [JsonProperty("score_value")]
        public ulong ScoreValue { get; set; }
        [JsonProperty("score_timestamp")]
        public string ScoreTimestamp { get; set; }
        [JsonProperty("map_id")]
        public int MapId { get; set; }
        [JsonProperty("team_id")]
        public int TeamId { get; set; }
        
        [JsonConstructor]
        public ScoreInfo(int score_id, ulong score_value, string score_timestamp, int map_id, int team_id)
        {
            ScoreId = score_id;
            ScoreValue = score_value;
            ScoreTimestamp = score_timestamp;
            MapId = map_id;
            TeamId = team_id;
        }
    }

    [Serializable]
    public struct InsertScoreInfo
    {
        [JsonProperty("score_value")]
        public ulong ScoreValue { get; set; }
        [JsonProperty("map_id")]
        public int MapId { get; set; }
        [JsonProperty("team_id")]
        public int TeamId { get; set; }
        
        [JsonConstructor]
        public InsertScoreInfo(ulong score_value, int map_id, int team_id)
        {
            ScoreValue = score_value;
            MapId = map_id;
            TeamId = team_id;
        }
    }
    
    
    
    /// <summary>
    /// ScoreApiClient est la classe qui permet de communiquer avec l'API pour les scores.
    /// Elle propose des méthodes pour récupérer les scores, les ajouter et les modifier.
    /// </summary>
    [Serializable]
    public class ScoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        
        public ScoreApiClient(string apiBaseUrl)
        {
            _apiBaseUrl = apiBaseUrl;
            _httpClient = new HttpClient();
        }
        
        /// <summary>
        /// Liste les scores de la base de données.
        /// </summary>
        /// <returns>Une liste de ScoreInfo contenant les scores de la base de données.</returns>
        public async Task<List<ScoreInfo>> ListScoreInfoAsync()
        {
            try
            {
                string url = $"{_apiBaseUrl}/scores/all";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonContent = await response.Content.ReadAsStringAsync();
                List<ScoreInfo> scoreList = JsonConvert.DeserializeObject<List<ScoreInfo>>(jsonContent);
                return scoreList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        /// <summary>
        /// Retroouve les scores reliés à une certaine map.
        /// </summary>
        /// <param name="mapId">L'ID de la map</param>
        /// <returns>La liste de ScoreInfo contenant les scores enregistrés pour une map</returns>
        public async Task<List<ScoreInfo>> GetScoreInfoAsync(int mapId)
        {
            try
            {
                string url = $"{_apiBaseUrl}/scores/{mapId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonContent = await response.Content.ReadAsStringAsync();
                List<ScoreInfo> leaderboard = JsonConvert.DeserializeObject<List<ScoreInfo>>(jsonContent);
                return leaderboard;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async void AddScoreInfoAsync(InsertScoreInfo scoreInfo)
        {
            try
            {
                string url = $"{_apiBaseUrl}/scores/add";
                string jsonContent = JsonConvert.SerializeObject(scoreInfo);
                Debug.Log(jsonContent);
                HttpResponseMessage response = await _httpClient.PutAsync(url, new StringContent(jsonContent, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw;
            }

        }

        public async void UpdateScoreInfoAsync(ScoreInfo scoreInfo)
        {
            try
            {
                string url = $"{_apiBaseUrl}/scores/update";
                string jsonContent = JsonConvert.SerializeObject(scoreInfo);
                HttpResponseMessage response = await _httpClient.PatchAsync(url, new StringContent(jsonContent));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}