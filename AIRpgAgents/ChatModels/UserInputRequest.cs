namespace AIRpgAgents.ChatModels;

public class UserInputRequest
{
	public string? Character { get; set; }
	public string? ChatInput { get; set; }
	public UserInputFieldType UserInputFieldType { get; set; }
	public string? ImageUrlInput { get; set; }
	public FileUpload? FileUpload { get; set; }
	public List<FileUpload> FileUploads { get; set; } = [];
}
public class FileUpload
{
    public string? FileBase64 { get; set; }
    public string? FileName { get; set; }
    public byte[] FileBytes { get; set; } = [];
}
public class MultiFileUpload
{
    public List<FileUpload> FileUploads { get; set; } = [];
}