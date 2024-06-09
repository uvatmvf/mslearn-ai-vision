using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Azure;

 // Import namespaces
 using Azure.AI.Vision.ImageAnalysis;


namespace read_text
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string aiSvcEndpoint = configuration["AIServicesEndpoint"];
                string aiSvcKey = configuration["AIServicesKey"];

                // Authenticate Azure AI Vision client
                ImageAnalysisClient client = new ImageAnalysisClient(
                    new Uri(aiSvcEndpoint),
                    new AzureKeyCredential(aiSvcKey));

                // Menu for text reading functions
                Console.WriteLine("\n1: Use Read API for image (Lincoln.jpg)\n2: Read handwriting (Note.jpg)\nAny other key to quit\n");
                Console.WriteLine("Enter a number:");
                string command = Console.ReadLine();
                string imageFile;

                switch (command)
                {
                    case "1":
                        imageFile = "images/Lincoln.jpg";
                        GetTextRead(imageFile, client);
                        break;
                    case "2":
                        imageFile = "images/Note.jpg";
                        GetTextRead(imageFile, client);
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

        static void GetTextRead(string imageFile, ImageAnalysisClient client)
        {
            Console.WriteLine($"\nReading text from {imageFile} \n");

            // Use a file stream to pass the image data to the analyze call
            using FileStream stream = new FileStream(imageFile,
                                                     FileMode.Open);

            analyze_image(imageFile, client, stream);

            static void analyze_image(string imageFile, ImageAnalysisClient client, FileStream stream)
            {
                // Use Analyze image function to read text in image
                ImageAnalysisResult result = client.Analyze(
                    BinaryData.FromStream(stream),
                    // Specify the features to be retrieved
                    VisualFeatures.Read);

                stream.Close();

                // Display analysis results
                if (result.Read != null)
                {
                    Console.WriteLine($"Text:");

                    // Prepare image for drawing
                    System.Drawing.Image image = System.Drawing.Image.FromFile(imageFile);
                    Graphics graphics = Graphics.FromImage(image);
                    Pen pen = new Pen(Color.Cyan, 3);

                    foreach (var line in result.Read.Blocks.SelectMany(block => block.Lines))
                    {
                        // Return the text detected in the image
                        Console.WriteLine($"   '{line.Text}'");

                        // Draw bounding box around line
                        var drawLinePolygon = true;
                        // Return the position bounding box around each line
                        Console.WriteLine($"   Bounding Polygon: [{string.Join(" ", line.BoundingPolygon)}]");
                        drawLinePolygon = get_work_position_boundaries(graphics, pen, line, drawLinePolygon);
                        // Draw line bounding polygon
                        if (drawLinePolygon)
                        {
                            var r = line.BoundingPolygon;

                            Point[] polygonPoints = {
                                new Point(r[0].X, r[0].Y),
                                new Point(r[1].X, r[1].Y),
                                new Point(r[2].X, r[2].Y),
                                new Point(r[3].X, r[3].Y)
                            };

                            graphics.DrawPolygon(pen, polygonPoints);
                        }


                    }

                    // Save image
                    String output_file = "text.jpg";
                    image.Save(output_file);
                    Console.WriteLine("\nResults saved in " + output_file + "\n");
                }

                static bool get_work_position_boundaries(Graphics graphics, Pen pen, DetectedTextLine line, bool drawLinePolygon)
                {
                    // Return each word detected in the image and the position bounding box around each word with the confidence level of each word
                    // Return each word detected in the image and the position bounding box around each word with the confidence level of each word
                    foreach (DetectedTextWord word in line.Words)
                    {
                        Console.WriteLine($"     Word: '{word.Text}', Confidence {word.Confidence:F4}, Bounding Polygon: [{string.Join(" ", word.BoundingPolygon)}]");

                        // Draw word bounding polygon
                        drawLinePolygon = false;
                        var r = word.BoundingPolygon;

                        Point[] polygonPoints = {
                            new Point(r[0].X, r[0].Y),
                            new Point(r[1].X, r[1].Y),
                            new Point(r[2].X, r[2].Y),
                            new Point(r[3].X, r[3].Y)
                        };

                        graphics.DrawPolygon(pen, polygonPoints);
                    }

                    return drawLinePolygon;
                }
            }

        }
    }
}

