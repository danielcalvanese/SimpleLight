using System;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows;
using Calvanese.Platform.WPF.NativeAdapters;
using Calvanese.Windows.API.NativeAdapters;

namespace Calvanese.Platform.WPF;

public sealed class GlobalHotKey : IDisposable {
    #region Constructors

    public GlobalHotKey(ModifierKeys modifierKeys, Key key, IntPtr windowHandle, Action<GlobalHotKey> onKeyAction) {
        // Save data.
        Key = key;
        ModifierKeys = modifierKeys;
        _identifier = GetHashCode();
        _handle = windowHandle == IntPtr.Zero ? UserNativeAdapter.GetForegroundWindow() : windowHandle;
        _dispatcher = Dispatcher.CurrentDispatcher;

        // Register the hot key.
        RegisterHotKey();

        // Register the thread preprocess message method.  
        ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;

        // Register the on key action when the hot key is pressed.
        HotKeyPressed += onKeyAction;
    }

    public GlobalHotKey(ModifierKeys modifierKeys, Key key, Window window, Action<GlobalHotKey> onKeyAction)
            : this(modifierKeys, key, new WindowInteropHelper(window), onKeyAction) {}

    public GlobalHotKey(ModifierKeys modifierKeys, Key key, WindowInteropHelper window, Action<GlobalHotKey> onKeyAction)
            : this(modifierKeys, key, window.Handle, onKeyAction) {}

    ~GlobalHotKey() {
        // Dispose.
        Dispose();

        // Suppress.
        GC.SuppressFinalize(this);
    }

    public void Dispose() {
        try {
            // Unregister the thread preprocess message method.  
            ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
        }
        catch(Exception) {
            // Nothing.
        }
        finally {
            // Unregister.
            UnregisterHotKey();
        }
    }

    #endregion
    
    #region Data (Dispatcher)

    private Dispatcher _dispatcher;

    #endregion

    #region Data (Events)

    public event Action<GlobalHotKey> HotKeyPressed;

    #endregion

    #region Data (Flags)

    private bool _isKeyRegistered;

    #endregion

    #region Data (Handles)

    private readonly IntPtr _handle;

    #endregion

    #region Data (Identifiers)

    private readonly int _identifier;

    #endregion

    #region Data (Keys)

    public Key Key { get; private set; }
    public ModifierKeys ModifierKeys { get; private set; }
    private int _virtualKey => KeyInterop.VirtualKeyFromKey(Key);

    #endregion

    #region Support (Hot Keys)

    private void RegisterHotKey() {
        // Stop if the key is undefined.
        if(Key == Key.None) return;

        // Unregister the key if it's already registered.
        if(_isKeyRegistered) UnregisterHotKey();

        // Register the key.
        _isKeyRegistered = WPFUserNativeAdapter.RegisterHotKey(_handle, _identifier, ModifierKeys, _virtualKey);

        // Notify on failure.
        if(!_isKeyRegistered) throw new ApplicationException("Error:  Couldn't register a hot key.  It may be registered to another application.");
    }

    private void UnregisterHotKey() {
        // Unregister the hot key.
        _isKeyRegistered = !WPFUserNativeAdapter.UnregisterHotKey(_handle, _identifier);
    }

    #endregion

    #region Support (Callbacks)

    private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled) {
        // Stop if already handled.
        if(handled) return;

        // Stop if the message is not a hot key message or if the message doesn't match this hotkey's identifier.
        if(msg.message != WPFUserNativeAdapter.HotKeyWindowsMessageIdentifier || (int)(msg.wParam) != _identifier) return;

        // Invoke the hot key pressed callback.
        OnHotKeyPressed();

        // Signify handled.
        handled = true;
    }

    private void OnHotKeyPressed() {
        // Invoke the dispatcher to call the keypress event.
        _dispatcher.Invoke(
            delegate {
                // Invoke the hot key pressed event.
                HotKeyPressed?.Invoke(this);
            });
    }

    #endregion
}
