using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.AI.Vision.ImageAnalysis;

// Import namespaces

namespace image_analysis
{
    class Program
    {

        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string aiSvcEndpoint = configuration["AIServicesEndpoint"];
                string aiSvcKey = configuration["AIServicesKey"];

                // Get image
                string imageFile = "images/street.jpg";
                if (args.Length > 0)
                {
                    imageFile = args[0];
                }

                // Authenticate Azure AI Vision client
                ImageAnalysisClient client = new ImageAnalysisClient(
                                            new Uri(aiSvcEndpoint),
                                            new AzureKeyCredential(aiSvcKey));
                
                // Analyze image
                AnalyzeImage(imageFile, client);

                // Remove the background or generate a foreground matte from the image
                await BackgroundForeground(imageFile, aiSvcEndpoint, aiSvcKey);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void AnalyzeImage(string imageFile, ImageAnalysisClient client)
        {
            Console.WriteLine($"\nAnalyzing {imageFile} \n");

            // Use a file stream to pass the image data to the analyze call
            using FileStream stream = new FileStream(imageFile,
                                                     FileMode.Open);

            // Get result with specified features to be retrieved
            ImageAnalysisResult result = client.Analyze(
                                            BinaryData.FromStream(stream),
                                            VisualFeatures.Caption |
                                            VisualFeatures.DenseCaptions |
                                            VisualFeatures.Objects |
                                            VisualFeatures.Tags |
                                            VisualFeatures.People);

            // Display analysis results
            if (result.Caption.Text != null)
            {
                Console.WriteLine(" Caption:");
                Console.WriteLine($"   \"{result.Caption.Text}\", Confidence {result.Caption.Confidence:0.00}\n");
            }

            get_dense_captions(result);

            get_tags(result);

            get_objects(imageFile, stream, result);

            get_people(imageFile, result);

            static void get_people(string imageFile, ImageAnalysisResult result)
            {
                // Get people in the image
                if (result.People.Values.Count > 0)
                {
                    Console.WriteLine($" People:");

                    // Prepare image for drawing
                    System.Drawing.Image image = System.Drawing.Image.FromFile(imageFile);
                    Graphics graphics = Graphics.FromImage(image);
                    Pen pen = new Pen(Color.Cyan, 3);
                    Font font = new Font("Arial", 16);
                    SolidBrush brush = new SolidBrush(Color.WhiteSmoke);

                    foreach (DetectedPerson person in result.People.Values)
                    {
                        // Draw object bounding box
                        var r = person.BoundingBox;
                        Rectangle rect = new Rectangle(r.X, r.Y, r.Width, r.Height);
                        graphics.DrawRectangle(pen, rect);

                        // Return the confidence of the person detected
                        //Console.WriteLine($"   Bounding box {person.BoundingBox.ToString()}, Confidence: {person.Confidence:F2}");
                    }

                    // Save annotated image
                    String output_file = "persons.jpg";
                    image.Save(output_file);
                    Console.WriteLine("  Results saved in " + output_file + "\n");
                }
            }

            static void get_objects(string imageFile, FileStream stream, ImageAnalysisResult result)
            {
                // Get objects in the image
                if (result.Objects.Values.Count > 0)
                {
                    Console.WriteLine(" Objects:");

                    // Prepare image for drawing
                    stream.Close();
                    System.Drawing.Image image = System.Drawing.Image.FromFile(imageFile);
                    Graphics graphics = Graphics.FromImage(image);
                    Pen pen = new Pen(Color.Cyan, 3);
                    Font font = new Font("Arial", 16);
                    SolidBrush brush = new SolidBrush(Color.WhiteSmoke);

                    foreach (DetectedObject detectedObject in result.Objects.Values)
                    {
                        Console.WriteLine($"   \"{detectedObject.Tags[0].Name}\"");

                        // Draw object bounding box
                        var r = detectedObject.BoundingBox;
                        Rectangle rect = new Rectangle(r.X, r.Y, r.Width, r.Height);
                        graphics.DrawRectangle(pen, rect);
                        graphics.DrawString(detectedObject.Tags[0].Name, font, brush, (float)r.X, (float)r.Y);
                    }

                    // Save annotated image
                    String output_file = "objects.jpg";
                    image.Save(output_file);
                    Console.WriteLine("  Results saved in " + output_file + "\n");
                }
            }

            static void get_tags(ImageAnalysisResult result)
            {
                // get image tags
                if (result.Tags.Values.Count > 0)
                {
                    Console.WriteLine($"\n Tags:");
                    foreach (DetectedTag tag in result.Tags.Values)
                    {
                        Console.WriteLine($"   '{tag.Name}', Confidence: {tag.Confidence:F2}");
                    }
                }
            }

            static void get_dense_captions(ImageAnalysisResult result)
            {
                // Get image dense captions
                Console.WriteLine(" Dense Captions:");
                foreach (DenseCaption denseCaption in result.DenseCaptions.Values)
                {
                    Console.WriteLine($"   Caption: '{denseCaption.Text}', Confidence: {denseCaption.Confidence:0.00}");
                }
            }
        }
        static async Task BackgroundForeground(string imageFile, string endpoint, string key)
        {
            // Remove the background from the image or generate a foreground matte
            
        }
    }
}
