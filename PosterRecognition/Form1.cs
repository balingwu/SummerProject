using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PosterRecognition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        const string uriBase =
            "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/ocr";
        const string subscriptionKey = "2c421e4c61514be1b1b468d7461ef81f";

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = string.Empty;
            pictureBox1.Image = null;
            pictureBox1.Refresh();

            try
            {
                string imageFilePath = textBox1.Text;
                pictureBox1.Load(textBox1.Text);

                if (File.Exists(imageFilePath))
                {
                    // Make the REST API call.
                    label1.Text = "Wait a moment for the results to appear.";
                    MakeOCRRequest(imageFilePath).Wait();
                }
                else
                {
                    label1.Text = "\nInvalid file path";
                }
            }
            catch (Exception ex)
            {
                if (ex is System.TypeInitializationException)
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

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters.
                string requestParameters = "language=unk&detectOrientation=true";

                // Assemble the URI for the REST API Call.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

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
                label1.Text = "\nResponse:\n\n{0}\n" + JToken.Parse(contentString).ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show("\n" + e.Message);
            }
        }

        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}
