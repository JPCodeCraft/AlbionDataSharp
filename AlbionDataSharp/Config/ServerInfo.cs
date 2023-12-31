namespace AlbionDataSharp.Config
{
    public class ServerInfo
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public AlbionServer AlbionServer { get; set; }
        public UploadType UploadType { get; set; }
        public string Color { get; set; }
        public bool IsReachable { get; set; } = false;
    }
}