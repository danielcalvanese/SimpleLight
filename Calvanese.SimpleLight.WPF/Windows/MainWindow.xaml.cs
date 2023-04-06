using Calvanese.Platform.Forms.Adapters;
using Calvanese.Platform.WPF;
using Calvanese.Platform.WPF.Extensions;
using Calvanese.Platform.WPF.Loaders;
using Calvanese.Platform.WPF.Tools;
using Calvanese.Windows.API.Configurations;
using Calvanese.Windows.API.Contexts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Calvanese.SimpleLight.WPF.Windows;

public partial class MainWindow : Window {
    #region Constructors

    public MainWindow() {
        // WPF.
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        // Hide the window.  We needed to show it for a moment so the global hot keys can be registered.
        Visibility = Visibility.Hidden;

        // Setup global hot keys.  Will generate startup exceptions if the keys are mapped in other programs.
        SetupBrightnessGlobalHotKeys();
        SetupTemperatureGlobalHotKeys();
        SetupResetGlobalHotKeys();

        // Stop the application if there are startup exceptions.
        if(_startupExceptions.Count > 0) {
            // Create the aggregate of exceptions.
            AggregateException aggregateExceptions = new AggregateException(_startupExceptions);

            // Notify failure.
            MessageBox.Show(aggregateExceptions.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Close the application.
            this.Close();
        }

        // Setup system tray.
        SetupSystemTray();

        // Start with the gamma set to its default.
        SetDeviceGammaRamp(ColorConfiguration.Default);
    }

    private void Shutdown() {
        // Remove the notification icon.
        _notifyIcon.Visible = false;

        try {
            // Reset gamma to default.
            SetDeviceGammaRamp(ColorConfiguration.Default);

            // Shutdown the global hot key adapter.
            foreach (GlobalHotKey globalHotKey in _globalHotKeys) globalHotKey.Dispose();
            _globalHotKeys.Clear();
        }
        catch (Exception) {
            // Nothing.
        }
        finally {
            // Close the window.
            this.Close(); 
        }
    }

    #endregion

    #region Data (Configurations)

    // Start at zero so we can set it to its max default at startup.
    private ColorConfiguration _lastConfiguration = new ColorConfiguration(0, 0.0);

    #endregion

    #region Data (Devices)

    private IReadOnlyList<DeviceContext> _deviceContexts = Array.Empty<DeviceContext>();

    #endregion

    #region Data (Flags)

    private bool _isUpdatingGamma = false;
    private bool _hasValidDeviceContexts = false;

    #endregion

    #region Data (Global Hot Keys)

    private List<GlobalHotKey> _globalHotKeys = new List<GlobalHotKey>();

    #endregion

    #region Data (Registry)

    private static readonly string RegistryKeyTitle = "SimpleLight";

    #endregion

    #region Data (Standards)

    private static readonly double _errorMargin = 0.0001;

    private static readonly double _standardBrightnessDifferential = 0.2;
    private static readonly double _shortenedBrightnessDifferential = 0.1;
    
    private static readonly double _standardTemperatureDifferential = 1625.0;
    private static readonly double _shortenedTemperatureDifferential = 406.25;

    // Derived from the above.  Do not move.
    private static readonly double _brightnessCutoff = ColorConfiguration.Maximum.Brightness - 2.0 * _standardBrightnessDifferential;
    private static readonly double _temperatureCutoff = ColorConfiguration.Maximum.Temperature - 2.0 * _standardTemperatureDifferential;

    #endregion

    #region Data (Startup Exceptions)

    private List<Exception> _startupExceptions = new List<Exception>();

    #endregion

    #region Data (System Tray)

    private System.Windows.Forms.NotifyIcon _notifyIcon = new System.Windows.Forms.NotifyIcon();
    private System.Windows.Forms.ToolStripMenuItem _toggleRunOnStartupToolStripItem = new System.Windows.Forms.ToolStripMenuItem();

    #endregion

    #region Methods (Colors)

    public void SetDeviceGammaRamp(ColorConfiguration configuration) {
        // Validate the device context when it is invalid.
        if (!_hasValidDeviceContexts) RefreshDeviceContexts();
        
        // Confirm update start.
        _isUpdatingGamma = true;

        // Set device gama ramp for each device context.
        foreach (var deviceContext in _deviceContexts) {
            deviceContext.SetDeviceGammaRamp(
                GetRed(configuration) * configuration.Brightness,
                GetGreen(configuration) * configuration.Brightness,
                GetBlue(configuration) * configuration.Brightness
            );
        }

        // Confirm update end.
        _isUpdatingGamma = false;

        // Save configuration.
        _lastConfiguration = configuration;
    }

    #endregion

    #region Support (Colors)

    // From http://tannerhelland.com/4435/convert-temperature-rgb-algorithm-code

    private static double GetRed(ColorConfiguration configuration) {
        // Return red.
        if (configuration.Temperature > 6600) {
            return Math.Clamp(
                Math.Pow(configuration.Temperature / 100 - 60, -0.1332047592) * 329.698727446 / 255,
                0, 1
            );
        }

        // Return max red for the lowest eye pain.
        return 1;
    }

    private static double GetGreen(ColorConfiguration configuration) {
        // Return green.
        if (configuration.Temperature > 6600) {
            return Math.Clamp(
                Math.Pow(configuration.Temperature / 100 - 60, -0.0755148492) * 288.1221695283 / 255,
                0, 1
            );
        }

        // Return green.
        return Math.Clamp(
            (Math.Log(configuration.Temperature / 100) * 99.4708025861 - 161.1195681661) / 255,
            0, 1
        );
    }

    private static double GetBlue(ColorConfiguration configuration) {
        // Return lowest blue for the lowest eye pain.  
        if (configuration.Temperature >= 6600)
            return 1;

        // Return zero blue when super low.
        if (configuration.Temperature <= 1900)
            return 0;

        // Return blue.
        return Math.Clamp(
            (Math.Log(configuration.Temperature / 100 - 10) * 138.5177312231 - 305.0447927307) / 255,
            0, 1
        );
    }

    #endregion

    #region Support (Devices)

    private void InvalidateDeviceContexts() {
        // Invalidate the device context handle.
        _hasValidDeviceContexts = false;
    }

    private void RefreshDeviceContexts() {
        // Validate the device context handle.
        _hasValidDeviceContexts = true;

        // Refresh the device contexts.
        _deviceContexts.DisposeAll();
        _deviceContexts = ScreenAdapter.GetDeviceContexts();
    }

    #endregion

    #region Support (Differentials)

    private double GetBrightnessDifferential(System.Windows.Forms.ArrowDirection arrowDirection) {
        // Return brightness differential.
        if(arrowDirection == System.Windows.Forms.ArrowDirection.Up) {
            if(_lastConfiguration.Brightness + _errorMargin >= _brightnessCutoff) return +_standardBrightnessDifferential;
            else return +_shortenedBrightnessDifferential;
        }
        else {
            if(_lastConfiguration.Brightness - _errorMargin <= _brightnessCutoff) return -_shortenedBrightnessDifferential;
            else return -_standardBrightnessDifferential;
        }
    }

    private double GetTemperatureDifferential(System.Windows.Forms.ArrowDirection arrowDirection) {
        // Return temperature differential.
        if(arrowDirection == System.Windows.Forms.ArrowDirection.Up) {
            if(_lastConfiguration.Temperature + _errorMargin >= _temperatureCutoff) return +_standardTemperatureDifferential;
            else return +_shortenedTemperatureDifferential;
        }
        else {
            if(_lastConfiguration.Temperature - _errorMargin <= _temperatureCutoff) return -_shortenedTemperatureDifferential;
            else return -_standardTemperatureDifferential;
        }
    }

    #endregion

    #region Support (Global Hot Keys)

    private void SetupResetGlobalHotKeys() {
        try {
            // Create brightness global hot keys.
            GlobalHotKey resetGlobalHotKey = new GlobalHotKey(ModifierKeys.Alt, Key.End, this,
                (hotkey) => {
                    // Invalidate device contexts.
                    InvalidateDeviceContexts();

                    // Create next configuration.
                    ColorConfiguration adjustedConfiguration = _lastConfiguration;

                    // Adjust the configuration.
                    SetDeviceGammaRamp(adjustedConfiguration);
                }
            );

            GlobalHotKey resetGlobalHotKeyAlternate = new GlobalHotKey(ModifierKeys.Shift | ModifierKeys.Alt, Key.End, this,
                (hotkey) => {
                    // Invalidate device contexts.
                    InvalidateDeviceContexts();

                    // Create next configuration.
                    ColorConfiguration adjustedConfiguration = _lastConfiguration;

                    // Adjust the configuration.
                    SetDeviceGammaRamp(adjustedConfiguration);
                }
            );
        
            // Add reset global hot keys.
            _globalHotKeys.Add(resetGlobalHotKey);
            _globalHotKeys.Add(resetGlobalHotKeyAlternate);
        }
        catch (Exception) {
            // Store.
            _startupExceptions.Add(new Exception("The reset global hot keys failed to initialize.  They may be mapped by an alternate application."));
        }
    }

    private void SetupBrightnessGlobalHotKeys() {
        try {
            // Create brightness global hot keys.
            GlobalHotKey brightnessDownHotKey = new GlobalHotKey(ModifierKeys.Alt, Key.PageDown, this,
                (hotkey) => {
                    // Invalidate device contexts.
                    InvalidateDeviceContexts();

                    // Create next configuration.
                    ColorConfiguration adjustedConfiguration = _lastConfiguration.CreateAdjustedConfiguration(0.0, GetBrightnessDifferential(System.Windows.Forms.ArrowDirection.Down));

                    // Adjust the configuration.
                    SetDeviceGammaRamp(adjustedConfiguration);
                }
            );
            GlobalHotKey brightnessUpHotKey = new GlobalHotKey(ModifierKeys.Alt, Key.PageUp, this,
                (hotkey) => {
                    // Invalidate device contexts.
                    InvalidateDeviceContexts();

                    // Create next configuration.@
                    ColorConfiguration adjustedConfiguration = _lastConfiguration.CreateAdjustedConfiguration(0.0, GetBrightnessDifferential(System.Windows.Forms.ArrowDirection.Up));

                    // Adjust the configuration.
                    SetDeviceGammaRamp(adjustedConfiguration);
                }
            );
        
            // Add brightness global hot keys.
            _globalHotKeys.Add(brightnessDownHotKey);
            _globalHotKeys.Add(brightnessUpHotKey);
        }
        catch (Exception) {
            // Notify.
            _startupExceptions.Add(new Exception("The brightness global hot keys failed to initialize.  They may be mapped by an alternate application."));
        }
    }

    private void SetupTemperatureGlobalHotKeys() {
        try {
            // Create color global hot keys.
            GlobalHotKey colorDownHotKey = new GlobalHotKey((ModifierKeys.Shift | ModifierKeys.Alt), Key.PageDown, this,
                (hotkey) => {
                    // Invalidate device contexts.
                    InvalidateDeviceContexts();

                    // Create next configuration.
                    ColorConfiguration adjustedConfiguration = _lastConfiguration.CreateAdjustedConfiguration(GetTemperatureDifferential(System.Windows.Forms.ArrowDirection.Down), 0.0);

                    // Adjust the configuration.
                    SetDeviceGammaRamp(adjustedConfiguration);
                }
            );
            GlobalHotKey colorUpHotKey = new GlobalHotKey((ModifierKeys.Shift | ModifierKeys.Alt), Key.PageUp, this,
                (hotkey) => {
                    // Invalidate device contexts.
                    InvalidateDeviceContexts();

                    // Create next configuration.
                    ColorConfiguration adjustedConfiguration = _lastConfiguration.CreateAdjustedConfiguration(GetTemperatureDifferential(System.Windows.Forms.ArrowDirection.Up), 0.0);

                    // Adjust the configuration.
                    SetDeviceGammaRamp(adjustedConfiguration);
                }
            );

            // Add color global hot keys.
            _globalHotKeys.Add(colorDownHotKey);
            _globalHotKeys.Add(colorUpHotKey);
        }
        catch (Exception) {
            // Notify.
            _startupExceptions.Add(new Exception("The temperature global hot keys failed to initialize.  They may be mapped by an alternate application."));
        }
    }

    #endregion

    #region Support (Registry)

    private void ToggleRunOnStartup(object? sender, EventArgs eventArgs) {
        // Check.
        bool isInStartup = StartupTools.IsInStartup(RegistryKeyTitle, System.Windows.Forms.Application.ExecutablePath);

        // Toggle is in startup.
        if(isInStartup) StartupTools.RemoveFromStartup(RegistryKeyTitle, System.Windows.Forms.Application.ExecutablePath);
        else StartupTools.AddToStartup(RegistryKeyTitle, System.Windows.Forms.Application.ExecutablePath);

        // Confirm toggle.
        if(_toggleRunOnStartupToolStripItem is not null)
            _toggleRunOnStartupToolStripItem.Checked = StartupTools.IsInStartup(RegistryKeyTitle, System.Windows.Forms.Application.ExecutablePath);
    }

    #endregion

    #region Support (System Tray)

    private void SetupSystemTray() {
        // Setup the notification icon
        _notifyIcon.Icon = ResourceLoader.LoadIcon(Assembly.GetExecutingAssembly(), "Calvanese.SimpleLight.WPF.Resources.Icons.SimpleLight.ico", new System.Drawing.Size(16, 16));
        _notifyIcon.Text = "SimpleLight";
        _notifyIcon.Visible = true;

        // Setup the context menu.
        _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();

        // Add toggles.
        _toggleRunOnStartupToolStripItem.Text = "Toggle Run on Startup";
        _toggleRunOnStartupToolStripItem.Checked = StartupTools.IsInStartup(RegistryKeyTitle, System.Windows.Forms.Application.ExecutablePath);
        _toggleRunOnStartupToolStripItem.Click += ToggleRunOnStartup;
        _notifyIcon.ContextMenuStrip.Items.Add(_toggleRunOnStartupToolStripItem);

        // Add separator.
        _notifyIcon.ContextMenuStrip.Items.Add("-");

        // Add instructions
        _notifyIcon.ContextMenuStrip.Items.Add("Alt + Page Up-Down for Brightness");
        _notifyIcon.ContextMenuStrip.Items.Add("Alt + Shift + Page Up-Down for Temperature");
        _notifyIcon.ContextMenuStrip.Items.Add("Alt + End for Reapplication");

        // Add separator.
        _notifyIcon.ContextMenuStrip.Items.Add("-");

        // Add exit.
        _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (o, e) => {
            // Shutdown.
            Shutdown();
        });

    }

    #endregion
}
