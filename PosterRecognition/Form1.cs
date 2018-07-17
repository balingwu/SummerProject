using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json.Linq;

namespace PosterRecognition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label1.Text = "Enter the path to the TextBox with text you wish to read: ";
            pictureBox1.Image = null;
        }

        const string uriBase =
            "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/ocr";
        const string subscriptionKey = "b32a840e9c784d3d9f2a7375b92099bb";

        private async void button1_Click(object sender, EventArgs e)
        {
            label1.Text = string.Empty;
            pictureBox1.Refresh();

            try
            {
                string imageFilePath = textBox1.Text;

                if (File.Exists(imageFilePath))
                {
                    pictureBox1.Load(imageFilePath);
                    // Make the REST API call.
                    label1.Text += "\nWait a moment for the results to appear.";
                    await MakeOCRRequest(imageFilePath);
                }
                else
                {
                    label1.Text = "Invalid file path";
                }
            }
            catch (Exception ex)
            {
                if (ex is TypeInitializationException)
                {
                    MessageBox.Show("识别错误：" + ex.Message);
                }
            }
        }

        private async Task MakeOCRRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters.
                //queryString["mode"] = "Printed";
                queryString["language"] = "unk";
                queryString["detectOrientation"] = "true";
                //var uri = "https://westus.api.cognitive.microsoft.com/vision/v2.0/recognizeText?" + queryString;

                // Assemble the URI for the REST API Call.
                string uri = uriBase + "?" + queryString;

                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = File.ReadAllBytes(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Make the REST API call.
                    response = await client.PostAsync(uri, content);
                }

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                label1.Text += "\nResponse:\n" + JToken.Parse(contentString).ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show("\n" + e.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = String.Empty;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar=='\r')
            {
                button1_Click(sender, e);
            }
        }
    }
}
