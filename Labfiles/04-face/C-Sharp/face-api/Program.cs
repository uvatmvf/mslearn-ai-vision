using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

 using Microsoft.Azure.CognitiveServices.Vision.Face;
 using Microsoft.Azure.CognitiveServices.Vision.Face.Models;



namespace analyze_faces
{
    class Program
    {

        private static FaceClient faceClient;
        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcEndpoint = configuration["AIServicesEndpoint"];
                string cogSvcKey = configuration["AIServiceKey"];

                // Authenticate Face client
                ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
                faceClient = new FaceClient(credentials)
                {
                    Endpoint = cogSvcEndpoint
                };


                // Menu for face functions
                Console.WriteLine("1: Detect faces\nAny other key to quit");
                Console.WriteLine("Enter a number:");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "1":
                        await DetectFaces("images/people.jpg");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task DetectFaces(string imageFile)
        {
            Console.WriteLine($"Detecting faces in {imageFile}");

            // Specify facial features to be retrieved
            specify_features_to_retrieve();

            static void specify_features_to_retrieve()
            {
                IList<FaceAttributeType> features = new FaceAttributeType[]
                            {
                FaceAttributeType.Occlusion,
                FaceAttributeType.Blur,
                FaceAttributeType.Glasses
                            };
            }


            // Get faces
            await get_faces(imageFile);

            static async Task get_faces(string imageFile)
            {
                using (var imageData = File.OpenRead(imageFile))
                {
                    var detected_faces = await faceClient.Face.DetectWithStreamAsync(imageData, returnFaceAttributes: features, returnFaceId: false);

                    if (detected_faces.Count() > 0)
                    {
                        Console.WriteLine($"{detected_faces.Count()} faces detected.");

                        // Prepare image for drawing
                        Image image = Image.FromFile(imageFile);
                        Graphics graphics = Graphics.FromImage(image);
                        Pen pen = new Pen(Color.LightGreen, 3);
                        Font font = new Font("Arial", 4);
                        SolidBrush brush = new SolidBrush(Color.White);
                        int faceCount = 0;

                        // Draw and annotate each face
                        foreach (var face in detected_faces)
                        {
                            faceCount++;
                            Console.WriteLine($"\nFace number {faceCount}");

                            // Get face properties
                            Console.WriteLine($" - Mouth Occluded: {face.FaceAttributes.Occlusion.MouthOccluded}");
                            Console.WriteLine($" - Eye Occluded: {face.FaceAttributes.Occlusion.EyeOccluded}");
                            Console.WriteLine($" - Blur: {face.FaceAttributes.Blur.BlurLevel}");
                            Console.WriteLine($" - Glasses: {face.FaceAttributes.Glasses}");

                            // Draw and annotate face
                            var r = face.FaceRectangle;
                            Rectangle rect = new Rectangle(r.Left, r.Top, r.Width, r.Height);
                            graphics.DrawRectangle(pen, rect);
                            string annotation = $"Face number {faceCount}";
                            graphics.DrawString(annotation, font, brush, r.Left, r.Top);
                        }

                        // Save annotated image
                        String output_file = "detected_faces.jpg";
                        image.Save(output_file);
                        Console.WriteLine(" Results saved in " + output_file);
                    }
                }
            }

        }
    }
}
