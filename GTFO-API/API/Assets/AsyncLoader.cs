using System;
using System.Collections;
using GTFO.API.Utilities;
using UnityEngine;

namespace GTFO.API.Assets
{
    internal sealed class AsyncLoader<TResult> where TResult : UnityEngine.Object
    {
        public TResult LoadedObject
        {
            get => _LoadedObject;
            set
            {
                if (_LoadedObject != value)
                {
                    _LoadedObject = value;
                    OnLoadedObjectChanged?.Invoke();
                }
            }
        }

        public bool IsLoaded => LoadedObject != null;
        public bool IsLoading { get; private set; }
        public event Action OnLoaded;
        public event Action OnLoadedObjectChanged;

        private TResult _LoadedObject;
        private Coroutine _LoadingCoroutine;
        private Coroutine _TopLoadingCoroutine;

        public void Load(IEnumerator loadingCoroutine)
        {
            if (IsLoaded)
            {
                OnLoaded?.Invoke();
                OnLoaded = null;
            }

            if (IsLoading)
            {
                return;
            }

            _TopLoadingCoroutine = CoroutineDispatcher.StartCoroutine(DoLoad(loadingCoroutine));
            IsLoading = true;
        }

        public void SynchronouslyLoaded()
        {
            StopLoading();
            OnLoaded?.Invoke();
            OnLoaded = null;
        }

        public void Unload()
        {
            StopLoading();
            LoadedObject = null;
        }

        private void StopLoading()
        {
            if (_TopLoadingCoroutine != null)
            {
                CoroutineDispatcher.StopCoroutine(_TopLoadingCoroutine);
                _TopLoadingCoroutine = null;
            }

            if (_LoadingCoroutine != null)
            {
                CoroutineDispatcher.StopCoroutine(_LoadingCoroutine);
                _LoadingCoroutine = null;
            }

            IsLoading = false;
        }

        private IEnumerator DoLoad(IEnumerator loadingCoroutine)
        {
            _LoadingCoroutine = CoroutineDispatcher.StartCoroutine(loadingCoroutine);
            yield return _LoadingCoroutine;

            _TopLoadingCoroutine = null;
            _LoadingCoroutine = null;
            IsLoading = false;

            OnLoaded?.Invoke();
            OnLoaded = null;
        }
    }
}
