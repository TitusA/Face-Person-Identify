using FaceIdentify.Properties;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace FaceIdentify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string endPoint = @"https://westus.api.cognitive.microsoft.com/";
        private const string faceImg1 = @"Img/face1.jpg";
        private const string faceImg0 = "Img/face0.jpg";//for detecting similar faces
        private const string personGroupId = "my-unique-person-group";
        private Settings settings = new Settings();
        private FaceClient faceClient;

        public MainWindow()
        {
            InitializeComponent();

            ApiKeyServiceClientCredentials serviceClientCredentials = new ApiKeyServiceClientCredentials(settings.Key);
            faceClient = new FaceClient(serviceClientCredentials);
            faceClient.Endpoint = endPoint;

        }

        private async void verifyFace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonsPanel.IsEnabled = false;
                await verifyFace(faceClient);
            }
            finally
            {
                ButtonsPanel.IsEnabled = true;
            }
        }

        private async Task verifyFace(FaceClient faceClient)
        {
            if (File.Exists(faceImg1))
            {
                ResponseLV.Items.Add("Face image found.");
                using (FileStream fs = new FileStream(faceImg1, FileMode.Open))
                {
                    Microsoft.Rest.HttpOperationResponse<IList<DetectedFace>> res = await faceClient.Face.DetectWithStreamWithHttpMessagesAsync(fs, returnFaceAttributes: new List<FaceAttributeType> { { FaceAttributeType.Emotion }, { FaceAttributeType.Age } });
                    if (res.Response.IsSuccessStatusCode)
                        if (res.Body.Count > 0)
                        {
                            ResponseLV.Items.Add(String.Format("Face {0} detected", res.Body[0].FaceId));
                            var verifyFace = await faceClient.Face.VerifyFaceToFaceWithHttpMessagesAsync(res.Body[0].FaceId.Value, res.Body[0].FaceId.Value);
                            ResponseLV.Items.Add(String.Format("Faces simalar confidence: {0}", verifyFace.Body.Confidence));
                        }
                }
            }
        }
        private async Task personGroup(FaceClient faceClient, string personGroupId)
        {

            // Create an empty PersonGroup
            try
            {
                await faceClient.PersonGroup.CreateAsync(personGroupId, "My Friends");
            }
            catch (APIErrorException ex)
            { }
        }

        private async Task personFace(FaceClient faceClient, string personGroupId)
        {
            // Define Anna
            ResponseLV.Items.Add("Define Anna");
            var friend = await faceClient.PersonGroupPerson.CreateAsync(
                // Id of the PersonGroup that the person belonged to
                personGroupId,
                // Name of the person
                "Anna"
            );
            // Directory contains image files of Anna
            ResponseLV.Items.Add("Add images for Anna");
            string imageDir = Path.GetDirectoryName(faceImg0);

            foreach (string imagePath in Directory.GetFiles(imageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Anna
                    await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                        personGroupId, friend.PersonId, s);
                }
            }
        }
        private async Task personGroupTrain(FaceClient faceClient, string personGroupId)
        {
            ResponseLV.Items.Add("Train PersonGroup");

            await faceClient.PersonGroup.TrainAsync(personGroupId);
            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    ResponseLV.Items.Add("Training complete");
                    break;
                }
                ResponseLV.Items.Add("Training running");
                await Task.Delay(1000);
            }
        }

        private async Task personGroupIdentify(FaceClient faceClient, string personGroupId)
        {
            ResponseLV.Items.Add("Start Person identify in group.");
            using (Stream s = File.OpenRead(faceImg1))
            {
                ResponseLV.Items.Add(faceImg1 + " file found.");
                var faces = await faceClient.Face.DetectWithStreamAsync(s);
                var faceIds = faces.Select(face => face.FaceId.Value).ToList();

                var results = await faceClient.Face.IdentifyAsync(faceIds, personGroupId);
                foreach (var identifyResult in results)
                {
                    ResponseLV.Items.Add(String.Format("Result of face: {0}", identifyResult.FaceId));
                    if (identifyResult.Candidates.Count == 0)
                    {
                        ResponseLV.Items.Add("No one identified");
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                        ResponseLV.Items.Add(String.Format("Identified as {0}", person.Name));
                    }
                }
            }
        }

        private void ResponseLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void TrainBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonsPanel.IsEnabled = false;

                ResponseLV.Items.Add("Start training");
                await personGroup(faceClient, personGroupId);
                await personFace(faceClient, personGroupId);
                await personGroupTrain(faceClient, personGroupId);
                await personGroupIdentify(faceClient, personGroupId);
            }
            finally
            {
                ButtonsPanel.IsEnabled = true;
            }
        }

    }
}
