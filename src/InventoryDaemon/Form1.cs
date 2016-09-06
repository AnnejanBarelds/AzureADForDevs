using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json;

namespace InventoryDaemon
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Waiting...";
            Application.DoEvents();
            var token = GetToken();
            System.Net.HttpStatusCode statusCode;
            var result = CallAPI(token, out statusCode);

            label1.Text = statusCode.ToString();
            textBox1.Text = TokenPrettify(result);
        }

        private string GetToken()
        {
            var aadinstance = ConfigurationManager.AppSettings["AadInstance"];
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            var resource = ConfigurationManager.AppSettings["InventoryService"];

            var credential = new ClientCredential(clientId, clientSecret);
            var authContext = new AuthenticationContext(aadinstance);
            var token = authContext.AcquireTokenAsync(resource, credential).Result;

            return token.AccessToken;
        }

        private string CallAPI(string token, out System.Net.HttpStatusCode statusCode)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetUrl() + "/api/inventory/update");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = client.SendAsync(request).Result;
            statusCode = response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            return null;
        }

        private string GetUrl()
        {
            var url = "https://localhost:44321";
            return url;
        }

        public static string TokenPrettify(string token)
        {
            var i = token.IndexOf("}.{");
            var headerString = token.Substring(0, i + 1);
            var valueString = token.Substring(i + 2);

            var header = JsonPrettify(headerString);
            var value = JsonPrettify(valueString);

            return header + Environment.NewLine + "." + Environment.NewLine + value;
        }

        public static string JsonPrettify(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }
    }
}
