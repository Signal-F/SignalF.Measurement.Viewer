using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

using SignalF.Measurement.Viewer.Data;

namespace SignalF.Measurement.Viewer.Controllers
{
    public partial class ExportSignalFDbController : ExportController
    {
        private readonly SignalFDbContext context;
        private readonly SignalFDbService service;

        public ExportSignalFDbController(SignalFDbContext context, SignalFDbService service)
        {
            this.service = service;
            this.context = context;
        }

        [HttpGet("/export/SignalFDb/buildings/csv")]
        [HttpGet("/export/SignalFDb/buildings/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportBuildingsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetBuildings(), Request.Query, false), fileName);
        }

        [HttpGet("/export/SignalFDb/buildings/excel")]
        [HttpGet("/export/SignalFDb/buildings/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportBuildingsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetBuildings(), Request.Query, false), fileName);
        }

        [HttpGet("/export/SignalFDb/devices/csv")]
        [HttpGet("/export/SignalFDb/devices/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDevicesToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetDevices(), Request.Query, false), fileName);
        }

        [HttpGet("/export/SignalFDb/devices/excel")]
        [HttpGet("/export/SignalFDb/devices/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDevicesToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetDevices(), Request.Query, false), fileName);
        }

        [HttpGet("/export/SignalFDb/measurements/csv")]
        [HttpGet("/export/SignalFDb/measurements/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMeasurementsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetMeasurements(), Request.Query, false), fileName);
        }

        [HttpGet("/export/SignalFDb/measurements/excel")]
        [HttpGet("/export/SignalFDb/measurements/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportMeasurementsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetMeasurements(), Request.Query, false), fileName);
        }

        [HttpGet("/export/SignalFDb/rooms/csv")]
        [HttpGet("/export/SignalFDb/rooms/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportRoomsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetRooms(), Request.Query, false), fileName);
        }

        [HttpGet("/export/SignalFDb/rooms/excel")]
        [HttpGet("/export/SignalFDb/rooms/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportRoomsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetRooms(), Request.Query, false), fileName);
        }
    }
}
