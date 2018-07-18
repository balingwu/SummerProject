using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace PosterRecognition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox2.Text = "Enter the path to the TextBox with text you wish to read: ";
            pictureBox1.Image = null;
        }

        const string uriBase =
            "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/ocr";
        const string subscriptionKey = "b32a840e9c784d3d9f2a7375b92099bb";
        string poster = "";
        string language = "";

        private async void button1_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
            pictureBox1.Refresh();

            try
            {
                string imageFilePath = textBox1.Text;

                if (File.Exists(imageFilePath))
                {
                    pictureBox1.Load(imageFilePath);
                    // Make the REST API call.
                    textBox2.AppendText("\nWait a moment for the results to appear.");
                    await MakeOCRRequest(imageFilePath);
                }
                else
                {
                    textBox2.Text = "Invalid file path";
                }

                textBox2.AppendText("\n===== Poster - Key ======\n");

                // Create a client.
                ITextAnalyticsAPI client = new TextAnalyticsAPI(new ApiKeyServiceClientCredentials())
                {
                    AzureRegion = AzureRegions.Westcentralus
                };

                // Getting key-phrases
                KeyPhraseBatchResult result2 = client.KeyPhrasesAsync(input: new MultiLanguageBatchInput(
                            documents: new List<MultiLanguageInput>()
                            {
                            new MultiLanguageInput(language, "0", poster),
                            })
                    ).Result;
                // Printing keyphrases
                foreach (var document in result2.Documents)
                {
                    textBox2.AppendText(String.Format("\nDocument ID: {0} \n", document.Id));

                    foreach (string keyphrase in document.KeyPhrases)
                    {
                        textBox2.AppendText("  " + keyphrase);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "发生异常：" + ex, "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textBox2.Text = "上一次操作中发生了异常。";
            }
        }

        private async Task MakeOCRRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(String.Empty);

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters.
                //queryString["mode"] = "Printed";
                queryString["language"] = "unk";
                queryString["detectOrientation"] = "true";

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
                JToken jToken = JToken.Parse(contentString);
                textBox2.Clear();
                textBox2.AppendText("Response:\n" + jToken.ToString());
                textBox2.AppendText("\n");

                // Find the text and the language.
                string[] arraystring = contentString.Split(new string[] { "language\":\"", "\",\"orientation", "\"}", "text\":\"" }, StringSplitOptions.None);

                for (int i = 0; i < arraystring.Length; i++)
                {
                    textBox2.AppendText("\n" + i + ": " + arraystring[i]);
                }
                language = arraystring[1];
                string split = language.Contains("zh") ? String.Empty : " ";
                for (int i = 3; i < arraystring.Length - 1; i += 2)
                {
                    poster += arraystring[i] + split;
                }

                textBox2.AppendText("\nposter: " + poster + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "发生异常：" + ex, "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textBox2.Text = "上一次操作中出现了异常。";
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar=='\r')
            {
                button1_Click(sender, e);
            }
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!e.Cancel)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
        }

        /// <summary>
        /// Container for subscription credentials. Make sure to enter your valid key.
        /// </summary>
        class ApiKeyServiceClientCredentials : ServiceClientCredentials
        {
            public override Task ProcessHttpRequestAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                request.Headers.Add("Ocp-Apim-Subscription-Key", "516da86980ca4a5085bfdb41b0f5160e");
                return base.ProcessHttpRequestAsync(request, cancellationToken);
            }
        }
    }
}
