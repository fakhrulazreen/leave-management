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

        public bool Create(LeaveRequest entity)
        {
            _db.LeaveRequests.Add(entity);
            return Save();
        }

        public bool Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return Save();
        }

        public ICollection<LeaveRequest> FindAll()
        {
            var leaveRequests = _db.LeaveRequests
                .Include(q => q.RequestingEmployee) // join employee
                .Include(q => q.ApprovedBy) // join employee
                .Include(q => q.LeaveType) // join leave type
                .ToList();
            return leaveRequests;
        }

        public LeaveRequest FindById(int id)
        {
            var leaveRequest = _db.LeaveRequests
                .Include(q => q.RequestingEmployee) // join employee
                .Include(q => q.ApprovedBy) // join employee
                .Include(q => q.LeaveType) // join leave type
                .FirstOrDefault(q => q.Id == id); //get first row or null record
            return leaveRequest;
        }

        public ICollection<LeaveRequest> GetLeaveRequestByEmployee(string employeeId)
        {
            var leaveRequests = FindAll()
                .Where(q => q.RequestingEmployeeId == employeeId)
                .ToList();
            return leaveRequests;
        }

        public bool isExists(int id)
        {
            var exists = _db.LeaveRequests.Any(q => q.Id == id); // any() check if empty record is empty or not
            return exists;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0; //bcs anything update/delete/add will have one or more record, if less than one then error on the operation
        }

        public bool Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);

            return Save();
        }
    }
}
