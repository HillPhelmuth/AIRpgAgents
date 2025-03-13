using AIRpgAgents.ChatModels;
using AIRpgAgents.GameEngine.Extensions;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AIRpgAgents.Components.ChatComponents;

public partial class UploadImageWindow
{
    private FileUpload _fileUpload = new();
    private int maxFileSize = 1024 * 1024 * 500;
    [Inject]
    private DialogService DialogService { get; set; } = default!;

    private MultiFileUpload _multiFileUpload = new(){FileUploads = [new FileUpload()]};
    private void Submit(MultiFileUpload fileUploads)
    {
        foreach (var fileUpload in fileUploads.FileUploads)
        {
            if (FileHelper.TryConvertFromBase64String(fileUpload.FileBase64!, out var bytes))
            {
                fileUpload.FileBytes = bytes;
            }
        }
        
        DialogService.Close(fileUploads);
    }
    private void Add()
    {
        if (_multiFileUpload.FileUploads.Count < 5)
            _multiFileUpload.FileUploads.Add(new FileUpload());
    }
    private void Remove(FileUpload fileUpload)
    {
        if (_multiFileUpload.FileUploads.Count > 1)
            _multiFileUpload.FileUploads.Remove(fileUpload);
    }
}