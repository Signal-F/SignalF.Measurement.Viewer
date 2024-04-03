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
                .FirstAsync();
        }

    }
}
