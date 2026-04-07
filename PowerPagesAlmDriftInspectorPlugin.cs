using System.ComponentModel.Composition;
using PowerPagesAlmDriftInspector.Controls;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace PowerPagesAlmDriftInspector
{
    [Export(typeof(IXrmToolBoxPlugin))]
    [ExportMetadata("Name", "Power Pages ALM Drift Inspector")]
    [ExportMetadata("Description", "Compare Power Pages site settings across Dataverse environments to identify ALM drift.")]
    [ExportMetadata("SmallImageBase64", "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAABaElEQVR4nGNgGGDAiMxRqrr2n14W32vTYmRgYGBgQrf8bqvmXWw0tQHMPkZslmMDytXXlWnhECZkDswSXDQtACMDAwODQsyq/0xyOrSyAyf49+gKIgTO1WrS1XKYfSzYBOkJmAgroQ44VM+IVZwFXSCi9xzDimIjhojecwy2msJ4Dc32kifa8tlHGBjssMjRLQRgDkEHGCGwotgIhaaGpb5htgyzjxzGKk9xFPBvMmH46HeGKMf4htkybK5nZLBrRJT4NI0CmO/xAZpHweZVhzHYh5BCgaIoqPoTzMDAwMDAv8kERRwWJchBjS0HMDDQORdgA5RFwSYaOICkXOB3BiMX2C1kYDhEiQPIAXYLMfmH4sl0ALVzASFAcSLUf09cIYQLUBwF6EFNSvAzMAyCbEh1B5Die5o4gFQw6oDB4YAHS8IYDVJX09Vig9TVDA+WhDEyoQvSy3IYwGglKsSsonkP+cGSMLi9AA3+hLXqfUG7AAAAAElFTkSuQmCC")]
    [ExportMetadata("BigImageBase64", "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAACWklEQVR4nO2bu0oDQRSG/5X04hMoptJGCbbJAwhJZ7AQrKwEGztFgojpbAQrq4BFiNhE8AFiq6IgpoqoLxB8AbUIo5O9zM4OOzkz43zNJpPdzDn/ucxkwwIez78mEH04v/fyPSlDdPPaXIz1NXbQJcfDhIWYCp/gsvNA1L+IAP+NMQFE0R8cLwxUx0yD91MqA5hTvHOyY6YjJUBxv1/kj1nGTGesI7reAHnYajCWAV8fzzTWTBjeT78KhAdcz4Kwf4W4kx4OFgAApaO+fosmBPNpeUtCgPBFLuN7ALUB1HgBqA3Ii15DeGsjEWET5Fk/eYgdb++WcHbzrjQ5AGyvzipfy+g1AlTrZVw3AlQOs21mnckAVawXgEW/tnOLar2cuRSkS6C9W0r8LI80psLqDGDR58maBcYIMN1dyXR+nPMqGCNAnmTJAiOWQR3Rl10WncmA7qlaOVgnQFL0azu3kTGZUiBfBrOmf95ICzAJ0sS4vr8fHTvRaIvGe4JeYJQAaYgaWkXxO0l7AHX6A8TL4J7SVfli3SqQN6QCNAtXlNMDMGAZRPfv5WftLvaUSgvobapPIYK8BJKcDlNp6ZmfXABqjNoHpEVZRykY8WtwaXiHpxmaPYF1JZB3LzBGgKWhXDMkKwFXb4oa0QTTosrSXsdewJgSoMIaAZzdCVJjhQC6og9YIoBOvADUBlDjBaA2gBovAP/m7aIeAMDy1iWNNZphfjE/AZ8BUQFczYK46AOC5wbnNjq//0M9nq9pM0w3fCDDzgMpD07yIthOnPNAigAMm4VIctzjGfED8yHNczaWVVYAAAAASUVORK5CYII=")]
    [ExportMetadata("BackgroundColor", "White")]
    [ExportMetadata("PrimaryFontColor", "Black")]
    [ExportMetadata("SecondaryFontColor", "DimGray")]
    public class PowerPagesAlmDriftInspectorPlugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new PowerPagesAlmDriftInspectorControl();
        }
    }
}
