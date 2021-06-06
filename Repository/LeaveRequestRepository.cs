using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Create(LeaveRequest entity)
        {
            await _db.LeaveRequests.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            var leaveRequests = await _db.LeaveRequests
                .Include(q => q.RequestingEmployee) // join employee
                .Include(q => q.ApprovedBy) // join employee
                .Include(q => q.LeaveType) // join leave type
                .ToListAsync();
            return leaveRequests;
        }

        public async Task<LeaveRequest> FindById(int id)
        {
            var leaveRequest = await _db.LeaveRequests
                .Include(q => q.RequestingEmployee) // join employee
                .Include(q => q.ApprovedBy) // join employee
                .Include(q => q.LeaveType) // join leave type
                .FirstOrDefaultAsync(q => q.Id == id); //get first row or null record
            return leaveRequest;
        }

        public async Task<ICollection<LeaveRequest>> GetLeaveRequestByEmployee(string employeeId)
        {
            var leaveRequests = await FindAll();
            return leaveRequests.Where(q => q.RequestingEmployeeId == employeeId)
                .ToList();
        }

        public async Task<bool> isExists(int id)
        {
            var exists = await _db.LeaveRequests.AnyAsync(q => q.Id == id); // any() check if empty record is empty or not
            return exists;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0; //bcs anything update/delete/add will have one or more record, if less than one then error on the operation
        }

        public async Task<bool> Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}
