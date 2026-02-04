using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace KannadaNudiWeb.Services
{
    public class SpeechService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<SpeechService>? _objRef;

        public event Action<string>? OnResult;
        public event Action<string>? OnError;
        public event Action? OnStarted;
        public event Action? OnEnded;

        public SpeechService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync(string triggerId, string langSelectId)
        {
            _objRef = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync("speechInterop.init", _objRef, triggerId, langSelectId);
        }

        public async Task StartAsync(string languageCode = "kn-IN")
        {
            // Kept for backward compatibility or direct invocation if needed
            if (_objRef == null)
            {
                _objRef = DotNetObjectReference.Create(this);
            }
            await _jsRuntime.InvokeVoidAsync("speechInterop.start", _objRef, languageCode);
        }

        public async Task StopAsync()
        {
            await _jsRuntime.InvokeVoidAsync("speechInterop.stop");
        }

        [JSInvokable]
        public void OnSpeechResult(string text)
        {
            OnResult?.Invoke(text);
        }

        [JSInvokable]
        public void OnSpeechError(string error)
        {
            OnError?.Invoke(error);
        }

        [JSInvokable]
        public void OnSpeechStarted()
        {
            OnStarted?.Invoke();
        }

        [JSInvokable]
        public void OnSpeechEnded()
        {
            OnEnded?.Invoke();
        }

        public async ValueTask DisposeAsync()
        {
            if (_objRef != null)
            {
                await StopAsync();
                _objRef.Dispose();
            }
        }
    }
}
