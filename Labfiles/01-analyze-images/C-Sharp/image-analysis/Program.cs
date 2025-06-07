using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Azure;

// Import namespaces
using Azure.AI.Vision.ImageAnalysis;

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
                ImageAnalysisClient client = new(new Uri(aiSvcEndpoint), new AzureKeyCredential(aiSvcKey));

                // Analyze image
                // AnalyzeImage(imageFile, client);

                using FileStream stream = new(imageFile, FileMode.Open);
                Console.WriteLine($"\nAnalyzing {imageFile} \n");

                ImageAnalysisResult result = client.Analyze(
                    BinaryData.FromStream(stream),
                    VisualFeatures.Caption |
                    VisualFeatures.DenseCaptions |
                    VisualFeatures.Objects |
                    VisualFeatures.Tags |
                    VisualFeatures.People);

                if (result.Objects.Values.Count > 0)
                {
                    Console.WriteLine("\nObjects:");
                    foreach (DetectedObject detectedObject in result.Objects.Values)
                    {
                        Console.WriteLine($" {detectedObject.Tags[0].Name} ({detectedObject.Tags[0].Confidence:P2})");
                    }

                   
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
                        graphics.DrawString(detectedObject.Tags[0].Name,font,brush,(float)r.X, (float)r.Y);
                    }

                    // Save annotated image
                    String output_file = "objects.jpg";
                    image.Save(output_file);
                    Console.WriteLine("  Results saved in " + output_file + "\n");
                    
                }

                if (result.Tags.Values.Count > 0)
                    {
                        Console.WriteLine($"\n Tags:");
                        foreach (DetectedTag tag in result.Tags.Values)
                        {
                            Console.WriteLine($"  '{tag.Name}', Confidence: {tag.Confidence:P2}");
                        }
                    }

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

                // Get image captions
                if (result.Caption.Text != null)
                {
                    Console.WriteLine("\nCaption:");
                    Console.WriteLine($"   \"{result.Caption.Text}\", Confidence {result.Caption.Confidence:0.00}\n");
                }

                Console.WriteLine(" Dense Captions:");
                foreach (DenseCaption denseCaption in result.DenseCaptions.Values)
                {
                    Console.WriteLine($"   Caption: '{denseCaption.Text}', Confidence: {denseCaption.Confidence:0.00}");
                }
               

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
