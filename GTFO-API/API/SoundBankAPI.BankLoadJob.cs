using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GTFO.API;

internal sealed class SoundBanksLoadJob : LoadingJob
{
    public override string JobName => "SoundBanks";
    public override string DisplayName => "Loading SoundBanks";

    public ICollection<string> BankPathsToLoad = [];

    protected override IEnumerator Job()
    {
        var tasks = new List<(string path, Task<byte[]> task)>();
        foreach (var path in BankPathsToLoad)
        {
            tasks.Add((path, File.ReadAllBytesAsync(path)));
        }

        yield return WaitUntil(() => tasks.All(task => task.task.IsCompleted));

        foreach (var (path, task) in tasks)
        {
            var file = path;
            var buffer = task.Result;

            unsafe
            {
                uint length = (uint)buffer.Length;
                void* nativeBank = NativeMemory.AlignedAlloc(length, 0x10);
                Unsafe.CopyBlock(ref Unsafe.AsRef<byte>(nativeBank), ref buffer[0], length);

                AKRESULT loadResult = AkSoundEngine.LoadBank((nint)nativeBank, length, out uint bankId);
                if (loadResult == AKRESULT.AK_Success)
                {
                    APILogger.Info(nameof(SoundBankAPI), $"Loaded sound bank '{file}' (bankId: {bankId:X2})");
                }
                else
                {
                    APILogger.Error(nameof(SoundBankAPI), $"Error while loading sound bank '{file}' ({loadResult})");
                    NativeMemory.AlignedFree(nativeBank);
                }
            }
            yield return null;
        }
    }
}
