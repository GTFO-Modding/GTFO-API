using System;
using System.Collections;
using GTFO.API.EventArg;
using GTFO.API.Utilities;
using UnityEngine;

namespace GTFO.API.Assets.ObjectLoaders;

internal abstract class ObjectAsyncLoader<TObject, TAsyncOperation>
    where TObject : UnityEngine.Object
    where TAsyncOperation : AsyncOperation
{
    public TObject LoadedObject
    {
        get => _LoadedObject;
        set
        {
            if (_LoadedObject == value)
                return;

            _LoadedObject = value;
            LoadedObjectChanged?.Invoke(_LoadedObject);
        }
    }

    public event Action<TObject> LoadedObjectChanged;

    private TObject _LoadedObject = null;
    private TAsyncOperation _WorkingOperation = null;
    private bool _IsAsyncLoading = false;
    private event UnityObjectRequestCallback _OnLoaded;
    private long _AsyncLoadStartTick = 0;

    public void Load(Func<TObject> blockingCreateFunc)
    {
        if (LoadedObject != null)
        {
            return;
        }

        if (_IsAsyncLoading)
        {
            LoadedObject = GetResult(_WorkingOperation);
        }
        else
        {
            LoadedObject = blockingCreateFunc.Invoke();
        }
        Finish();
    }

    public void LoadAsync(Func<TAsyncOperation> createRequest, UnityObjectRequestCallback onLoaded)
    {
        if (LoadedObject != null)
        {
            onLoaded?.Invoke(new UnityObjectRequestEventArgs()
            {
                LoadedObject = LoadedObject,
                Success = true,
                Elapsed = TimeSpan.Zero
            });
            return;
        }

        _OnLoaded += onLoaded;

        if (_IsAsyncLoading)
            return;

        _IsAsyncLoading = true;
        _WorkingOperation = createRequest.Invoke();
        _AsyncLoadStartTick = StopwatchNoalloc.GetTimestamp();
        CoroutineDispatcher.StartCoroutine(DoLoad());
    }

    public void Unload()
    {
        if (_IsAsyncLoading)
        {
            LoadedObject = GetResult(_WorkingOperation);
        }

        if (LoadedObject != null)
        {
            DoUnload(LoadedObject);
            LoadedObject = null;
        }

        Finish();
    }

    protected abstract void DoUnload(TObject unloadingObject);

    protected abstract TObject GetResult(TAsyncOperation operation);

    private IEnumerator DoLoad()
    {
        var waitFor = _WorkingOperation;
        yield return waitFor;

        if (waitFor != _WorkingOperation)
            yield break;

        LoadedObject = GetResult(waitFor);
        Finish();
    }

    private void Finish()
    {
        _IsAsyncLoading = false;
        _WorkingOperation = null;

        var eventArgs = new UnityObjectRequestEventArgs()
        {
            LoadedObject = LoadedObject,
            Success = LoadedObject != null,
            Elapsed = StopwatchNoalloc.GetElapsedFrom(_AsyncLoadStartTick)
        };

        SafeInvoke.InvokeDelegate<UnityObjectRequestCallback>(_OnLoaded, (del) =>
        {
            del(eventArgs);
        });
        _OnLoaded = null;
    }
}
