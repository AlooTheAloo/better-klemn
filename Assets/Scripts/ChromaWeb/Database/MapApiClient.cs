using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace ChromaWeb.Database
{
    [Serializable]
    public struct MapInfo
    {
        [JsonProperty("map_id")]
        public int MapId { get; set; }
        [JsonProperty("map_name")]
        public string MapName { get; set; }
        [JsonProperty("map_artist")]
        public string MapArtist { get; set; }
        [JsonProperty("map_creator")]
        [CanBeNull] public string MapCreator { get; set; }
        
        public MapInfo(int map_id, string map_name, string map_artist, [CanBeNull] string map_creator)
        {
            MapId = map_id;
            MapName = map_name;
            MapArtist = map_artist;
            MapCreator = map_creator;
        }
    }

    [Serializable]
    public struct InsertMapInfo
    {
        [JsonProperty("map_name")]
        public string MapName { get; set; }
        [JsonProperty("map_artist")]
        public string MapArtist { get; set; }
        [JsonProperty("map_creator")]
        [CanBeNull] public string MapCreator { get; set; }
        
        public InsertMapInfo(string map_name, string map_artist, [CanBeNull] string map_creator)
        {
            MapName = map_name;
            MapArtist = map_artist;
            MapCreator = map_creator;
        }
    }

    public class MapApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        public MapApiClient(string apiBaseUrl)
        {
            _apiBaseUrl = apiBaseUrl;
            _httpClient = new HttpClient();
        }

        public async Task<List<MapInfo>> ListMapInfoAsync()
        {
            try
            {
                string url = $"{_apiBaseUrl}/maps/all";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonContent = await response.Content.ReadAsStringAsync();
                List<MapInfo> mapList = JsonConvert.DeserializeObject<List<MapInfo>>(jsonContent);
                return mapList;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new();
            }
        }

        public async Task<MapInfo> GetMapInfoAsync(int mapId)
        {
            try
            {
                string url = $"{_apiBaseUrl}/maps/{mapId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonContent = await response.Content.ReadAsStringAsync();
                MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(jsonContent);
                return mapInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new MapInfo();
            }
        }
        
        public async void AddMapInfoAsync(InsertMapInfo mapInfo)
        {
            try
            {
                string url = $"{_apiBaseUrl}/maps/add";
                string jsonContent = JsonConvert.SerializeObject(mapInfo);
                HttpResponseMessage response = await _httpClient.PutAsync(url, new StringContent(jsonContent));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                 
            }
        }

        public async void PatchMapInfoAsync(MapInfo mapInfo)
        {
            try
            {
                string url = $"{_apiBaseUrl}/maps/update";
                string jsonContent = JsonConvert.SerializeObject(mapInfo);
                HttpResponseMessage response = await _httpClient.PatchAsync(url, new StringContent(jsonContent));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                 
            }
        }
    }
}

