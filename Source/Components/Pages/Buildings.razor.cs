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
    public partial class Buildings
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

        protected IEnumerable<SignalF.Measurement.Viewer.Models.SignalFDb.Building> buildings;

        protected RadzenDataGrid<SignalF.Measurement.Viewer.Models.SignalFDb.Building> grid0;

        [Inject]
        protected SecurityService Security { get; set; }
        protected override async Task OnInitializedAsync()
        {
            buildings = await SignalFDbService.GetBuildings();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddBuilding>("Add Building", null);
            await grid0.Reload();
        }

        protected async Task EditRow(SignalF.Measurement.Viewer.Models.SignalFDb.Building args)
        {
            await DialogService.OpenAsync<EditBuilding>("Edit Building", new Dictionary<string, object> { {"Id", args.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, SignalF.Measurement.Viewer.Models.SignalFDb.Building building)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await SignalFDbService.DeleteBuilding(building.Id);

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
                    Detail = $"Unable to delete Building"
                });
            }
        }
    }
}