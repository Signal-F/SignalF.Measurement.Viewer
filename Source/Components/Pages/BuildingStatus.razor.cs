using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SignalF.Measurement.Viewer.Data;
using SignalF.Measurement.Viewer.Models.SignalFDb;

namespace SignalF.Measurement.Viewer.Components.Pages
{
    public partial class BuildingStatus
    {
        [Inject] protected SignalFDbContext DbContext { get; set; }

        [Parameter] public string BuildingId { get; set; }

        private Building Building { get; set; }

        [Inject]
        protected SecurityService Security { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Building = await DbContext.Buildings
                .Where(entity => entity.Id == new Guid(BuildingId))
                .Include(entity => entity.Rooms)
                .ThenInclude(entity => entity.Devices)
                .FirstAsync();
        }

        private Device GetDevice(Room room, string type)
        {
            if (room.Devices == null)
            {
                return null;
            }

            return room.Devices.FirstOrDefault(device => device.Name.StartsWith(type));
        }

        private string GetState(Device device)
        {
            string state = "rz-color-danger";
            if(device == null)
            {
                return state;
            }
                            
            switch(device.State)
            {
                case 0:
                    state = "rz-color-success";
                    break;
                case 1:
                    state = "rz-color-warning";
                    break;
                case 2:
                    state = "rz-color-danger";
                    break;

            }

            return state;
        }
    }
}
