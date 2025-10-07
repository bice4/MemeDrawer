namespace MemDrawer.Contracts.Http.Api.v1.Requests;

public class DrawOnImageRequest
{
    public int ImageId { get; set; }
    
    public string? TopText { get; set; }
    
    public string? BottomText { get; set; }
    
    public string? TextColorHex { get; set; }
    
    public string? BackgroundColorHex { get; set; }
    
    public byte? BackgroundOpacity { get; set; }
    
    public bool WithOutline { get; set; }
}