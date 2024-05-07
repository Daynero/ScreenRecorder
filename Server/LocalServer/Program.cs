using System.Drawing.Imaging;
using System.Net;

class Program
{
    static async Task Main(string[] args)
    {
        string url = "http://localhost:8080/";
        var server = new SimpleServer(url);
        server.Start();

        Console.WriteLine($"Server is running at {url}");
        Console.WriteLine("Press any key to stop...");
        Console.ReadKey();

        server.Stop();
    }
}

class SimpleServer
{
    private readonly HttpListener _listener;
    private readonly string _prefix;

    public SimpleServer(string url)
    {
        _listener = new HttpListener();
        _prefix = url;
        _listener.Prefixes.Add(_prefix);
    }

    public void Start()
    {
        _listener.Start();
        Task.Run(Listen);
    }

    public void Stop()
    {
        _listener.Stop();
        _listener.Close();
    }

    private async Task Listen()
    {
        while (_listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        try
        {
            using (var reader = new BinaryReader(context.Request.InputStream))
            {
                byte[] imageBytes = reader.ReadBytes((int)context.Request.ContentLength64);

                using (var ms = new MemoryStream(imageBytes))
                {
                    using (var image = System.Drawing.Image.FromStream(ms))
                    {
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string fileName = Path.Combine(documentsPath, $"{Guid.NewGuid()}.jpg");
                        image.Save(fileName, ImageFormat.Jpeg);

                        Console.WriteLine($"Image saved to {fileName}");
                    }
                }
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.Close();
        }
    }
}
