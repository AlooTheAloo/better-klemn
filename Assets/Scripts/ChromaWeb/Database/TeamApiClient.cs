using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ChromaWeb.Database
{
    [Serializable]
    public struct TeamInfo
    {
        [JsonProperty("team_id")]
        public int TeamId { get; set; }
        [JsonProperty("team_name")]
        public string TeamName { get; set; }
        
        [JsonProperty("first_player_name")]
        public string NomPremierJoueur { get; set; }
        
        [JsonProperty("second_player_name")]
        public string NomSecondJoueur { get; set; }

        public int NbTours;


        [JsonConstructor]
        public TeamInfo(int team_id, string team_name, string first_player_name, string second_player_name)
        {
            TeamId = team_id;
            TeamName = team_name;
            NomPremierJoueur = first_player_name;
            NomSecondJoueur = second_player_name;
            NbTours = Constantes.NOMBRE_TOURS;
        }
    }
    
    [Serializable]
    public struct InsertTeamInfo
    {
        [JsonProperty("team_name")]
        public string TeamName { get; set; }
        
        [JsonProperty("first_player_name")]
        public string NomPremierJoueur { get; set; }
        
        [JsonProperty("second_player_name")]
        public string NomSecondJoueur { get; set; }
        
        [JsonConstructor]
        public InsertTeamInfo(string team_name, string first_player_name, string second_player_name)
        {
            TeamName = team_name;
            NomPremierJoueur = first_player_name;
            NomSecondJoueur = second_player_name;
        }
    }
    
    public class TeamApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        
        public TeamApiClient(string apiBaseUrl)
        {
            _apiBaseUrl = apiBaseUrl;
            _httpClient = new HttpClient();
        }
        
        public async Task<List<TeamInfo>> ListTeamInfoAsync()
        {
            try
            {
                string url = $"{_apiBaseUrl}/equipes/all";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonContent = await response.Content.ReadAsStringAsync();
                List<TeamInfo> teamList = JsonConvert.DeserializeObject<List<TeamInfo>>(jsonContent);
                return teamList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task<TeamInfo?> GetNextTeamAsync()
        {
            try
            {
                string url = $"{_apiBaseUrl}/next";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonContent = await response.Content.ReadAsStringAsync();
                TeamInfo teamInfo = JsonConvert.DeserializeObject<TeamInfo>(jsonContent);
                teamInfo.NbTours = Constantes.NOMBRE_TOURS;
                return teamInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }


        public async Task<TeamInfo> GetTeamInfoAsync(int teamId)
        {
            try
            {
                string url = $"{_apiBaseUrl}/equipes/{teamId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonContent = await response.Content.ReadAsStringAsync();
                TeamInfo teamInfo = JsonConvert.DeserializeObject<TeamInfo>(jsonContent);
                return teamInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async void AddTeamInfoAsync(InsertTeamInfo teamInfo)
        {
            try
            {
                string url = $"{_apiBaseUrl}/equipes/add";
                string jsonContent = JsonConvert.SerializeObject(teamInfo);
                HttpResponseMessage response = await _httpClient.PutAsync(url, new StringContent(jsonContent));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public async void UpdateTeamInfoAsync(TeamInfo teamInfo)
        {
            try
            {
                string url = $"{_apiBaseUrl}/equipes/update";
                string jsonContent = JsonConvert.SerializeObject(teamInfo);
                HttpResponseMessage response = await _httpClient.PatchAsync(url, new StringContent(jsonContent));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<int> GetNombreTours()
        {
            try
            {
                string url = $"{_apiBaseUrl}/admin/tours";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                int teamInfo = int.Parse(content);
                return teamInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}