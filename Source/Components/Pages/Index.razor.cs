using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Radzen;
using SignalF.Measurement.Viewer.Data;
using SignalF.Measurement.Viewer.Models.SignalFDb;

namespace SignalF.Measurement.Viewer.Components.Pages;

public partial class Index
{
    [Inject] protected IJSRuntime JSRuntime { get; set; }

    [Inject] protected NavigationManager NavigationManager { get; set; }

    [Inject] protected DialogService DialogService { get; set; }

    [Inject] protected TooltipService TooltipService { get; set; }

    [Inject] protected ContextMenuService ContextMenuService { get; set; }

    [Inject] protected NotificationService NotificationService { get; set; }

    [Inject] protected SignalFDbContext DbContext { get; set; }

    protected IEnumerable<Building> Buildings { get; set; }

    [Inject]
    protected SecurityService Security { get; set; }


    protected override async Task OnInitializedAsync()
    {
        Buildings = await DbContext.Buildings.ToListAsync();
    }
}