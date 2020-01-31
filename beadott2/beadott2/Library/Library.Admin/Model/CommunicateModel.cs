using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System;
using Library.Data;

namespace Library.Admin
{
    public class CommunicateModel
    {
        private readonly HttpClient _client;

        private const string EndPoint = "http://localhost:19707/api/librarian";
        private const string IdentityEndPoint = "http://localhost:19707/api/identity";

        public CommunicateModel()
        {
            _client = new HttpClient();
        }

        #region Listings

        public async Task<IEnumerable<BookDTO>> ListBooks()
        {
            var response = await _client.GetAsync(EndPoint + "/ListBooks");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<IEnumerable<BookDTO>>();         
        }

        public async Task<IEnumerable<LendingDTO>> ListLendings()
        {
            var response = await _client.GetAsync(EndPoint + "/ListLendings");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<IEnumerable<LendingDTO>>();
        }

        #endregion

        #region Login

        public async Task Login(string userName, string userPassword)
        {
            UserDTO user = new UserDTO()
            {
                UserName = userName,
                UserPassword = userPassword
            };
            
            string userJson = JsonConvert.SerializeObject(user);
            StringContent content = new StringContent(userJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(IdentityEndPoint + "/Login", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task Logout()
        {
            var response = await _client.GetAsync(IdentityEndPoint + "/Logout");
            response.EnsureSuccessStatusCode();
        }

        // Only for creating test user.
        public async Task Register(string userName, string userPassword)
        {
            UserDTO user = new UserDTO()
            {
                UserName = userName,
                UserPassword = userPassword,
                Id = 1
            };
            string userJson = JsonConvert.SerializeObject(user);
            StringContent content = new StringContent(userJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(IdentityEndPoint + "/Login", content);
            response.EnsureSuccessStatusCode();
        }

        #endregion

        #region Database modifier methods

        public async Task<bool> DeleteVol(int volId)
        {
            string volIdJson = JsonConvert.SerializeObject(volId);
            StringContent content = new StringContent(volIdJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(EndPoint + "/DeleteVol", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddBook(BookDTO bookDTO)
        {
            string bookDTOJson = JsonConvert.SerializeObject(bookDTO);
            StringContent content = new StringContent(bookDTOJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(EndPoint + "/AddBook", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddVol(int bookID)
        {
            string bookIDJson = JsonConvert.SerializeObject(bookID);
            StringContent content = new StringContent(bookIDJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(EndPoint + "/AddVol", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActivateLending(int lendID)
        {
            string lendIDJson = JsonConvert.SerializeObject(lendID);
            StringContent content = new StringContent(lendIDJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(EndPoint + "/ActivateLending", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> InactivateLending(int lendID)
        {
            string lendIDJson = JsonConvert.SerializeObject(lendID);
            StringContent content = new StringContent(lendIDJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(EndPoint + "/InactivateLending", content);
            return response.IsSuccessStatusCode;
        }

        #endregion
    }
}
