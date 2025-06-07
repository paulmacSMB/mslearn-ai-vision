using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.AI.Vision.ImageAnalysis;

// Import namespaces


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
                ImageAnalysisClient client = new(new Uri(aiSvcEndpoint), new AzureKeyCredential(aiSvcKey));

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

            // // Use a file stream to pass the image data to the analyze call
            // using FileStream stream = new FileStream(imageFile,
            //                                          FileMode.Open);

            // Use Analyze image function to read text in image
            using FileStream stream = new FileStream(imageFile, FileMode.Open);
            Console.WriteLine($"\nReading text from {imageFile} \n");

            ImageAnalysisResult result = client.Analyze(BinaryData.FromStream(stream), VisualFeatures.Read);

            stream.Close();

            if (result.Read != null)
            {
                Console.WriteLine($"Text:");
                 // Prepare image for drawing
                System.Drawing.Image image = System.Drawing.Image.FromFile(imageFile);
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Cyan, 3);
                
                foreach (var line in result.Read.Blocks.SelectMany(block => block.Lines))
                {
                    Console.WriteLine($" {line.Text}");
                }

               // Save image
                String output_file = "text.jpg";
                image.Save(output_file);
                Console.WriteLine("\nResults saved in " + output_file + "\n");   
            }
    
        }
    }
}

