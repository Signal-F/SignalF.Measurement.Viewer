using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace SignalF.Measurement.Viewer.Components.Pages
{
    public partial class Measurements
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public SignalFDbService SignalFDbService { get; set; }

        protected IEnumerable<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> measurements;

        protected RadzenDataGrid<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> grid0;
        protected override async Task OnInitializedAsync()
        {
            measurements = await SignalFDbService.GetMeasurements(new Query { Expand = "Device" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddMeasurement>("Add Measurement", null);
            await grid0.Reload();
        }

        protected async Task EditRow(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement args)
        {
            await DialogService.OpenAsync<EditMeasurement>("Edit Measurement", new Dictionary<string, object> { {"Id", args.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, SignalF.Measurement.Viewer.Models.SignalFDb.Measurement measurement)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await SignalFDbService.DeleteMeasurement(measurement.Id);

                    if (deleteResult != null)
                    {
                        await grid0.Reload();
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Error",
                    Detail = $"Unable to delete Measurement"
                });
            }
        }
    }
}