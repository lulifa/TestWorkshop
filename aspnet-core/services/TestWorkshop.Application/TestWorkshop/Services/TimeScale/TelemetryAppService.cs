using Microsoft.AspNetCore.Http;
using System.IO;
using TestWorkshop.TimeScale;
using Volo.Abp.BlobStoring;

namespace TestWorkshop;

public class TelemetryAppService : TestWorkshopAppService, ITelemetryAppService
{
    private readonly IBlobContainer _blob;
    public TelemetryAppService(IBlobContainer blob)
    {
        _blob = blob;
    }

    public async Task Upload(IFormFile file)
    {
        //var fileId = Guid.NewGuid();
        //var blobName = fileId.ToString("N") + Path.GetExtension(file.FileName);

        //// 1️⃣ 存文件
        //await using (var stream = file.OpenReadStream())
        //{
        //    await _blob.SaveAsync(blobName, stream);
        //}

        //// 2️⃣ 写任务
        //var task = new TelemetryTask
        //{
        //    FileId = fileId,
        //    BlobName = blobName,
        //    Status = 0,
        //    CreatedAt = DateTime.UtcNow
        //};

        //await _db.TelemetryTasks.AddAsync(task);
        //await _db.SaveChangesAsync();

        //return fileId;
    }
}
