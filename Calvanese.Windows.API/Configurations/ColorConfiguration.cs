using System;
using System.Runtime.InteropServices;

namespace Calvanese.Windows.API.Configurations;

public struct ColorConfiguration {
    #region Constructors

    public ColorConfiguration(double temperature, double brightness) {
        Temperature = temperature;
        Brightness = brightness;
    }

    #endregion

    #region Data (Colors)

    public readonly double Temperature { get; } = 0.0;
    public readonly double Brightness { get; } = 0.0;

    #endregion

    #region Data (Defaults)

    public static ColorConfiguration Default { get; } = new(6500.0, 1.0);

    public static ColorConfiguration Minimum { get; } = new ColorConfiguration(0.0, 0.2);
    public static ColorConfiguration Maximum { get; } = new ColorConfiguration(6500.0, 1.0);
    

    #endregion

    #region Methods (Adjuster)

    public ColorConfiguration CreateAdjustedConfiguration(double temperatureAdjustment, double brightnessAdjustment) {
        // Resolve the adjusted values.
        double adjustedTemperature = Math.Round(Temperature + temperatureAdjustment, 5);
        double adjustedBrightness = Math.Round(Brightness + brightnessAdjustment, 5);

        // Restrict minimum settings.
        if(adjustedTemperature < Minimum.Temperature) adjustedTemperature = Minimum.Temperature;
        if(adjustedBrightness < Minimum.Brightness) adjustedBrightness = Minimum.Brightness;

        // Restrict maximum settings.
        if(adjustedTemperature > Maximum.Temperature) adjustedTemperature = Maximum.Temperature;
        if(adjustedBrightness > Maximum.Brightness) adjustedBrightness = Maximum.Brightness;

        // Create adjustment configuration.
        ColorConfiguration adjustedConfiguration = new ColorConfiguration(adjustedTemperature, adjustedBrightness);

        // Return.
        return adjustedConfiguration;
    }

    #endregion
}
