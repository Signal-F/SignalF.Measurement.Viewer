using System;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Radzen;

using SignalF.Measurement.Viewer.Data;

namespace SignalF.Measurement.Viewer
{
    public partial class SignalFDbService
    {
        SignalFDbContext Context
        {
           get
           {
             return this.context;
           }
        }

        private readonly SignalFDbContext context;
        private readonly NavigationManager navigationManager;

        public SignalFDbService(SignalFDbContext context, NavigationManager navigationManager)
        {
            this.context = context;
            this.navigationManager = navigationManager;
        }

        public void Reset() => Context.ChangeTracker.Entries().Where(e => e.Entity != null).ToList().ForEach(e => e.State = EntityState.Detached);

        public void ApplyQuery<T>(ref IQueryable<T> items, Query query = null)
        {
            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Filter))
                {
                    if (query.FilterParameters != null)
                    {
                        items = items.Where(query.Filter, query.FilterParameters);
                    }
                    else
                    {
                        items = items.Where(query.Filter);
                    }
                }

                if (!string.IsNullOrEmpty(query.OrderBy))
                {
                    items = items.OrderBy(query.OrderBy);
                }

                if (query.Skip.HasValue)
                {
                    items = items.Skip(query.Skip.Value);
                }

                if (query.Top.HasValue)
                {
                    items = items.Take(query.Top.Value);
                }
            }
        }


        public async Task ExportBuildingsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/signalfdb/buildings/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/signalfdb/buildings/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportBuildingsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/signalfdb/buildings/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/signalfdb/buildings/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnBuildingsRead(ref IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Building> items);

        public async Task<IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Building>> GetBuildings(Query query = null)
        {
            var items = Context.Buildings.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnBuildingsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnBuildingGet(SignalF.Measurement.Viewer.Models.SignalFDb.Building item);
        partial void OnGetBuildingById(ref IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Building> items);


        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Building> GetBuildingById(Guid id)
        {
            var items = Context.Buildings
                              .AsNoTracking()
                              .Where(i => i.Id == id);

 
            OnGetBuildingById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnBuildingGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnBuildingCreated(SignalF.Measurement.Viewer.Models.SignalFDb.Building item);
        partial void OnAfterBuildingCreated(SignalF.Measurement.Viewer.Models.SignalFDb.Building item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Building> CreateBuilding(SignalF.Measurement.Viewer.Models.SignalFDb.Building building)
        {
            OnBuildingCreated(building);

            var existingItem = Context.Buildings
                              .Where(i => i.Id == building.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Buildings.Add(building);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(building).State = EntityState.Detached;
                throw;
            }

            OnAfterBuildingCreated(building);

            return building;
        }

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Building> CancelBuildingChanges(SignalF.Measurement.Viewer.Models.SignalFDb.Building item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnBuildingUpdated(SignalF.Measurement.Viewer.Models.SignalFDb.Building item);
        partial void OnAfterBuildingUpdated(SignalF.Measurement.Viewer.Models.SignalFDb.Building item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Building> UpdateBuilding(Guid id, SignalF.Measurement.Viewer.Models.SignalFDb.Building building)
        {
            OnBuildingUpdated(building);

            var itemToUpdate = Context.Buildings
                              .Where(i => i.Id == building.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(building);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterBuildingUpdated(building);

            return building;
        }

        partial void OnBuildingDeleted(SignalF.Measurement.Viewer.Models.SignalFDb.Building item);
        partial void OnAfterBuildingDeleted(SignalF.Measurement.Viewer.Models.SignalFDb.Building item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Building> DeleteBuilding(Guid id)
        {
            var itemToDelete = Context.Buildings
                              .Where(i => i.Id == id)
                              .Include(i => i.Rooms)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnBuildingDeleted(itemToDelete);


            Context.Buildings.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterBuildingDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportDevicesToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/signalfdb/devices/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/signalfdb/devices/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportDevicesToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/signalfdb/devices/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/signalfdb/devices/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnDevicesRead(ref IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Device> items);

        public async Task<IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Device>> GetDevices(Query query = null)
        {
            var items = Context.Devices.AsQueryable();

            items = items.Include(i => i.Room);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnDevicesRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnDeviceGet(SignalF.Measurement.Viewer.Models.SignalFDb.Device item);
        partial void OnGetDeviceById(ref IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Device> items);


        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Device> GetDeviceById(Guid id)
        {
            var items = Context.Devices
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Room);
 
            OnGetDeviceById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnDeviceGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnDeviceCreated(SignalF.Measurement.Viewer.Models.SignalFDb.Device item);
        partial void OnAfterDeviceCreated(SignalF.Measurement.Viewer.Models.SignalFDb.Device item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Device> CreateDevice(SignalF.Measurement.Viewer.Models.SignalFDb.Device device)
        {
            OnDeviceCreated(device);

            var existingItem = Context.Devices
                              .Where(i => i.Id == device.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Devices.Add(device);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(device).State = EntityState.Detached;
                throw;
            }

            OnAfterDeviceCreated(device);

            return device;
        }

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Device> CancelDeviceChanges(SignalF.Measurement.Viewer.Models.SignalFDb.Device item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnDeviceUpdated(SignalF.Measurement.Viewer.Models.SignalFDb.Device item);
        partial void OnAfterDeviceUpdated(SignalF.Measurement.Viewer.Models.SignalFDb.Device item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Device> UpdateDevice(Guid id, SignalF.Measurement.Viewer.Models.SignalFDb.Device device)
        {
            OnDeviceUpdated(device);

            var itemToUpdate = Context.Devices
                              .Where(i => i.Id == device.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(device);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterDeviceUpdated(device);

            return device;
        }

        partial void OnDeviceDeleted(SignalF.Measurement.Viewer.Models.SignalFDb.Device item);
        partial void OnAfterDeviceDeleted(SignalF.Measurement.Viewer.Models.SignalFDb.Device item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Device> DeleteDevice(Guid id)
        {
            var itemToDelete = Context.Devices
                              .Where(i => i.Id == id)
                              .Include(i => i.Measurements)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnDeviceDeleted(itemToDelete);


            Context.Devices.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterDeviceDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportMeasurementsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/signalfdb/measurements/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/signalfdb/measurements/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportMeasurementsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/signalfdb/measurements/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/signalfdb/measurements/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnMeasurementsRead(ref IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> items);

        public async Task<IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement>> GetMeasurements(Query query = null)
        {
            var items = Context.Measurements.AsQueryable();

            items = items.Include(i => i.Device);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnMeasurementsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnMeasurementGet(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement item);
        partial void OnGetMeasurementById(ref IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> items);


        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> GetMeasurementById(Guid id)
        {
            var items = Context.Measurements
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Device);
 
            OnGetMeasurementById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnMeasurementGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnMeasurementCreated(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement item);
        partial void OnAfterMeasurementCreated(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> CreateMeasurement(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement measurement)
        {
            OnMeasurementCreated(measurement);

            var existingItem = Context.Measurements
                              .Where(i => i.Id == measurement.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Measurements.Add(measurement);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(measurement).State = EntityState.Detached;
                throw;
            }

            OnAfterMeasurementCreated(measurement);

            return measurement;
        }

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> CancelMeasurementChanges(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnMeasurementUpdated(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement item);
        partial void OnAfterMeasurementUpdated(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> UpdateMeasurement(Guid id, SignalF.Measurement.Viewer.Models.SignalFDb.Measurement measurement)
        {
            OnMeasurementUpdated(measurement);

            var itemToUpdate = Context.Measurements
                              .Where(i => i.Id == measurement.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(measurement);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterMeasurementUpdated(measurement);

            return measurement;
        }

        partial void OnMeasurementDeleted(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement item);
        partial void OnAfterMeasurementDeleted(SignalF.Measurement.Viewer.Models.SignalFDb.Measurement item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> DeleteMeasurement(Guid id)
        {
            var itemToDelete = Context.Measurements
                              .Where(i => i.Id == id)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnMeasurementDeleted(itemToDelete);


            Context.Measurements.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterMeasurementDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportRoomsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/signalfdb/rooms/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/signalfdb/rooms/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportRoomsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/signalfdb/rooms/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/signalfdb/rooms/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnRoomsRead(ref IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Room> items);

        public async Task<IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Room>> GetRooms(Query query = null)
        {
            var items = Context.Rooms.AsQueryable();

            items = items.Include(i => i.Building);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnRoomsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnRoomGet(SignalF.Measurement.Viewer.Models.SignalFDb.Room item);
        partial void OnGetRoomById(ref IQueryable<SignalF.Measurement.Viewer.Models.SignalFDb.Room> items);


        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Room> GetRoomById(Guid id)
        {
            var items = Context.Rooms
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Building);
 
            OnGetRoomById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnRoomGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnRoomCreated(SignalF.Measurement.Viewer.Models.SignalFDb.Room item);
        partial void OnAfterRoomCreated(SignalF.Measurement.Viewer.Models.SignalFDb.Room item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Room> CreateRoom(SignalF.Measurement.Viewer.Models.SignalFDb.Room room)
        {
            OnRoomCreated(room);

            var existingItem = Context.Rooms
                              .Where(i => i.Id == room.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Rooms.Add(room);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(room).State = EntityState.Detached;
                throw;
            }

            OnAfterRoomCreated(room);

            return room;
        }

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Room> CancelRoomChanges(SignalF.Measurement.Viewer.Models.SignalFDb.Room item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnRoomUpdated(SignalF.Measurement.Viewer.Models.SignalFDb.Room item);
        partial void OnAfterRoomUpdated(SignalF.Measurement.Viewer.Models.SignalFDb.Room item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Room> UpdateRoom(Guid id, SignalF.Measurement.Viewer.Models.SignalFDb.Room room)
        {
            OnRoomUpdated(room);

            var itemToUpdate = Context.Rooms
                              .Where(i => i.Id == room.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(room);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterRoomUpdated(room);

            return room;
        }

        partial void OnRoomDeleted(SignalF.Measurement.Viewer.Models.SignalFDb.Room item);
        partial void OnAfterRoomDeleted(SignalF.Measurement.Viewer.Models.SignalFDb.Room item);

        public async Task<SignalF.Measurement.Viewer.Models.SignalFDb.Room> DeleteRoom(Guid id)
        {
            var itemToDelete = Context.Rooms
                              .Where(i => i.Id == id)
                              .Include(i => i.Devices)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnRoomDeleted(itemToDelete);


            Context.Rooms.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterRoomDeleted(itemToDelete);

            return itemToDelete;
        }
        }
}