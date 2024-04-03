namespace SignalF.Measurement.Viewer.Models.SignalFDb;

[Flags]
public enum DeviceState
{
    Disabled = 0,

    Measuring = 1,

    Warning = 2,

    Alarm = 4
}