namespace MemDrawer.Domain;

public class Image(string blobFileName, string md5Hash)
{
    public int Id { get; private set; }

    public string BlobFileName { get; private set; } = blobFileName;
    
    public string Md5Hash { get; private set; } = md5Hash;

}