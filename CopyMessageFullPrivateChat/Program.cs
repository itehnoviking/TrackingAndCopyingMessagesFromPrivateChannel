using TrackingAndCopyingMessagesFromPrivateChannel;
using System;
using TL;

static class Program
{
    static async Task Main(string[] _)
    {
        TgClient client = new TgClient();
        var task = Task.Run(() => client.CopyMashine());

        task.Wait();
    }
}

