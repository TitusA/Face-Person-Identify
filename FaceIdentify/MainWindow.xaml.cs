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
        private const string faceImg0 = "Img/face0.jpg";
        private const string personGroupId = "myfriends";
        private Settings settings = new Settings();
        private FaceClient faceClient;

        public MainWindow()
        {
            InitializeComponent();

            ApiKeyServiceClientCredentials serviceClientCredentials = new ApiKeyServiceClientCredentials(settings.Key);
            faceClient = new FaceClient(serviceClientCredentials);
            faceClient.Endpoint = endPoint;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //await verifyFace(faceClient);
            // await personFace(faceClient, personGroupId);
            //await personGroupTrain(faceClient, personGroupId);
            await personGroupIdentify(faceClient, personGroupId);
        }

        private async Task verifyFace(FaceClient faceClient)
        {
            if (File.Exists(faceImg1))
            {
                ResponseLV.Items.Add("File found.");
                using (FileStream fs = new FileStream(faceImg1, FileMode.Open))
                {
                    this.Title = fs.CanRead.ToString();
                    Microsoft.Rest.HttpOperationResponse<IList<DetectedFace>> res = await faceClient.Face.DetectWithStreamWithHttpMessagesAsync(fs, returnFaceAttributes: new List<FaceAttributeType> { { FaceAttributeType.Emotion }, { FaceAttributeType.Age } });
                    this.Title = res.Response.Headers.ToString();
                    var verifyFace = await faceClient.Face.VerifyFaceToFaceWithHttpMessagesAsync(res.Body[0].FaceId.Value, res.Body[0].FaceId.Value);
                    this.Title = verifyFace.Response.Headers.ToString();
                }
            }
        }
        private async Task personGroup(FaceClient faceClient)
        {

            // Create an empty PersonGroup
            string personGroupId = MainWindow.personGroupId;
            await faceClient.PersonGroup.CreateAsync(personGroupId, "My Friends");
        }

        private async Task personFace(FaceClient faceClient, string personGroupId)
        {
            // Define Anna
            var friend = await faceClient.PersonGroupPerson.CreateAsync(
                // Id of the PersonGroup that the person belonged to
                personGroupId,
                // Name of the person
                "Anna"
            );
            // Directory contains image files of Anna
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
            await faceClient.PersonGroup.TrainAsync(personGroupId);
            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    break;
                }

                await Task.Delay(1000);
            }
        }

        private async Task personGroupIdentify(FaceClient faceClient, string personGroupId)
        {
            using (Stream s = File.OpenRead(faceImg1))
            {
                var faces = await faceClient.Face.DetectWithStreamAsync(s);
                var faceIds = faces.Select(face => face.FaceId.Value).ToList();

                var results = await faceClient.Face.IdentifyAsync(faceIds,personGroupId);
                foreach (var identifyResult in results)
                {
                    Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Count == 0)
                    {
                        Console.WriteLine("No one identified");
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                        Console.WriteLine("Identified as {0}", person.Name);
                    }
                }
            }
        }



        private void ResponseLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TrainBtn_Click(object sender, RoutedEventArgs e)
        {
            personFace(faceClient, personGroupId);
            personGroupTrain(faceClient, personGroupId);
        }
    }
}
